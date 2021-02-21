using System;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using RTNS.Core.Model;
using RTNS.Core.Storage;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace RTNS.AWS.Subscriptions
{
    public class Functions
    {
        private readonly SubscriptionsRepository subscriptionsRepository;

        public Functions()
        {
            var dependencyResolver = new DependencyResolver();
            subscriptionsRepository = dependencyResolver.GetService<SubscriptionsRepository>();
        }

        public async Task<APIGatewayProxyResponse> Subscribe(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                var connectionId = request.RequestContext.ConnectionId;

                context.Logger.LogLine($"Subscription request by {connectionId}.");

                var payload = JObject.Parse(request.Body)["topics"].ToString();
                context.Logger.LogLine(payload);

                var topics = JsonConvert.DeserializeObject<string[]>(payload);

                await subscriptionsRepository.Store(
                    new Subscription(
                        new Subscriber(connectionId),
                        topics.Select(topicName => new Topic(topicName)).ToArray()));

                return new APIGatewayProxyResponse
                {
                    StatusCode = 200,
                    Body = "Subscribed."
                };
            }
            catch (Exception e)
            {
                context.Logger.LogLine($"ERROR!: {e.Message}");
                return new APIGatewayProxyResponse
                {
                    StatusCode = 500,
                    Body = $"Failed to subscribe: {e.Message}"
                };
            }
        }

        public async Task<APIGatewayProxyResponse> Unsubscribe(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                var connectionId = request.RequestContext.ConnectionId;
                if (string.IsNullOrWhiteSpace(request.Body))
                {
                    await RemoveAllSubscriptionsOfSubscriber(context, connectionId);
                }
                else
                {
                    var selectedTopics = JObject.Parse(request.Body)["topics"];
                    context.Logger.LogLine($"selected topics: {selectedTopics}");
                    if (selectedTopics == null || !selectedTopics.HasValues)
                    {
                        await RemoveAllSubscriptionsOfSubscriber(context, connectionId);
                    }
                    else
                    {
                        await RemoveSelectedSubscriptionsOnly(connectionId, selectedTopics);
                    }
                }

                return new APIGatewayProxyResponse
                {
                    StatusCode = 200,
                    Body = "Unsubscribed."
                };
            }
            catch (Exception e)
            {
                context.Logger.LogLine($"ERROR!: {e.Message}");
                return new APIGatewayProxyResponse
                {
                    StatusCode = 500,
                    Body = $"Failed to unsubscribe: {e.Message}"
                };
            }
        }

            private async Task RemoveAllSubscriptionsOfSubscriber(ILambdaContext context, string connectionId)
            {
                context.Logger.LogLine($"Unsubscribing entire {connectionId} - no topics selected");
                await subscriptionsRepository.RemoveBy(new Subscriber(connectionId));
            }

            private async Task RemoveSelectedSubscriptionsOnly(string connectionId, JToken selectedTopics)
            {
                var subscriber = new Subscriber(connectionId);
                var topicsNames = JsonConvert.DeserializeObject<string[]>(selectedTopics.ToString());
                var topics = topicsNames.Select(topicName => new Topic(topicName)).ToArray();
                var subscription = new Subscription(subscriber, topics);
                await subscriptionsRepository.Remove(subscription);
            }

        public async Task<string> RemoveGone(SQSEvent sqsEvent, ILambdaContext context)
        {
            try
            {
                var subscribersToRemove = sqsEvent.Records.Select(record => new Subscriber(record.Body));
                foreach (var subscriber in subscribersToRemove)
                {
                    await subscriptionsRepository.RemoveBy(subscriber);
                }

                var message = $"Successfully removed {sqsEvent.Records.Count} gone subscribers!";
                context.Logger.LogLine(message);
                return message;
            }
            catch (Exception e)
            {
                context.Logger.LogLine($"Error processing gone queue records! {e.Message}");
                context.Logger.LogLine(e.StackTrace);
                throw;
            }
        }
    }
}
