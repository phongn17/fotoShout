using FotoShoutApi.Models;
using FotoShoutApi.Utils;
using FotoShoutData.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace FotoShoutApi.Services {
    internal class PhotoAnnotationService {
        internal static Guest GenerateGuest(GuestTDO tdo, EventOption evo) {
            Guest guest = new Guest();
            guest.GuestId = Guid.NewGuid();
            UpdateGuest(guest, tdo, evo);
            return guest;
        }

        internal static void UpdateGuest(Guest guest, GuestTDO tdo, EventOption evo) {
            if (evo.SalutationOption)
                guest.Salutation = tdo.Salutation;

            if (evo.FirstNameOption)
                guest.FirstName = tdo.FirstName;
            
            if (evo.LastNameOption)
                guest.LastName = tdo.LastName;

            if (evo.EmailOption)
                guest.Email = tdo.Email;

            if (evo.PhoneOption) {
                guest.PrimaryPhone = tdo.PrimaryPhone;
                guest.OtherPhone = tdo.OtherPhone;
            }

            if (evo.FaxOption)
                guest.Fax = tdo.Fax;

            if (evo.AddressOption) {
                guest.Address1 = tdo.Address1;
                guest.Address2 = tdo.Address2;
                guest.City = tdo.City;
                guest.Region = tdo.Region;
                guest.PostalCode = tdo.PostalCode;
                guest.Country = tdo.Country;
            }

            if (evo.SignatureOption && tdo.Signature != null) {
                guest.Signature = tdo.Signature;
            }
        }

        internal static GuestTDO GenerateTDO(Guest guest, EventOption evo, bool signature = true) {
            GuestTDO tdo = new GuestTDO();
            tdo.GuestId = guest.GuestId;
            if (evo.SalutationOption)
                tdo.Salutation = guest.Salutation;
            
            if (evo.FirstNameOption)
                tdo.FirstName = guest.FirstName;
            
            if (evo.LastNameOption)
                tdo.LastName = guest.LastName;
            
            if (evo.EmailOption)
                tdo.Email = guest.Email;
            
            if (evo.PhoneOption) {
                tdo.PrimaryPhone = guest.PrimaryPhone;
                tdo.OtherPhone = guest.OtherPhone;
            }
            
            if (evo.FaxOption)
                tdo.Fax = guest.Fax;

            if (evo.AddressOption) {
                tdo.Address1 = guest.Address1;
                tdo.Address2 = guest.Address2;
                tdo.City = guest.City;
                tdo.Region = guest.Region;
                tdo.PostalCode = guest.PostalCode;
                tdo.Country = guest.Country;
            }

            if (signature && evo.SignatureOption) {
                tdo.Signature = guest.Signature;
            }

            return tdo;
        }

        internal static dynamic GetGuestInfo(Guest guest, EventOption evo, bool signature = true) {
            dynamic tdo = new ExpandoObject();
            tdo.GuestId = guest.GuestId;
            if (evo.SalutationOption)
                tdo.Salutation = guest.Salutation;

            if (evo.FirstNameOption)
                tdo.FirstName = guest.FirstName;

            if (evo.LastNameOption)
                tdo.LastName = guest.LastName;

            if (evo.EmailOption)
                tdo.Email = guest.Email;

            if (evo.PhoneOption) {
                tdo.PrimaryPhone = guest.PrimaryPhone;
                tdo.OtherPhone = guest.OtherPhone;
            }

            if (evo.FaxOption)
                tdo.Fax = guest.Fax;

            if (evo.AddressOption) {
                tdo.Address1 = guest.Address1;
                tdo.Address2 = guest.Address2;
                tdo.City = guest.City;
                tdo.Region = guest.Region;
                tdo.PostalCode = guest.PostalCode;
                tdo.Country = guest.Country;
            }

            if (signature && evo.SignatureOption) {
                tdo.Signature = guest.Signature;
            }

            return tdo;
        }

        internal static IEnumerable<GuestTDO> GetGuestTDOs(int id, FotoShoutDbContext db) {
            Event ev = db.Events.Find(id);
            if (ev == null)
                return null;
            
            IEnumerable<Guest> gs = ev.Guests.OrderBy(g => g.LastName);
            HashSet<GuestTDO> tdos = new HashSet<GuestTDO>();
            if (gs.Any()) {
                foreach (Guest g in gs) {
                    GuestTDO tdo = GenerateTDO(g, ev.EventOption);
                    tdos.Add(tdo);
                }
            }

            return tdos;
        }

        // id to Guid
        internal static GuestTDO GetGuestTDOById(int id, Guid guestId, FotoShoutDbContext db) {
            Event ev = db.Events.Find(id);
            if (ev == null)
                return null;
            
            Guest guest = ev.Guests.Where(g => g.GuestId == guestId).SingleOrDefault();
            if (guest == null)
                return null;
            
            return GenerateTDO(guest, ev.EventOption);
        }
        
        internal static GuestTDO GetGuestTDOByEmail(int id, string email, FotoShoutDbContext db) {
            Event ev = db.Events.Find(id);
            if (ev == null)
                return null;

            Guest guest = ev.Guests.Where(g => g.Email != null && g.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
            if (guest == null)
                return null;

            return GenerateTDO(guest, ev.EventOption);
        }


        // id to Guid
        internal static byte[] GetSignatureById(int id, Guid guestId, FotoShoutDbContext db) {
            Event ev = db.Events.Find(id);
            if (ev == null)
                throw new PhotoAnnotationException(string.Format("Event {0} not found.", id));

            Guest guest = ev.Guests.Where(g => g.GuestId == guestId).SingleOrDefault();
            if (guest == null)
                throw new PhotoAnnotationException(string.Format("Guest {0} not found in the {1} event.", guestId, id));

            return guest.Signature;
        }

        internal static PhotoAnnotation GetPhotoAnnotation(Photo photo) {
            HashSet<GuestTDO> tdos = GetGuestTDOs(photo, true);

            return new PhotoAnnotation { Rating = photo.Rating, Guests = tdos };
        }

        internal static HashSet<GuestTDO> GetGuestTDOs(Photo photo, bool signature) {
            EventOption evo = (photo.Event != null) ? photo.Event.EventOption : null;
            if (evo == null)
                throw new PhotoAnnotationException(string.Format("The {0} event is not associated with any event option.", photo.Event.EventName));

            IEnumerable<GuestPhoto> gps = (from gp in photo.GuestPhotos select gp);
            HashSet<GuestTDO> tdos = new HashSet<GuestTDO>();
            if (gps.Any()) {
                foreach (GuestPhoto gp in gps) {
                    GuestTDO tdo = PhotoAnnotationService.GenerateTDO(gp.Guest, evo, signature);
                    tdo.AuthorizePublish = gp.AuthorizePublish;
                    tdos.Add(tdo);
                }
            }
            return tdos;
        }

        internal static IEnumerable<dynamic> GetGuestsInfo(Photo photo, bool signature) {
            EventOption evo = (photo.Event != null) ? photo.Event.EventOption : null;
            if (evo == null)
                throw new PhotoAnnotationException(string.Format("The {0} event is not associated with any event option.", photo.Event.EventName));

            IEnumerable<GuestPhoto> gps = (from gp in photo.GuestPhotos select gp);
            HashSet<dynamic> tdos = new HashSet<dynamic>();
            if (gps.Any()) {
                foreach (GuestPhoto gp in gps) {
                    dynamic tdo = PhotoAnnotationService.GetGuestInfo(gp.Guest, evo, signature);
                    tdo.AuthorizePublish = gp.AuthorizePublish;
                    tdos.Add(tdo);
                }
            }
            return tdos;
        }

        internal static bool AnnotatePhoto(Photo photo, PhotoAnnotation photoAnnotation, FotoShoutDbContext db) {
            // Updating photo rating
            photo.Rating = photoAnnotation.Rating;
            
            // Adding the list of guests to the photo
            Event ev = photo.Event;
            ICollection<GuestPhoto> gps = photo.GuestPhotos;
            ICollection<GuestTDO> tdos = photoAnnotation.Guests;
            foreach (GuestTDO tdo in tdos) {
                Guest guest = !string.IsNullOrEmpty(tdo.Email) ? GetGuestByEmail(ev.EventId, tdo.Email, db) : GetGuestById(ev.EventId, tdo.GuestId, db);
                if (guest == null) {
                    guest = PhotoAnnotationService.GenerateGuest(tdo, photo.Event.EventOption);
                    ev.Guests.Add(guest);
                }
                else {
                    PhotoAnnotationService.UpdateGuest(guest, tdo, photo.Event.EventOption);
                }
                GuestPhoto guestPhoto = gps.Where(gp => gp.Guest.GuestId == guest.GuestId).SingleOrDefault();
                if (guestPhoto == null) {
                    guestPhoto = new GuestPhoto { Id = Guid.NewGuid(), Event_EventId = photo.Event.EventId, Photo = photo, Guest = guest, AuthorizePublish = tdo.AuthorizePublish };
                    gps.Add(guestPhoto);
                }
                else
                    guestPhoto.AuthorizePublish = tdo.AuthorizePublish;
            }
            photo.Error = "";
            photo.Status = (byte)PhotoStatus.Submitted;

            return true;
        }

        internal static bool ReannotatePhoto(Photo photo, PhotoAnnotation photoAnnotation, FotoShoutDbContext db) {
            Event ev = photo.Event;

            ICollection<GuestTDO> tdos = photoAnnotation.Guests;
            if (tdos == null || tdos.Count == 0) {
                if (photo.GuestPhotos.Any()) {
                    while (photo.GuestPhotos.Any())
                        db.GuestPhotos.Remove(photo.GuestPhotos.FirstOrDefault());
                    ClearAllGuests(photo);
                }
                photo.Rating = photoAnnotation.Rating;
                photo.Status = (byte)PhotoStatus.Unselected;
                photo.Submitted = null;
                photo.SubmittedBy = 0;
                photo.Error = "";
            }
            else {
                ICollection<GuestPhoto> gps = photo.GuestPhotos;
                HashSet<GuestPhoto> removedGuests = new HashSet<GuestPhoto>();
                foreach (GuestPhoto gp in gps) {
                    GuestTDO tdo = null;
                    if (string.IsNullOrEmpty(gp.Guest.Email))
                        tdo = tdos.Where(t => t.GuestId == gp.Guest.GuestId).SingleOrDefault();
                    else
                        tdo = tdos.Where(t => t.GuestId == gp.Guest.GuestId ||
                                                (!string.IsNullOrEmpty(t.Email) && t.Email.Equals(gp.Guest.Email, StringComparison.InvariantCultureIgnoreCase))).SingleOrDefault();
                    if (tdo == null)
                        removedGuests.Add(gp);
                }

                foreach (GuestPhoto removedGuest in removedGuests) {
                    gps.Remove(removedGuest);
                    db.GuestPhotos.Remove(removedGuest);
                }

                if (!AnnotatePhoto(photo, photoAnnotation, db))
                    return false;

            }

            return true;
        }
        
        internal static void MovePhoto(string folder, string subfolder, string filename) {
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
                folder += Path.DirectorySeparatorChar;

            DirectoryInfo dirInfo = FotoShoutUtils.Utils.IO.DirectoryUtils.CreateSubFolder(folder, subfolder);
            if (dirInfo != null) {
                File.Move(folder + filename, dirInfo.FullName + Path.DirectorySeparatorChar + filename);
            }
        }

        internal static void RestorePhoto(string folder, string subfolder, string filename) {
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
                folder += Path.DirectorySeparatorChar;

            File.Move(folder + subfolder + Path.DirectorySeparatorChar + filename, folder + filename);
        }

        internal static void CheckConstrains(PhotoAnnotation photoAnnotation, bool reAnnotate) {
            if (photoAnnotation == null)
                throw new ArgumentNullException("Data is empty.");
            
            if (!reAnnotate && (photoAnnotation.Guests == null || photoAnnotation.Guests.Count == 0))
                throw new PhotoAnnotationException("A photo must have at least one guest prior to submit.");
        }

        private static Guest GetGuestByEmail(int evId, string email, FotoShoutDbContext db) {
            if (string.IsNullOrEmpty(email))
                return null;

            var ev = db.Events.Find(evId);
            if (ev != null)
                return ev.Guests.Where(g => g.Email != null && email.Equals(g.Email, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();

            return null;
        }

        // id to Guid
        private static Guest GetGuestById(int evId, Guid id, FotoShoutDbContext db) {
            var ev = db.Events.Find(evId);
            if (ev != null)
                return ev.Guests.Where(g => g.GuestId == id).SingleOrDefault();

            return null;
        }

        private static void ClearAllGuests(Photo photo) {
            photo.GuestPhotos.Clear();
        }

        internal static string getSignatureContentType(byte[] signature) {
            return "image/png";
        }
    }
}