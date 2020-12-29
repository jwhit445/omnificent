using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Reactions;
using DiscordBot.Services;
using DiscordBot.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordBot.Workers
{
    public class Worker : BackgroundService
    {

        private BaseSocketClient _client;
        private readonly RoleAssignmentReactions _roleAssignmentReactions;
        private readonly RogueCompanyReactions _rogueCompanyReactions;
        private readonly CommandHandler _commandHandler;
        private RoleSettings _roleSettings;
        private readonly EmoteSettings _emoteSettings;
        private readonly BotSettings _botSettings;
        private readonly ChannelSettings _channelSettings;
        private readonly TeamReactions _teamReactions;
        private readonly ReportReactions _reportReactions;
        private readonly MatchReactions _matchReactions;
        private readonly UserService _userService;


        public Worker(IOptions<RoleSettings> roleSettings, IOptions<BotSettings> botSettings,
            IOptions<ChannelSettings> channelSettings, BaseSocketClient client, CommandHandler commandHandler
            , RoleAssignmentReactions roleAssignmentReactions, RogueCompanyReactions rogueCompanyReactions,
            IOptions<EmoteSettings> emoteSettings, TeamReactions teamReactions, ReportReactions reportReactions,
            UserService userService, MatchReactions matchReactions)
        {
            _roleSettings = roleSettings.Value;
            _botSettings = botSettings.Value;
            _channelSettings = channelSettings.Value;
            _emoteSettings = emoteSettings.Value;
            _client = client;
            _roleAssignmentReactions = roleAssignmentReactions;
            _rogueCompanyReactions = rogueCompanyReactions;
            _reportReactions = reportReactions;
            _teamReactions = teamReactions;
            _commandHandler = commandHandler;
            _userService = userService;
            _matchReactions = matchReactions;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _client.ReactionAdded += HandleReactionAddedAsync;
            _client.ReactionRemoved += HandleReactionRemovedAsync;
            _client.UserJoined += HandleUserJoinedAsync;

            await _client.LoginAsync(TokenType.Bot, _botSettings.Token);
            await _client.StartAsync();
            await _commandHandler.Init();
            await base.StartAsync(cancellationToken);
        }

        private async Task HandleUserJoinedAsync(SocketGuildUser arg)
        {
            IUser user = arg;

            if (await _userService.GetById(user.Id) == null)
            {
                await _userService.Register(user);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task HandleReactionAddedAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.UserId == _botSettings.Id) return;

            if(channel is SocketDMChannel dmChannel)
            {
                await _matchReactions.HandleDMReactionAddedAsync(message, dmChannel, reaction);
            }
            else if (channel is SocketTextChannel socketTextChannel)
            {
                /* COMMENTED OUT AS MEMBERS ARE SCREENED NOW THROUGH DISCORD
                 * 
                if (socketTextChannel.Id == _channelSettings.RegisterChannelId)
                {
                    await _roleAssignmentReactions.HandleReactionAddedAsync(message, socketTextChannel, reaction);
                    return;
                }
                */

                if(reaction.Emote.Name == _emoteSettings.FireEmoteName)
                {
                    if(channel is SocketTextChannel stChannel)
                        await _matchReactions.HandleReactionAddedAsync(message, stChannel, reaction);
                }

                if(reaction.Emote.Name == _emoteSettings.OneEmoteName || reaction.Emote.Name == _emoteSettings.TwoEmoteName
                    || reaction.Emote.Name == _emoteSettings.XEmoteName)
                {
                    // reporting a match
                    await _reportReactions.HandleReactionAddedAsync(message, socketTextChannel, reaction);
                }

                if(reaction.Emote.Name == _emoteSettings.PlayEmoteName)
                {
                    // this is a scrim accept
                    await _teamReactions.HandleReactionAddedAsync(message, socketTextChannel, reaction);
                }

                if (socketTextChannel.CategoryId == _channelSettings.RoleAssignmentCategoryId)
                    await _roleAssignmentReactions.HandleReactionAddedAsync(message, socketTextChannel, reaction);

                else if (socketTextChannel.CategoryId == _channelSettings.RoCoCategoryId)
                    await _rogueCompanyReactions.HandleReactionAddedAsync(message, socketTextChannel, reaction);
            }
        }

        private async Task HandleReactionRemovedAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            
            if (reaction.UserId == _botSettings.Id) return;

            if (channel is SocketTextChannel socketTextChannel)
            {
                if (socketTextChannel.Id == _channelSettings.RegisterChannelId)
                {
                    await _roleAssignmentReactions.HandleReactionRemovedAsync(message, socketTextChannel, reaction);
                    return;
                }

                if (reaction.Emote.Name == _emoteSettings.OneEmoteName || reaction.Emote.Name == _emoteSettings.TwoEmoteName)
                {
                    // reporting a match reversal
                    await _reportReactions.HandleReactionRemovedAsync(message, socketTextChannel, reaction);
                }

                if (socketTextChannel.CategoryId == _channelSettings.RoleAssignmentCategoryId)
                    await _roleAssignmentReactions.HandleReactionRemovedAsync(message, socketTextChannel, reaction);
                else if (socketTextChannel.CategoryId == _channelSettings.RoCoCategoryId)
                    await _rogueCompanyReactions.HandleReactionRemovedAsync(message, socketTextChannel, reaction);
            }
        }
    }
    public class CommandHandler
    {
        private readonly BotSettings _botSettings;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly LoggingService _logging;
        private readonly IServiceProvider _sp;
        private readonly UserService _userService;
        private readonly ChannelSettings _channelSettings;
        private readonly EmoteSettings _emoteSettings;
        private readonly RoCoPugService _rocoService;

        public CommandHandler(IOptions<BotSettings> botSettings, 
            IServiceProvider sp, DiscordSocketClient client, 
            CommandService commandService, LoggingService loggingService, UserService userService, RoCoPugService rocoService
            , IOptions<ChannelSettings> channelSettings, IOptions<EmoteSettings> emoteSettings)
        {
            _botSettings = botSettings.Value;
            _client = client;
            _commands = commandService;
            _logging = loggingService;
            _sp = sp;
            _userService = userService;
            _channelSettings = channelSettings.Value;
            _emoteSettings = emoteSettings.Value;
            _rocoService = rocoService;
        }

        public async Task Init()
        {
            _client.MessageReceived += HandleCommandAsync;
            _client.Ready += RunOneTimeDiscordCommands;
            await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: _sp);
        }

        protected async Task RunOneTimeDiscordCommands()
        {
            //Get Register channel
            try
            {
                _rocoService.Init(_client);
                var channel = _client.GetChannel(_channelSettings.RegisterChannelId);
                if (channel is SocketTextChannel socketTextChannel)
                {
                    var lastMessage = (await socketTextChannel.GetMessagesAsync(1).FlattenAsync()).FirstOrDefault();
                    if (lastMessage != null)
                    {
                        //Get all users that reacted to first post
                        var usersWhoReacted = await lastMessage.GetReactionUsersAsync(new Emoji(_emoteSettings.FraggerZEmoteUnicode), 1000).FlattenAsync();
                        //Get all users from db
                        var usersFromDb = await _userService.GetAll();
                        HashSet<ulong> usersDbLookup = new HashSet<ulong>(usersFromDb.Select(x => x.DiscordId));
                        //Register every user that reacted to the post but is not in the database
                        foreach (var user in usersWhoReacted)
                        {
                            if (!usersDbLookup.Contains(user.Id))
                            {
                                await _userService.Register(user);
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
            //Get users that are not in db, but have reacted to post, and register them
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a system message
            if (!(messageParam is SocketUserMessage message)) return;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!(message.HasCharPrefix(_botSettings.Prefix, ref argPos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            var context = new SocketCommandContext(_client, message);

            // we dont do anything with the result, so we dont need to get it
            await _commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: _sp);

        }
    }

    public class LoggingService
    {
        public LoggingService(BaseSocketClient client, CommandService command)
        {
            client.Log += LogAsync;
            command.Log += LogAsync;
        }

        private Task LogAsync(LogMessage message)
        {
            if (message.Exception is CommandException cmdException)
            {
                Console.WriteLine($"[Command/{message.Severity}] {cmdException.Command.Aliases.First()}"
                    + $" failed to execute in {cmdException.Context.Channel}.");
                Console.WriteLine(cmdException);
            }
            else
                Console.WriteLine($"[General/{message.Severity}] {message}");

            return Task.CompletedTask;
        }
    }
}
