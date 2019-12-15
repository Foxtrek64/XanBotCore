using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using XanBotCore.CommandSystem.Commands.ArchonCommands;
using XanBotCore.Exceptions;
using XanBotCore.Logging;
using XanBotCore.Permissions;
using XanBotCore.ServerRepresentation;
using XanBotCore.UserObjects;

namespace XanBotCore.CommandSystem.Commands {

	/// <summary>
	/// Responsible for the exection of Archon Commands.
	/// </summary>
	public class CommandArchonCommand : Command {

		public override string Name { get; } = "archoncmd";

		public override string Description { get; } = "Offers commands intended for low-level control of the bot.";

		public override string Syntax => Name + " <cmd> [cmdArgs]`\nUse `archoncmd help` to get a list of Archon Commands.";

		public override byte RequiredPermissionLevel { get; } = PermissionRegistry.PERMISSION_LEVEL_ADMINISTRATOR;

		public override async Task ExecuteCommandAsync(BotContext context, XanBotMember executingMember, DiscordMessage originalMessage, string[] args, string allArgs) {
			if (args.Length == 0) {
				throw new CommandException(this, "Invalid argument count. Expected at least one arg.");
			}
			string subCommand = args[0];
			args = args.Skip(1).ToArray();
			foreach (ArchonCommand cmd in CommandMarshaller.ArchonCommands) {
				if (cmd.Name.ToLower() == subCommand.ToLower()) {
					await cmd.ExecuteCommandAsync(context, executingMember, originalMessage, args, allArgs);
					return;
				}
			}
			throw new CommandException(this, "Unable to execute Archon Command `" + subCommand + "` because it does not exist.");
		}
	}
}
