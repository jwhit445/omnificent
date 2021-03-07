using Discord;
using DiscordBot.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscordBot.Services {
    public interface IRoCoPugService {
        Dictionary<ulong, (PlayerQueue queue, QueueType queueType)> DuoInviteStarted { get; }
        Dictionary<ulong, (PlayerQueue queue, ulong player2)> DuoPartners { get; }
        PlayerQueue NAMainQueue { get; set; }
        Dictionary<ulong, PlayerQueue> DictQueueForChannel { get; set; }

        Task Init(IDiscordClient client);
        Task Join(string region, IUser socketGuildUser, ITextChannel socketTextChannel);
        Task StartDuoQueueAttempt(QueueType queueType, IUser user);
        Task InviteDuoPartner(IUser requestingUser, IUser invitedUser);
        Task FinalizeDuoPartners(ulong player1);
        Task Leave(string region, IUser socketGuildUser, ITextChannel socketTextChannel);
        Task LeaveDuo(string region, IUser socketGuildUser, ITextChannel socketTextChannel);
    }
}