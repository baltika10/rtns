namespace RTNS.Core.Notifications
{
    using System.Threading.Tasks;

    using RTNS.Core.Model;

    public interface NotificationBuilder
    {
        Task<Notification[]> BuildNotificationsFor(Topic[] topics);
    }
}
