using Discord.Commands;
using System.Threading.Tasks;
using DiscordBot.Services;
using DiscordBot.Models;
using DiscordBot.Settings;
using Discord;
using Discord.WebSocket;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using System;
using DiscordBot.Util;
using DiscordBot.Caches;

public class TeamsModule : ModuleBase<SocketCommandContext>
{
	private IEmbedService _embedService { get; }
	private ChannelSettings _channelSettings { get; }
	private BotSettings _botSettings { get; }

	private ITeamService _teamService { get; }
	private IMatchService _matchService { get; }
	private IUserService _userService { get; }
	private IDiscordUserCache _discordUserCache { get; }

	public TeamsModule(IUserService userService, IEmbedService embedService, 
		ITeamService teamService, IOptions<ChannelSettings> channelSettings,
		IOptions<BotSettings> botSettings, IMatchService matchService, IDiscordUserCache discordUserCache)
	{
		_matchService = matchService;
		_teamService = teamService;
		_userService = userService;
		_embedService = embedService;
		_channelSettings = channelSettings.Value;
		_botSettings = botSettings.Value;
		_discordUserCache = discordUserCache;
	}


	/// <summary>
	/// Challenge another team to a scrim match.
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	[Command("scrim")]
	[Alias(new string[1] { "challenge" })]
	public async Task Scrim(string myTeamName = null, string theirTeamName = null)
	{
		if(Context.Channel is SocketTextChannel channel)
		{
			if(myTeamName == null || theirTeamName == null)
			{
				await Context.Message.DeleteAsync();
				var deleteMe = await ReplyAsync($"Command usage: `{_botSettings.Prefix}scrim myTeamName theirTeamName`");
				await deleteMe.DeleteAfter(10000);
				return;
			}

			if(channel.CategoryId == _channelSettings.RoCoCategoryId)
			{
				if(channel.Id != _channelSettings.RoCoTeamChannelId)
				{
					await Context.Message.DeleteAsync();
					var deleteMe = await ReplyAsync($"Please make sure to use this command in " +
						$"the correct channel: <#{_channelSettings.RoCoTeamChannelId}>");
					await deleteMe.DeleteAfter(10000);
					return;
				}
			}

			Team team1 = await _teamService.Get(myTeamName);
			Team team2 = await _teamService.Get(theirTeamName);

			if(team1 == null)
			{
				await Context.Message.DeleteAsync();
				var deleteMe = await ReplyAsync($"We could not find a team by the name of {myTeamName}");
                await deleteMe.DeleteAfter(10000);
				return;
			}

			if (team2 == null)
			{
				await Context.Message.DeleteAsync();
				var deleteMe = await ReplyAsync($"We could not find a team by the name of {theirTeamName}");
				await deleteMe.DeleteAfter(10000);
				return;
			}
			foreach(var id in team1.MemberDiscordIds.Union(team2.MemberDiscordIds)) {
				await _discordUserCache.SetValueAsync(id, Context.Guild.GetUser(id));
            }

			await _embedService.SendScrimChallenge(team1, team2, channel);
		}
	}

	[Command("create")]
	[Summary("Create a team.")]
	public async Task CreateTeam(string name, SocketUser u1 = null, SocketUser u2 = null, SocketUser u3 = null, SocketUser u4 = null)
	{
		if(Context.Channel is SocketTextChannel socketTextChannel)
		{
			ulong channelId = 0;
			string gameName = "";

			// Check category to decide game and correct channelId
			if (socketTextChannel.CategoryId == _channelSettings.RoCoCategoryId)
			{
				channelId = _channelSettings.RoCoTeamChannelId;
				gameName = "Rogue Company";
			}

			if(socketTextChannel.Id != channelId)
			{
				await Context.Message.DeleteAsync();
				var deleteMe = await ReplyAsync($"Please make sure to use this command in <#{channelId}>");
				await Task.Delay(5000);
				await deleteMe.DeleteAsync();
				return;
			}

			if (name.Length < 3 || name.Length > 12)
			{
				await Context.Message.DeleteAsync();
				var deleteMe = await ReplyAsync("Please choose a team name that is between 3 and 12 characters.");
                await deleteMe.DeleteAfter(10000);
				return;
			}

			//Check if too many users mentioned - begin by retrieving correct game from settings
			foreach(DiscordBot.Models.Game gameCurr in GameSettings.Games)
			{
				if(gameCurr.Name == gameName)
				{
					if (Context.Message.MentionedUsers.Count != gameCurr.TeamSize - 1)
					{
						await Context.Message.DeleteAsync();
						var deleteMe = await ReplyAsync($"Provide a team name and make sure to mention {gameCurr.TeamSize - 1} users when creating your {gameName} team.");
						await deleteMe.DeleteAfter(10000);
						return;
					}
					break;
				}
			}

			User captain = await _userService.GetById(Context.User.Id);

            // Check if this team already exists and report team name if so

            // Build team
            Team team = new Team(name, gameName)
            {
                CaptainId = captain.Id
            };

            team.MemberIds.Add(captain.Id);

			List<SocketUser> mentions = Context.Message.MentionedUsers.ToList();
			
			List<User> members = new List<User>();

			for(int i = 0; i < mentions.Count; i++)
			{
				members.Add(await _userService.GetById(mentions[i].Id));	
			}

			// Check for duplicate teams with these members
			bool checkForDuplicateTeam = true;
			Team duplicateTeam = null;
			foreach(string teamId in captain.TeamIds)
			{
				Team teamCurr = await _teamService.Get(teamId);

				//TODO: I don't think this code does what it's intended to do.
				// This checks for 1 of the team members to NOT exist in a team that is already existing for the captain.
				// I would expect it to check that there is an existing team that contains ALL of the members
				foreach (User userCurr in members)
					if (!teamCurr.MemberDiscordIds.Contains(userCurr.DiscordId))
					{
						checkForDuplicateTeam = false;
						duplicateTeam = teamCurr;
						break;
					}

				if (!checkForDuplicateTeam) break;
			}
			if(duplicateTeam != null)
			{
				await Context.Message.DeleteAsync();
				await ReplyAsync($"You already have a team: `{duplicateTeam.TeamName}` created with these members.");
				await Context.Channel.SendMessageAsync(null, false, await _embedService.Team(duplicateTeam));
				return;
			}

			// Make team
			foreach (User userCurr in members)
			{
				userCurr.TeamIds.Add(team.Id);
				await _userService.Update(userCurr);
				team.MemberIds.Add(userCurr.Id);
			}

			try
			{
				await _teamService.Register(team);
			}
			catch
			{
				await ReplyAsync("Failed to register team. Team name might already exist!\n\n");
			}

			await Context.Channel.SendMessageAsync(null, false, await _embedService.Team(team));
		}
	}
}
