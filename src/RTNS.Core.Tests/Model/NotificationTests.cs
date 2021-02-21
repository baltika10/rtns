namespace RTNS.Core.Tests.Model
{
    using NUnit.Framework;
    using RTNS.Core.Model;

    public class NotificationTests
    {
        [Test]
        public void Constructor_SubscriberIsNull_ThrowsException()
        {
            Assert.That(() =>
                new Notification(null, new[] { new Topic("1") }), Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_TopicIsNull_ThrowsException()
        {
            Assert.That(() =>
                new Notification(new Subscriber("id"), null), Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_TopicIsEmpty_ThrowsException()
        {
            Assert.That(() =>
                new Notification(new Subscriber("id"), new Topic[0]), Throws.ArgumentException);
        }
    }
}
