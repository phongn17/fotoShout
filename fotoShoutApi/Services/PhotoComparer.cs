using FotoShoutApi.Models;
using FotoShoutData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FotoShoutApi.Services {
    public class PhotoComparer<P> : IEqualityComparer<P> {

        /// <summary>
        /// Photos are equal if their id are equal.
        /// </summary>
        /// <param name="p1">First photo to be compared</param>
        /// <param name="p2">Second photo to be compared</param>
        /// <returns></returns>
        public bool Equals(P p1, P p2) {
            if (Object.ReferenceEquals(p1, p2))
                return true;

            if (Object.ReferenceEquals(p1, null) || Object.ReferenceEquals(p2, null))
                return false;

            if (p1 is Photo && p2 is Photo) {
                Photo photo1 = p1 as Photo;
                Photo photo2 = p2 as Photo;

                return (photo1.PhotoId == photo2.PhotoId);
            }

            if (p1 is PhotoTDO && p2 is PhotoTDO) {
                PhotoTDO photo1 = p1 as PhotoTDO;
                PhotoTDO photo2 = p2 as PhotoTDO;

                return (photo1.PhotoId == photo2.PhotoId);
            }

            return false;
        }

        public int GetHashCode(P p) {
            if (Object.ReferenceEquals(p, null))
                return 0;

            return (p is Photo) ? (p as Photo).PhotoId.GetHashCode() : ((p is PhotoTDO) ? (p as PhotoTDO).PhotoId.GetHashCode() : 0);
        }
    }
}