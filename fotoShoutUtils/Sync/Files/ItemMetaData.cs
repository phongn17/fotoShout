using Microsoft.Synchronization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutUtils.Sync.Files {
    [Serializable()]
    public class ItemMetadata {
        private DateTime? _lastWriteTimeUtc;

        public SyncId ItemId { get; set; }
        public SyncVersion CreationVersion { get; set; }
        public SyncVersion ChangeVersion { get; set; }
        public string Uri { get; set; }
        public bool IsTombstone { get; set; }

        public DateTime LastWriteTimeUtc {
            get {
                if (!_lastWriteTimeUtc.HasValue)
                    throw new InvalidOperationException("LastWriteTimeUtc not yet set.");

                return _lastWriteTimeUtc.Value;
            }

            set {
                _lastWriteTimeUtc = value;
            }
        }

        public ItemMetadata Clone() {
            return (ItemMetadata)this.MemberwiseClone();
        }
    }
}
