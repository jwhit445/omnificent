using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Omni.Bot.Services;
using Omni.Bot.Settings;

namespace Omni.Bot.Worker {
	public class MasterWorker : BackgroundService {
		
		private BotSettings _botSettings { get; }
		private DiscordSocketClient _discordClient { get; }
		private CommandService _commandService { get; }
		private IServiceProvider _serviceProvider { get; }
		private LoggingService _loggingService { get; }

		public MasterWorker(IOptions<BotSettings> botSettings, DiscordSocketClient discordClient, CommandService commandService, IServiceProvider serviceProvider, 
			LoggingService loggingService) 
		{
			_botSettings = botSettings.Value;
			_discordClient = discordClient;
			_commandService = commandService;
			_serviceProvider = serviceProvider;
			_loggingService = loggingService;
		}

		public override async Task StartAsync(CancellationToken cancellationToken) {
			await base.StartAsync(cancellationToken);
			_loggingService.Init();
			_discordClient.MessageReceived += OnMessageReceived;
			await _discordClient.LoginAsync(TokenType.Bot, _botSettings.Token);
			await _discordClient.StartAsync();
			await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
			await ExUtils.SwallowAnyExceptionAsync(async () => {
				await Task.Delay(Timeout.Infinite, stoppingToken);
			});
		}

		public override async Task StopAsync(CancellationToken cancellationToken) {
			await base.StopAsync(cancellationToken);
		}

		private async Task OnMessageReceived(SocketMessage arg) {
			if(!(arg is SocketUserMessage message)) {
				return;
			}
			int argPos = 0;
			// Determine if the message is a command based on the prefix and make sure no bots trigger commands
			if(!(message.HasCharPrefix(_botSettings.Prefix, ref argPos) || message.HasMentionPrefix(_discordClient.CurrentUser, ref argPos))
				|| message.Author.IsBot) 
			{
				return;
			}
			SocketCommandContext context = new SocketCommandContext(_discordClient, message);
			await _commandService.ExecuteAsync(context: context, argPos: argPos, services: _serviceProvider);
		}
	}
}
