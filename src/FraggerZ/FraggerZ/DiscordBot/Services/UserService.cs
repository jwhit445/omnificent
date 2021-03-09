using Discord;
using Discord.WebSocket;
using DiscordBot.Models;
using DiscordBot.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DiscordBot.Services {
    public class UserService : IUserService {
        public HttpClient httpClient;
        private readonly APISettings _apiSettings;
        private readonly RoleSettings _roleSettings;

        public UserService(IOptions<APISettings> apiSettings, IOptions<RoleSettings> roleSettings) {
            _apiSettings = apiSettings.Value;
            httpClient = new HttpClient();
            _roleSettings = roleSettings.Value;
        }

        public string GetTier(double value) {
            if (value >= 7000)
                return "S";
            else if (value >= 5500)
                return "G";
            else if (value >= 5000) return "A+";
            else if (value >= 4500) return "A";
            else if (value >= 4000) return "A-";
            else if (value >= 3500) return "B+";
            else if (value >= 3000) return "B";
            else if (value >= 2500) return "B-";
            else if (value >= 2000) return "C+";
            else if (value >= 1500) return "C";
            else if (value >= 1000) return "C-";
            else if (value >= 500) return "D+";
            else if (value >= 250) return "D";
            else if (value >= 100) return "D-";
            else if (value >= 0) return "F";
            else return "dev is best";
        }

        public string GetTier(User user) {
            if (user.PlacementMatchIds.Count < 10) return "U";
            return GetTier(user.RoCoMMR * 100);
        }

        /// <summary>
        /// Called automatically when a discord user clicks accept rules.
        /// </summary>
        /// <param name="iUser"></param>
        public async Task Register(IUser user) {
            if (user == null) {
                return;
            }
            // Ping the API to create a user;
            User dbUser = new User(user.Username, user.Id);
            var JSON = JsonConvert.SerializeObject(dbUser);
            var response = await httpClient.PostAsync(_apiSettings.BaseURL + $"/user", new StringContent(JSON, System.Text.Encoding.UTF8));
        }

        public bool IsUserPremium(IGuildUser user) {
            if (user == null) {
                return false;
            }
            bool foundRole = false;
            foreach (ulong roleId in user.RoleIds) {
                if (roleId == _roleSettings.PremiumRoleId) { 
                    foundRole = true; 
                    break;
                }
            }

            return foundRole;
        }

        public async Task Update(User user) {
            var JSON = JsonConvert.SerializeObject(user);
            var response = await httpClient.PutAsync(_apiSettings.BaseURL + $"/user/{user.DiscordId}", new StringContent(JSON, System.Text.Encoding.UTF8));
        }

        public async Task<StatSummary> GetStatSummary(User user) {
            var response = await httpClient.GetAsync(_apiSettings.BaseURL + $"/user/{user.Id}/stats");
            StatSummary stats = null;
            if (response == null || response.StatusCode != System.Net.HttpStatusCode.OK) {
                throw new Exception("Couldn't get StatSummary");
            }
            var jsonString = await response.Content.ReadAsStringAsync();
            try { stats = JsonConvert.DeserializeObject<StatSummary>(jsonString); }
            catch (Exception e) { Console.WriteLine(e.Message); }
            return stats;
        }

        public async Task<Stats> GetStats(User user) {
            ///user/{id}/stats
            var response = await httpClient.GetAsync(_apiSettings.BaseURL + $"/user/{user.Id}/stats");
            Stats stats = null;
            if (response != null) {
                var jsonString = await response.Content.ReadAsStringAsync();
                try { stats = JsonConvert.DeserializeObject<Stats>(jsonString); }
                catch (Exception e) { Console.WriteLine(e.Message); }
            }
            return stats;
        }

        public async Task<User> GetById(ulong userId) {
            var response = await httpClient.GetAsync(_apiSettings.BaseURL + $"/user/{userId}");
            User user = null;
            if (response != null) {
                var jsonString = await response.Content.ReadAsStringAsync();
                try { user = JsonConvert.DeserializeObject<User>(jsonString); }
                catch (Exception e) { Console.WriteLine(e.Message); }
            }
            return user;
        }

        public async Task<List<User>> GetAll() {
            var response = await httpClient.GetAsync(_apiSettings.BaseURL + $"/user");
            List<User> listUsers = null;
            if (response != null) {
                var jsonString = await response.Content.ReadAsStringAsync();
                try { listUsers = JsonConvert.DeserializeObject<List<User>>(jsonString); }
                catch (Exception e) { Console.WriteLine(e.Message); }
            }
            return listUsers;
        }

        public async Task<List<User>> GetLeaderboard() {
            var response = await httpClient.GetAsync(_apiSettings.BaseURL + $"/user/leaderboard");
            List<User> listUsers = null;
            if (response != null) {
                var jsonString = await response.Content.ReadAsStringAsync();
                try { listUsers = JsonConvert.DeserializeObject<List<User>>(jsonString); }
                catch (Exception e) { Console.WriteLine(e.Message); }
            }
            return listUsers;
        }
    }
}
