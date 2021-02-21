using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using RTNS.Core.Model;
using RTNS.Core.Notifications;
using RTNS.Core.Tests.Storage;

namespace RTNS.Core.Tests.Notifications
{
    public class RepositoryBasedNotificationBuilderTests
    {
        [Test]
        public async Task BuildNotificationsFor_Topics_ReturnsCorrectResult()
        {
            var repo = SubscriptionsRepositoryForTesting.GetWorkingInMemoryFake();

            var subscriberA = new Subscriber("A");
            var subscriberB = new Subscriber("B");
            var subscriberC = new Subscriber("C");

            var topic1 = new Topic("1");
            var topic2 = new Topic("2");
            var topic3 = new Topic("3");

            var subscriptionA = new Subscription(subscriberA, new []{ topic1, topic2 });
            var subscriptionB = new Subscription(subscriberB, new[] { topic2 });
            var subscriptionC = new Subscription(subscriberC, new[] { topic3 });

            await repo.Store(subscriptionA);
            await repo.Store(subscriptionB);
            await repo.Store(subscriptionC);

            var rbnb = new RepositoryBasedNotificationBuilder(repo);
            var notificationsForTopic1 = await rbnb.BuildNotificationsFor(new[] {topic1});
            var notification = notificationsForTopic1[0];

            Assert.AreEqual(subscriberA, notification.Subscriber);
            Assert.AreEqual(topic1, notification.Topics[0]);

            Assert.That(() => notificationsForTopic1, Has.Exactly(1).Items);
            Assert.That(async () => await rbnb.BuildNotificationsFor(new[] { topic2 }), Has.Exactly(2).Items);
            Assert.That(async () => await rbnb.BuildNotificationsFor(new[] { topic3 }), Has.Exactly(1).Items);


        }
    }
}
