using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Omni.API;
using Core.Omni.API.Models;
using Discord;
using Discord.Commands;

namespace Omni.Bot.Modules {
	public class LogModule : ModuleBase<SocketCommandContext> {

		private const string PING_COMMAND = "ping";
		private const string LOGS_COMMAND = "logs";

		private IOmniAPI _omniAPI { get; }

		public LogModule(IOmniAPI omniAPI) {
			_omniAPI = omniAPI;
		}

		[Command(PING_COMMAND)]
		[Summary("Checks whether the bot is working or not.")]
		public async Task Ping() {
			await ReplyAsync("Pong!");
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
			/*response = new GetLogsForUserResponse { RecentUserEvents = new Dictionary<string, List<LogEvent>> { 
				{ "Luke", 
					new List<LogEvent> { 
					new LogEvent { 
						StartDateTime = DateTime.UtcNow,
						EndDateTime = DateTime.UtcNow.AddMinutes(20),
						DownloadLink = "https://google.com"
					},
					new LogEvent {
						StartDateTime = DateTime.UtcNow.AddDays(1),
						EndDateTime = DateTime.UtcNow.AddDays(1).AddMinutes(30),
						DownloadLink = "https://google.com"
					}
				} } ,
				{ "Josh",
					new List<LogEvent> {
					new LogEvent {
						StartDateTime = DateTime.UtcNow,
						EndDateTime = DateTime.UtcNow.AddMinutes(20),
						DownloadLink = "https://google.com"
					},
					new LogEvent {
						StartDateTime = DateTime.UtcNow.AddDays(1),
						EndDateTime = DateTime.UtcNow.AddDays(1).AddMinutes(30),
						DownloadLink = "https://google.com"
					}
				} } ,
			} 
			};*/
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
