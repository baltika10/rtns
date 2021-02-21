namespace RTNS.Core.Notifications
{
    using System;

    using RTNS.Core.Model;

    public class NotificationResult
    {
        public NotificationResult(Notification[] successful, FailedNotification[] failed)
        {
            if (successful == null)
            {
                throw new ArgumentNullException(nameof(successful));
            }

            if (failed == null)
            {
                throw new ArgumentNullException(nameof(failed));
            }

            Successful = successful;
            Failed = failed;
        }

        public Notification[] Successful { get; }

        public FailedNotification[] Failed { get; }
    }
}