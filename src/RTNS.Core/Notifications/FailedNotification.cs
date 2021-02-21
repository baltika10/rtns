namespace RTNS.Core.Notifications
{
    using System;

    using RTNS.Core.Model;

    public class FailedNotification
    {
        public FailedNotification(Notification notification, Exception exception)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            Notification = notification;
            Exception = exception;
        }

        public Notification Notification { get; }

        public Exception Exception { get; }
    }
}
