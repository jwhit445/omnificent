using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using DiscordBot.Settings;
using Microsoft.Extensions.Options;
using DiscordBot.Services;

namespace DiscordBot.Reactions
{
    public class RoleAssignmentReactions
    {
        private readonly RoleSettings _roleSettings;
        private readonly ChannelSettings _channelSettings;
        private readonly EmoteSettings _emoteSettings;
        private readonly UserService _userService;

        public RoleAssignmentReactions(IOptions<RoleSettings> roleSettings, 
            IOptions<ChannelSettings> channelSettings, 
            IOptions<EmoteSettings> emoteSettings, UserService userService)
        {
            _roleSettings = roleSettings.Value;
            _channelSettings = channelSettings.Value;
            _emoteSettings = emoteSettings.Value;
            _userService = userService;
        }

        public async Task HandleReactionAddedAsync(Cacheable<IUserMessage,
            ulong> message, SocketTextChannel channel, SocketReaction reaction)
        {
            if (channel.Id == _channelSettings.GameChannelId) await HandleGameReactionAddedAsync(channel, reaction);
            else await HandleRoleReactionAddedAsync(channel, reaction);
        }

        public async Task HandleReactionRemovedAsync(Cacheable<IUserMessage,
            ulong> message, SocketTextChannel channel, SocketReaction reaction)
        {
            if (channel.Id == _channelSettings.GameChannelId) await HandleGameReactionRemovedAsync(channel, reaction);
            else await HandleRoleReactionRemovedAsync(channel, reaction);
        }

        async Task HandleGameReactionAddedAsync(SocketTextChannel channel, SocketReaction reaction)
        {
            if (reaction.Emote.Name == _emoteSettings.RoCoGameEmoteName)
                await AddRoleToUser(channel, reaction, _roleSettings.RoCoRoleId);
            else if(reaction.Emote.Name == _emoteSettings.CrossFireGameEmoteName)
                await AddRoleToUser(channel, reaction, _roleSettings.CrossFireRoleId);
            else if (reaction.Emote.Name == _emoteSettings.IronSightGameEmoteName)
                await AddRoleToUser(channel, reaction, _roleSettings.IronSightRoleId);
        }

        async Task HandleRoleReactionAddedAsync(SocketTextChannel channel, SocketReaction reaction)
        {
            if (reaction.Emote.Name == _emoteSettings.FraggerZEmoteName)
            {
                await AddRoleToUser(channel, reaction, _roleSettings.AcceptedRulesRoleId);
                IUser user = channel.GetUser(reaction.UserId);

                if (await _userService.GetById(user.Id) == null)
                {
                    await _userService.Register(user);
                }

            }
            else if (reaction.Emote.Name == _emoteSettings.PUGPingerEmoteName)
                await AddRoleToUser(channel, reaction, _roleSettings.PUGPingerRoleId);
            else if (reaction.Emote.Name == _emoteSettings.NAEmoteName)
                await AddRoleToUser(channel, reaction, _roleSettings.NARoleId);
            else if (reaction.Emote.Name == _emoteSettings.EUEmoteName)
                await AddRoleToUser(channel, reaction, _roleSettings.EURoleId);
        }

        async Task AddRoleToUser(SocketTextChannel channel, SocketReaction reaction, ulong roleId)
        {
            var roles = channel.Guild.Roles;
            foreach (SocketRole role in roles)
            {
                if (role.Id == roleId)
                {
                    try
                    {
                        await channel.GetUser(reaction.UserId).AddRoleAsync(role);
                    }
                    catch (System.Exception)
                    {
                    }
                    return;
                }
            }
        }

        async Task HandleGameReactionRemovedAsync(SocketTextChannel channel, SocketReaction reaction)
        {

            if (reaction.Emote.Name == _emoteSettings.RoCoGameEmoteName)
                await RemoveRoleFromUser(channel, reaction, _roleSettings.RoCoRoleId);
            else if (reaction.Emote.Name == _emoteSettings.CrossFireGameEmoteName)
                await RemoveRoleFromUser(channel, reaction, _roleSettings.CrossFireRoleId);
            else if (reaction.Emote.Name == _emoteSettings.IronSightGameEmoteName)
                await RemoveRoleFromUser(channel, reaction, _roleSettings.IronSightRoleId);
        }

        async Task HandleRoleReactionRemovedAsync(SocketTextChannel channel, SocketReaction reaction)
        {
            if (reaction.Emote.Name == _emoteSettings.FraggerZEmoteName)
                await RemoveRoleFromUser(channel, reaction, _roleSettings.AcceptedRulesRoleId);
            else if (reaction.Emote.Name == _emoteSettings.PUGPingerEmoteName)
                await RemoveRoleFromUser(channel, reaction, _roleSettings.PUGPingerRoleId);
            else if (reaction.Emote.Name == _emoteSettings.NAEmoteName)
                await RemoveRoleFromUser(channel, reaction, _roleSettings.NARoleId);
            else if (reaction.Emote.Name == _emoteSettings.EUEmoteName)
                await RemoveRoleFromUser(channel, reaction, _roleSettings.EURoleId);
        }

        async Task RemoveRoleFromUser(SocketTextChannel channel, SocketReaction reaction, ulong roleId)
        {
            var roles = channel.Guild.Roles;
            foreach (SocketRole role in roles)
            {
                if (role.Id == roleId)
                {
                    try
                    {
                        await channel.GetUser(reaction.UserId).RemoveRoleAsync(role);
                    }
                    catch (System.Exception)
                    {
                    }
                    return;
                }
            }
        }
    }
}
