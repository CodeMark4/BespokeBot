using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DSharpPlus.Interactivity.Extensions;
using MongoDB.Bson;
using MongoDB.Driver;
using BespokeBot.Serialization;

namespace BespokeBot.Commands
{
    public class ModeratorCommands : BaseCommandModule
    {
        public BespokeData BespokeData { private get; set; }

        [Command("mute")]
        [RequirePermissions(DSharpPlus.Permissions.MuteMembers)]
        [Description("Mutes a specified channel member.")]
        public async Task Mute(CommandContext ctx,
           [Description("Member username")] DiscordMember member)
        {
            await member.SetMuteAsync(true);
            await ctx.Message.RespondAsync("Successfully muted " + member.Mention);
        }

        [Command("unmute")]
        [RequirePermissions(DSharpPlus.Permissions.MuteMembers)]
        [Description("Unmutes a specified channel member.")]
        public async Task Unmute(CommandContext ctx,
           [Description("Member username")] DiscordMember member)
        {
            await member.SetMuteAsync(false);
        }

        [Command("deafen")]
        [RequirePermissions(DSharpPlus.Permissions.DeafenMembers)]
        [Description("Deafens a specified channel member.")]
        public async Task Deafen(CommandContext ctx,
          [Description("Member username")] DiscordMember member)
        {
            await member.SetDeafAsync(true);
            await ctx.Message.RespondAsync("Successfully deafened " + member.Mention);
        }

        [Command("undeafen")]
        [RequirePermissions(DSharpPlus.Permissions.DeafenMembers)]
        [Description("Undeafens a specified channel member.")]
        public async Task Undeafen(CommandContext ctx,
           [Description("Member username")] DiscordMember member)
        {
            await member.SetDeafAsync(false);
        }

        [Command("timeout")]
        [RequirePermissions(DSharpPlus.Permissions.ModerateMembers)]
        [Description("Times out a specified channel member for a given duration.")]
        public async Task Timeout(CommandContext ctx,
           [Description("Member username")] DiscordMember member,
           [Description("Timeout duration (eg. 5m)")] TimeSpan time)
        {
            var duration = DateTimeOffset.Now + time;
            if (duration > DateTimeOffset.Now.AddMinutes(15))
            {
                await ctx.RespondAsync("You can't timeout a member for more than 15 minutes.");
            }
            else
            {
                await member.TimeoutAsync(duration);
                await ctx.Message.RespondAsync("Timed out " + member.Mention + " for " + duration);
            }
        }

        [Command("memberinfo")]
        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        [Description("Displays information about a specified member.")]
        public async Task MemberInfo(CommandContext ctx,
            [Description("Member username")] DiscordMember member)
        {
            var nick = member.Nickname;
            var roles = member.Roles;
            var roleNames = new List<string>();

            foreach (var role in roles)
            {
                roleNames.Add(role.Name);
            }

            if (string.IsNullOrWhiteSpace(member.Nickname))
                nick = "<none>";

            var memberEmbed = new DiscordEmbedBuilder
            {
                Title = "Some information about " + member.Username,
                Description =
                "\nUsername: " + member.Username +
                "\nNickname: " + nick +
                "\nJoined: " + member.JoinedAt.ToString("d") +
                "\nRoles: " + string.Join(", ", roleNames) + 
                "\nBespokePoints: " + BespokeData.DbHelper.GetBespokePoints(member) + 
                "\nWarnings: " + BespokeData.DbHelper.GetWarnings(member),
                ImageUrl = member.AvatarUrl,
                Color = DiscordColor.White
            };

            await ctx.Channel.SendMessageAsync(embed: memberEmbed).ConfigureAwait(false);
        }

        [Command("nick")]
        [RequirePermissions(DSharpPlus.Permissions.ManageNicknames)]
        [Description("Changes a member's nickname.")]
        public async Task Nick(CommandContext ctx,
           [Description("Member username")] DiscordMember member,
           [RemainingText, Description("New nickname")] string newNickname)
        {
            await member.ModifyAsync(x => x.Nickname = newNickname);
        }

