using FotoShoutData.Models;
using FotoShoutData.Models.Authenticate;
using FotoShoutUtils.Networking;
using FotoShoutUtils.Service;
using FotoShoutUtils.Sync;
using FotoShoutUtils.Sync.Db;
using FotoShoutUtils.Sync.Files;
using FotoShoutUtils.Utils;
using FotoShoutUtils.Utils.IO;
using FsSyncWebService.Db.Sql;
using FsSyncWebService.Files;
using log4net;
using Microsoft.Synchronization;
using Microsoft.Synchronization.Data;
using Microsoft.Synchronization.Files;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.Text;

namespace FotoShoutSyncService {
    class SyncEngine: Executor {
        private const string WFC_SERVICE_SQLSERVER = "SqlServerSyncService";
        private const string WFC_SERVICE_FILESERVER = "RemoteFileSyncService";

        static ILog _logger = LogManager.GetLogger(typeof(SyncEngine));

        private FsApiWebService _fsServerService = new FsApiWebService(AppConfig.FsApiServerBaseAddress, AppConfig.FsApiPrefix, FotoShoutUtils.Constants.MEDIATYPE_APPLICATION_JSON);
        private FsApiWebService _fsClientService = new FsApiWebService(AppConfig.FsApiClientBaseAddress, AppConfig.FsApiPrefix, FotoShoutUtils.Constants.MEDIATYPE_APPLICATION_JSON);

        private DbClientSync _sqlEventClientSync = null;
        private DbClientSync _sqlPhotoClientSync = null;
        private SqlServerSyncProviderProxy _sqlEventServerSync = null;
        private SqlServerSyncProviderProxy _sqlPhotoServerSync = null;

        public UserTDO ClientUser { get; set; }
        
        private UserTDO User { get; set; }

        public override bool Initialize(bool timerUsed) {
            try {
                FotoShoutUtils.Log.LogManager.Info(_logger, Assembly.GetExecutingAssembly().GetName().Version);
                FotoShoutUtils.Log.LogManager.Info(_logger, "Initializing...");

                if (!base.Initialize(timerUsed))
                    return false;

                FotoShoutUtils.Log.LogManager.Info(_logger, "Done Initalizing process.");

                return true;
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString() + "\n");
            }

            return false;
        }

        public override bool DeInitialize() {
            if (!base.DeInitialize())
                return false;

            // De-register event handlers for db sync
            if (_sqlEventClientSync != null) {
                _sqlEventClientSync.ApplyChangeFailed -= new ApplyChangeFailedEventHandler(OnDbApplyChangeFailed);
                _sqlEventClientSync.ItemConflicting -= new FotoShoutUtils.Sync.Db.ItemConflictingEventHandler(OnDbItemConflicting);
                _sqlEventClientSync.ItemConstraint -= new FotoShoutUtils.Sync.Db.ItemConstraintEventHandler(OnDbItemConstraint);
                _sqlEventClientSync.Synchronized -= new FotoShoutUtils.Sync.Db.SynchronizedEventHandler(OnDbDownloadSynchronized);
            }

            return true;
        }

        public override void Execute() {
            try {
                string syncAction = AppConfig.SyncAction;
                if (string.IsNullOrEmpty(syncAction) || !syncAction.Equals(FotoShoutUtils.Constants.ASV_SYNCACTION_DEPROVISIONSTORE, StringComparison.InvariantCultureIgnoreCase)) {
                    // Initialize paramerters for FotoShout API
                    if (!InitializeApi(_fsServerService, true))
                        return;

                    if (InitializeApi(_fsClientService, false)) {
                        FotoShoutUtils.Log.LogManager.Info(_logger, "Synchronizing events' data from local to central server...");
                        UploadEventsData();
                        FotoShoutUtils.Log.LogManager.Info(_logger, "Done synchronized events' data.");
                    }
                }

                // Initialize parameters for Sync database 
                if (!InitializeEventSyncProviders()) {
                    return;
                }

                FotoShoutUtils.Log.LogManager.Info(_logger, "Downloading database records from central server to local...");
                _sqlEventClientSync.Synchronize(_sqlEventServerSync);
                //_sqlEventClientSync.Synchronize(sqlServerSync.Provider);
                
                FotoShoutUtils.Log.LogManager.Info(_logger, "Done downloaded database.\n");
            }
            catch (Exception ex) {
                if (ex is CommunicationObjectFaultedException) {
                    _sqlEventServerSync = null;
                    _sqlEventClientSync = null;
                    FotoShoutUtils.Log.LogManager.Error(_logger, ex.Message + "\n The service will be re-initiated in the next round.");
                }
                else {
                    FotoShoutUtils.Log.LogManager.Error(_logger, ex.ToString() + "\n");
                }
            }
        }

