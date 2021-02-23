using System.Threading;

namespace RTNS.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;

    using NUnit.Framework;

    using Newtonsoft.Json;

    using RTNS.Core.Model;
    using RTNS.IntegrationTests.Clients;

    public class SmokeTests
    {
        // DEV
        private readonly Uri websocketUri = new Uri("wss://dev-ws.apfie.com/rtns");
        private readonly Uri httpUri = new Uri("https://dev-api.apfie.com/rtns/");

        // QA
        //private readonly Uri websocketUri = new Uri("wss://qa-ws.apfie.com/qa-RTNS-websocket");
        //private readonly Uri httpUri = new Uri("https://qa-api.apfie.com/rtns/");

        // PROD
        //private readonly Uri websocketUri = new Uri("wss://ws.apfie.com/prod-RTNS-websocket");
        //private readonly Uri httpUri = new Uri("https://api.apfie.com/rtns/");

        [TestCase(1)]
        public async Task ClientSubscribesToSingleTopic_ThenIsNotifiedSuccessfully(int topicsCount)
        {
            await SusbcribeAndWaitForNotification(topicsCount);
        }
        
        [TestCase(50)]
        [TestCase(100)]
        [TestCase(500)]
        [TestCase(1000)]
        public async Task ClientSubscribesToTopics_ThenIsNotifiedSuccessfully(int topicsCount)
        {
            await SusbcribeAndWaitForNotification(topicsCount);
        }

        [Test]
        public async Task ClientSubscribesToSelectedTopics_IsNotified_Unsubscribes_IsNotifiedOnlyOfRemaining()
        {
            var messageContent = "testPayload";
            var topic1 = "Topic1";
            var topic2 = "Topic2";
            var notificationRequest = new NotificationRequest(new[] { topic1, topic2 }, messageContent);

            var wsClient = new RtnsWebsocketClient(websocketUri);
            await wsClient.Unsubscribe(new Topic[] { new Topic("warmup") });
            await wsClient.SubscribeAndStartListening(notificationRequest.Topics);

            var httpClient = new RtnsHttpClient(httpUri);
            await httpClient.Warmup();

            await NotifyAndWaitForAnswer(notificationRequest, notificationRequest.Topics, httpClient, wsClient);

            var unsubscribeFrom = new Topic[] { new Topic("Topic2") };
            await wsClient.Unsubscribe(unsubscribeFrom);

            Thread.Sleep(3000);

            var expectedTopics = new Topic[] { new Topic("Topic1") };
            await NotifyAndWaitForAnswer(notificationRequest, expectedTopics, httpClient, wsClient);

            await wsClient.Unsubscribe(new Topic[] { });
            await NotifyAndWaitForAnswer(notificationRequest, new Topic[] { }, httpClient, wsClient);
        }

        private static async Task NotifyAndWaitForAnswer(
            NotificationRequest notificationRequest,
            Topic[] expected,
            RtnsHttpClient httpClient,
            RtnsWebsocketClient wsClient)
        {
            
            var notifyResult = await httpClient.NotifyAbout(notificationRequest);

            var expectedNotificationMessage = JsonConvert.SerializeObject(new
            {
                topics = expected.Select(t => t.Name).ToArray(),
                message = notificationRequest.Message
            });
            string notification = await wsClient.WaitForNotification(TimeSpan.FromSeconds(5));
            if (expected.Any())
            {
                Assert.AreEqual(expectedNotificationMessage, notification);
            }
        }

        private async Task SusbcribeAndWaitForNotification(int topicsCount)
        {
            var messageContent = "testPayload";
            var manyTopics = new List<Topic>();

            for (int i = 1; i <= topicsCount; i++)
            {
                manyTopics.Add(new Topic($"Test_{i}"));
            }
            var notificationRequest = new NotificationRequest(manyTopics, messageContent);

            var wsClient = new RtnsWebsocketClient(websocketUri);
            await wsClient.SubscribeAndStartListening(manyTopics.ToArray());

            var httpClient = new RtnsHttpClient(httpUri);
            await httpClient.Warmup();

            await httpClient.NotifyAbout(notificationRequest);

            var expectedNotificationMessage = JsonConvert.SerializeObject(new
            {
                topics = manyTopics.Select(t => t.Name).ToArray(),
                message = messageContent
            });
            string notification = await wsClient.WaitForNotification(TimeSpan.FromSeconds(30));
            Assert.AreEqual(expectedNotificationMessage, notification);
        }
    }
}