        [Command("grantrole")]
        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        [Description("Grants a specified role to a chosen member.")]
        public async Task GrantRole(CommandContext ctx,
           [Description("Member username")] DiscordMember member,
           [Description("Role name")] DiscordRole role)
        {
            await member.GrantRoleAsync(role);
        }

        [Command("revokerole")]
        [RequirePermissions(DSharpPlus.Permissions.ManageRoles)]
        [Description("Revokes a specified role from a chosen member.")]
        public async Task RevokeRole(CommandContext ctx,
           [Description("Member username")] DiscordMember member,
           [Description("Role name")] DiscordRole role)
        {
            await member.RevokeRoleAsync(role);
        }

        [Command("kick")]
        [RequirePermissions(DSharpPlus.Permissions.KickMembers)]
        [Description("Kicks a specified member from the server.")]
        public async Task Kick(CommandContext ctx,
           [Description("Member username")] DiscordMember member)
        {
            var interactivity = ctx.Client.GetInteractivity();

            await ctx.Channel.SendMessageAsync($"Are you sure you want to kick {member.Mention}? [yes/no]");
            var answer = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member);

            if (answer.Result.Content.ToLower() == "yes")
            {
                await ctx.Message.RespondAsync("Kicked " + member.DisplayName);
                await member.RemoveAsync();
            }
            else if (answer.Result.Content.ToLower() == "no")
                await ctx.Channel.SendMessageAsync(member.Mention + " has been spared... For now.");
            else
                await ctx.Channel.SendMessageAsync("Invalid answer.");
        }

        [Command("ban")]
        [RequirePermissions(DSharpPlus.Permissions.BanMembers)]
        [Description("Bans a specified member from the server and deletes all their messeges from this day.")]
        public async Task Ban(CommandContext ctx,
            [Description("Member username")] DiscordMember member,
            [Description("Reason for the ban")] params string[] reason)
        {
            var interactivity = ctx.Client.GetInteractivity();

            await ctx.Channel.SendMessageAsync($"Are you sure you want to ban {member.Mention}? [yes/no]");
            var answer = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member);

