using FotoShoutApi.Models;
using FotoShoutApi.Utils;
using FotoShoutApi.Utils.IO;
using FotoShoutData.Models;
using FotoShoutUtils;
using FotoShoutUtils.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Web;

namespace FotoShoutApi.Services {
    internal class EventPhotosService {
        internal const string EVENTTYPE_ALL = "All";
        internal const string EVENTTYPE_OPEN = "Open";
        internal const string EVENTTYPE_COMPLETED = "Completed";
        internal const string EVENTTYPE_NEW = "New";

        internal static EventStatus GetEventStatus(string eventType) {
            if (string.IsNullOrEmpty(eventType) || eventType.Equals(EventPhotosService.EVENTTYPE_OPEN, StringComparison.InvariantCultureIgnoreCase))
                return EventStatus.Open;
            else if (eventType.Equals(EventPhotosService.EVENTTYPE_COMPLETED, StringComparison.InvariantCultureIgnoreCase))
                return EventStatus.Completed;
            else
                return EventStatus.Undefined;
        }

        internal static bool IsEventType(Event ev, string eventType) {
            if (string.IsNullOrEmpty(eventType))
                return (ev.EventStatus == (byte)EventStatus.Open);

            if (eventType.Equals(EventPhotosService.EVENTTYPE_ALL, StringComparison.InvariantCultureIgnoreCase))
                return true;

            if (eventType.Equals(EventPhotosService.EVENTTYPE_NEW, StringComparison.InvariantCultureIgnoreCase)) {
                return (ev.EventStatus == (byte)EventStatus.Open && GetNumProcessedPhotos(ev, null) == 0);
            }
            else if (eventType.Equals(EventPhotosService.EVENTTYPE_OPEN, StringComparison.InvariantCultureIgnoreCase)) {
                return (ev.EventStatus == (byte)EventStatus.Open);
            }
            else if (eventType.Equals(EventPhotosService.EVENTTYPE_COMPLETED, StringComparison.InvariantCultureIgnoreCase)) {
                return (ev.EventStatus == (byte)EventStatus.Completed);
            }

            return false;
        }

        internal static int GetNumPhotos(Event ev) {
            if (string.IsNullOrEmpty(ev.EventFolder))
                throw new ArgumentNullException("Folder name is empty.");

            // Get number of photos that belong to the event
            return PagingFileUtils.GetNumFiles(ev.EventFolder, GetImageExts());
        }

        internal static IEnumerable<PhotoTDO> GetPhotos(Event ev) {
            if (string.IsNullOrEmpty(ev.EventFolder))
                throw new ArgumentNullException("Folder name is empty.");

            HashSet<PhotoTDO> photos = new HashSet<PhotoTDO>();
            // Get all photos that belong to the event
            string[] filenames = PagingFileUtils.GetFileNames(ev.EventFolder, GetImageExts());
            string folder = ev.EventFolder.EndsWith(Path.DirectorySeparatorChar.ToString()) ? ev.EventFolder : (ev.EventFolder + Path.DirectorySeparatorChar);
            DirectoryInfo dirInfo = FotoShoutUtils.Utils.IO.DirectoryUtils.CreateSubFolder(folder, Constants.STR_THUMB);
            foreach (string filename in filenames) {
                bool succeeded = (dirInfo != null) ? ImageUtils.CreateThumbnailFileIfAny(folder + filename, GetThumbnailPath(folder, filename)) : false;
                string thumbnail = succeeded ? GetThumbnailVirtualPath(ev.EventVirtualPath, filename) : "";
                PhotoTDO photo = GenerateTDO(ev, filename, thumbnail);
                photos.Add(photo);
            }

            return photos.OrderBy(p => p.Filename);
        }

        internal static int GetNumUnclaimedPhotos(Event ev) {
            if (string.IsNullOrEmpty(ev.EventFolder))
                throw new ArgumentNullException("Folder name is empty.");

            // Get number of unclaimed photos belong to the event
            string folder = (ev.EventFolder.EndsWith(Path.DirectorySeparatorChar.ToString()) ? ev.EventFolder : (ev.EventFolder + Path.DirectorySeparatorChar)) + Constants.STR_UNCLAIMED;
            try {
                return PagingFileUtils.GetNumFiles(folder, GetImageExts());
            }
            catch (DirectoryNotFoundException) {
                return 0;
            }
        }

