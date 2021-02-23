namespace RTNS.Core.Model
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class NotificationRequest
    {
        [JsonConstructor]
        public NotificationRequest(string[] topics, string message)
        {
            Topics = topics.Select(name => new Topic(name)).ToArray();
            Message = message;
        }

        public NotificationRequest(IEnumerable<Topic> topics, string message)
        {
            Topics = topics.ToArray();
            Message = message;
        }

        public Topic[] Topics { get; }

        public string Message { get; }
    }
}