            if (answer.Result.Content.ToLower() == "yes")
            {
                await ctx.Message.RespondAsync("Banned " + member.DisplayName);
                await member.BanAsync(1, string.Join(' ', reason));
            }
            else if (answer.Result.Content.ToLower() == "no")
                await ctx.Channel.SendMessageAsync(member.Mention + " has been spared... For now.");
            else
                await ctx.Channel.SendMessageAsync("Invalid answer.");
        }

        [Command("move")]
        [RequirePermissions(DSharpPlus.Permissions.MoveMembers)]
        [Description("Moves a specified member to a chosen voice channel.")]
        public async Task Move(CommandContext ctx,
           [Description("Member username")] DiscordMember member,
           [Description("Channel name")] DiscordChannel channel)
        {
            await member.PlaceInAsync(channel);
        }

        [Command("blacklist")]
        [RequirePermissions(DSharpPlus.Permissions.ModerateMembers)]
        [Description("Blacklists a word.")]
        public async Task Blacklist(CommandContext ctx,
           [Description("Word to blacklist")] string word)
        {
            await BespokeData.DbHelper.BlacklistWordAsync(word);
            await ctx.Message.RespondAsync("Word blacklisted");
        }

        [Command("whitelist")]
        [RequirePermissions(DSharpPlus.Permissions.ModerateMembers)]
        [Description("Whitelist a word.")]
        public async Task Whitelist(CommandContext ctx,
            [Description("Word to whitelist")] string word)
        {
            await BespokeData.DbHelper.WhitelistWordAsync(word);
            await ctx.Message.RespondAsync("Word whitelisted");
        }

        [Command("addpoints")]
        [RequirePermissions(DSharpPlus.Permissions.ModerateMembers)]
        [Description("Grant BespokePoints to a member.")]
        public async Task AddPoints(CommandContext ctx, DiscordMember member, int points)
        {
            await BespokeData.DbHelper.AddBespokePointsAsync(member, points);
            int newPoints = BespokeData.DbHelper.GetBespokePoints(member);

            await ctx.Message.RespondAsync($"{member.Mention} now has {newPoints} BespokePoints");
        }

        [Command("trivia")]
        [RequirePermissions(DSharpPlus.Permissions.ModerateMembers)]
        [Description("Runs a game of trivia.")]
        public async Task Trivia(CommandContext ctx,
            [Description("Number of questions")] int numberOfQuestions = 1)
        {
            if (numberOfQuestions < 1)
            {
                numberOfQuestions = 1;
            }
            else if (numberOfQuestions > 30)
            {
                numberOfQuestions = 30;
            }

            var httpClient = new HttpClient();
            var apiUrl = "https://api.api-ninjas.com/v1/trivia?limit=" + numberOfQuestions;

            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Add("X-Api-key", BespokeData.NinjasKey);

            var trivia = await httpClient.GetStringAsync(apiUrl);

            var questions = JsonSerializer.Deserialize<List<TriviaQuestion>>(trivia);

            foreach (var question in questions)
            {
                if (questions.IndexOf(question) > 0)
                {
                    await ctx.Channel.SendMessageAsync("Next question...");
                }
                
                string category = string.IsNullOrWhiteSpace(question.Category) ? "random" : question.Category;

                var triviaEmbed = new DiscordEmbedBuilder
                {
                    Title = "Category: " + category,
                    Description = question.Question
                };

                await ctx.Channel.SendMessageAsync(embed: triviaEmbed);

                var interactivity = ctx.Client.GetInteractivity();

                var message = await interactivity.WaitForMessageAsync(x => x.Content.ToLower() == question.Answer.ToLower() || x.Content.ToLower() == "skip");

                if (message.Result != null)
                {
                    if (message.Result.Content.ToLower() == question.Answer.ToLower())
                    {
                        await BespokeData.DbHelper.AddBespokePointsAsync((DiscordMember)ctx.Message.Author, 10);
                        await ctx.Channel.SendMessageAsync($"{ctx.Message.Author.Mention} got it right and won 10 BespokePoints!");
                    }
                }
            }

            await ctx.Channel.SendMessageAsync("Trivia is over. Thanks for participating!");
        }

        [Command("addcity")]
        [RequirePermissions(DSharpPlus.Permissions.ModerateMembers)]
        [Description("Adds a city to the weather report.")]
        public async Task AddCity(CommandContext ctx, string city)
        {
            await BespokeData.DbHelper.AddCityAsync(city);

            await ctx.Message.RespondAsync($"Added {city} to the weather report");
        }

        [Command("removecity")]
        [RequirePermissions(DSharpPlus.Permissions.ModerateMembers)]
        [Description("Removes a city from the weather report.")]
        public async Task RemoveCity(CommandContext ctx, string city)
        {
            await BespokeData.DbHelper.RemoveCityAsync(city);

            await ctx.Message.RespondAsync($"Removed {city} from the weather report");
        }

        [Command("fullweather")]
        [RequirePermissions(DSharpPlus.Permissions.ModerateMembers)]
        [Description("Displays a weather report.")]
        public async Task FullWeather(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("Preparing weather report!");
            var cities = BespokeData.DbHelper.GetCities();

            foreach (var city in cities)
            {
                var httpClient = new HttpClient();
                var apiUrl = "https://api.api-ninjas.com/v1/weather?city=" + city;

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Add("X-Api-key", BespokeData.NinjasKey);

                var response = await httpClient.GetStringAsync(apiUrl);

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
        }
    }
}