        internal static IEnumerable<PhotoTDO> GetUnclaimedPhotos(Event ev, int page, int pageSize) {
            HashSet<PhotoTDO> photos = new HashSet<PhotoTDO>();

            if (string.IsNullOrEmpty(ev.EventFolder))
                return photos.AsEnumerable();

            // Getting the list of photos' extensions
            string imageExts = GetImageExts();
            // Get all photos that belong to the event's folder
            string folder = (ev.EventFolder.EndsWith(Path.DirectorySeparatorChar.ToString()) ? ev.EventFolder : (ev.EventFolder + Path.DirectorySeparatorChar)) + Constants.STR_UNCLAIMED;
            try {
                string[] filenames = PagingFileUtils.GetFileNames(folder, imageExts, page, pageSize);
                foreach (string filename in filenames) {
                    string thumbnail = GetThumbnailVirtualPath(ev.EventVirtualPath, filename);
                    PhotoTDO photo = GenerateUnclaimedTDO(ev, filename, thumbnail);
                    photos.Add(photo);
                }

                return photos.OrderBy(p => p.Created);
            }
            catch (ObjectNotFoundException) {
                return photos.AsEnumerable();
            }
        }

        internal static IEnumerable<PhotoTDO> GetSubmittedPhotos(Event ev, FotoShoutDbContext db) {
            return GetPhotos(ev, PhotoStatus.Submitted, 0, 0, db);
        }

        internal static IEnumerable<PhotoTDO> GetPublishedPhotos(Event ev, FotoShoutDbContext db) {
            return GetPhotos(ev, PhotoStatus.Published, 0, 0, db);
        }

        internal static int GetNumProcessedPhotos(Event ev, string created) {
            Func<Photo, Boolean> func;
            if (string.IsNullOrEmpty(created))
                func = p => (p.Status == (byte)PhotoStatus.Submitted || p.Status == (byte)PhotoStatus.PendingPublish || p.Status == (byte)PhotoStatus.Published);
            else {
                created = DateTime.Parse(created).ToShortDateString();
                func = p => (p.Created.ToShortDateString().Equals(created, StringComparison.InvariantCultureIgnoreCase) && (p.Status == (byte)PhotoStatus.Submitted || p.Status == (byte)PhotoStatus.PendingPublish || p.Status == (byte)PhotoStatus.Published));
            }
            IEnumerable<Photo> photos = ev.Photos.Where(func);

            return photos.Count();
        }

        internal static IEnumerable<PhotoTDO> GetProcessedPhotos(Event ev, string created, int page, int pageSize, FotoShoutDbContext db) {
            Func<Photo, Boolean> func;
            if (string.IsNullOrEmpty(created))
                func = p => (p.Status == (byte)PhotoStatus.Submitted || p.Status == (byte)PhotoStatus.PendingPublish || p.Status == (byte)PhotoStatus.Published);
            else {
                created = DateTime.Parse(created).ToShortDateString();
                func = p => (p.Created.ToShortDateString().Equals(created, StringComparison.InvariantCultureIgnoreCase) && (p.Status == (byte)PhotoStatus.Submitted || p.Status == (byte)PhotoStatus.PendingPublish || p.Status == (byte)PhotoStatus.Published));
            }
            IEnumerable<Photo> photos = ev.Photos.Where(func).OrderBy(p => p.Created);
            if (page > 0 && pageSize > 0 && (photos.Count() > pageSize))
                photos = photos.Skip((page - 1) * pageSize).Take(pageSize);
            
            HashSet<PhotoTDO> tdos = new HashSet<PhotoTDO>();
            if (!photos.Any())
                return tdos;

            foreach (Photo photo in photos) {
                PhotoTDO tdo = EventPhotosService.GenerateTDO(ev, photo);
                tdo.Guests = PhotoAnnotationService.GetGuestTDOs(photo, true);
                tdos.Add(tdo);
            }

            return tdos;
        }

