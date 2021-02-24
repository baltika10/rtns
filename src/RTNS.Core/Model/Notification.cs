namespace RTNS.Core.Model
{
    using System;
    using System.Linq;

    public class Notification
    {
        public Notification(Subscriber subscriber, Topic[] topics, string message)
        {
            if (subscriber == null)
                throw new ArgumentNullException(nameof(subscriber));

            if (topics == null)
                throw new ArgumentNullException(nameof(topics));

            if (!topics.Any())
                throw new ArgumentException($"{nameof(topics)} can't be empty!");

            Subscriber = subscriber;
            Topics = topics;
            Message = message;
        }

        public Subscriber Subscriber { get; }

        public Topic[] Topics { get; }

        public string Message { get; }
    }
}
