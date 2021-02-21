namespace RTNS.AWS
{
    using System;

    public class DynamoDbTableSettings
    {
        public DynamoDbTableSettings(
            string tableName, 
            string topicSubscribersIndex,
            string hashKey,
            string rangeKey)
        {
            if (tableName == null || string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            if (topicSubscribersIndex == null || string.IsNullOrWhiteSpace(topicSubscribersIndex))
            {
                throw new ArgumentNullException(nameof(topicSubscribersIndex));
            }

            if (hashKey == null || string.IsNullOrWhiteSpace(hashKey))
            {
                throw new ArgumentNullException(nameof(hashKey));
            }

            if (rangeKey == null || string.IsNullOrWhiteSpace(rangeKey))
            {
                throw new ArgumentNullException(nameof(rangeKey));
            }

            TableName = tableName;
            TopicSubscribersIndex = topicSubscribersIndex;
            HashKey = hashKey;
            RangeKey = rangeKey;
        }

        public string TableName { get; }

        public string TopicSubscribersIndex { get; }

        public string HashKey { get; }

        public string RangeKey { get; }
    }
}
