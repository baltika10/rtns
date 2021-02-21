namespace RTNS.AWS.Queues
{
    using System.Threading.Tasks;

    using RTNS.Core.Model;

    public interface NotificationQueue
    {
        Task Enqueue(params Notification[] notifications);
    }
}
