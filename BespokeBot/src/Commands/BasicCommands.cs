using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BespokeBot.Serialization;
using DSharpPlus.Interactivity.Extensions;

namespace BespokeBot.Commands
{
    public class BasicCommands : BaseCommandModule
    {
        public BespokeData BespokeData { private get; set; }

        [Command("ping")]
        [Description("Responds with \"Pong\" and displays bot's reaction time.")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.Message.RespondAsync($"Pong! {ctx.Client.Ping}ms");
        }

        [Command("echo")]
        [Description("Echoes the next message you send to the channel.")]
        public async Task Echo(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();

            var message = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member);

            await ctx.Channel.SendMessageAsync(message.Result.Content);
        }

        [Command("coinflip")]
        [Description("Perform a virtual coin flip. The result is random and is either \"Heads\" or \"Tails\".")]
        public async Task CoinFlip(CommandContext ctx)
        {
            var rng = new Random();
            var result = rng.Next(2) == 0 ? "Heads" : "Tails";

            var coinEmbed = new DiscordEmbedBuilder
            {
                Title = "Flipping a coin! :coin:",
                Description = $"The result is: {result}",
                Color = DiscordColor.Gold
            };

            await ctx.Channel.SendMessageAsync(coinEmbed);
        }

        [Command("diceroll")]
        [Description("Perform a virtual dice roll. Specify type and number of dice.")]
        public async Task DiceRoll(CommandContext ctx,
           [Description("Specify dice type (number of sides).")] int diceType,
           [Description("Specify number of dice you'd like to roll.")] int numberOfDice)
        {
            if (diceType > 100 || diceType <= 0)
            {
                await ctx.Message.RespondAsync("Invalid dice type. Sorry!");
            }
            else if (numberOfDice > 20 || numberOfDice <= 0)
            {
                await ctx.Message.RespondAsync("Number of dice should be between 1 and 20.");
            }
            else
            {
                var rng = new Random();
                var outcomes = new List<int>();
                var result = string.Empty;
                var sum = 0;

                for (var i = 0; i < numberOfDice; i++)
                {
                    var roll = rng.Next(1, diceType + 1);
                    outcomes.Add(roll);
                    sum += roll;
                }

                result = string.Join(" ", outcomes);

                await ctx.Channel.SendMessageAsync("Roll result: " + result + "\nSum: " + sum);
            }
        }

        [Command("reminder")]
        [Description("Sets a reminder for the user.")]
        public async Task Reminder(CommandContext ctx,
           [Description("Specify a time for the reminder (eg. \"1d 1h 1s\").")] TimeSpan reminderTime,
           [Description("Write a note for the reminder (eg. \"turn off the oven\").")] params string[] reminderNote)
        {
            var note = string.Join(' ', reminderNote);
            await ctx.Message.RespondAsync("Reminder set for " + (DateTime.Now + reminderTime));

            var reminderEmbed = new DiscordEmbedBuilder
            {
                Title = "Reminder " + ":bell:",
                Description = note,
                Color = DiscordColor.White,
            };

            var timer = new Timer(_ => ctx.Channel.SendMessageAsync(ctx.User.Mention, embed: reminderEmbed));
            timer.Change((int)(reminderTime).TotalMilliseconds, Timeout.Infinite);
        }

        [Command("fact")]
        [Description("Displays a random, interesting fact.")]
        public async Task Fact(CommandContext ctx)
        {
            var httpClient = new HttpClient();
            var apiUrl = "https://api.api-ninjas.com/v1/facts?limit=1";

            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Add("X-Api-key", BespokeData.NinjasKey);

            var fact = await httpClient.GetStringAsync(apiUrl);

            var factJson = JsonSerializer.Deserialize<List<Fact>>(fact);

            var factEmbed = new DiscordEmbedBuilder
            {
                Title = "Did you know...",
                Description = factJson.First().Text
            };

            await ctx.Channel.SendMessageAsync(embed: factEmbed);
        }

        [Command("joke")]
        [Description("The bot tells you a joke.")]
        public async Task Joke(CommandContext ctx)
        {
            var httpClient = new HttpClient();
            var apiUrl = "https://api.api-ninjas.com/v1/jokes?limit=1";

            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Add("X-Api-key", BespokeData.NinjasKey);

            var joke = await httpClient.GetStringAsync(apiUrl);

            var jokeJson = JsonSerializer.Deserialize<List<Joke>>(joke);

            var jokeEmbed = new DiscordEmbedBuilder
            {
                Title = "Here's a good one...",
                Description = jokeJson.First().Text
            };

            await ctx.Channel.SendMessageAsync(embed: jokeEmbed);
        }

        [Command("quote")]
        [Description("Displays a random quote from a famous person.")]
        public async Task Quote(CommandContext ctx)
        {
            var httpClient = new HttpClient();
            var apiUrl = "https://api.api-ninjas.com/v1/quotes?limit=1";

            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Add("X-Api-key", BespokeData.NinjasKey);

            var quote = await httpClient.GetStringAsync(apiUrl);

            var quoteJson = JsonSerializer.Deserialize<List<Quote>>(quote);

            var footer = new DiscordEmbedBuilder.EmbedFooter()
            {
                Text = "- " + quoteJson.First().Author
            };

            var quoteEmbed = new DiscordEmbedBuilder
            {
                Title = "Here's your quote for the day...",
                Description = quoteJson.First().Text,
                Footer = footer
            };

            await ctx.Channel.SendMessageAsync(embed: quoteEmbed);
        }

        [Command("points")]
        [Description("Displays the number of BespokePoints a member has.")]
        public async Task Points(CommandContext ctx)
        {
            int points = BespokeData.DbHelper.GetBespokePoints((DiscordMember) ctx.Message.Author);

            await ctx.Message.RespondAsync($"You have {points} BespokePoints");
        }

        [Command("warnings")]
        [Description("Displays the number of BespokePoints a member has.")]
        public async Task Warnings(CommandContext ctx)
        {
            int warnings = BespokeData.DbHelper.GetWarnings((DiscordMember) ctx.Message.Author);

            await ctx.Message.RespondAsync($"You have {warnings} warnings");
        }

        [Command("weather")]
        [Description("Displays current weather for a specified city.")]
        public async Task Weather(CommandContext ctx, string city)
        {
            var httpClient = new HttpClient();
            var apiUrl = "https://api.api-ninjas.com/v1/weather?city=" + city;

            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Add("X-Api-key", BespokeData.NinjasKey);

            var cities = BespokeData.DbHelper.GetCities();
            var response = await httpClient.GetStringAsync(apiUrl);

            try
            {
                var weather = JsonSerializer.Deserialize<Weather>(response);

                var weatherEmbed = new DiscordEmbedBuilder
                {
                    Title = "Here's the weather for " + city + " " + BespokeData.GetEmoteForCloudPct(weather.CloudPct),
                    Description = "Cloud percentage: " + weather.CloudPct +
                                  "\nTemperature: " + weather.Temp + 
                                  "\nFeels like: " + weather.FeelsLike + 
                                  "\nHumidity: " + weather.Humidity + 
                                  "\nWind speed [m/s]: " + weather.WindSpeed
                };

                await ctx.Channel.SendMessageAsync(embed: weatherEmbed);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
        }
    }
}