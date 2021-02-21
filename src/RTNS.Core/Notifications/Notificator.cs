namespace RTNS.Core.Notifications
{
    using System.Threading.Tasks;

    using RTNS.Core.Model;

    public interface Notificator
    {
        Task<NotificationResult> Push(Notification[] notifications);
    }
}
