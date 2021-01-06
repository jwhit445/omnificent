using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace Omni.Bot.Modules {
	public class LogModule : ModuleBase<SocketCommandContext> {

		private const string PING_COMMAND = "ping";
		private const string LOGS_COMMAND = "logs";

		public LogModule() {

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
			StringBuilder strBuilder = new StringBuilder();
			strBuilder.AppendLine("The selected users were:");
			foreach(string username in listUsernames) {
				strBuilder.AppendLine(username);
			}
			await ReplyAsync(strBuilder.ToString());
		}
	}
}
