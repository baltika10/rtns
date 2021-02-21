namespace RTNS.AWS
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;

    using Amazon.ApiGatewayManagementApi;
    using Amazon.ApiGatewayManagementApi.Model;
    using Amazon.Runtime;

    using Newtonsoft.Json;

    using RTNS.AWS.Queues;
    using RTNS.Core.Model;
    using RTNS.Core.Notifications;
    using RTNS.Core.Storage;

    public class WebSocketApiGatewayNotificator : Notificator
    {
        private readonly SubscriptionsRepository subscriptionsRepository;
        private readonly IAmazonApiGatewayManagementApi apiGatewayManager;
        private readonly GoneQueue goneQueue;

        public WebSocketApiGatewayNotificator(
            SubscriptionsRepository subscriptionsRepository,
            IAmazonApiGatewayManagementApi apiGatewayManager,
            GoneQueue goneQueue)
        {
            if (subscriptionsRepository == null)
                throw new ArgumentNullException(nameof(subscriptionsRepository));

            if (apiGatewayManager == null)
                throw new ArgumentNullException(nameof(apiGatewayManager));

            if (goneQueue == null)
                throw new ArgumentNullException(nameof(goneQueue));

            this.subscriptionsRepository = subscriptionsRepository;
            this.apiGatewayManager = apiGatewayManager;
            this.goneQueue = goneQueue;
        }

        public async Task<NotificationResult> Push(Notification[] notifications)
        {
            var successfullyNotified = new ConcurrentBag<Notification>();
            var gone = new ConcurrentBag<Subscriber>();
            var failed = new ConcurrentBag<FailedNotification>();

            var tasks = notifications.Select(notif => SendNotification(notif, successfullyNotified, gone, failed));
            await Task.WhenAll(tasks);

            // https://docs.aws.amazon.com/apigateway/latest/developerguide/apigateway-websocket-api-route-keys-connect-disconnect.html
            // $disconnect is a best-effort event. API Gateway will try its best to deliver
            // the $disconnect event to your integration, but it cannot guarantee delivery.
            // So it's possible that the store still holds records of closed connections.
            // Removing them manually is the only way to clear them from the store
            await EnqueueGoneSubscribers(gone);

            return new NotificationResult(successfullyNotified.ToArray(), failed.ToArray());
        }

        private async Task SendNotification(Notification notification, ConcurrentBag<Notification> successfullyNotified, ConcurrentBag<Subscriber> gone, ConcurrentBag<FailedNotification> failed)
        {
            try
            {
                var data = new
                {
                    topics = notification.Topics.Select(t => t.Name).ToArray()
                };
                var serialized = JsonConvert.SerializeObject(data);

                var stream = new MemoryStream(Encoding.UTF8.GetBytes(serialized));
                await apiGatewayManager.PostToConnectionAsync(
                    new PostToConnectionRequest
                    {
                        ConnectionId = notification.Subscriber.Id,
                        Data = stream
                    });
                successfullyNotified.Add(notification);
            }
            catch (AmazonServiceException e)
            {
                // API Gateway returns a status of 410 GONE when the connection is no longer available. 
                if (e.StatusCode == HttpStatusCode.Gone)
                {
                    gone.Add(notification.Subscriber);
                }
                else
                {
                    failed.Add(new FailedNotification(notification, e));
                }
            }
        }

        private async Task EnqueueGoneSubscribers(IEnumerable<Subscriber> gone)
        {
            await goneQueue.Enqueue(gone.ToArray());
        }
    }
}
