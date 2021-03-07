using DiscordBot.Models;
using System.Threading.Tasks;

namespace DiscordBot.Services {
    public interface ITeamService {
        Task Register(Team team);
        Task Update(Team team);
        Task<StatSummary> GetStatSummary(string id);
        Task<Team> Get(string id);
        Task<Scrim> GetScrimByMessageId(ulong messageId);
        Task RemoveCurrentScrim(Scrim scrim);
        Task<bool> ScrimExists(Team team1, Team team2);
        Task AddScrim(Scrim scrim);
        Task<Scrim> GetScrim(ulong messageId);
    }
}