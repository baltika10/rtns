namespace RTNS.AWS
{
    using System;

    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Amazon.DynamoDBv2;
    using Amazon.DynamoDBv2.Model;

    using RTNS.Core.Model;
    using RTNS.Core.Storage;

    public class DynamoDbSubscriptionsRepository : SubscriptionsRepository
    {
        private const int BatchSize = 25;

        private readonly string TableName;
        private readonly string TopicSubscribersIndex;
        private readonly string HashKey;
        private readonly string RangeKey;

        private readonly IAmazonDynamoDB ddbclient;

        public DynamoDbSubscriptionsRepository(
            IAmazonDynamoDB ddblicent,
            DynamoDbTableSettings dynamoDbTableSettings)
        {
            if (ddblicent == null)
            {
                throw new ArgumentNullException(nameof(ddblicent));
            }

            if (dynamoDbTableSettings == null)
            {
                throw new ArgumentNullException(nameof(dynamoDbTableSettings));
            }

            TableName = dynamoDbTableSettings.TableName;
            TopicSubscribersIndex = dynamoDbTableSettings.TopicSubscribersIndex;
            HashKey = dynamoDbTableSettings.HashKey;
            RangeKey = dynamoDbTableSettings.RangeKey;

            this.ddbclient = ddblicent;
        }

        public async Task Store(Subscription subscription)
        {
            if (subscription == null)
                throw new ArgumentNullException(nameof(subscription));

            var writeRequests = new List<WriteRequest>();
            foreach (var topic in subscription.Topics)
            {
                writeRequests.Add(new WriteRequest(
                    new PutRequest
                    {
                        Item = new Dictionary<string, AttributeValue>
                        {
                            {HashKey, new AttributeValue {S = subscription.Subscriber.Id}},
                            {RangeKey, new AttributeValue {S = topic.Name}}
                        }
                    }
                ));
            }

            await ExecuteInBatches(writeRequests);
        }

        public async Task RemoveBy(Subscriber subscriber)
        {
            if (subscriber == null)
                throw new ArgumentNullException(nameof(subscriber));

            var topicNames = await GetTopicNamesOf(subscriber);

            await DeleteTopicsForSubscriber(subscriber, topicNames);
        }

        public async Task Remove(Subscription subscription)
        {
            await DeleteTopicsForSubscriber(
                subscription.Subscriber,
                subscription.Topics.Select(t => t.Name).ToArray());
        }

        private async Task DeleteTopicsForSubscriber(Subscriber subscriber, string[] topicNames)
        {
            var writeRequests = new List<WriteRequest>();
            foreach (var topicName in topicNames)
            {
                writeRequests.Add(new WriteRequest(
                    new DeleteRequest
                    {
                        Key = new Dictionary<string, AttributeValue>()
                        {
                            {HashKey, new AttributeValue {S = subscriber.Id}},
                            {RangeKey, new AttributeValue {S = topicName}}
                        }
                    }
                ));
            }

            await ExecuteInBatches(writeRequests);
        }

        private async Task<string[]> GetTopicNamesOf(Subscriber subscriber)
            {
                var qr = BuildQueryRequestToGetRangeKeyValuesByHashKey(
                    HashKey, subscriber.Id, RangeKey);
                var result = await ddbclient.QueryAsync(qr);
                var objectIds = result.Items.Select(i => i.First(k => k.Key == RangeKey).Value.S).ToArray();
                return objectIds;
            }

        // https://forums.aws.amazon.com/thread.jspa?threadID=170405
        // DynamoDB currently supports a single hash key for a Query operation.
        // If you want to retrieve ranges for multiple hash keys you can use the Query API multiple times.
        public async Task<Subscriber[]> GetSubscribersBy(Topic topic)
        {
            var qr = BuildQueryRequestToGetRangeKeyValuesByHashKey(
                TopicSubscribersIndex, RangeKey, topic.Name, HashKey);
            var result = await ddbclient.QueryAsync(qr);
            return result.Items
                .Select(i => new Subscriber(i.First(k => k.Key == HashKey).Value.S))
                .ToArray();
        }

        private async Task ExecuteInBatches(List<WriteRequest> writeRequests)
        {
            int currentBatch = 0;
            var batch = writeRequests.Skip(currentBatch * BatchSize).Take(BatchSize).ToList();
            while (batch.Any())
            {
                var batchWriteRequest = new BatchWriteItemRequest
                {
                    ReturnConsumedCapacity = "TOTAL",
                    RequestItems = new Dictionary<string, List<WriteRequest>>
                    {
                        {
                            TableName, batch.ToList()
                        }
                    }
                };

                // https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/batch-operation-lowlevel-dotnet.html
                await ddbclient.BatchWriteItemAsync(batchWriteRequest);

                currentBatch++;
                batch = writeRequests.Skip(currentBatch * BatchSize).Take(BatchSize).ToList();
            }
        }

        private QueryRequest BuildQueryRequestToGetRangeKeyValuesByHashKey(
            string hashKeyName, string hashKeyValue, string rangeKeyName)
        {
            var qr = new QueryRequest
            {
                TableName = TableName,
                KeyConditionExpression = "#value = :v1",
                ExpressionAttributeNames = new Dictionary<string, string> { { "#value", hashKeyName } },
                ExpressionAttributeValues =
                    new Dictionary<string, AttributeValue> { { ":v1", new AttributeValue(hashKeyValue) } },
                ProjectionExpression = rangeKeyName,
            };

            return qr;
        }

        private QueryRequest BuildQueryRequestToGetRangeKeyValuesByHashKey(
            string indexName, string hashKeyName, string hashKeyValue, string rangeKeyName)
        {
            var qr = BuildQueryRequestToGetRangeKeyValuesByHashKey(hashKeyName, hashKeyValue, rangeKeyName);
            qr.IndexName = indexName;
            return qr;
        }
    }
}
