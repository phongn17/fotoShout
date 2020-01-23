using FotoShoutData.Models;
using FotoShoutData.Models.Authenticate;
using FotoShoutData.Models.Publish;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FotoShoutUtils.Service {
    public class PublishApiWebService: AuthenticationWebService {
        static ILog _logger = LogManager.GetLogger(typeof(PublishApiWebService));

        public PublishApiWebService(string baseAddress, string prefix, string mediaType) 
            : base(baseAddress, prefix, mediaType, _logger) {
            AuthorizationHeader = Constants.PUBLISH_AUTHORIZATION;
        }

        public IEnumerable<ChannelGroupTDO> GetChannelGroups() {
            if (string.IsNullOrEmpty(Authorization))
                return null;

            return GetList<ChannelGroup>("Template", Constants.INFO_CHANNELGROUP_LIST).Select(t => new ChannelGroupTDO {
                ChannelGroupId = t.ID,
                ChannelGroupName = t.Name
            }).ToList();
        }

        public IDictionary<string, string> GetPermaLinks(int broadcastId) {
            IDictionary<string, string> permaLinks = new Dictionary<string, string>();
            BroadcastDetail bcDetail = this.Get<BroadcastDetail>("Broadcast/" + broadcastId, true);
            if (bcDetail != null && bcDetail.Channels.Any()) {
                bool publishPending = false;
                foreach (Channel channel in bcDetail.Channels) {
                    if (!string.IsNullOrEmpty(channel.PermaLink)) {
                        if (permaLinks.ContainsKey(channel.ThumbnailURL))
                            permaLinks.Add(new KeyValuePair<string, string>(channel.ThumbnailURL + "?1", channel.PermaLink));
                        else
                            permaLinks.Add(new KeyValuePair<string, string>(channel.ThumbnailURL, channel.PermaLink));
                    }
                    else {
                        publishPending = true;
                    }
                }
                if (publishPending)
                    permaLinks.Add("pending", "true");
            }

            return permaLinks;
        }

        protected override Object GenerateCredentials(string apiKey, LoginModel model) {
            return new FotoShoutData.Models.Publish.Credentials { APIKey = apiKey, UserEmail = model.UserName, Password = model.Password };
        }
    }
}