        internal static IEnumerable<PhotoDetailsTDO> GetProcessedPhotosDetailing(Event ev, string created, int page, int pageSize, FotoShoutDbContext db) {
            Func<Photo, Boolean> func;
            if (string.IsNullOrEmpty(created))
                func = p => (p.Status == (byte)PhotoStatus.Submitted || p.Status == (byte)PhotoStatus.PendingPublish || p.Status == (byte)PhotoStatus.Published);
            else {
                created = DateTime.Parse(created).ToShortDateString();
                func = p => (p.Created.ToShortDateString().Equals(created, StringComparison.InvariantCultureIgnoreCase) && (p.Status == (byte)PhotoStatus.Submitted || p.Status == (byte)PhotoStatus.PendingPublish || p.Status == (byte)PhotoStatus.Published));
            }
            IEnumerable<Photo> photos = ev.Photos.Where(func).OrderBy(p => p.Filename);
            if (page > 0 && pageSize > 0 && (photos.Count() > pageSize))
                photos = photos.Skip((page - 1) * pageSize).Take(pageSize);

            IEnumerable<PhotoDetailsTDO> photosDetails = (from p in photos
                                                         join e in db.EventBroadcasts on p.PhotoId equals e.PhotoId into pe
                                                         from subset in pe.DefaultIfEmpty()
                                                         select new PhotoDetailsTDO {
                                                             PhotoId = p.PhotoId,
                                                             Filename = p.Filename,
                                                             Status = p.Status,
                                                             Submitted = p.Submitted,
                                                             SubmittedBy = p.SubmittedBy,
                                                             Thumbnail = string.IsNullOrEmpty(p.Thumbnail) ? "" : AppConfigs.VirtualRoot + p.Thumbnail,
                                                             Error = p.Error,
                                                             Thumbnails =  (subset == null) ? string.Empty : subset.Thumbnails,
                                                             PermaLinks = (subset == null) ? string.Empty : subset.PermaLinks
                                                         }).ToList();
                
            if (!photosDetails.Any())
                return photosDetails;

            foreach (Photo photo in photos) {
                PhotoDetailsTDO tdo = photosDetails.Where(p => p.PhotoId == photo.PhotoId).Single();
                tdo.Guests = PhotoAnnotationService.GetGuestTDOs(photo, true);
            }

            return photosDetails;
        }

        internal static EventDetailsTDO GetProcessedPhotosReporting(Event ev, FotoShoutDbContext db) {
            EventDetailsTDO evDetails = new EventDetailsTDO();
            IEnumerable<Photo> photos = ev.Photos.Where(p => p.Status > (byte)PhotoStatus.Selected).OrderBy(p => p.Filename);

            if (!photos.Any())
                return evDetails;

            HashSet<dynamic> photosDetails = new HashSet<dynamic>();
            foreach (var photo in photos) {
                dynamic tdo = new ExpandoObject();
                tdo.PhotoId = photo.PhotoId;
                tdo.Filename = photo.Filename;
                tdo.Guests = PhotoAnnotationService.GetGuestsInfo(photo, false);
                photosDetails.Add(tdo);
            }

            if (photosDetails.Any()) {
                evDetails.EventOption = ev.EventOption;
                evDetails.PhotosDetails = photosDetails;
            }

            return evDetails;
        }

        internal static IEnumerable<PhotoGroupTDO> GetProcessedPhotoGroupsByDate(Event ev, FotoShoutDbContext db) {
            IEnumerable<IGrouping<string, Photo>> photoGroups = ev.Photos.Where(p => (p.Status == (byte)PhotoStatus.Submitted || p.Status == (byte)PhotoStatus.PendingPublish || p.Status == (byte)PhotoStatus.Published)).GroupBy(p => p.Created.ToShortDateString());

            HashSet<PhotoGroupTDO> tdos = new HashSet<PhotoGroupTDO>();
            if (!photoGroups.Any())
                return tdos;

            foreach (IGrouping<string, Photo> photoGroup in photoGroups) {
                if (photoGroup.Any()) {
                    PhotoGroupTDO tdo = new PhotoGroupTDO {
                        Created = DateTime.Parse(photoGroup.Key),
                        NumPhotos = photoGroup.Count(),
                        Thumbnail = AppConfigs.VirtualRoot + photoGroup.FirstOrDefault().Thumbnail
                    };
                    tdos.Add(tdo);
                }
            }

            return tdos.OrderByDescending(tdo => tdo.Created);
        }