        private void UploadEventsData() {
            FotoShoutUtils.Log.LogManager.Info(_logger, "Getting events...");
            IEnumerable<EventTDO> events = _fsClientService.GetEvents("Open");
            if (!events.Any()) {
                FotoShoutUtils.Log.LogManager.Info(_logger, "There is no events in the local machine for the current FotoShout user.");
                return;
            }

            IEnumerable<EventTDO> serverEvents = _fsServerService.GetEvents("All");
            foreach (EventTDO ev in events) {
                bool processSync = (!string.IsNullOrEmpty(AppConfig.SyncAction) && !AppConfig.SyncAction.Equals(FotoShoutUtils.Constants.ASV_SYNCACTION_PROVISION, StringComparison.InvariantCultureIgnoreCase));
                if (!processSync) {
                    EventTDO serverEvent = serverEvents.Where(se => se.EventId == ev.EventId && se.EventName.Equals(ev.EventName, StringComparison.InvariantCulture)).SingleOrDefault();
                    processSync = (serverEvent != null);
                }
                if (processSync)
                    UploadEventData(ev);
                else
                    FotoShoutUtils.Log.LogManager.Info(_logger, string.Format("The \"{0}\" event was locally created on the PC, so its photos won't be uploaded to the central server.", ev.EventName));
            }
        }

        private void UploadEventData(EventTDO ev) {
            SynchronizeFiles(ev);
            SynchronizePhotos(ev);
        }

