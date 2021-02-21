namespace RTNS.AWS.Queues
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Amazon.SQS;
    using Amazon.SQS.Model;

    public abstract class SqsQueue
    {
        private const int SqsBatchSize = 10;

        private readonly IAmazonSQS sqs;
        private readonly string queueAddress;

        public SqsQueue(IAmazonSQS sqs, string queueAddress)
        {
            if (sqs == null)
            {
                throw new ArgumentNullException(nameof(sqs));
            }

            if (queueAddress == null || string.IsNullOrWhiteSpace(queueAddress))
            {
                throw new ArgumentNullException(nameof(queueAddress));
            }

            this.sqs = sqs;
            this.queueAddress = queueAddress;
        }

        protected async Task Enqueue(params string[] messages)
        {
            int currentBatch = 0;
            var batch = messages.Skip(currentBatch * SqsBatchSize).Take(SqsBatchSize);
            while (batch.Any())
            {
                var batchWriteRequest = BuildBatchWriteRequest(batch);
                await sqs.SendMessageBatchAsync(batchWriteRequest);

                currentBatch++;
                batch = messages.Skip(currentBatch * SqsBatchSize).Take(SqsBatchSize);
            }
        }

            private SendMessageBatchRequest BuildBatchWriteRequest(IEnumerable<string> batch)
            {
                int entryId = 0;
                var entries =
                    batch.Select(message =>
                        new SendMessageBatchRequestEntry(entryId++.ToString(), message)).ToList();

                var batchWriteRequest = new SendMessageBatchRequest(queueAddress, entries);
                return batchWriteRequest;
            }
    }
}
