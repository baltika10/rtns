namespace RTNS.Core.Tests.Storage
{
    using System.Collections.Generic;
    using System.Linq;

    using FakeItEasy;

    using RTNS.Core.Model;

    public static class SubscriptionsRepositoryForTesting
    {
        public static RTNS.Core.Storage.SubscriptionsRepository GetWorkingInMemoryFake()
        {
            var subscriptionsRepository = A.Fake<Core.Storage.SubscriptionsRepository>();

            var subscriptions = new List<Subscription>();
            A.CallTo(() => subscriptionsRepository.Store(A<Subscription>._))
                .Invokes((Subscription subscription) => subscriptions.Add(subscription));
            A.CallTo(() => subscriptionsRepository.RemoveBy(A<Subscriber>._))
                .Invokes((Subscriber subscriber) =>
                {
                    int indexOfSubscription =
                        subscriptions.FindIndex(0, s => s.Subscriber.Id == subscriber.Id);
                    subscriptions.RemoveAt(indexOfSubscription);
                });

            A.CallTo(() => subscriptionsRepository.GetSubscribersBy(A<Topic>._))
                .ReturnsLazily((Topic topic) =>
                {
                    return subscriptions.Where(s => s.Topics.Any(t => t.Name == topic.Name))
                        .Select(s => s.Subscriber)
                        .ToArray();
                });

            return subscriptionsRepository;
        }
    }
}
