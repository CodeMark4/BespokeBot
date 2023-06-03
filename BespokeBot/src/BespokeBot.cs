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
        public BespokeData? BespokeData { get; private set; }

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
            BespokeData = ServiceProvider.GetService<BespokeData>();
            BespokeData.NinjasKey = bespokeConfig.NinjasKey;
            BespokeData.DbHelper = new DbHelper(bespokeConfig.DBConnection, "bespoke_db");

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
            Commands.RegisterCommands<ModeratorCommands>();

            Client.MessageCreated += OnMessageAdded;
            Client.GuildMemberAdded += OnMemberAdded;
            Client.GuildMemberRemoved += OnMemberRemoved;

            Interactivity = Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromSeconds(30),
            });

            await Client.ConnectAsync();
            await Task.Delay(-1);
        }

        public async Task OnMessageAdded(DiscordClient client, MessageCreateEventArgs e)
        {
            var dbHelper = BespokeData.DbHelper;

            //Query database for blacklisted words and respond accordingly
            if (dbHelper.ContainsBlacklistedWords(e.Message.Content))
            {
                await e.Message.DeleteAsync();
                await e.Channel.SendMessageAsync($"Watch your language {e.Author.Mention}!");

                var discordMember = (DiscordMember) e.Author;
                await dbHelper.AddWarningAsync(discordMember);
                int warnings = dbHelper.GetWarnings(discordMember);

                if (warnings > 3)
                {
                    var timeoutDuration = 2 * warnings;
                    await discordMember.TimeoutAsync(DateTimeOffset.Now.AddMinutes(timeoutDuration));
                    await e.Channel.SendMessageAsync($"Warning limit reached, timed out {discordMember.DisplayName} for {timeoutDuration} minutes");
                }
            }
        }

        public async Task OnMemberAdded(DiscordClient client, GuildMemberAddEventArgs e)
        {
            foreach (var channel in e.Guild.Channels.Values)
            {
                if (channel.Position == 1)
                {
                    await channel.SendMessageAsync($"Hello {e.Member.Mention}! Welcome onboard!");
                }
            }

            //Add member to database
            await BespokeData.DbHelper.AddMemberAsync(e.Member);
        }

        public async Task OnMemberRemoved(DiscordClient client, GuildMemberRemoveEventArgs e)
        {
            if (e.Guild.GetBanAsync(e.Member).Result != null)
            {
                BespokeData.DbHelper.DeleteMemberAsync(e.Member);
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
