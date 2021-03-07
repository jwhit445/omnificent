using DiscordBot.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    public static class MapRecordService
    {
        public async static Task<List<Stats>> GetListFromTeam(Team team)
        {
            // MOCK RETURN VAL - Ping API to retrieve a list of map records based on the MapRecordIds
            return null;
        }

        public async static Task<List<Stats>> GetListFromUser(User user, string gameName)
        {
            // MOCK RETURN VAL - Ping API to retrieve a list of map records based on the user.Id and gameName filter
            return null;
        }
    }
}
