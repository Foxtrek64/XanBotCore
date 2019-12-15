using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XanBotCore.DataPersistence;
using XanBotCore.Logging;
using XanBotCore.Permissions;
using XanBotCore.ServerRepresentation;
using XanBotCore.UserObjects;
using XanBotCore.Utility;

namespace XanBotCore.CommandSystem.Commands {
	class CommandShutdown : Command {

		public override string Name { get; } = "shutdown";

		public override string Description { get; } = "Shuts down the bot";

		public override string Syntax {
			get {
				return Name;
			}
		}

		public override byte RequiredPermissionLevel { get; } = PermissionRegistry.PERMISSION_LEVEL_ADMINISTRATOR;

		public override async Task ExecuteCommandAsync(BotContext context, XanBotMember executingMember, DiscordMessage originalMessage, string[] args, string allArgs) {
			if (originalMessage != null) {
				await ResponseUtil.RespondToAsync(originalMessage, "Sending shutdown signal and shutting down...");
			}
			XanBotCoreSystem.Exit();
		}
	}
}
