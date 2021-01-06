using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Omni.Bot.Services {
    public class LoggingService {

        private BaseSocketClient _client { get; }
        private CommandService _commandService { get; }

        public LoggingService(BaseSocketClient client, CommandService command) {
            _client = client;
            _commandService = command;
        }

        public void Init() {
            _client.Log += LogAsync;
            _commandService.Log += LogAsync;
        }

        private Task LogAsync(LogMessage message) {
            switch(message.Severity) {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }
            if(message.Exception is CommandException cmdException) {
                Console.WriteLine($"[Command/{message.Severity}] {cmdException.Command.Aliases.FirstOrDefault()}"
                    + $" failed to execute in {cmdException.Context.Channel}.");
                Console.WriteLine(cmdException);
            }
            else {
                Console.WriteLine($"[General/{message.Severity}] {message}");
            }
            Console.ResetColor();
            return Task.CompletedTask;
        }
    }
}
