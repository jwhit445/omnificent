using Discord;
using DiscordBot.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscordBot.Services {
    public interface IUserService {
        string GetTier(double value);
        string GetTier(User user);
        Task Register(IUser user);
        bool IsUserPremium(IGuildUser user);
        Task Update(User user);
        Task<StatSummary> GetStatSummary(User user);
        Task<Stats> GetStats(User user);
        Task<User> GetById(ulong userId);
        Task<List<User>> GetAll();
        Task<List<User>> GetLeaderboard();
    }
}