        private void SynchronizeFiles(EventTDO ev) {
            FileStoreSync fsClientSync = null;
            FileStoreProxy fsServerSync = null;
            try {
                string evFolder = ev.EventFolder;
                if (string.IsNullOrEmpty(evFolder)) {
                    FotoShoutUtils.Log.LogManager.Error(_logger, string.Format("The event folder name for the \"{0}\" event is empty.", ev.EventName));
                    return;
                }

                DirectoryInfo di = new DirectoryInfo(evFolder);
                if (!di.Exists) {
                    FotoShoutUtils.Log.LogManager.Error(_logger, string.Format("The \"{0}\" folder of the \"{1}\" event not found.", evFolder, ev.EventName));
                    return;
                }

                FotoShoutUtils.Log.LogManager.Info(_logger, string.Format("Synchronizing photos of the \"{0}\" event.", ev.EventName));
                string[] imgExts = ImageUtils.GetImageExts().Split('|');
                fsClientSync = InitializeSyncFileSystem(evFolder, SyncDirectionOrder.Upload, imgExts);
                fsServerSync = new FileStoreProxy(evFolder, SyncEngine.WFC_SERVICE_FILESERVER, imgExts);
                FotoShoutUtils.Log.LogManager.Info(_logger, fsServerSync.Ping());

                fsClientSync.Synchronize(fsServerSync, SyncDetails.BATCHSIZE_DEFAULT);
                FotoShoutUtils.Log.LogManager.Info(_logger, string.Format("Successfully Synchronized photos of the \"{0}\" event.", ev.EventName));
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, string.Format("Exception when synchronizing photos of the \"{0}\" event.\n{1}", ev.EventName, ex.ToString()));
            }
            finally {
                if (fsClientSync != null) {
                    // De-register event handlers for file system sync
                    fsClientSync.AppliedChange -= new AppliedChangeEventHandler(OnFileSystemAppliedChange);
                    fsClientSync.SkippedChange -= new SkippedChangeEventHandler(OnFileSystemSkippedChange);
                    fsClientSync.ItemConflicting -= new FotoShoutSyncService.ItemConflictingEventHandler(OnFileSystemItemConflicting);
                    fsClientSync.ItemConstraint -= new FotoShoutSyncService.ItemConstraintEventHandler(OnFileSystemItemConstraint);
                    fsClientSync.Synchronized -= new FotoShoutSyncService.SynchronizedEventHandler(OnFileSystemSynchronized);
                }
            }
        }

        private bool SynchronizePhotos(EventTDO ev) {
            try {
                string scopeName = GenerateScopeName(AppConfig.PhotoSyncScopeName, ev.EventId.ToString());
                if (!InitializePhotoSyncProviders(ev, scopeName, AppConfig.PhotoStaticSyncTables, AppConfig.PhotoDynamicSyncTables))
                    return false;
                    
                _sqlPhotoClientSync.ChangesApplied += new ChangesAppliedEventHandler(OnDbChangesApplied);

                FotoShoutUtils.Log.LogManager.Info(_logger, "Uploading photos and guests to central server...");
                _sqlPhotoClientSync.Synchronize(_sqlPhotoServerSync);
                //_sqlPhotoClientSync.Synchronize(sqlServerSync.Provider);
                
                FotoShoutUtils.Log.LogManager.Info(_logger, "Successfully uploaded.");

                return true;
            }
            catch (Exception ex) {
                FotoShoutUtils.Log.LogManager.Error(_logger, string.Format("Exception when synchronizing database of photos of the \"{0}\" event.\n{1}", ev.EventName, ex.ToString()));
            }
            finally {
                RemoveClientUploadSyncEventHandlers(_sqlPhotoClientSync);
            }
            
            return false;
        }

        private void RemoveClientUploadSyncEventHandlers(DbClientSync sqlClientUploadSync) {
            if (sqlClientUploadSync != null) {
                sqlClientUploadSync.ApplyChangeFailed -= new ApplyChangeFailedEventHandler(OnDbApplyChangeFailed);
                sqlClientUploadSync.ItemConflicting -= new FotoShoutUtils.Sync.Db.ItemConflictingEventHandler(OnDbItemConflicting);
                sqlClientUploadSync.ItemConstraint -= new FotoShoutUtils.Sync.Db.ItemConstraintEventHandler(OnDbItemConstraint);
                sqlClientUploadSync.Synchronized -= new FotoShoutUtils.Sync.Db.SynchronizedEventHandler(OnDbUploadSynchronized);
            }
        }

        #region Db sync event handlers

        private void OnDbItemConstraint(object sender, ItemConstraintEventArgs ev) {
            FotoShoutUtils.Log.LogManager.Error(_logger, string.Format("Item constraint conflict: {0}, {1}, {2}, {3}.", ev.SourceChange.ToString(), ev.SourceChangeData.ToString(), ev.DestinationChange.ToString(), ev.DestinationChangeData.ToString()));
        }

        private void OnDbItemConflicting(object sender, ItemConflictingEventArgs ev) {
            FotoShoutUtils.Log.LogManager.Error(_logger, string.Format("Item conflict: {0}, {1}, {2}, {3}.", ev.SourceChange.ToString(), ev.SourceChangeData.ToString(), ev.DestinationChange.ToString(), ev.DestinationChangeData.ToString()));
        }

        private void OnDbApplyChangeFailed(object sender, Microsoft.Synchronization.Data.DbApplyChangeFailedEventArgs ev) {
            if ((ev.Conflict.Type == DbConflictType.LocalInsertRemoteInsert) || (ev.Conflict.Type == DbConflictType.LocalUpdateRemoteUpdate)) {
                DataTable conflictingLocalChange = ev.Conflict.LocalChange;
                StringBuilder sb = new StringBuilder();
                DataTable conflictingRemoteChange = ev.Conflict.RemoteChange;
                for (int idx = 0; idx < conflictingRemoteChange.Columns.Count; idx++) {
                    if (sb.Length > 0)
                        sb.Append("|");
                    sb.Append(conflictingRemoteChange.Rows[0][idx]);
                }
                if (conflictingLocalChange.TableName.Equals(FotoShoutUtils.Constants.TABLE_ACCOUNTS, StringComparison.InvariantCultureIgnoreCase) ||
                    conflictingLocalChange.TableName.Equals(FotoShoutUtils.Constants.TABLE_USERROLES, StringComparison.InvariantCultureIgnoreCase) ||
                    conflictingLocalChange.TableName.Equals(FotoShoutUtils.Constants.TABLE_USERAUTHORIZATIONS, StringComparison.InvariantCultureIgnoreCase) ||
                    conflictingLocalChange.TableName.Equals(FotoShoutUtils.Constants.TABLE_USERS, StringComparison.InvariantCultureIgnoreCase) ||
                    conflictingLocalChange.TableName.Equals(FotoShoutUtils.Constants.TABLE_EVENTOPTIONS, StringComparison.InvariantCultureIgnoreCase) ||
                    conflictingLocalChange.TableName.Equals(FotoShoutUtils.Constants.TABLE_EVENTS, StringComparison.InvariantCultureIgnoreCase)) {
                    ev.Action = ApplyAction.RetryWithForceWrite;
                    FotoShoutUtils.Log.LogManager.Error(_logger, string.Format("Row from the {0} table was conflicted and will be forced to write to the client: {1}", conflictingRemoteChange.TableName, sb.ToString()));
                }
                else {
                    ev.Action = ApplyAction.Continue;
                    FotoShoutUtils.Log.LogManager.Error(_logger, string.Format("Row from the {0} table was conflicted and will be bypass: {1}", conflictingRemoteChange.TableName, sb.ToString()));
                }
            }
            else {
                FotoShoutUtils.Log.LogManager.Error(_logger, string.Format("Conflict of type {0} was detected.", ev.Conflict.Type));
            }
        }

        private void OnDbChangesApplied(object sender, Microsoft.Synchronization.Data.DbChangesAppliedEventArgs ev) {
            FotoShoutUtils.Log.LogManager.Error(_logger, string.Format("Changes applied: {0}.", ev.Context.ToString()));
        }

        private void OnDbDownloadSynchronized(object sender, DbSynchronizedEventArgs ev) {
            OnDownloadSynchronized(sender, ev);
        }

        private void OnDbUploadSynchronized(object sender, DbSynchronizedEventArgs ev) {
            OnUploadSynchronized(sender, ev);
        }

        #endregion // Db sync event handlers

        #region File system sync event handlers

        private void OnFileSystemAppliedChange(object sender, AppliedChangeEventArgs ev) {
            switch (ev.ChangeType) {
                case ChangeType.Create:
                    FotoShoutUtils.Log.LogManager.Info(_logger, string.Format("Applied CREATE for the \"{0}\" file.", ev.NewFilePath));
                    break;
                case ChangeType.Delete:
                    FotoShoutUtils.Log.LogManager.Info(_logger, string.Format("Applied DELETE for the \"{0}\" file.", ev.OldFilePath));
                    break;
                case ChangeType.Update:
                    FotoShoutUtils.Log.LogManager.Info(_logger, string.Format("Applied OVERWRITE for the \"{0}\" file.", ev.OldFilePath));
                    break;
                case ChangeType.Rename:
                    FotoShoutUtils.Log.LogManager.Info(_logger, string.Format("Applied RENAME for the \"{0}\" file as \"{1}\".", ev.OldFilePath, ev.NewFilePath));
                    break;
            }
        }

        private void OnFileSystemSkippedChange(object sender, SkippedChangeEventArgs ev) {
            FotoShoutUtils.Log.LogManager.Error(_logger, string.Format("Skipped {0} for the \"{1}\" due to error.", 
                                                            ev.ChangeType.ToString().ToUpper(), (!string.IsNullOrEmpty(ev.CurrentFilePath)) ? ev.CurrentFilePath : ev.NewFilePath));
            if (ev.Exception != null)
                FotoShoutUtils.Log.LogManager.Error(_logger, ev.Exception.ToString());
        }

        private void OnFileSystemItemConstraint(object sender, ItemConstraintEventArgs ev) {
            throw new NotImplementedException();
        }

        private void OnFileSystemItemConflicting(object sender, ItemConflictingEventArgs ev) {
            throw new NotImplementedException();
        }

        private void OnFileSystemSynchronized(object sender, FileSystemSynchronizedEventArgs ev) {
            OnUploadSynchronized(sender, ev);
        }

        #endregion // File system sync event handlers

        #region Sync Initialization

        private FileStoreSync InitializeSyncFileSystem(string folder, SyncDirectionOrder syncDirection, string[] filters) {
            FileStoreSync fsSync = new FileStoreSync(folder, syncDirection, filters);
            // Register event handlers for file system sync
            FotoShoutUtils.Log.LogManager.Info(_logger, "Registering event handlers for file system sync...");
            fsSync.AppliedChange += new AppliedChangeEventHandler(OnFileSystemAppliedChange);
            fsSync.SkippedChange += new SkippedChangeEventHandler(OnFileSystemSkippedChange);
            fsSync.ItemConflicting += new FotoShoutSyncService.ItemConflictingEventHandler(OnFileSystemItemConflicting);
            fsSync.ItemConstraint += new FotoShoutSyncService.ItemConstraintEventHandler(OnFileSystemItemConstraint);
            fsSync.Synchronized += new FotoShoutSyncService.SynchronizedEventHandler(OnFileSystemSynchronized);
            FotoShoutUtils.Log.LogManager.Info(_logger, "Successfully registered event handlers for file system sync...");
            
            return fsSync;
        }

        private FileClientSync InitializeFileClientSync(SyncDirectionOrder syncDirection) {
            FileClientSync fsSync = new FileClientSync();
            fsSync.SyncDirection = syncDirection;
            // Register event handlers for file system sync
            FotoShoutUtils.Log.LogManager.Info(_logger, "Registering event handlers for file system sync...");
            fsSync.AppliedChange += new AppliedChangeEventHandler(OnFileSystemAppliedChange);
            fsSync.SkippedChange += new SkippedChangeEventHandler(OnFileSystemSkippedChange);
            fsSync.ItemConflicting += new FotoShoutUtils.Sync.Files.ItemConflictingEventHandler(OnFileSystemItemConflicting);
            fsSync.ItemConstraint += new FotoShoutUtils.Sync.Files.ItemConstraintEventHandler(OnFileSystemItemConstraint);
            fsSync.Synchronized += new FotoShoutUtils.Sync.Files.SynchronizedEventHandler(OnFileSystemSynchronized);
            FotoShoutUtils.Log.LogManager.Info(_logger, "Successfully registered event handlers for file system sync...");

            return fsSync;
        }

        private bool InitializeApi(FsApiWebService fsApiService, bool serverAuthenticated) {
            if (serverAuthenticated) {
                if (User != null)
                    return true;
            }
            else if (ClientUser != null)
                return true;

            LoginModel model = new LoginModel { UserName = AppConfig.FsUserEmail, Password = AppConfig.FsPassword };

            UserTDO user = null;
            try {
                FotoShoutUtils.Log.LogManager.Info(_logger, string.Format("Authorizing for fotoShout on {0}...", fsApiService.BaseAddress));
                user = fsApiService.Login(AppConfig.FsApiKey, model);
                if (user == null) {
                    FotoShoutUtils.Log.LogManager.Info(_logger, string.Format("There is no user on {0} associated with the fotoShout Authorization key. The authentication will be retried in the next round.", fsApiService.BaseAddress));
                    return false;
                }
                FotoShoutUtils.Log.LogManager.Info(_logger, "Succeeded to access to the user info.");
            }
            catch (Exception ex) {
                if (ex is WebException)
                    FotoShoutUtils.Log.LogManager.Error(_logger, string.Format("Exception on {0}: {1}.", fsApiService.BaseAddress, ex.Message));
                else
                    FotoShoutUtils.Log.LogManager.Error(_logger, string.Format("There is no user on {0} associated with the fotoShout Authorization key. The authentication will be retried in the next round.", fsApiService.BaseAddress));
                return false;
            }

            if (serverAuthenticated)
                User = user;
            else
                ClientUser = user;
                
            return true;
        }

        //FilteringSqlServerSync sqlServerSync = null;
        
        private bool InitializeEventSyncProviders() {
            if (string.IsNullOrEmpty(AppConfig.EventDbServerSyncClass)) {
                FotoShoutUtils.Log.LogManager.Error(_logger, "The type of the database server download sync need to be defined in app config.");
                return false;
            }

            DbSyncScopeDescription scopeDescription = null;
            string scopeName = GenerateServerScopeName(AppConfig.EventSyncScopeName);
            if (_sqlEventServerSync == null) {
                _sqlEventServerSync = new SqlServerSyncProviderProxy(AppConfig.SyncAction, scopeName, AppConfig.ServerConnection, AppConfig.EventStaticSyncTables, null, AppConfig.EventDbServerSyncClass, SyncEngine.WFC_SERVICE_SQLSERVER);
                FotoShoutUtils.Log.LogManager.Info(_logger, "Server Download Sync - " + _sqlEventServerSync.Ping());
            }

            //sqlServerSync = new FilteringSqlServerSync();
            //sqlServerSync.Configure(AppConfig.SyncAction, scopeName, AppConfig.ServerConnection, AppConfig.EventStaticSyncTables, null);
                
            if (_sqlEventClientSync == null) {
                scopeDescription = _sqlEventServerSync.GetScopeDescription(scopeName);
                //scopeDescription = sqlServerSync.GetScopeDescription(scopeName);

                _sqlEventClientSync = InitiateSyncScope(scopeName, scopeDescription, 
                    (SyncDirectionOrder)Enum.Parse(typeof(SyncDirectionOrder), AppConfig.EventSyncDirection));
                if (_sqlEventClientSync != null)
                    _sqlEventClientSync.Synchronized += new FotoShoutUtils.Sync.Db.SynchronizedEventHandler(OnDbDownloadSynchronized);
            }

            return (_sqlEventClientSync != null);

        }

        //FilteringSqlServerSync sqlServerSync = null;
        
        private bool InitializePhotoSyncProviders(EventTDO ev, string scopeName, string[] staticSyncTables, string[] dynamicSyncTables) {
            if (string.IsNullOrEmpty(AppConfig.PhotoDbServerSyncClass)) {
                FotoShoutUtils.Log.LogManager.Error(_logger, "The type of the database server upload sync need to be defined in app config.");
                return false;
            }

            //sqlServerSync = new FilteringSqlServerSync();
            //sqlServerSync.Configure(AppConfig.SyncAction, scopeName, AppConfig.ServerConnection, staticSyncTables, dynamicSyncTables);

            _sqlPhotoServerSync = new SqlServerSyncProviderProxy(AppConfig.SyncAction, scopeName, AppConfig.ServerConnection, staticSyncTables, dynamicSyncTables, AppConfig.PhotoDbServerSyncClass, SyncEngine.WFC_SERVICE_SQLSERVER);
            FotoShoutUtils.Log.LogManager.Info(_logger, "Server Upload Sync - " + _sqlPhotoServerSync.Ping());

            DbSyncScopeDescription scopeDescription = _sqlPhotoServerSync.GetScopeDescription(scopeName);
            //DbSyncScopeDescription scopeDescription = sqlServerSync.GetScopeDescription(scopeName);
            _sqlPhotoClientSync = InitiateSyncScope(scopeName, scopeDescription, (SyncDirectionOrder)Enum.Parse(typeof(SyncDirectionOrder), AppConfig.PhotoSyncDirection));
            if (_sqlPhotoClientSync != null) {
                _sqlPhotoClientSync.Synchronized += new FotoShoutUtils.Sync.Db.SynchronizedEventHandler(OnDbUploadSynchronized);
            }

            return (_sqlPhotoClientSync != null);
        }

        private DbClientSync InitiateSyncScope(string scopeName, DbSyncScopeDescription scopeDescription, SyncDirectionOrder syncDirection) {
            Type type = Type.GetType(AppConfig.DbClientSyncClass);
            if (type == null) {
                FotoShoutUtils.Log.LogManager.Error(_logger, "The type of the database client sync is not supported.");
                return null;
            }
            
            DbClientSync clientSync = (DbClientSync)Activator.CreateInstance(type);
            // clientSync.ServerConnection = AppConfig.ServerConnection;
            clientSync.ServerScopeDescription = scopeDescription;
            clientSync.Configure(AppConfig.SyncAction, scopeName, AppConfig.ClientConnection);

            if (string.IsNullOrEmpty(AppConfig.SyncAction) || AppConfig.SyncAction.Equals(FotoShoutUtils.Constants.ASV_SYNCACTION_PROVISION, StringComparison.InvariantCultureIgnoreCase)) {
                // Asign sync direction
                clientSync.SyncDirection = syncDirection;

                // Register event handlers for db sync
                FotoShoutUtils.Log.LogManager.Info(_logger, "Registering event handlers for database sync parties...");
                clientSync.ApplyChangeFailed += new ApplyChangeFailedEventHandler(OnDbApplyChangeFailed);
                clientSync.ItemConflicting += new FotoShoutUtils.Sync.Db.ItemConflictingEventHandler(OnDbItemConflicting);
                clientSync.ItemConstraint += new FotoShoutUtils.Sync.Db.ItemConstraintEventHandler(OnDbItemConstraint);
                FotoShoutUtils.Log.LogManager.Info(_logger, "Successfully registered event handlers for database sync parties...");

                return clientSync;
            }

            return null;
        }

        #endregion // Sync Initialization

        private void OnUploadSynchronized(object sender, SynchronizedEventArgs ev) {
            SyncOperationStatistics syncStats = ev.SyncStats;
            FotoShoutUtils.Log.LogManager.Info(_logger, string.Format("Start time: {0}", syncStats.SyncStartTime));
            FotoShoutUtils.Log.LogManager.Info(_logger, string.Format("Total upload changes: {0}", syncStats.UploadChangesTotal));
            FotoShoutUtils.Log.LogManager.Info(_logger, string.Format("Total upload changes applied: {0}", syncStats.UploadChangesApplied));
            FotoShoutUtils.Log.LogManager.Info(_logger, string.Format("Total upload changes failed: {0}", syncStats.UploadChangesFailed));
            FotoShoutUtils.Log.LogManager.Info(_logger, string.Format("Complete time: {0}", syncStats.SyncEndTime));
        }

        private void OnDownloadSynchronized(object sender, DbSynchronizedEventArgs ev) {
            SyncOperationStatistics syncStats = ev.SyncStats;
            FotoShoutUtils.Log.LogManager.Info(_logger, string.Format("Start time: {0}", syncStats.SyncStartTime));
            FotoShoutUtils.Log.LogManager.Info(_logger, string.Format("Total download changes: {0}", syncStats.DownloadChangesTotal));
            FotoShoutUtils.Log.LogManager.Info(_logger, string.Format("Total download changes applied: {0}", syncStats.DownloadChangesApplied));
            FotoShoutUtils.Log.LogManager.Info(_logger, string.Format("Total download changes failed: {0}", syncStats.DownloadChangesFailed));
            FotoShoutUtils.Log.LogManager.Info(_logger, string.Format("Complete time: {0}", syncStats.SyncEndTime));
        }

        private string GenerateScopeName(string prefix, string id) {
            return string.Format("{0}_{1}", prefix, id);
        }
        
        private string GenerateServerScopeName(string prefix) {
            string syncAction = AppConfig.SyncAction;
            if (string.IsNullOrEmpty(syncAction) || !syncAction.Equals(FotoShoutUtils.Constants.ASV_SYNCACTION_DEPROVISIONSTORE, StringComparison.InvariantCultureIgnoreCase)) {
                Account account = _fsServerService.GetAccount();
                string authId = _fsServerService.GetAuthId();
                return string.Format("{0}_{1}_{2}_{3}", prefix, User.Id, authId, (account != null) ? account.Id.ToString() : "");
            }

            return FotoShoutUtils.Constants.ASV_SYNCACTION_DEPROVISIONSTORE;
        }
    }
}