        internal static IEnumerable<PhotoTDO> GetPublishAuthorizedPhotos(Event ev, FotoShoutDbContext db) {
            IEnumerable<Photo> photos = ev.Photos.Where(p => (p.Status == (byte)PhotoStatus.Submitted));
            HashSet<PhotoTDO> authorizedPhotos = new HashSet<PhotoTDO>();
            foreach (Photo photo in photos) {
                GuestPhoto gp = photo.GuestPhotos.Where(g => g.AuthorizePublish == false).FirstOrDefault();
                if (gp == null)
                    authorizedPhotos.Add(EventPhotosService.GenerateTDO(ev, photo));
            }
            
            return authorizedPhotos;
        }

        internal static IEnumerable<PhotoTDO> GetUnauthorizedPhotos(Event ev, FotoShoutDbContext db) {
            IEnumerable<Photo> photos = ev.Photos.Where(p => (p.Status == (byte)PhotoStatus.Submitted));
            HashSet<PhotoTDO> unauthorizedPhotos = new HashSet<PhotoTDO>();
            foreach (Photo photo in photos) {
                IEnumerable<GuestPhoto> gps = photo.GuestPhotos.Where(g => g.AuthorizePublish == false);
                if (gps != null && gps.Any())
                    unauthorizedPhotos.Add(EventPhotosService.GenerateTDO(ev, photo));
            }

            return unauthorizedPhotos;
        }

        internal static IEnumerable<PhotoTDO> GetSubmittedPhotosByGuest(Event ev, string name, FotoShoutDbContext db) {
            return GetPhotosByGuest(ev, name, PhotoStatus.Submitted, 0, 0, db);
        }

        internal static IEnumerable<PhotoTDO> GetPublishedPhotosByGuest(Event ev, string name, FotoShoutDbContext db) {
            return GetPhotosByGuest(ev, name, PhotoStatus.Published, 0, 0, db);
        }

        internal static int GetNumProcessedPhotosByGuest(Event ev, string name, string created) {
            Func<GuestPhoto, Boolean> func;
            if (string.IsNullOrEmpty(created))
                func = gp => (gp.Photo.Event.EventId == ev.EventId) && (gp.Photo.Status == (byte)PhotoStatus.Submitted || gp.Photo.Status == (byte)PhotoStatus.PendingPublish || gp.Photo.Status == (byte)PhotoStatus.Published);
            else {
                created = DateTime.Parse(created).ToShortDateString();
                func = gp => (gp.Photo.Event.EventId == ev.EventId) && gp.Photo.Created.ToShortDateString().Equals(created, StringComparison.InvariantCultureIgnoreCase) && (gp.Photo.Status == (byte)PhotoStatus.Submitted || gp.Photo.Status == (byte)PhotoStatus.PendingPublish || gp.Photo.Status == (byte)PhotoStatus.Published);
            }
            IEnumerable<Guest> guests = ev.Guests.Where(g => (g.FirstName + " " + g.LastName + " " + g.MiddleInitial).Contains(name, StringComparison.InvariantCultureIgnoreCase));
            HashSet<PhotoTDO> photos = new HashSet<PhotoTDO>();
            foreach (Guest guest in guests) {
                IEnumerable<PhotoTDO> ps = guest.GuestPhotos.Where(func).Select(gp => new PhotoTDO {
                    PhotoId = gp.Photo.PhotoId,
                    Filename = gp.Photo.Filename,
                    Created = gp.Photo.Created
                });
                if (ps.Any())
                    photos.UnionWith(ps);
            }

            return photos.Distinct(new PhotoComparer<PhotoTDO>()).Count();
        }

