using FotoShoutUtils.Sync.Files;
using Microsoft.Synchronization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FsSyncWebService.Files {
    public class RemoteFileSyncService : IRemoteFileSyncService {
        public void Initialize() {
        }

        public string Ping() {
            return "Remote file sync service is ready.";
        }
        
        public SyncKnowledge GetCurrentSyncKnowledge(string folderPath, string[] filters) {
            using (SyncDetails sync = new SyncDetails(folderPath, filters)) {
                return sync.SyncKnowledge;
            }
        }

        public ItemsChangeInfo GetChanges(string folderPath, ChangeBatch sourceChanges, string[] filters) {
            using (RemoteSyncDetails sync = new RemoteSyncDetails(folderPath, filters)) {
                List<ItemChangeMetadata> itemChanges = sync.GetMetadataForChanges(sourceChanges);
                ItemsChangeInfo lst = new ItemsChangeInfo();
                lst.IdFormats = sync.IdFormats;
                lst.ReplicaId = sync.ReplicaId;
                lst.ItemChanges = itemChanges;

                return lst;
            }
        }

        public void UploadFile(RemoteFileInfo request) {
            FileInfo fi = new FileInfo(Path.Combine(request.FolderPath, request.Metadata.Uri));

            if (!fi.Directory.Exists) {
                fi.Directory.Create();
            }

            fi.Delete();

            int chunkSize = 2048;
            byte[] buffer = new byte[chunkSize];
            using (FileStream writeStream = new FileStream(fi.FullName, FileMode.CreateNew, FileAccess.Write)) {
                do {
                    // read bytes from input stream
                    int bytesRead = request.FileByteStream.Read(buffer, 0, chunkSize);
                    if (bytesRead == 0) break;

                    // write bytes to output stream
                    writeStream.Write(buffer, 0, bytesRead);
                } while (true);

                writeStream.Close();
                // fi.LastWriteTimeUtc;
            }

            using (SyncDetails sync = new SyncDetails(request.FolderPath)) {
                sync.UpdateItemItem(request.Metadata);
            }
        }

        public void DeleteFile(string folderPath, SyncId itemID, string[] filters) {
            SyncDetails sync = new SyncDetails(folderPath, filters);
            sync.DeleteItem(itemID);
        }

        public void StoreKnowledgeForScope(string folderPath, SyncKnowledge knowledge, ForgottenKnowledge forgotten, string[] filters) {
            using (SyncDetails sync = new SyncDetails(folderPath, filters)) {
                sync.StoreKnowledgeForScope(knowledge, forgotten);
            }
        }

        public void Cleanup() {
        }
    }
}
