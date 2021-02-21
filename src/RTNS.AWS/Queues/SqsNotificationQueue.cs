namespace RTNS.AWS.Queues
{
    using System.Linq;
    using System.Threading.Tasks;

    using Amazon.SQS;

    using Newtonsoft.Json;

    using RTNS.Core.Model;

    public class SqsNotificationQueue : SqsQueue, NotificationQueue
    {
        public SqsNotificationQueue(IAmazonSQS sqs, string queueAddress) 
            : base(sqs, queueAddress)
        { }

        public async Task Enqueue(params Notification[] notifications)
        {
            var messages = notifications.Select(JsonConvert.SerializeObject).ToArray();
            await base.Enqueue(messages);
        }
    }
}
