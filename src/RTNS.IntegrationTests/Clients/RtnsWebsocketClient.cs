using System.Reactive.Concurrency;
using System.Reactive.Linq;
using NUnit.Framework;

namespace RTNS.IntegrationTests.Clients
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using RTNS.Core.Model;

    using Websocket.Client;

    internal class RtnsWebsocketClient : IDisposable
    {
        private readonly IWebsocketClient client;

        private string lastMessage = null;
        private ManualResetEvent messageReceived = new ManualResetEvent(false);

        public RtnsWebsocketClient(Uri websocketServerAddress)
        {
            if (websocketServerAddress == null)
                throw new ArgumentNullException(nameof(websocketServerAddress));
            if (websocketServerAddress.Scheme != "wss")
                throw new ArgumentException($"{nameof(websocketServerAddress)} must be wss protocol!");

            client = new WebsocketClient(websocketServerAddress);
        }

        public async Task SubscribeAndStartListening(params Topic[] topics)
        {
            if (!topics.Any())
                throw new ArgumentException($"{nameof(topics)} must have at least one element!");

            if (!client.IsStarted)
            {
                await client.Start();
            }
            
            ListenForMessages();

            var subscriptionRequest = new
            {
                message = "subscribe",
                topics = topics.Select(t => t.Name).ToArray()
            };

            var serializedRequest = JsonConvert.SerializeObject(subscriptionRequest);

            await client.SendInstant(serializedRequest);
        }

        private void ListenForMessages()
        {
            client.MessageReceived.Subscribe(msg =>
            {
                lastMessage = msg.Text;
                messageReceived.Set();
            });
        }

        public async Task<string> WaitForNotification(TimeSpan timeout)
        {
            return await Task.Run(() =>
            {
                messageReceived.WaitOne(timeout);

                return lastMessage;
            });
        }

        public void Dispose()
        {
            client?.Dispose();
            messageReceived?.Dispose();
        }

        public async Task Unsubscribe(Topic[] topics)
        {
            if (!client.IsStarted)
            {
                await client.Start();
            }
            
            messageReceived.Reset();

            var unsubscribeRequest = new
            {
                message = "unsubscribe",
                topics = topics.Select(t => t.Name).ToArray()
            };

            var serializedRequest = JsonConvert.SerializeObject(unsubscribeRequest);

            await client.SendInstant(serializedRequest);
        }
    }
}
