namespace RTNS.IntegrationTests.Clients
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using RTNS.Core.Model;

    internal class RtnsHttpClient
    {
        private readonly HttpClient client;
        private readonly Uri httpServerAddress;

        public RtnsHttpClient(Uri httpServerAddress)
        {
            if(httpServerAddress == null)
                throw new ArgumentNullException(nameof(httpServerAddress));

            this.httpServerAddress = httpServerAddress;
            client = new HttpClient();
        }

        public async Task Warmup()
        {
            var notificationRequest = new NotificationRequest(new[] { new Topic("warmup") }, "message");
            await NotifyAbout(notificationRequest);
        }

        public async Task<HttpResponseMessage> NotifyAbout(NotificationRequest notificationRequest)
        {
            if (!notificationRequest.Topics.Any())
                throw new ArgumentException($"{nameof(notificationRequest.Topics)} must have at least one element!");
            
            var sc = new StringContent(JsonConvert.SerializeObject(new
            {
                topics = notificationRequest.Topics.Select(t => t.Name).ToArray(),
                message = notificationRequest.Message
            }),
            Encoding.UTF8,
            "application/json");

            return await client.PostAsync(httpServerAddress, sc);
        }
    }
}
