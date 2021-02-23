namespace RTNS.Core.Notifications
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using RTNS.Core.Model;
    using RTNS.Core.Storage;

    public class RepositoryBasedNotificationBuilder : NotificationBuilder
    {
        private readonly SubscriptionsRepository subscriptionsRepository;

        private ConcurrentDictionary<Subscriber, List<Topic>> subscribersTopics;

        public RepositoryBasedNotificationBuilder(SubscriptionsRepository subscriptionsRepository)
        {
            if (subscriptionsRepository == null)
                throw new ArgumentNullException(nameof(subscriptionsRepository));

            this.subscriptionsRepository = subscriptionsRepository;
        }

        public async Task<Notification[]> BuildNotificationsFor(Topic[] topics, string message)
        {
            if (topics == null)
                throw new ArgumentNullException(nameof(topics));

            subscribersTopics = new ConcurrentDictionary<Subscriber, List<Topic>>();
            foreach (var topic in topics)
            {
                await LoadSubscribersOf(topic);
            }

            return subscribersTopics.Select(st => new Notification(st.Key, st.Value.ToArray(), message)).ToArray();
        }

            private async Task LoadSubscribersOf(Topic topic)
            {
                var subscribers = await subscriptionsRepository.GetSubscribersBy(topic);
                foreach (var subscriber in subscribers)
                {
                    subscribersTopics.AddOrUpdate(
                        subscriber,
                        (s) => new List<Topic>() { topic },
                        (s, l) =>
                        {
                            l.Add(topic);
                            return l;
                        });
                }
            }
    }
}
