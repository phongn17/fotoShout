using FotoShoutApi.Models;
using FotoShoutData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FotoShoutApi.Services {
    internal class EventBroadcastsService {
        internal static IEnumerable<EventBroadcast> GetBroadcasts(Event ev, FotoShoutDbContext db) {
            return db.EventBroadcasts.Where(eb => eb.EventId == ev.EventId && (eb.Status == (byte)EventBroadcastStatus.Pending || eb.Status == (byte)EventBroadcastStatus.PublishPending));
        }
    }
}