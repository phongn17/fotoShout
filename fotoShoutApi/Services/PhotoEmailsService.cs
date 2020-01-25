using FotoShoutApi.Models;
using FotoShoutData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace FotoShoutApi.Services {
    public class PhotoEmailsService {
        internal static IEnumerable<PhotoEmail> GetPhotoEmails(Photo photo, FotoShoutDbContext db) {
            return (IEnumerable<PhotoEmail>)db.PhotoEmails.Where<PhotoEmail>(pe => pe.PhotoId == photo.PhotoId);
        }

        internal static IEnumerable<IGrouping<Guid, PhotoGuest>> GetPhotoEmailsGroupByPhoto(IEnumerable<Guid> photoIds, FotoShoutDbContext db) {
            return (IEnumerable<IGrouping<Guid, PhotoGuest>>)db.PhotoEmails.Where<PhotoEmail>(pe => photoIds.Contains<Guid>(pe.PhotoId)).Select<PhotoEmail, PhotoGuest>(pe => new PhotoGuest {
                PhotoId = pe.PhotoId,
                GuestId = pe.GuestId
            }).GroupBy<PhotoGuest, Guid>(pe => pe.PhotoId);
        }
    }
}