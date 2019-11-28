using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XanBotCore {

	/// <summary>
	/// A hacky representation of the Ori Discord server.<para/>
	/// This class exists for global ease of access. It is instantiated in the main bot code.
	/// </summary>
	public class OriServerData {

#if DEBUG
		public static readonly bool IsDebugMode = true;
#else
		public static readonly bool IsDebugMode = false;
#endif

		/// <summary>
		/// A reference to the current active OriServerData. This is done for debug purposes as there may be testing performed in an external server.
		/// </summary>
		public static OriServerData Current { get; set; } = null;

		/// <summary>
		/// A reference to the Discord server.
		/// </summary>
		public DiscordGuild Server { get; internal set; } = null;

		/// <summary>
		/// A reference to this bot's role in the Discord server.
		/// </summary>
		public DiscordRole BotRole { get; internal set; } = null;

		/// <summary>
		/// A reference to the Discord Client.
		/// </summary>
		public DiscordClient Client { get; internal set; } = null;

		/// <summary>
		/// A reference to the bot channel.
		/// </summary>
		public DiscordChannel BotChannel { get; internal set; } = null;

		/// <summary>
		/// A reference to the moderator bot channel.
		/// </summary>
		public DiscordChannel ModeratorBotChannel { get; internal set; } = null;

		/// <summary>
		/// The muted role for this server.
		/// </summary>
		public DiscordRole MutedRole { get; internal set; } = null;

		/// <summary>
		/// The moderator role for this server.
		/// </summary>
		public DiscordRole ModRole { get; internal set; } = null;

		/// <summary>
		/// Whether or not this <see cref="OriServerData"/> is running in the test server or not.
		/// </summary>
		public bool IsTestServer { get; internal set; } = false;

		private OriServerData() { }

		/// <summary>
		/// Initialize a new OriServerData from the specified parameters.
		/// </summary>
		/// <param name="botClient">A reference to the connected Discord client. In the case of the Ori bot specifically, this should be OriBot.Discord</param>
		/// <param name="serverId">The ID of the server.</param>
		/// <param name="botRoleId">The ID of the bot role.</param>
		/// <param name="botChannelId">The ID of the bot channel.</param>
		/// <param name="adminBotChannelId">The ID of the moderator-only bot channel.</param>
		/// <param name="mutedRoleId">The ID of the muted role.</param>
		/// <param name="modRoleId">The ID of the moderator role.</param>
		/// <param name="isTestServerInstance">Whether or not this should be instantiated as the test server.</param>
		/// <returns></returns>
		public static async Task<OriServerData> InitializeOriServerData(DiscordClient botClient, ulong serverId, ulong botRoleId, ulong botChannelId, ulong adminBotChannelId, ulong mutedRoleId, ulong modRoleId, bool isTestServerInstance) {
			DiscordGuild server = await botClient.GetGuildAsync(serverId);
			OriServerData serverData = new OriServerData {
				Client = botClient,
				Server = server,
				BotRole = server.GetRole(botRoleId),
				BotChannel = server.GetChannel(botChannelId),
				ModeratorBotChannel = server.GetChannel(adminBotChannelId),
				MutedRole = server.GetRole(mutedRoleId),
				ModRole = server.GetRole(modRoleId),
				IsTestServer = isTestServerInstance,
			};
			return serverData;
		}

	}
}
