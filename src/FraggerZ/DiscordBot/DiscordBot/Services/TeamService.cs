using DiscordBot.Models;
using DiscordBot.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    public class TeamService
    {
        public List<Scrim> CurrentScrims { get; set; }

        public HttpClient httpClient;
        private readonly APISettings _apiSettings;

        public TeamService(IOptions<APISettings> apiSettings)
        {
            httpClient = new HttpClient();
            _apiSettings = apiSettings.Value;
            CurrentScrims = new List<Scrim>();
        }

        public async Task Register(Team team)
        {
            var JSON = JsonConvert.SerializeObject(team);
            var response = await httpClient.PostAsync(_apiSettings.BaseURL + "/team", new StringContent(JSON, System.Text.Encoding.UTF8));
            Console.WriteLine("Attempt to save team to DB - Status: " + response.StatusCode);
        }

        public async Task Update(Team team)
        {
            var JSON = JsonConvert.SerializeObject(team);
            var response = await httpClient.PutAsync(_apiSettings.BaseURL + $"/team/{team.Id}", new StringContent(JSON, System.Text.Encoding.UTF8));
            Console.WriteLine($"Attempt to update team: {team.TeamName} in DB - Status: " + response.StatusCode);
        }

        public async Task<StatSummary> GetStatSummary(string id)
        {
            var response = await httpClient.GetAsync(_apiSettings.BaseURL + $"/team/{id}/stats");
            StatSummary stat = null;
            if (response != null)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                try { stat = JsonConvert.DeserializeObject<StatSummary>(jsonString); }
                catch (Exception e) { Console.WriteLine(e.Message); }
            }
            Console.WriteLine("Attempt to get team stats summary from API - Status: " + response.StatusCode);
            return stat;
        }

        public async Task<Team> Get(string id)
        {
            var response = await httpClient.GetAsync(_apiSettings.BaseURL + $"/team/{id}");
            Team team = null;
            if (response != null)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                try { team = JsonConvert.DeserializeObject<Team>(jsonString); }
                catch (Exception e) { Console.WriteLine(e.Message); }
            }
            Console.WriteLine("Attempt to get team from DB - Status: " + response.StatusCode);
            return team;
        }

        public async Task<Scrim> GetScrimByMessageId(ulong messageId)
        {
            // USES THE CACHE UNTIL API CAN BE ACCESSED
            foreach(Scrim scrimCurr in CurrentScrims)
            {
                if (scrimCurr.ChallengeMessageId == messageId) return scrimCurr;
            }
            return null;
        }

        /* COMMENTED OUT UNTIL MAP STATS ARE DONE 
        public async Task<Stats> GetStats(Team team)
        {
            ///user/{id}/stats
            var response = await httpClient.GetAsync(_apiSettings.BaseURL + $"/team/{team.Id}/stats");
            Stats stats = null;
            if (response != null)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                try { stats = JsonConvert.DeserializeObject<Stats>(jsonString); }
                catch (Exception e) { Console.WriteLine(e.Message); }
            }
            Console.WriteLine("Attempt to get team stats from API - Status: " + response.StatusCode);
            return stats;
        }
        */

        public async Task RemoveCurrentScrim(Scrim scrim)
        {
            // USES THE CACHE UNTIL API CAN BE ACCESSED
            int removeIndex = -1;
            for(int i = 0; i < CurrentScrims.Count; i++)
            {
                if (CurrentScrims[i].ChallengeMessageId == scrim.ChallengeMessageId) removeIndex = i;
            }
            if (removeIndex >= 0) CurrentScrims.RemoveAt(removeIndex);
        }

        public async Task<bool> ScrimExists(Team team1, Team team2)
        {
            // USES THE CACHE UNTIL API CAN BE ACCESSED
            foreach(Scrim scrimCurr in CurrentScrims)
            {
                if (scrimCurr.Team1Id == team1.Id && scrimCurr.Team2Id == team2.Id) return true;
            }
            return false;
        }

        public async Task AddScrim(Scrim scrim)
        {
            // USES THE CACHE UNTIL API CAN BE ACCESSED
            CurrentScrims.Add(scrim);
        }

        public async Task<Scrim> GetScrim(ulong messageId)
        {
            foreach(Scrim scrim in CurrentScrims)
                if (scrim.ChallengeMessageId == messageId) return scrim;
            return null;
        }
    }
}
