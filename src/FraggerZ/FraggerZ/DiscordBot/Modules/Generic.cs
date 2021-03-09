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
