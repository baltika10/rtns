namespace RTNS.Core.Tests.Model
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;
    using RTNS.Core.Model;
    using System.Runtime.InteropServices;

    public class NotificationTests
    {
        [Test]
        public void Constructor_SubscriberIsNull_ThrowsException()
        {
            Assert.That(() =>
                new Notification(null, new[] { new Topic("1") }, "test"), Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_TopicIsNull_ThrowsException()
        {
            Assert.That(() =>
                new Notification(new Subscriber("id"), null, "test"), Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_TopicIsEmpty_ThrowsException()
        {
            Assert.That(() =>
                new Notification(new Subscriber("id"), new Topic[0], "test"), Throws.ArgumentException);
        }

        [Test]
        public void ParseNotificationTest()
        {
            var input = @"{ ""topics"": [""abcd""], ""message"": ""aaaabbbb""}";
            var payload = JObject.Parse(input);
            var notificationRequest = new NotificationRequest(payload["topics"].ToObject<string[]>(), payload["message"].ToString());
            var parsed = JsonConvert.DeserializeObject<NotificationRequest>(input);
        }
    }
}
