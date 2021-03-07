using Discord;
using DiscordBot.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscordBot.Services {
    public interface IEmbedService {
        Task SendTeamScrambleEmbed(Match match, ITextChannel matchTextChannel);
        Task SendBanEmbed(Match match, IGuildUser user, ITextChannel channel, string rogue);
        Task SendPlayersNeedReadyUpMessage(Match match, ITextChannel anySTChannel);
        Task SendAllPlayersReadyEmbed(Match match, ITextChannel anyChannel);
        Embed GetSendPickBansEmbed(IMentionable team1BanUser, IMentionable team2BanUser);
        Task SendScrimChallenge(Team team1, Team team2, ITextChannel channel);
        Task<Embed> Leaderboard(string gameName, ITextChannel channel);
        Task<Embed> Match(Match match, Team team1 = null, Team team2 = null, List<User> listAllUsers = null);
        Task<Embed> PickingMatchEmbed(Match match);
        Embed TeamInvite(Team team);
        Embed StartDuoMessage(string queueName);
        Embed InviteDuo(string queueName, IUser inviter);
        Embed DuoQueueJoined(string channelName, IUser partner);
        Task<Embed> Team(Team team);
        Task<Embed> User(User user, string gameName);
        Task UpdateQueueEmbed(PlayerQueue queue, ITextChannel channel);
    }
}