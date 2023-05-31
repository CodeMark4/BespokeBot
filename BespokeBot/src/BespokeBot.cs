using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BespokeBot.Commands;
using BespokeBot.Serialization;
using Microsoft.Extensions.DependencyInjection;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics.Metrics;
using System.Threading.Channels;
using DSharpPlus.Entities;

namespace BespokeBot
{
    public class BespokeBot
    {
        private static readonly string RELEASE_CONFIG_PATH = Path.Join(Directory.GetCurrentDirectory(), "config/bespoke_config.json");
        private static readonly string DEBUG_CONFIG_PATH = Path.Join(Directory.GetCurrentDirectory(), "../../../config/bespoke_config.json");

        public DiscordClient? Client { get; private set; }
        public CommandsNextExtension? Commands { get; private set; }
        public InteractivityExtension? Interactivity { get; private set; }
        public ServiceProvider? ServiceProvider { get; private set; }

        public async Task RunAsync()
        {
            var bespokeConfig = LoadConfig();

            var clientConfig = new DiscordConfiguration
            {
                Token = bespokeConfig.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                Intents = DiscordIntents.All,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug
            };

            Client = new DiscordClient(clientConfig);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<BespokeData>();

            ServiceProvider = serviceCollection.BuildServiceProvider();
            ServiceProvider.GetService<BespokeData>().NinjasKey = bespokeConfig.NinjasKey;

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { bespokeConfig.Prefix },
                EnableDms = false,
                EnableMentionPrefix = true,
                DmHelp = false,
                EnableDefaultHelp = true,
                Services = ServiceProvider
            };

            Commands = Client.UseCommandsNext(commandsConfig);

            Commands.RegisterCommands<BasicCommands>();

            Client.MessageCreated += OnMessageAdded;
            Client.GuildMemberAdded += OnMemberAdded;

            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(1),
            });

            await Client.ConnectAsync();
            await Task.Delay(-1);
        }

        public async Task OnMessageAdded(DiscordClient client, MessageCreateEventArgs e)
        {
            //Query database for blacklisted words and respond accordingly
            await Task.CompletedTask;
        }

        public async Task OnMemberAdded(DiscordClient client, GuildMemberAddEventArgs e)
        {
            foreach (var channel in e.Guild.Channels.Values)
            {
                if (channel.Position == 0)
                {
                    await channel.SendMessageAsync($"Hello {e.Member.Mention}! Welcome onboard!");
                }
            }
        }

        public static BespokeConfig LoadConfig()
        {
            string configPath = "";

            if (File.Exists(RELEASE_CONFIG_PATH))
            {
                configPath = RELEASE_CONFIG_PATH;
            }
            else if (File.Exists(DEBUG_CONFIG_PATH))
            {
                configPath = DEBUG_CONFIG_PATH;
            }
            else
            {
                throw new Exception($"Could not find configuration file. Looked under: \n{RELEASE_CONFIG_PATH}, \n{DEBUG_CONFIG_PATH}");
            }

            return JsonSerializer.Deserialize<BespokeConfig>(File.ReadAllText(configPath));
        }
    }
}