        internal static IEnumerable<PhotoTDO> GetProcessedPhotosByGuest(Event ev, string name, string created, int page, int pageSize, FotoShoutDbContext db) {
            Func<GuestPhoto, Boolean> func;
            if (string.IsNullOrEmpty(created))
                func = gp => (gp.Photo.Event.EventId == ev.EventId) && (gp.Photo.Status == (byte)PhotoStatus.Submitted || gp.Photo.Status == (byte)PhotoStatus.PendingPublish || gp.Photo.Status == (byte)PhotoStatus.Published);
            else {
                created = DateTime.Parse(created).ToShortDateString();
                func = gp => (gp.Photo.Event.EventId == ev.EventId) && gp.Photo.Created.ToShortDateString().Equals(created, StringComparison.InvariantCultureIgnoreCase) && (gp.Photo.Status == (byte)PhotoStatus.Submitted || gp.Photo.Status == (byte)PhotoStatus.PendingPublish || gp.Photo.Status == (byte)PhotoStatus.Published);
            }
            IEnumerable<Guest> guests = ev.Guests.Where(g => (g.FirstName + " " + g.LastName + " " + g.MiddleInitial).Contains(name, StringComparison.InvariantCultureIgnoreCase));
            HashSet<PhotoTDO> photos = new HashSet<PhotoTDO>();
            foreach (Guest guest in guests) {
                IEnumerable<PhotoTDO> ps = guest.GuestPhotos.Where(func).Select(gp => new PhotoTDO {
                    PhotoId = gp.Photo.PhotoId,
                    Filename = gp.Photo.Filename,
                    Folder = gp.Photo.Folder,
                    Status = gp.Photo.Status,
                    Submitted = gp.Photo.Submitted,
                    SubmittedBy = gp.Photo.SubmittedBy,
                    Thumbnail = AppConfigs.VirtualRoot + gp.Photo.Thumbnail,
                    Created = gp.Photo.Created,
                    Image = AppConfigs.VirtualRoot + ev.EventVirtualPath + "/" + Constants.STR_PROCESSED + "/" + gp.Photo.Filename,
                    Rating = gp.Photo.Rating
                });
                if (ps.Any())
                    photos.UnionWith(ps);
            }

            IEnumerable<PhotoTDO> ret = (page > 0 && pageSize > 0 && (photos.Count() > pageSize)) ?
                photos.Distinct(new PhotoComparer<PhotoTDO>()).OrderBy(p => p.Created).Skip((page - 1) * pageSize).Take(pageSize) :
                photos.Distinct(new PhotoComparer<PhotoTDO>()).OrderBy(p => p.Created);

            return ret;
        }

        internal static string GetImageExts() {
            string imageExts = System.Configuration.ConfigurationManager.AppSettings["ImageExts"];
            if (string.IsNullOrEmpty(imageExts))
                imageExts = ImageUtils.IMG_EXTS_DEFAULT;

            return imageExts;
        }

        internal static string GetThumbnailUrl(int eventId, string physicalPath, string virtualPath, FotoShoutDbContext db) {
            // Getting the list of photos' extensions
            string imageExts = EventPhotosService.GetImageExts();
            // Get the first photo that belongs to the event's folder
            string filename = PagingFileUtils.GetFirstFileName(physicalPath, imageExts);
            if (!string.IsNullOrEmpty(filename)) {
                string folder = physicalPath.EndsWith(Path.DirectorySeparatorChar.ToString()) ? physicalPath : (physicalPath + Path.DirectorySeparatorChar);
                DirectoryInfo dirInfo = FotoShoutUtils.Utils.IO.DirectoryUtils.CreateSubFolder(folder, Constants.STR_THUMB);
                bool succeeded = (dirInfo != null) ? ImageUtils.CreateThumbnailFileIfAny(folder + filename, GetThumbnailPath(folder, filename)) : false;
                if (succeeded)
                    return GetThumbnailVirtualPath(virtualPath, filename);
            }

            return "";
        }

        internal static PhotoTDO GenerateTDO(Event ev, Photo photo) {
            return new PhotoTDO {
                PhotoId = photo.PhotoId,
                Filename = photo.Filename,
                Folder = photo.Folder,
                Status = photo.Status,
                Submitted = photo.Submitted,
                SubmittedBy = photo.SubmittedBy,
                Thumbnail = string.IsNullOrEmpty(photo.Thumbnail) ? "" : AppConfigs.VirtualRoot + photo.Thumbnail,
                Created = photo.Created,
                Image = AppConfigs.VirtualRoot + ev.EventVirtualPath + "/" + ((photo.Status >= (byte)PhotoStatus.Submitted) ? (Constants.STR_PROCESSED + "/") : "") + photo.Filename,
                Rating = photo.Rating
            };
        }

        private static int GetNumPhotos(Event ev, PhotoStatus status) {
            return ev.Photos.Where(p => p.Status == (byte)status).Count();
        }

