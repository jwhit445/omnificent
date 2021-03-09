using Core.Omni.API;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Omni.Bot.Modules;
using Omni.Bot.Services;
using Omni.Bot.Settings;
using Omni.Bot.Worker;

namespace Omni.Bot {
    public class Program {

        public static void Main(string[] args) {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) {
            return Host
                .CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) => {
                    IConfiguration config = hostContext.Configuration;
                    services
                        .Configure<BotSettings>(config.GetSection(nameof(BotSettings)));
                    services
                        .AddSingleton<IOmniAPI, OmniAPI>()
                        .AddSingleton<LogModule>()
                        .AddSingleton<LoggingService>()
                        .AddSingleton(serviceProvider => {
                            return new CommandService(new CommandServiceConfig {
                                LogLevel = LogSeverity.Info,
                                CaseSensitiveCommands = false,
                            });
                        })
                        .AddSingleton<DiscordSocketClient>()
                        .AddSingleton<BaseSocketClient, DiscordSocketClient>(sp => {
                            return sp.GetRequiredService<DiscordSocketClient>();
                        })
                        .AddSingleton(_ => new CommandService(new CommandServiceConfig() {
                            DefaultRunMode = RunMode.Async,
                        }))
                        .AddHostedService<MasterWorker>();
                });
        }

    }
}
