using Discord.Commands;
using System.Threading.Tasks;
using DiscordBot.Services;
using Microsoft.Extensions.Options;
using DiscordBot.Settings;
using DiscordBot.Models;
using System;
using DiscordBot.Util;
using Discord.WebSocket;
using Discord;
using System.Collections.Generic;

public class StatsModule : ModuleBase<SocketCommandContext>
{
	private readonly EmbedService _embedService;
	private readonly ChannelSettings _channelSettings;
	private readonly UserService _userService;

	public StatsModule(UserService userService, IOptions<ChannelSettings> channelSettings, EmbedService embedService)
	{
		_userService = userService;
		_embedService = embedService;
		_channelSettings = channelSettings.Value;
	}


	[Command("stream")]
	[Summary("Set your stream URL.")]
	public async Task SetSteamURL(string url)
	{
		if(Context.Channel.Id != _channelSettings.RoCoStatsChannelId)
		{
			var message = await ReplyAsync($"Please use this command in <#{_channelSettings.RoCoStatsChannelId}>");
			await message.DeleteAfter(5000);
			return;
		}

		// https://twitch.tv/connorwrightkappa -> twitch.tv as authority
		// https://youtube.com/pewdiepie (idk) -> youtube.com as authority
		var enteredUri = new Uri(url);
		var uriAuthority = enteredUri.Authority;

		if (uriAuthority != "youtube.com" && uriAuthority != "twitch.tv")
		{
			await Context.Message.DeleteAsync();
			var deleteMe = await ReplyAsync($"{Context.User.Mention}, we only allow youtube.com and twitch.tv URLs at the moment");

			await deleteMe.DeleteAfter(10000);
			return;
		}
		try
		{
			User user = await _userService.GetById(Context.User.Id);
			if(!Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute)) {
				throw new ArgumentException("Invalid stream url");
            }

			user.StreamURL = url;
			await _userService.Update(user);
			await ReplyAsync($"Your stream url has been set to: `{ url }`");
		}
		catch(ArgumentException ex)
		{
			await ReplyAsync(ex.Message);
		}
		catch
		{
			await ReplyAsync($"Unable to find your user. Try re-registering in #register and try again.");
		}
	}

	[Command("ign")]
	[Summary("Set your in game name.")]
	public async Task SetIGN([Remainder] string ign)
	{
		if (Context.Channel.Id != _channelSettings.RoCoStatsChannelId)
		{
			var message = await ReplyAsync($"Please use this command in <#{_channelSettings.RoCoStatsChannelId}>");
			await message.DeleteAfter(5000);
			return;
		}

		if (ign.Length > 18)
		{
			await Context.Message.DeleteAsync();
			var message = await ReplyAsync($"That IGN is too long.");
			await message.DeleteAfter(10000);

			return;
		}
		try
		{
			User user = await _userService.GetById(Context.User.Id);
			user.IGN = ign;
			await _userService.Update(user);
			await ReplyAsync($"Your ign has been set to: `{ ign }`");
		}
		catch
		{
			await ReplyAsync($"Unable to find your user. Try re-registering in #register and try again.");
		}
	}

	[Command("resetstats")]
	[Summary("resets user stats back to default.")]
	public async Task ResetStats()
	{
		if (Context.Channel.Id != _channelSettings.RoCoStatsChannelId) {
			var message = await ReplyAsync($"Please use this command in <#{_channelSettings.RoCoStatsChannelId}>");
			await message.DeleteAfter(5000);
			return;
		}

		if(!_userService.IsUserPremium(Context.User as SocketGuildUser))
		{
			EmbedBuilder builder = new EmbedBuilder() { Color = Color.Green, Description = "This feature is only available to Premium users." };
			var errorMessage = await Context.Channel.SendMessageAsync(null, false, builder.Build());
			await errorMessage.DeleteAfter(5000);
			return;
		}

		var user = await _userService.GetById(Context.User.Id);
		if(user.DateTimeLastStatReset.Year == DateTime.Now.Year && user.DateTimeLastStatReset.Month == DateTime.Now.Month) {
			EmbedBuilder builder = new EmbedBuilder() { Color = Color.Green, Description = "You've already used your stat reset this month." };
			var errorMessage = await Context.Channel.SendMessageAsync(null, false, builder.Build());
			await errorMessage.DeleteAfter(5000);
			return;
        }
        try {
            user.DateTimeLastStatReset = DateTime.Now;
            user.RoCoMMR = 25;
            user.RoCoSigma = 8.333;
            user.PlacementMatchIds = new List<string>();
            await _userService.Update(user);
			EmbedBuilder builder = new EmbedBuilder() { Color = Color.Green, Description = "Your stats and rank have been reset. You can't reset again until next month." };
			var errorMessage = await Context.Channel.SendMessageAsync(null, false, builder.Build());
		}
        catch (Exception e)
		{
			EmbedBuilder builder = new EmbedBuilder() { Color = Color.Green, Description = "Couldn't reset your stats: " + e.Message };
			await Context.Channel.SendMessageAsync(null, false, builder.Build());
			return;
		}
	}

	[Command("stats")]
	[Alias(new string[2] { "profile", "user"})]
	[Summary("Get user or team stats.")]
	public async Task GetStats(SocketGuildUser user = null)
	{
		if (Context.Channel.Id != _channelSettings.RoCoStatsChannelId)
		{
			var message = await ReplyAsync($"Please use this command in <#{_channelSettings.RoCoStatsChannelId}>");
			await message.DeleteAfter(5000);
			return;
		}

		if (user == null)
			await Context.Channel.SendMessageAsync(null, false, await _embedService.User(await _userService.GetById(Context.User.Id), "Rogue Company"));
		else
			await Context.Channel.SendMessageAsync(null, false, await _embedService.User(await _userService.GetById(user.Id), "Rogue Company"));
	}

	[Command("leaderboard")]
	[Alias(new string[1] { "lb" })]
	public async Task GetLeaderboard(string region = null)
	{
		if (Context.Channel.Id != _channelSettings.RoCoStatsChannelId)
		{
			var message = await ReplyAsync($"Please use this command in <#{_channelSettings.RoCoStatsChannelId}>");
			await message.DeleteAfter(5000);
			return;
		}

		string gameName = "";

		if(Context.Channel is SocketTextChannel channel)
		{
			gameName = "Rogue Company";
			if (channel.Id != _channelSettings.RoCoStatsChannelId)
			{
				await Context.Message.DeleteAsync();
				var deleteMe = await ReplyAsync($"Please use this in the correct channel: <#{_channelSettings.RoCoStatsChannelId}>");
				await Task.Delay(10000);
				await deleteMe.DeleteAsync();
				return;
			}

			if (region != null)
			{
				if (region.ToLower() == "na")
					await Context.Channel.SendMessageAsync(null, false, await _embedService.Leaderboard(gameName, channel, "NA"));
				else if (region.ToLower() == "eu")
					await Context.Channel.SendMessageAsync(null, false, await _embedService.Leaderboard(gameName, channel, "EU"));
			}
			else await Context.Channel.SendMessageAsync(null, false, await _embedService.Leaderboard(gameName, channel));
		}
	}
}
