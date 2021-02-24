namespace RTNS.IntegrationTests.Clients
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    using RTNS.Core.Model;
    using Amazon.Runtime;
    using Amazon.SecurityToken;

    internal class RtnsHttpClient
    {
        private readonly HttpClient client;
        private readonly Uri httpServerAddress;
        private readonly ImmutableCredentials credentials;

        public RtnsHttpClient(Uri httpServerAddress)
        {
            if (httpServerAddress == null)
                throw new ArgumentNullException(nameof(httpServerAddress));

            this.httpServerAddress = httpServerAddress;
            client = new HttpClient();
            var sts = new AmazonSecurityTokenServiceClient();
            var token = sts.GetSessionTokenAsync().Result;
            credentials = new ImmutableCredentials(token.Credentials.AccessKeyId, token.Credentials.SecretAccessKey, token.Credentials.SessionToken);
        }

        public async Task Warmup()
        {
            var notificationRequest = new NotificationRequest(new[] { new Topic("warmup") }, "message");
            await NotifyAbout(notificationRequest);
        }

        public async Task<int> NotifyAbout(NotificationRequest notificationRequest)
        {
            if (!notificationRequest.Topics.Any())
                throw new ArgumentException($"{nameof(notificationRequest.Topics)} must have at least one element!");

            var serializedContent = JsonConvert.SerializeObject(new
            {
                topics = notificationRequest.Topics.Select(t => t.Name).ToArray(),
                message = notificationRequest.Message
            });


            var sc = new StringContent(serializedContent, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(
                  httpServerAddress,
                  content: sc,
                  regionName: "eu-central-1",
                  serviceName: "execute-api",
                  credentials: credentials);
            var responseContent =  await response.Content.ReadAsStringAsync();
            var parsedResponse = JsonConvert.DeserializeObject<SendNotificationResponse>(responseContent);
            return parsedResponse.TotalNotifications;
        }
    }
}
