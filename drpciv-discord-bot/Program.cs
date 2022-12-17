using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Discord;
using Discord.WebSocket;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Discord.Interactions;
using MySqlConnector;

namespace drpciv_discord_bot
{
    class Program
    {
        public DiscordSocketClient _client;
        private IConfiguration _configuration;
        private IServiceProvider _services;

        public static ConfigJson cfgjson = JsonConvert.DeserializeObject<ConfigJson>(File.ReadAllText("config.json"));
        public static MySqlConnection mysqlConnection = new MySqlConnection(cfgjson.Mysql);

        private readonly DiscordSocketConfig _socketConfig = new()
        {
            GatewayIntents = GatewayIntents.AllUnprivileged,
            AlwaysDownloadUsers = true,
        };

        public Program()
        {
            _configuration = new ConfigurationBuilder()
               .SetBasePath(AppContext.BaseDirectory)
               .Build();

            _services = new ServiceCollection()
               .AddSingleton(_configuration)
               .AddSingleton(_socketConfig)
               .AddSingleton<DiscordSocketClient>()
               .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
               .AddSingleton<InteractionHandler>()
               .BuildServiceProvider();
        }
        public static void Main(string[] args)
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }
        

        public async Task MainAsync()
        {
            _client = _services.GetRequiredService<DiscordSocketClient>();
            
            _client.Log += LogAsync;
            await _services.GetRequiredService<InteractionHandler>()
                .InitAsync();

            await _client.LoginAsync(TokenType.Bot, cfgjson.Token);
            await _client.StartAsync();

            await Task.Delay(Timeout.Infinite);
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

    }

    public struct ConfigJson
    {
        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("mysql")]
        public string Mysql { get; private set; }
    }

    public class questionsClass
    {
        [JsonProperty("1")]
        public string _1 { get; set; }

        [JsonProperty("2")]
        public string _2 { get; set; }

        [JsonProperty("3")]
        public string _3 { get; set; }

        [JsonProperty("4")]
        public string _4 { get; set; }

        [JsonProperty("5")]
        public object _5 { get; set; }

        [JsonProperty("6")]
        public string _6 { get; set; }

        [JsonProperty("7")]
        public string _7 { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("question_name")]
        public string QuestionName { get; set; }

        [JsonProperty("answer1")]
        public string Answer1 { get; set; }

        [JsonProperty("answer2")]
        public string Answer2 { get; set; }

        [JsonProperty("answer3")]
        public string Answer3 { get; set; }

        [JsonProperty("answer4")]
        public object Answer4 { get; set; }

        [JsonProperty("answer")]
        public string Answer { get; set; }

        [JsonProperty("explicatie")]
        public string Explicatie { get; set; }
    }


}

