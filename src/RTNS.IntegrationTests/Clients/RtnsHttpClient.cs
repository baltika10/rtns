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
            await NotifyAbout(new Topic("warmup"));
        }

        public async Task<HttpResponseMessage> NotifyAbout(params Topic[] topics)
        {
            if (!topics.Any())
                throw new ArgumentException($"{nameof(topics)} must have at least one element!");
            
            var sc = new StringContent(JsonConvert.SerializeObject(new
            {
                topics = topics.Select(t => t.Name).ToArray()
            }),
            Encoding.UTF8,
            "application/json");

            return await client.PostAsync(httpServerAddress, sc);
        }
    }
}