        private static IEnumerable<PhotoTDO> GetPhotos(Event ev, PhotoStatus status, int page, int pageSize, FotoShoutDbContext db) {
            IEnumerable<PhotoTDO> photos = ev.Photos.Where(p => p.Status == (byte)status).Select(p => new PhotoTDO {
                PhotoId = p.PhotoId,
                Filename = p.Filename,
                Folder = p.Folder,
                Status = p.Status,
                Submitted = p.Submitted,
                SubmittedBy = p.SubmittedBy,
                Thumbnail = AppConfigs.VirtualRoot + p.Thumbnail,
                Created = p.Created,
                Image = AppConfigs.VirtualRoot + ev.EventVirtualPath + "/" + Constants.STR_PROCESSED + "/" + p.Filename,
                Rating = p.Rating
            });

            if (page > 0 && pageSize > 0 && (photos.Count() > pageSize))
                return photos.Skip((page - 1) * pageSize).Take(pageSize);
            
            return photos;
        }

        private static IEnumerable<PhotoTDO> GetPhotosByGuest(Event ev, string name, PhotoStatus status, int page, int pageSize, FotoShoutDbContext db) {
            IEnumerable<Guest> guests = ev.Guests.Where(g => (g.FirstName + " " + g.LastName + " " + g.MiddleInitial).Contains(name, StringComparison.InvariantCultureIgnoreCase));
            HashSet<PhotoTDO> photos = new HashSet<PhotoTDO>();
            foreach (Guest guest in guests) {
                IEnumerable<PhotoTDO> ps = guest.GuestPhotos.Where(gp => gp.Photo.Event.EventId == ev.EventId && gp.Photo.Status == (byte)status).Select(gp => new PhotoTDO {
                    PhotoId = gp.Photo.PhotoId,
                    Filename = gp.Photo.Filename,
                    Folder = gp.Photo.Folder,
                    Status = gp.Photo.Status,
                    Submitted = gp.Photo.Submitted,
                    SubmittedBy = gp.Photo.SubmittedBy,
                    Thumbnail = AppConfigs.VirtualRoot + gp.Photo.Thumbnail,
                    Created = gp.Photo.Created,
                    Image = AppConfigs.VirtualRoot + ev.EventVirtualPath + "/" + Constants.STR_PROCESSED + "/" + gp.Photo.Filename,
                    Rating = gp.Photo.Rating
                });
                photos.UnionWith(ps);
            }

            return (page > 0 && pageSize > 0 && (photos.Count() > pageSize)) ?
                photos.Distinct(new PhotoComparer<PhotoTDO>()).Skip((page - 1) * pageSize).Take(pageSize) : photos.Distinct(new PhotoComparer<PhotoTDO>());
        }

        private static string GetThumbnailPath(string folder, string filename) {
            return (folder + Constants.STR_THUMB + Path.DirectorySeparatorChar + filename);
        }

        private static string GetThumbnailVirtualPath(string virtualPath, string filename) {
            return (virtualPath + "/" + Constants.STR_THUMB + "/" + filename);
        }

        private static PhotoTDO GenerateTDO(Event ev, string filename, string thumbnail) {
            return GenerateTDO(ev.EventFolder, ev.EventVirtualPath, filename, thumbnail);
        }

        private static PhotoTDO GenerateUnclaimedTDO(Event ev, string filename, string thumbnail) {
            string physicalFolder = (ev.EventFolder.EndsWith(Path.DirectorySeparatorChar.ToString()) ? ev.EventFolder : (ev.EventFolder + Path.DirectorySeparatorChar)) + Constants.STR_UNCLAIMED;
            string virtualFolder = ev.EventVirtualPath + "/" + Constants.STR_UNCLAIMED;
            return GenerateTDO(physicalFolder, virtualFolder, filename, thumbnail);
        }

        private static PhotoTDO GenerateTDO(string physicalFolder, string virtualFolder, string filename, string thumbnail) {
            string folder = physicalFolder.EndsWith(Path.DirectorySeparatorChar.ToString()) ? physicalFolder : (physicalFolder + Path.DirectorySeparatorChar);
            return new PhotoTDO {
                Folder = physicalFolder,
                Filename = filename,
                Image = AppConfigs.VirtualRoot + virtualFolder + "/" + filename,
                Thumbnail = string.IsNullOrEmpty(thumbnail) ? "" : AppConfigs.VirtualRoot + thumbnail,
                Created = File.GetCreationTime(folder + filename),
            };
        }
    }
}