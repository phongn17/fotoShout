using FotoShoutUtils.Sync.Files;
using Microsoft.Synchronization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FsSyncWebService.Files {
    public class RemoteSyncDetails: SyncDetails {

        public RemoteSyncDetails(string folderPath)
            : base (folderPath) {
        }

        public RemoteSyncDetails(string folderPath, string[] filters)
            : base(folderPath, filters) {
        }

        public List<ItemChangeMetadata> GetMetadataForChanges(ChangeBatch sourceChanges) {
            // Increment the tick count
            GetNextTickCount();

            // Increase local knowledge tick count.
            SyncKnowledge.SetLocalTickCount(TickCount);

            // Create a collection to hold the changes we'll put into our batch
            List<ItemChangeMetadata> changes = new List<ItemChangeMetadata>();
            foreach (ItemChange ic in sourceChanges) {
                ItemMetadata item;
                ItemChangeMetadata change;
                // Iterate through each item to get the corresponding version in the local store
                if (MetadataStore.TryGetItem(ic.ItemId, out item)) {
                    // Found the corresponding item in the local metadata
                    // Get the local creation version and change (update) version from the metadata 
                    change = new ItemChangeMetadata(item.ItemId, item.IsTombstone ? ChangeKind.Deleted : ChangeKind.Update, item.CreationVersion, item.ChangeVersion);
                }
                else {
                    // Remote item has no local counterpart
                    // This item is unknown to us
                    change = new ItemChangeMetadata(ic.ItemId, ChangeKind.UnknownItem, SyncVersion.UnknownVersion, SyncVersion.UnknownVersion);
                }

                // Add our change to the change list
                changes.Add(change);
            }

            return changes;
        }

        public static List<ItemChange> GenerateChanges(ItemsChangeInfo changeInfo) {
            List<ItemChange> changes = new List<ItemChange>();
            foreach (ItemChangeMetadata ic in changeInfo.ItemChanges) {
                ItemChange change = new ItemChange(changeInfo.IdFormats, changeInfo.ReplicaId, ic.ItemId, ic.ChangeKind, ic.CreationVersion, ic.ChangeVersion);

                // Add our change to the change list
                changes.Add(change);
            }

            return changes;
        }
    }
}
