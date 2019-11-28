using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XanBotCore.CommandSystem;
using XanBotCore.PassiveHandlers;

namespace XanBotCore.ServerRepresentation {

	/// <summary>
	/// Represents a virtual bot context, or a bot context that is created on the fly by the bot for a server that does not have a unique context specified for it.
	/// </summary>
	public class VirtualBotContext : BotContext {

		public override string Name { get; }

		public override ulong ServerId { get; }

		public override string DataPersistenceName {
			get {
				return "VirtualContext-" + ServerId;
			}
		}

		internal override bool IsVirtualInternal => true;

		/// <summary>
		/// Construct a new VirtualBotContext for the specified server. Developers should not call this manually and should instead let the bot handle this automatically.
		/// </summary>
		/// <param name="server">The server this virtual context exists in.</param>
		internal VirtualBotContext(DiscordGuild server) : base() {
			Name = server.Name;
			ServerId = server.Id;
		}
	}
}
