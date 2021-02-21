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
        private readonly Uri websocketUri = new Uri("wss://dev-ws.plexop.com/dev-public-RTNS-websocket");
        private readonly Uri httpUri = new Uri("https://dev-api.plexop.com/dev-public-RTNS-notify");

        // QA
        //private readonly Uri websocketUri = new Uri("wss://qa-ws.plexop.com/qa-private-RTNS-websocket");
        //private readonly Uri httpUri = new Uri("https://qa-api.plexop.com/qa-private-RTNS-notify");

        // PROD
        //private readonly Uri websocketUri = new Uri("wss://ws.plexop.com/prod-public-RTNS-websocket");
        //private readonly Uri httpUri = new Uri("https://api.plexop.com/prod-public-RTNS-notify");

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
            var topics = new Topic[] {new Topic("Topic1"), new Topic("Topic2") };

            var wsClient = new RtnsWebsocketClient(websocketUri);
            await wsClient.Unsubscribe(new Topic[] { new Topic("warmup") });
            await wsClient.SubscribeAndStartListening(topics);

            var httpClient = new RtnsHttpClient(httpUri);
            await httpClient.Warmup();

            await NotifyAndWaitForAnswer(topics, topics, httpClient, wsClient);

            var unsubscribeFrom = new Topic[] { new Topic("Topic2") };
            await wsClient.Unsubscribe(unsubscribeFrom);

            Thread.Sleep(3000);

            var expectedTopics = new Topic[] { new Topic("Topic1") };
            await NotifyAndWaitForAnswer(topics, expectedTopics, httpClient, wsClient);

            await wsClient.Unsubscribe(new Topic[] { });
            await NotifyAndWaitForAnswer(topics, new Topic[] { }, httpClient, wsClient);
        }

        private static async Task NotifyAndWaitForAnswer(
            Topic[] topics, 
            Topic[] expected,
            RtnsHttpClient httpClient,
            RtnsWebsocketClient wsClient)
        {
            await httpClient.NotifyAbout(topics);

            var expectedNotificationMessage = JsonConvert.SerializeObject(new
            {
                topics = expected.Select(t => t.Name).ToArray()
            });
            string notification = await wsClient.WaitForNotification(TimeSpan.FromSeconds(5));
            if (expected.Any())
            {
                Assert.AreEqual(expectedNotificationMessage, notification);
            }
        }

        private async Task SusbcribeAndWaitForNotification(int topicsCount)
        {
            var manyTopics = new List<Topic>();

            for (int i = 1; i <= topicsCount; i++)
            {
                manyTopics.Add(new Topic($"Test_{i}"));
            }

            var wsClient = new RtnsWebsocketClient(websocketUri);
            await wsClient.SubscribeAndStartListening(manyTopics.ToArray());

            var httpClient = new RtnsHttpClient(httpUri);
            await httpClient.Warmup();

            await httpClient.NotifyAbout(manyTopics.ToArray());

            var expectedNotificationMessage = JsonConvert.SerializeObject(new
            {
                topics = manyTopics.Select(t => t.Name).ToArray()
            });
            string notification = await wsClient.WaitForNotification(TimeSpan.FromSeconds(30));
            Assert.AreEqual(expectedNotificationMessage, notification);
        }
    }
}