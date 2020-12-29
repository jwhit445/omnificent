using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Reactions;
using DiscordBot.Services;
using DiscordBot.Settings;
using DiscordBot.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DiscordBot
{
    public class Program
    {

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var config = hostContext.Configuration;

                    services
                        .Configure<RoleSettings>(config.GetSection(nameof(RoleSettings)))
                        .Configure<ChannelSettings>(config.GetSection(nameof(ChannelSettings)))
                        .Configure<EmoteSettings>(config.GetSection(nameof(EmoteSettings)))
                        .Configure<APISettings>(config.GetSection(nameof(APISettings)))
                        .Configure<BotSettings>(config.GetSection(nameof(BotSettings)));

                    services
                        .AddSingleton<EmbedService>()
                        .AddSingleton<MatchService>()
                        .AddSingleton<UserService>()
                        .AddSingleton<TeamService>()
                        .AddSingleton<DiscordSocketClient>()
                        .AddSingleton<BaseSocketClient, DiscordSocketClient>(sp =>
                        {
                            return sp.GetRequiredService<DiscordSocketClient>();
                        })
                        .AddSingleton<LoggingService>()
                        .AddSingleton<CommandHandler>()
                        .AddSingleton<RoCoPugService>()
                        .AddSingleton<RogueCompanyReactions>()
                        .AddSingleton<RoleAssignmentReactions>()
                        .AddSingleton<ReportReactions>()
                        .AddSingleton<TeamReactions>()
                        .AddSingleton<MatchReactions>()
                        .AddSingleton<ReactionRolesModule>()
                        .AddSingleton<MatchModule>()
                        .AddSingleton<PugsModule>()
                        .AddSingleton<StatsModule>()
                        .AddSingleton<TeamsModule>()
                        .AddSingleton(_ => new CommandService(new CommandServiceConfig() {
                            DefaultRunMode = RunMode.Async,
                        }))
                        .AddHostedService<Worker>();
                });
    }
}