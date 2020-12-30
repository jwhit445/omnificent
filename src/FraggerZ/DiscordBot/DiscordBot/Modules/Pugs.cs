using Discord.Commands;
using System.Threading.Tasks;
using DiscordBot.Services;
using Discord;
using Discord.WebSocket;
using DiscordBot.Models;
using System;
using DiscordBot.Settings;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

public class PugsModule : ModuleBase<SocketCommandContext>
{
    private readonly RoCoPugService _rocoPugService;
	private readonly UserService _userService;
    private readonly ChannelSettings _channelSettings;
	private readonly EmoteSettings _emoteSettings;

	public PugsModule(RoCoPugService rocoPugService, UserService userService, IOptions<ChannelSettings> channelSettings
		, IOptions<EmoteSettings> emoteSettings)
    {
		_rocoPugService = rocoPugService;
		_userService = userService;
		_channelSettings = channelSettings.Value;
		_emoteSettings = emoteSettings.Value;
    }

	[Command("suspend")]
	[RequireUserPermission(GuildPermission.KickMembers)]
	[Summary("Suspend a user from pugs for x minutes.")]
	public async Task SuspendUser(SocketGuildUser socketGuildUser, int minutes)
	{

		// basic validation
		if (minutes < 0)
        {
			await ReplyAsync("You cannot suspend someone for a negative amount of time!");
			return;
        }

		User user = await _userService.GetById(socketGuildUser.Id);
		if(user != null)
		{
			user.SuspensionReturnDate = DateTime.UtcNow.AddMinutes(minutes);
			await _userService.Update(user);

			await ReplyAsync($"{socketGuildUser.Mention} has been suspended from pugs for {minutes} minutes.");
		}
	}

	// Setup the pug queue embed.
	[Command("setupqueue")]
	[RequireUserPermission(GuildPermission.Administrator)]
	[Summary("Set up a pug queue.")]
	public async Task SetupQueue()
	{
		await Context.Message.DeleteAsync();

		// Send a message
		// React to message with play emoji
		string title = "PUG Queue"+ (Context.Channel.Id == _channelSettings.RoCoNAQueueCPlusUpChannelId ? " For Rank C+ and up": "");
		string description = $"Users in queue: `{_rocoPugService.NAQueue.PlayersInQueue.Count}` / 8\n\n";
		foreach(IUser user in _rocoPugService.NAQueue.PlayersInQueue)
		{
			description += user.Mention;
			description += "\n";
		}

		Embed embed = new EmbedBuilder()
		{
			Description = description,
			Title = title,
			Color = Color.Green
		}.Build();

		var message = await Context.Channel.SendMessageAsync(null, false, embed);
		List<IEmote> reactions = new List<IEmote>() {
			new Emoji(_emoteSettings.PlayEmoteUnicode)
		};
		if(_rocoPugService.DictQueueForChannel.ContainsKey(Context.Channel.Id) && _rocoPugService.DictQueueForChannel[Context.Channel.Id].QueueType != QueueType.NACPlus) {
			reactions.Add(new Emoji(_emoteSettings.PlayDuoEmoteUnicode));
        }
		await message.AddReactionsAsync(reactions.ToArray());
	}

}
