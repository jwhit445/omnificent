using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Reactions;
using DiscordBot.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
    public class Generic : ModuleBase<SocketCommandContext>
    {
        private readonly ChannelSettings _channelSettings;
        private readonly ReportReactions _reportReactions;
        private readonly EmoteSettings _emoteSettings;

        public Generic(IOptions<ChannelSettings> channelSettings, ReportReactions reportReactions, IOptions<EmoteSettings> emoteSettings)
        {
            _channelSettings = channelSettings.Value;
            _reportReactions = reportReactions;
            _emoteSettings = emoteSettings.Value;
        }

        [Command("ping")]
        [Summary("Check whether the bot is working or not")]
        public async Task Ping()
        {
            await ReplyAsync("Pong!");
        }

        [Command("reanalyze")]
        [Summary("Re distribute proper elos from match log reactions")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Reanalyze()
        {
            // get all messages in match log channel
            foreach (var channelCurr in Context.Guild.TextChannels)
            {
                if (channelCurr.Id == _channelSettings.RoCoMatchLogsChannelId)
                {
                    // get all messages
                    var messagesFlattened = await channelCurr.GetMessagesAsync(400).FlattenAsync();
                    var messages = messagesFlattened.ToList();
                    messages.Reverse();
                    foreach (var message in messages)
                    {
                        bool lessThanMatchNumber = false;
                        foreach (Embed embed in message.Embeds)
                        {
                            if (int.TryParse(embed.Title.Substring(11), out int matchNumber) && matchNumber < 225)
                            {
                                lessThanMatchNumber = true;
                                break;
                            }
                        }
                        if (lessThanMatchNumber) continue;

                        var reactions = message.Reactions;

                        int countTeam1Reaction = 0;
                        int countTeam2Reaction = 0;
                        int countCancelReaction = 0;
                        foreach (var reaction in reactions)
                        {
                            if (reaction.Key.Name == _emoteSettings.OneEmoteName)
                            {
                                countTeam1Reaction = reaction.Value.ReactionCount;
                            }
                            else if (reaction.Key.Name == _emoteSettings.TwoEmoteName)
                            {
                                countTeam2Reaction = reaction.Value.ReactionCount;
                            }
                            else if (reaction.Key.Name == _emoteSettings.XEmoteName)
                            {
                                countCancelReaction = reaction.Value.ReactionCount;
                            }



                            try
                            {
                                if (countTeam1Reaction > countTeam2Reaction && countTeam1Reaction > countCancelReaction)
                                {
                                    try
                                    {
                                        await _reportReactions.ReportMatchForMessage(message, channelCurr, 120253805538312195, _emoteSettings.OneEmoteName);
                                        break;
                                    }
                                    catch(Exception e)
                                    {
                                        await Task.Delay(2000);
                                        await _reportReactions.ReportMatchForMessage(message, channelCurr, 120253805538312195, _emoteSettings.OneEmoteName);
                                        break;
                                    }
                                }
                                else if (countTeam2Reaction > countTeam1Reaction && countTeam2Reaction > countCancelReaction)
                                {
                                    try
                                    {
                                        await _reportReactions.ReportMatchForMessage(message, channelCurr, 120253805538312195, _emoteSettings.TwoEmoteName);
                                        break;
                                    }
                                    catch (System.Exception e)
                                    {
                                        await Task.Delay(2000);
                                        await _reportReactions.ReportMatchForMessage(message, channelCurr, 120253805538312195, _emoteSettings.TwoEmoteName);
                                        break;
                                    }

                                }
                                else if (countCancelReaction > countTeam1Reaction && countCancelReaction > countTeam2Reaction)
                                {
                                    try
                                    {
                                        await _reportReactions.ReportMatchForMessage(message, channelCurr, 120253805538312195, _emoteSettings.XEmoteName);
                                        break;
                                    }
                                    catch (System.Exception e)
                                    {
                                        await Task.Delay(2000);
                                        await _reportReactions.ReportMatchForMessage(message, channelCurr, 120253805538312195, _emoteSettings.XEmoteName);
                                        break;
                                    }
                                }
                            }
                            catch
                            {
                                break;
                            }


                        }
                    }
                }
            }
        }

        [Command("post")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task PostEmbed([Remainder] string message)
        {
            await Context.Message.DeleteAsync();
            EmbedBuilder builder = new EmbedBuilder() { Color = Color.Green, Description = message};
            await Context.Channel.SendMessageAsync(null, false, builder.Build());
        }
    }
}
