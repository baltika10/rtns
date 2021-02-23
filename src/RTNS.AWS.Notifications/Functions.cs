using System;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using RTNS.AWS.Queues;
using RTNS.Core.Model;
using RTNS.Core.Notifications;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace RTNS.AWS.Notifications
{
    public class Functions
    {
        private readonly NotificationBuilder notificationBuilder;
        private readonly NotificationQueue notificationQueue;
        private readonly Notificator notificator;

        public Functions()
        {
            var dependencyResolver = new DependencyResolver();
            notificationBuilder = dependencyResolver.GetService<NotificationBuilder>();
            notificationQueue = dependencyResolver.GetService<NotificationQueue>();
            notificator = dependencyResolver.GetService<Notificator>();
        }

        public async Task<APIGatewayProxyResponse> EnqueueNotifications(APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            try
            {
                context.Logger.LogLine(request.Body);
                var notificationRequest = JsonConvert.DeserializeObject<NotificationRequest>(request.Body);

                var notifications = await notificationBuilder.BuildNotificationsFor(notificationRequest.Topics, notificationRequest.Message);
                await notificationQueue.Enqueue(notifications);

                var responseBody = new
                {
                    totalNotifications = notifications.Length
                };
                return new APIGatewayProxyResponse
                {
                    StatusCode = 200,
                    Body = JsonConvert.SerializeObject(responseBody)
                };
            }
            catch (Exception e)
            {
                context.Logger.LogLine("Error enqueueing. " + e.Message);
                context.Logger.LogLine(e.StackTrace);

                var errorResponseBody = new
                {
                    error = e.Message
                };

                return new APIGatewayProxyResponse
                {
                    StatusCode = 500,
                    Body = JsonConvert.SerializeObject(errorResponseBody)
                };
            }
        }

        public async Task<string> PushNotifications(SQSEvent sqsEvent, ILambdaContext context)
        {
            try
            {
                var notifications =
                    sqsEvent.Records.Select(record =>
                            JsonConvert.DeserializeObject<Notification>(record.Body))
                        .ToArray();

                var result = await notificator.Push(notifications);
                var message = $"Successfully notified {result.Successful.Length} subscribers. Failed to notify {result.Failed.Length}";
                context.Logger.LogLine(message);
                return message;
            }
            catch (Exception e)
            {
                context.Logger.LogLine($"Error processing queue records! {e.Message}");
                context.Logger.LogLine(e.StackTrace);
                throw;
            }
        }
    }
}
