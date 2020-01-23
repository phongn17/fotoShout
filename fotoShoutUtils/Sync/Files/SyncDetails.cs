using Microsoft.Synchronization;
using Microsoft.Synchronization.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutUtils.Sync.Files {
    public class SyncDetails : IDisposable, IChangeDataRetriever {
        public const int BATCHSIZE_DEFAULT = 100;

        private const string METADATA_STORE_FILENAME = "file.sync";

        private string folderPath;
        public FileSyncScopeFilter ScopeFilter { get; set; }
        private SyncIdFormatGroup _idFormats = null;
        private SyncKnowledge _syncKnowledge = null;
        private ForgottenKnowledge _forgottenKnowledge = null;
        private ulong tickCount = 1; // Default -- lowest valid tickCount
        private MetadataStore metadataStore = new MetadataStore();
        private bool localChangedDetected = false;

        public SyncDetails(string folderPath) {
            this.folderPath = folderPath;
            Load();
        }

        public SyncDetails(string folderPath, string[] filters) 
            : this(folderPath) {
            SetScopeFilter(filters);
        }

        private string FolderPath {
            get { return folderPath; }
        }

        protected ulong TickCount {
            get {
                return tickCount;
            }
        }

        public void SetScopeFilter(string[] filters) {
            FileSyncScopeFilter scopeFilter = new FileSyncScopeFilter();
            foreach (string filter in filters)
                scopeFilter.FileNameIncludes.Add(filter.ToUpperInvariant());
            ScopeFilter = scopeFilter;
        }

        protected MetadataStore MetadataStore {
            get {
                return metadataStore;
            }
        }

        public SyncId ReplicaId { get; set; }

        public SyncKnowledge SyncKnowledge { 
            get {
                return _syncKnowledge;
            }
            set {
                _syncKnowledge = value;
            }
        }

        public ForgottenKnowledge ForgottenKnowledge {
            get {
                return _forgottenKnowledge;
            }
            set {
                _forgottenKnowledge = value;
            }
        }

        public ChangeBatch GetChangeBatch(uint batchSize, SyncKnowledge destinationKnowledge, out ForgottenKnowledge forgottenKnowledge, out object changeDataRetriever) {
            // Increment the tick count
            GetNextTickCount();

            // Get local changes
            List<ItemChange> changes = DetectChanges(destinationKnowledge, batchSize);

            // Update the knowledge with an updated local tick count
            SyncKnowledge.SetLocalTickCount(tickCount);

            // Construct the ChangeBatch and return it
            ChangeBatch changeBatch = new ChangeBatch(IdFormats, destinationKnowledge, ForgottenKnowledge);
            changeBatch.BeginUnorderedGroup();
            changeBatch.AddChanges(changes);
            if (changes.Count < batchSize || changes.Count == 0)
                changeBatch.SetLastBatch();
            changeBatch.EndUnorderedGroup(SyncKnowledge, changeBatch.IsLastBatch);

            // Return the forgotten knowledge
            forgottenKnowledge = ForgottenKnowledge;

            changeDataRetriever = this;

            return changeBatch;
        }

        public List<ItemChange> GetChanges(ChangeBatch sourceChanges) {
            // Increment the tick count
            GetNextTickCount();

            // Increase local knowledge tick count.
            SyncKnowledge.SetLocalTickCount(tickCount);

            // Create a collection to hold the changes we'll put into our batch
            List<ItemChange> changes = new List<ItemChange>();
            foreach (ItemChange ic in sourceChanges) {
                ItemMetadata item;
                ItemChange change;
                // Iterate through each item to get the corresponding version in the local store
                if (metadataStore.TryGetItem(ic.ItemId, out item)) {
                    // Found the corresponding item in the local metadata
                    // Get the local creation version and change (update) version from the metadata 
                    change = new ItemChange(IdFormats, ReplicaId, item.ItemId, item.IsTombstone ? ChangeKind.Deleted: ChangeKind.Update, item.CreationVersion, item.ChangeVersion);
                }
                else {
                    // Remote item has no local counterpart
                    // This item is unknown to us
                    change = new ItemChange(IdFormats, ReplicaId, ic.ItemId, ChangeKind.UnknownItem, SyncVersion.UnknownVersion, SyncVersion.UnknownVersion);
                }

                // Add our change to the change list
                changes.Add(change);
            }

            return changes;
        }

        public void StoreKnowledgeForScope(SyncKnowledge knowledge, ForgottenKnowledge forgottenKnowledge) {
            // Update in memory knowledge
            SyncKnowledge = knowledge;
            ForgottenKnowledge = forgottenKnowledge;
        }

        public ItemMetadata GetItemMetaData(SaveChangeAction saveChangeAction, ItemChange change, TransferMechanism data) {
            ItemMetadata item;

            // Populate the URI field of the updated item metadata
            if (saveChangeAction == SaveChangeAction.UpdateVersionOnly || ((change.ChangeKind & ChangeKind.Deleted) == ChangeKind.Deleted)) {
                // Version only changes and deletions will not contain data,
                // so we cannot get the item URI from the change data
                //
                // So, we attempt to look up the item metadata instead (by item id) 
                // to populate the URI field
                if (!metadataStore.TryGetItem(change.ItemId, out item)) {
                    // Not found, must mean we've never seen this item locally
                    // Can safely use an empty URI
                    item = new ItemMetadata();
                    item.Uri = String.Empty;
                }
            }
            else {
                // Since this isn't a version-only change or a delete, context should contain
                // change data
                item = new ItemMetadata(); // Used for storing updated item metadata

                // Populate the Uri field 
                item.Uri = data.Uri;
            }

            // Now copy the rest of the metadata for the item from the change
            item.ItemId = change.ItemId;
            item.CreationVersion = change.CreationVersion;
            item.ChangeVersion = change.ChangeVersion;

            // If deletion change, mark it as a tombstone
            if ((change.ChangeKind & ChangeKind.Deleted) == ChangeKind.Deleted)
                item.IsTombstone = true;

            // Is this a new item?
            if (!metadataStore.Has(item.ItemId)) {
                ItemMetadata oldItem;

                // Not found
                // Need to check for duplicates (by URI)
                if (metadataStore.TryGetItem(item.Uri, out oldItem)) {
                    // Duplicate item (same Uri), so we deterministically choose a winner
                    // by comparing the guid part of the item ids
                    if (item.ItemId.CompareTo(oldItem.ItemId) > 0) {
                        // New item is the winner
                        // Record old item as a loser
                        oldItem.IsTombstone = true;
                        oldItem.Uri = String.Empty;
                        oldItem.ChangeVersion = new SyncVersion(0, tickCount);
                    }
                    else {
                        // Old item is the winner
                        // Record new item as a loser
                        item.IsTombstone = true;
                        item.Uri = String.Empty;
                        item.ChangeVersion = new SyncVersion(0, tickCount);
                    }

                    // Record the updated item metadata
                    metadataStore.SetItemInfo(item);
                    metadataStore.SetItemInfo(oldItem);
                }
            }

            // Save the updated item metadata
            metadataStore.SetItemInfo(item);

            return item;
        }

        public void GetUpdatedKnowledge(SaveChangeContext context) {
            context.GetUpdatedDestinationKnowledge(out _syncKnowledge, out _forgottenKnowledge);
        }

        public void DeleteItem(SyncId itemID) {
            ItemMetadata item;
            if (metadataStore.TryGetItem(itemID, out item)) {
                item.IsTombstone = true;
                File.Delete(Path.Combine(folderPath, item.Uri));
            }
        }

        public ulong GetNextTickCount() {
            return ++tickCount;
        }

        public void SetLocalTickCount() {
            SyncKnowledge.SetLocalTickCount(tickCount);
        }

        public void UpdateItemItem(ItemMetadata item) {
            metadataStore.SetItemInfo(item);
        }

        public void Save() {
            string syncFile = Path.Combine(folderPath, SyncDetails.METADATA_STORE_FILENAME);

            File.Delete(syncFile);

            using (FileStream stream = new FileStream(syncFile, FileMode.OpenOrCreate)) {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(stream, ReplicaId);
                bf.Serialize(stream, tickCount);
                // Serialize knowledge to the file
                bf.Serialize(stream, SyncKnowledge);
                bf.Serialize(stream, ForgottenKnowledge);
                // Serialize metadatastore
                metadataStore.Save(stream);
            }

            FileInfo fi = new FileInfo(syncFile);
            fi.Attributes = FileAttributes.Hidden | FileAttributes.System;
        }

        public void Load() {
            string syncFile = Path.Combine(folderPath, SyncDetails.METADATA_STORE_FILENAME);

            if (File.Exists(syncFile)) {
                using (FileStream stream = new FileStream(syncFile, FileMode.Open)) {
                    BinaryFormatter bf = new BinaryFormatter();
                    ReplicaId = (SyncId)bf.Deserialize(stream);
                    tickCount = (ulong)bf.Deserialize(stream);
                    // Load Knowledge File
                    SyncKnowledge = (SyncKnowledge)bf.Deserialize(stream);
                    if (SyncKnowledge.ReplicaId != ReplicaId)
                        throw new Exception("Replica id of loaded knowledge doesn't match replica id provided in constructor.");
                    ForgottenKnowledge = (ForgottenKnowledge)bf.Deserialize(stream);
                    if (ForgottenKnowledge.ReplicaId != ReplicaId)
                        throw new Exception("Replica id of loaded forgotten knowledge doesn't match replica id provided in constructor.");
                    // Load the meta data store
                    metadataStore.Load(stream);
                }
            }
            else {
                ReplicaId = new SyncId(Guid.NewGuid());
                SyncKnowledge = new SyncKnowledge(IdFormats, ReplicaId, tickCount);
                ForgottenKnowledge = new ForgottenKnowledge(IdFormats, SyncKnowledge);
            }
        }

        public SyncIdFormatGroup IdFormats {
            get {
                if (_idFormats == null)
                    _idFormats = SyncDetails.GetIdFormats();

                return _idFormats;
            }
        }

        public static SyncIdFormatGroup GetIdFormats() {
            SyncIdFormatGroup idFormats = new SyncIdFormatGroup();
            // 1 byte change unit id
            idFormats.ChangeUnitIdFormat.IsVariableLength = false;
            idFormats.ChangeUnitIdFormat.Length = 1;
            // Set format of replica id for using Guid ids
            idFormats.ReplicaIdFormat.IsVariableLength = false;
            idFormats.ReplicaIdFormat.Length = 16;
            // Set format of item id for using Sync id
            idFormats.ItemIdFormat.IsVariableLength = false;
            idFormats.ItemIdFormat.Length = 24;

            return idFormats;
        }

        public static DataContractSerializer GetSerializerWithKnownTypes(Type type) {
            List<Type> knownTypes = new List<Type>();
            knownTypes.Add(typeof(SyncIdFormatGroup));
            knownTypes.Add(typeof(SyncId));

            return new DataContractSerializer(type, knownTypes);
        }

        public object LoadChangeData(LoadChangeContext loadChangeContext) {
            ItemMetadata item;

            // Retrieve metadata for the changed item
            if (metadataStore.TryGetItem(loadChangeContext.ItemChange.ItemId, out item)) {
                // Make sure this isn't a delete
                if (item.IsTombstone)
                    throw new InvalidOperationException("Cannot load change data for a deleted item.");

                // Open the stream for the file we are transferring
                Stream dataStream = new FileStream(Path.Combine(FolderPath, item.Uri), FileMode.Open, FileAccess.Read, FileShare.Read);
                // Create an instance of our type for transferring change data
                TransferMechanism transferMechanism = new TransferMechanism(dataStream, item.Uri);

                // Return our data transfer object
                return transferMechanism;
            }

            return null;
        }

        public void Dispose() {
            try {
                Save();
            }
            catch {
                // eat any exception
            }
        }

        /// <summary>
        /// Detects changes not known to the destination and returns a change batch
        /// </summary>
        /// <param name="destinationKnowledge">Requester Knowledge</param>
        /// <param name="batchSize">Maximum number of changes to return</param>
        /// <returns>List of changes</returns>
        private List<ItemChange> DetectChanges(SyncKnowledge destinationKnowledge, uint batchSize) {
            List<ItemChange> changeBatch = new List<ItemChange>();

            if (destinationKnowledge == null)
                throw new ArgumentNullException("destinationKnowledge");
            if (batchSize < 0)
                throw new ArgumentOutOfRangeException("batchSize");

            if (!localChangedDetected) {
                FindLocalFileChanges(folderPath);
                localChangedDetected = true;
            }

            // Map the destination knowledge
            // This maps the knowledge from the remote replica key map (where the destination is replicaKey 0)
            // to the local replica key map (where the source is replicaKey)
            //
            // We do this because our metadata is relative to the local store (and local key map)
            // (This is typical of most sync providers)
            SyncKnowledge mappedKnowledge = SyncKnowledge.MapRemoteKnowledgeToLocal(destinationKnowledge);
            foreach (ItemMetadata item in metadataStore) {
                // Check if the current version of the item is known to the destination
                // We simply check if the update version is contained in his knowledge

                // If the metadata is for a tombstone, the change is a delete
                if (!mappedKnowledge.Contains(ReplicaId, item.ItemId, item.ChangeVersion)) {
                    ItemChange itemChange = new ItemChange(IdFormats, ReplicaId, item.ItemId, item.IsTombstone ? ChangeKind.Deleted : ChangeKind.Update, item.CreationVersion, item.ChangeVersion);
                    // Change isn't known to the remote store, so add it to the batch
                    changeBatch.Add(itemChange);
                }

                // If the batch is full, break
                //
                // N.B. Rest of changes will be detected in next batch. Current batch will not be
                // reenumerated (except in case destination doesn't successfully apply them) as
                // when destination applies the changes in this batch, they will add them to their
                // knowledge.
                if (changeBatch.Count == batchSize)
                    break;
            }

            return changeBatch;
        }

        /// <summary>
        /// Detect changes to the local store and update metadata store accordingly
        /// </summary>        
        private void FindLocalFileChanges(string path) {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);

            foreach (DirectoryInfo dir in directoryInfo.GetDirectories()) {
                FindLocalFileChanges(dir.FullName);
            }

            // Enumerate all files in the directory
            foreach (FileInfo fi in directoryInfo.GetFiles()) {
                string uri = fi.FullName.Substring(folderPath.Length + 1);
                // So, skip .Sync files
                if (IsIncluded(Path.GetExtension(fi.Name).ToUpperInvariant())) {
                    ItemMetadata existingItem;
                    // Lookup the item in the metadata store 
                    if (!metadataStore.TryGetItem(uri, out existingItem)) {
                        // Not already recorded, so create an entry
                        // This is a create

                        ItemMetadata item = new ItemMetadata();
                        item.Uri = uri;
                        item.IsTombstone = false; // Not a tombstone, since alive
                        item.ItemId = new SyncId(new SyncGlobalId(0, Guid.NewGuid()));
                        // Record the create version
                        // Local replica key is always 0
                        item.CreationVersion = new SyncVersion(0, tickCount);
                        // For a newly created version, ChangeVersion = CreationVersion
                        item.ChangeVersion = item.CreationVersion;
                        // Record LWT for the file so we can use it for change
                        // detection on subsequent syncs
                        item.LastWriteTimeUtc = fi.LastWriteTimeUtc;
                        // Store the new entry in the metadata store
                        metadataStore.SetItemInfo(item);
                    }
                    else {
                        // Not a create, so compare LWT to see if file was changed locally
                        if (fi.LastWriteTimeUtc > existingItem.LastWriteTimeUtc) {
                            // Later write time, so local change
                            // Update the change version
                            existingItem.ChangeVersion = new SyncVersion(0, tickCount);

                            // Record new LWT
                            existingItem.LastWriteTimeUtc = fi.LastWriteTimeUtc;

                            // Store the updated metadata
                            metadataStore.SetItemInfo(existingItem);
                        }
                    }
                }
            }

            // Now, using the metadata store, determine local deletes
            // (Item is deleted if we have a (non-tombstone) metadata entry
            // for it, but can't find the file locally
            List<ItemMetadata> toUpdate = new List<ItemMetadata>();
            foreach (ItemMetadata item in metadataStore) {
                if (!File.Exists(Path.Combine(FolderPath, item.Uri))) {
                    // Doesn't exist

                    // Check is already recorded as a tombstone
                    if (!item.IsTombstone) {
                        // Not recorded
                        // This is a new delete
                        item.IsTombstone = true;

                        // Record the version
                        item.ChangeVersion = new SyncVersion(0, tickCount);

                        // Record the updated item for save
                        toUpdate.Add(item);
                    }
                }
            }

            // Save any tombstone changes
            foreach (ItemMetadata item in toUpdate) {
                metadataStore.SetItemInfo(item);
            }
        }

        private bool IsIncluded(string ext) {
            return ScopeFilter.FileNameIncludes.Contains(ext);
        }
    }
}
