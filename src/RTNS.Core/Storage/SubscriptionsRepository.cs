namespace RTNS.Core.Storage
{
    using System.Threading.Tasks;

    using RTNS.Core.Model;

    public interface SubscriptionsRepository
    {
        Task Store(Subscription subscription);
        Task RemoveBy(Subscriber subscriber);
        Task Remove(Subscription subscription);
        Task<Subscriber[]> GetSubscribersBy(Topic topic);
    }
}
