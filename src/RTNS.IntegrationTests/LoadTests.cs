using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;

using NUnit.Framework;

using RTNS.Core.Model;
using RTNS.IntegrationTests.Clients;

namespace RTNS.IntegrationTests
{
    public class LoadTests
    {
        private readonly Uri websocketUri = new Uri("wss://dev-ws.plexop.com/dev-public-RTNS-websocket");
        private readonly Uri httpUri = new Uri("https://dev-api.plexop.com/dev-public-RTNS-notify");

        private readonly Topic[] topics = new[] { new Topic("1") };

        [TestCase(30)]
        [Ignore("Unfinished")]
        public async Task NotifyPerformanceCounter(int seconds)
        {
            var runningTime = TimeSpan.FromSeconds(seconds);

            var httpClient = new RtnsHttpClient(httpUri);
            await httpClient.Warmup();

            var responses = new List<NotificationResponse>();
            var sw = new Stopwatch();
            sw.Start();
            while (sw.Elapsed < runningTime)
            {
                //var swInner = new Stopwatch();
                //swInner.Start();
                var response = await httpClient.NotifyAbout(topics);
                //swInner.Stop();
                var body = await response.Content.ReadAsStringAsync();
                var notificationResponse = JsonConvert.DeserializeObject<NotificationResponse>(body);
                //notificationResponse.ElapsedSeconds = (int)swInner.Elapsed.TotalSeconds;
                responses.Add(notificationResponse);
            }

            var didWork = responses.Where(r => r.successful > 0).ToArray();
        }

        private class NotificationResponse
        {
            public int successful { get; set; }

            public int failed { get; set; }

            public int notificationsTookSeconds { get; set; }

            public int deletingGoneTookSeconds { get; set; }
        }
    }
}
