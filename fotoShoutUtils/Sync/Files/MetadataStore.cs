using Microsoft.Synchronization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutUtils.Sync.Files {
    public class MetadataStore : IEnumerable<ItemMetadata> {
        private string _path = null;
        private List<ItemMetadata> _data = new List<ItemMetadata>();

        public string Path {
            get {
                if (_path == null)
                    throw new InvalidOperationException("Path is not set until metadata store is created / loaded.");

                return _path;
            }

            private set { 
                _path = value; 
            }
        }


        private List<ItemMetadata> Data {
            get {
                if (_data == null)
                    throw new InvalidOperationException("Metadata store contains no data (need to load or create).");

                return _data;
            }

            set {
                _data = value;
            }
        }


        /// <summary>
        /// Creates a metadata store with a specified maximum capacity.
        /// </summary>
        /// <param name="path">Path where to create metadata store</param>
        public void Create(string path) {
            // Record the path
            Path = path;

            // Create data list
            Data = new List<ItemMetadata>();

            // Write out the new metadata store instance 
            Save();
        }

        public void Load(string path) {
            Path = path;
            
            FileStream inputStream = new FileStream(Path, FileMode.Open);
            Load(inputStream);

            // Close the file
            inputStream.Close();
        }

        public void Load(FileStream stream) {
            // Deserialize the data from the file
            BinaryFormatter bf = new BinaryFormatter();
            Data = bf.Deserialize(stream) as List<ItemMetadata>;
        }

        public void Save() {
            FileStream outputStream = new FileStream(Path, FileMode.OpenOrCreate);
            Save(outputStream);

            // Close the file
            outputStream.Close();
        }

        public void Save(FileStream stream) {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(stream, Data);
        }

        public bool TryGetItem(string uri, out ItemMetadata item) {
            ItemMetadata im = Data.Find(delegate(ItemMetadata compareItem) { return (compareItem.Uri == uri && compareItem.IsTombstone == false); });

            if (im == null) {
                item = null;
                return false; // Not found
            }

            item = im.Clone();

            return true;
        }

        public bool TryGetItem(SyncId itemId, out ItemMetadata item) {
            ItemMetadata im = Data.Find(delegate(ItemMetadata compareItem) { return (compareItem.ItemId == itemId); });

            if (im == null) {
                item = null;
                return false; // Not found
            }

            item = im.Clone();

            return true;
        }

        public ItemMetadata GetItem(int index) {
            return Data[index].Clone();
        }

        public bool Has(SyncId itemId) {
            return Data.Exists(delegate(ItemMetadata compareItem) { return (compareItem.ItemId == itemId); });
        }

        public int NumEntries {
            get {
                return Data.Count;
            }
        }

        public void SetItemInfo(ItemMetadata itemMetadata) {
            int index = Data.FindIndex(delegate(ItemMetadata compareItem) { return (compareItem.ItemId == itemMetadata.ItemId); });
            if (index >= 0) {
                // Found, so overwrite 
                Data[index] = itemMetadata.Clone();
            }
            else {
                // Not found, so add
                Data.Add(itemMetadata.Clone());
            }
        }

        public void ReplaceItem(SyncId oldItemId, ItemMetadata item) {
            int index = Data.FindIndex(delegate(ItemMetadata compareItem) { return (compareItem.ItemId == oldItemId); });
            // Error if not found
            if (index < 0)
                throw new KeyNotFoundException("Entry with ItemId == oldItemId not found.");

            Data[index] = item.Clone();
        }

        public IEnumerator<ItemMetadata> GetEnumerator() {
            return Data.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return ((System.Collections.IEnumerable)Data).GetEnumerator();
        }

        public void CleanTombstones() {
            for (int i = Data.Count; i > 0; i--) {
                if (Data[i - 1].IsTombstone) {
                    Data.RemoveAt(i - 1);
                }
            }
        }
    }
}
