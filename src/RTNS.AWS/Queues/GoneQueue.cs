namespace RTNS.AWS.Queues
{
    using System.Threading.Tasks;

    using RTNS.Core.Model;

    public interface GoneQueue
    {
        Task Enqueue(params Subscriber[] subscribers);
    }
}
