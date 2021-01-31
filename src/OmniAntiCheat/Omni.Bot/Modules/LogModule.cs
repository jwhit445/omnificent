using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Omni.API;
using Core.Omni.API.Models;
using Discord;
using Discord.Commands;
using UserStatus = Core.Omni.API.Models.UserStatus;

namespace Omni.Bot.Modules {
	public class LogModule : ModuleBase<SocketCommandContext> {

		private const string PING_COMMAND = "ping";
		private const string LOGS_COMMAND = "logs";
		private const string STATUS_COMMAND = "status";

		private IOmniAPI _omniAPI { get; }

		public LogModule(IOmniAPI omniAPI) {
			_omniAPI = omniAPI;
		}

		[Command(PING_COMMAND)]
		[Summary("Checks whether the bot is working or not.")]
		public async Task Ping() {
			await ReplyAsync("Pong!");
		}

		[Command(STATUS_COMMAND)]
		[Summary("Retrieves statuses for the given list of users.")]
		public async Task GetStatusesForUsers([Remainder]string arguments) {
			List<string> listUsernames = arguments
				.Split("//")
				.Select(x => x.Trim())
				.Distinct()
				.ToList();
			GetUserStatusesResponse response;
			try {
				response = await _omniAPI.GetUserStatuses(new GetUserStatusesRequest {
					ListUsernames = listUsernames,
				});
			} 
			catch(Exception e) {
				await ReplyAsync($"There was an error getting the statuses: {e.Message}");
				return;
			}
			StringBuilder strBuilder = new StringBuilder();
			foreach(KeyValuePair<string, UserStatus> userKeyValuePair in response.UserStatuses.OrderBy(x => x.Key)) {
				if(strBuilder.Length > 0) {
					strBuilder.AppendLine();
				}
				strBuilder.AppendLine($"**{userKeyValuePair.Key}:**");
				DateTime easternHeartbeat = UTCToEastern(userKeyValuePair.Value.LastHeartbeat);
				string timezone = easternHeartbeat.IsDaylightSavingTime() ? "EDT" : "EST";
				string dateTimeFormat = "MM/dd/yyyy h:mm tt";
				strBuilder.AppendLine($"Last Heartbeat: **{easternHeartbeat.ToString(dateTimeFormat)} ({timezone})**");
				strBuilder.AppendLine($"Is Moss Active: **{userKeyValuePair.Value.IsMossRunning}**");
				strBuilder.AppendLine($"Is Rogue Company Active: **{userKeyValuePair.Value.IsRogueCompanyRunning}**");
			}
			EmbedBuilder embed = new EmbedBuilder();
			string title = "Statuses";
			if(response.UserStatuses.Count != listUsernames.Count) {
				title = $"Unable to find user(s): {string.Join(",", listUsernames.Except(response.UserStatuses.Select(x => x.Key)))}";
			}
			embed.AddField(title, strBuilder.Length > 0 ? strBuilder.ToString() : "N/A");
			await ReplyAsync("", embed: embed.Build());
		}

		[Command(LOGS_COMMAND)]
		[Summary("Retrieves links to the last 5 logs for the given list of users.")]
		public async Task GetLogsForUsers([Remainder]string arguments) {
			List<string> listUsernames = arguments
				.Split("//")
				.Select(x => x.Trim())
				.Distinct()
				.ToList();
			GetLogsForUserResponse response;
			try {
				response = await _omniAPI.GetLogsForUsers(new GetLogsForUserRequest {
					ListUsername = listUsernames,
				});
			} 
			catch(Exception e) {
				await ReplyAsync($"There was an error getting the logs: {e.Message}");
				return;
			}
			StringBuilder strBuilder = new StringBuilder();
			foreach(KeyValuePair<string, List<LogEvent>> userKeyValuePair in response.RecentUserEvents.OrderBy(x => x.Key)) {
				if(strBuilder.Length > 0) {
					strBuilder.AppendLine();
				}
				strBuilder.AppendLine($"**{userKeyValuePair.Key}:**");
				foreach(LogEvent logEvent in userKeyValuePair.Value.OrderByDescending(x => x.StartDateTime)) {
					DateTime easternStartTime = UTCToEastern(logEvent.StartDateTime);
					DateTime easternEndTime = UTCToEastern(logEvent.EndDateTime);
					string timezone = easternStartTime.IsDaylightSavingTime() ? "EDT" : "EST";
					string dateTimeFormat = "MM/dd/yyyy h:mm tt";
					strBuilder.AppendLine($"{easternStartTime.ToString(dateTimeFormat)} - {(easternStartTime.Date != easternEndTime.Date ? easternEndTime.ToString(dateTimeFormat) : easternEndTime.ToString("h:mm tt"))} ({timezone})" +
						$": [Download]({logEvent.DownloadLink})");
				}
			}
			EmbedBuilder embed = new EmbedBuilder();
			string title = "Logs";
			if(response.RecentUserEvents.Count != listUsernames.Count) {
				title = $"Unable to find user(s): {string.Join(",", listUsernames.Except(response.RecentUserEvents.Select(x => x.Key)))}";
			}
			embed.AddField(title, strBuilder.Length > 0 ? strBuilder.ToString() : "N/A");
			await ReplyAsync("", embed: embed.Build());
		}

		private DateTime UTCToEastern(DateTime utc) {
			bool isDaylightSavings = DateTime.Now.IsDaylightSavingTime();
			TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(isDaylightSavings ? "Eastern Daylight Time" : "Eastern Standard Time");
			DateTime eastern = TimeZoneInfo.ConvertTimeFromUtc(utc, timeZone);
			return eastern;
		}
	}

}
