namespace RTNS.AWS.Notifications
{
    using System;

    using Microsoft.Extensions.DependencyInjection;

    using RTNS.AWS.Queues;

    using RTNS.Core.Notifications;
    using RTNS.Core.Storage;

    using Amazon.ApiGatewayManagementApi;
    using Amazon.DynamoDBv2;
    using Amazon.SQS;

    public class DependencyResolver
    {
        private readonly IServiceProvider servicesProvider;

        public DependencyResolver()
        {
            servicesProvider = ConfigureServices();
        }

        public T GetService<T>()
        {
            return servicesProvider.GetService<T>();
        }

        private Func<IServiceProvider> ConfigureServices = () =>
        {
            var servicesCollection = new ServiceCollection();

            servicesCollection.AddTransient<SubscriptionsRepository>(provider => 
                new DynamoDbSubscriptionsRepository(
                    new AmazonDynamoDBClient(), 
                    new DynamoDbTableSettings(
                        Environment.GetEnvironmentVariable("SubscriptionsTableName"),
                        Environment.GetEnvironmentVariable("TopicSubscribersIndex"),
                        Environment.GetEnvironmentVariable("SubscriptionsTableHashKey"),
                        Environment.GetEnvironmentVariable("SubscriptionsTableRangeKey"))));

            servicesCollection.AddTransient<NotificationBuilder, RepositoryBasedNotificationBuilder>();

            servicesCollection.AddTransient<IAmazonApiGatewayManagementApi>(provider =>
                new AmazonApiGatewayManagementApiClient(
                    new AmazonApiGatewayManagementApiConfig
                    {
                        ServiceURL = Environment.GetEnvironmentVariable("WebSocketApiEndpoint")
                    }));

            //servicesCollection.AddTransient<GoneQueue, VoidGoneQueue>();

            servicesCollection.AddTransient<NotificationQueue>(provider =>
                new SqsNotificationQueue(
                    new AmazonSQSClient(),
                    Environment.GetEnvironmentVariable("NotificationsQueueAddress")));

            // Replace above statement with the following one to send gone connections to a SQS queue.
            servicesCollection.AddTransient<GoneQueue>(provider =>
                new SqsGoneQueue(
                    new AmazonSQSClient(),
                    Environment.GetEnvironmentVariable("GoneQueueAddress")));

            servicesCollection.AddTransient<Notificator, WebSocketApiGatewayNotificator>();
            
            return servicesCollection.BuildServiceProvider();
        };
    }
}
