using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XanBotCore.Exceptions;
using XanBotCore.Logging;
using XanBotCore.ServerRepresentation;
using XanBotCore.UserObjects;
using XanBotCore.Utility;

namespace XanBotCore.CommandSystem.Commands.ArchonCommands {
	public class ArchonCommandToggleDebugLogging : ArchonCommand {
		public override string Name => "debuglogging";

		public override string Description => "Enables or disables debug logging even if debug mode is off. Can only be executed by the bot developer or backend console.";

		public override string Syntax => Name + " :true|false:";

		public override async Task ExecuteCommandAsync(BotContext context, XanBotMember executingMember, DiscordMessage originalMessage, string[] args, string allArgs) {
			if (executingMember != null && executingMember.PermissionLevel < 254) {
				throw new ArchonCommandException(this, "Cannot execute this command at current permission level. This command requires backend console access (permission level 254+)");
			}

			if (args.Length != 1) {
				throw new ArchonCommandException(this, "Expected one arg `<enabled>`.");
			}

			if (bool.TryParse(args[0], out bool enable)) {
				XanBotLogger.ShowDebugMessagesAnyway = enable;
				if (XanBotCoreSystem.IsDebugMode) {
					await ResponseUtil.RespondToAsync(originalMessage, "Forcefully " + (enable ? "ENABLING" : "DISABLING") + " debug-logging to the bot's console. **NOTE:** This will have no effect since the bot is currently running in debug mode.");
					return;
				}
				await ResponseUtil.RespondToAsync(originalMessage, "Forcefully " + (enable ? "ENABLING" : "DISABLING") + " debug-logging to the bot's console.");
			} else {
				throw new ArchonCommandException(this, $"Failed to cast `{args[0]}` into boolean (true/false).");
			}
		}
	}
}
