namespace RTNS.AWS.Queues
{
    using System.Linq;
    using System.Threading.Tasks;

    using Amazon.SQS;

    using RTNS.Core.Model;

    public class SqsGoneQueue : SqsQueue, GoneQueue
    {
        public SqsGoneQueue(IAmazonSQS sqs, string queueAddress)
            : base(sqs, queueAddress)
        { }

        public async Task Enqueue(params Subscriber[] subscribers)
        {
            var messages = subscribers.Select(subscriber => subscriber.Id).ToArray();
            await base.Enqueue(messages);
        }
    }
}
