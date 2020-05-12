using CommandLine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Tiny.RestClient;
using Microsoft.VisualStudio.Threading;
using System.Net.Http.Headers;
using System.Net;

namespace Paycor.HttpDriver
{
    class Program
    {
        private static readonly HttpClient Client = new HttpClient();

        static async Task Main(string[] args)
        {
            await Parser
                .Default
                .ParseArguments<Options>(args)
                .WithParsedAsync(async o =>
                {
                    int intervals = 0;
                    var tasks = new List<Task>();
                    var watch = new Stopwatch();

                    while (++intervals <= o.NumberOfIntervals)
                    {
                        watch.Restart();
                        tasks.Clear();
                        Enumerable
                            .Range(1, o.NumberOfRequestsPerInterval).ToList()
                            .ForEach(x => tasks.Add(Task.Run(SendRequestAsync)));

                        await Task.WhenAll(tasks);
                        watch.Stop();

                        if (watch.Elapsed.TotalMinutes < o.NumberOfMinutesBetweenIntervals)
                        { 
                            await Task.Delay(TimeSpan.FromMinutes(o.NumberOfMinutesBetweenIntervals) - TimeSpan.FromMinutes(watch.Elapsed.TotalMinutes));
                        }
                    };
                });

            await Console.Out.WriteLineAsync("Press ENTER to close.");
            Console.ReadLine();
        }

        private static async Task SendRequestAsync()
        {
            await Console.Out.WriteLineAsync("Sending request...");
            var client = new TinyRestClient(Client, "http://paycortaxcredintfnquarterly.trafficmanager.net/api");
            client.Settings.DefaultHeaders.AddBearer(BearerToken.Value);

            var response = await client.PostRequest("v1/manualrequest")
                .AddContent(
                    new ManualRequest
                    {
                        Id = Guid.NewGuid(),
                        EventType = "Payroll.Payrun.Distributed",
                        ClientId = 207002,
                        PlannerUid = 15184521809082
                    })
                .AllowAnyHttpStatusCode()
                .ExecuteAsync<HttpResponseMessage>();

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                BearerToken = new Lazy<string>(() => GetBearerTokenAsync().Result);
            }
            else
            {
                Debug.Assert(response.IsSuccessStatusCode);
            }
        }

        private static Lazy<string> BearerToken = new Lazy<string>(() => GetBearerTokenAsync().Result);

        private static async Task<string> GetBearerTokenAsync()
        {
            Console.WriteLine("Getting bearer token...");

            /*
            var baseAddress = new Uri("https://secure-quarterly.paycor.com");
            var route = "/accounts/api/securityhelper/getauthtoken";
            var cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler() { CookieContainer = cookieContainer };
            handler.UseCookies = true;

            using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
            {
                HttpResponseMessage response1 = null;

                try
                {
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var paycorApiKey = Environment.GetEnvironmentVariable("paycor-api-key");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("paycorapi", paycorApiKey);

                    response1 = await client.GetAsync(route);
                }
                catch { }

                var cookies = response1.Headers.GetValues("Set-Cookie");
                client.DefaultRequestHeaders.Add("Cookie", cookies.ToList().FirstOrDefault());
                
                var response2 = await client.GetAsync(route);
                return await response2.Content.ReadAsStringAsync();
            }
            */

            var bearerToken = Environment.GetEnvironmentVariable("bearer-token");
            return await Task.FromResult(bearerToken);
        }
    }

    public class ManualRequest
    {
        public Guid Id { get; set; }
        public string EventType { get; set; }
        public int ClientId { get; set; }
        public Int64 PlannerUid { get; set; }
    }
}
