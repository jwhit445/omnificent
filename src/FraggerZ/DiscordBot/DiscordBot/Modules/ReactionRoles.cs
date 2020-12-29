// Create a module with no prefix
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using DiscordBot.Services;
using DiscordBot.Settings;
using Discord;
using Microsoft.Extensions.Options;

public class ReactionRolesModule : ModuleBase<SocketCommandContext>
{
	private readonly RoleSettings _roleSettings;
	private readonly EmoteSettings _emoteSettings;

	public ReactionRolesModule(IOptions<RoleSettings> roleSettings, IOptions<EmoteSettings> emoteSettings)
    {
		_roleSettings = roleSettings.Value;
    }

	[Command("setupwelcome")]
	[RequireUserPermission(GuildPermission.Administrator)]
	public async Task SetupWelcome()
	{
		string description = "**FraggerZ** aims to serve the following games:\n\n" +
			"Rogue Company, CrossFire, IronSight\n\n" +
			"**FraggerZ** offers the following products in our system for competitive gaming:\n\n" +
			"`Fully customized Discord bot` to handle user registration and pick up game(PUGs) / scrim functionality, as well as scoreboard AI for automatic match reporting." +
			"\n\n`Downloadable client` to offer 3rd party anti - cheat, in game overlay, PUG lobby information, and PUG score submissions." +
			"\n\n`Website` to give advanced user statistics, the option to purchase a premium account and / or shop through our eCommerce selection." +
			"\n\n`Rules for server`:\n\n" +
			"- No racist remarks what so ever. Immediate ban." +
			"\n\n- No racist or offensive in game names / clan tags. Immediate ban." +
			"\n\n- Toxicity / harassment / flaming will be dealt with on a case by case basis." +
			"\n\n- Cheating / macro / attempts of this sort will result in an immediate ban." +
			"\n\n- Stream sniping is neither encouraged nor penalized.So please make sure to use a DELAY while streaming these events. There is no excuse - if you need help adding a delay feel free to ask a staff member." +
			"\n\n*Follow Discord and Twitch TOS*. Ask a staff member if you need the links to the TOS." +
			"\n\n**React to this message to accept our rules and gain entry to our server.**";
		Embed embed = new EmbedBuilder()
		{
			Description = description,
			Color = Color.Green
		}.Build();

		var msg = await Context.Channel.SendMessageAsync(null, false, embed);

		await msg.AddReactionsAsync(new IEmote[1] {
			new Emoji(_emoteSettings.FraggerZEmoteUnicode)
		});
	}

	// Setup the game reaction roles
	[Command("setupgames")]
	[RequireUserPermission(GuildPermission.Administrator)]
	[Summary("Set up the game role reactions.")]
	public async Task SetupQueue()
	{
		await Context.Message.DeleteAsync();
		// Send a message
		// React to message with play emoji
		string description = $"React with the corresponding emoji to gain access to that game's category.\n\n" +
			$"<:roco:783413639993884722> = Rogue Company\n\n" +
			$"<:cf:783413604661329951> = CrossFire\n\n" +
			$"<:ironsight:783413616040214529> = IronSight";

		Embed embed = new EmbedBuilder()
		{
			Description = description,
			Color = Color.Green
		}.Build();

		var message = await Context.Channel.SendMessageAsync(null, false, embed);

		await message.AddReactionsAsync(new IEmote[3] { 
			new Emoji(":roco:783413639993884722"), 
			new Emoji(":cf:783413604661329951"),
			new Emoji(":ironsight:783413616040214529")
		});
	}

	// Setup the pug queue embed.
	[Command("setupother")]
	[RequireUserPermission(GuildPermission.Administrator)]
	[Summary("Set up the other reaction roles.")]
	public async Task SetupOther()
	{
		await Context.Message.DeleteAsync();
		// Send a message
		// React to message with play emoji
		string description = $"React with any of the following to get the role.\n\n" +
			$"<:na:790261262796849192> for <@&{_roleSettings.NARoleId}>\n\n" +
			$"<:eu:790261271416799264> for <@&{_roleSettings.EURoleId}>\n\n" +
			$"<:ping:783454907558395905> for <@&{_roleSettings.PUGPingerRoleId}> role to be notified when PUGs are active.";

		Embed embed = new EmbedBuilder()
		{
			Description = description,
			Color = Color.Green
		}.Build();

		var message = await Context.Channel.SendMessageAsync(null, false, embed);

		await message.AddReactionsAsync(new IEmote[3] {
			new Emoji(":na:790261262796849192"),
			new Emoji(":eu:790261271416799264"),
			new Emoji(":ping:783454907558395905"),
		});
	}

}
