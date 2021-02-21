namespace RTNS.AWS.Subscriptions
{
    using System;

    using Microsoft.Extensions.DependencyInjection;

    using Amazon.DynamoDBv2;

    using RTNS.Core.Storage;
    
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
            
            return servicesCollection.BuildServiceProvider();
        };
    }
}
