using Discord;
using DiscordBot.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscordBot.Services {
    public interface IMatchService {
        public Task ReportWin(Match match);
        public Task UpdateMatchLog(Match reportedMatch, ITextChannel anyTextChannel = null);
        public Task SendMatchLog(Match match);
        public Task GenerateScrim(string gameName, ITextChannel channel, Team team1, Team team2);
        public Task GeneratePUG(string gameName, PlayerQueue queue);
        public Task<List<User>> SetupAutomaticTeams(Match match, string gameName, PlayerQueue queue);
        public Task<Match> SendToAPI(Match match);
        public Task Update(Match match);
        public Task<Match> Get(string matchId);
        public Task<Match> GetByNumber(int matchNumber);
    }
}