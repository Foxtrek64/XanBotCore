using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XanBotCore.Exceptions;
using XanBotCore.Permissions;
using XanBotCore.ServerRepresentation;
using XanBotCore.UserObjects;
using XanBotCore.Utility;

namespace XanBotCore.CommandSystem.Commands {
	class CommandSetPerms : Command {
		public override string Name { get; } = "setperms";

		public override string Description { get; } = "Sets the specified user's permissions. For security reasons, this requires the user's ID. See <https://support.discordapp.com/hc/en-us/articles/206346498-Where-can-I-find-my-User-Server-Message-ID->";

		public override string Syntax => Name + " <user GUID> <newPermissionLevel>";

		public override byte RequiredPermissionLevel { get; } = PermissionRegistry.PERMISSION_LEVEL_OPERATOR;

		public override async Task ExecuteCommandAsync(BotContext context, XanBotMember executingMember, DiscordMessage originalMessage, string[] args, string allArgs) {
			if (args.Length != 2) {
				throw new CommandException(this, "Invalid argument count! Expected a user ID and a permission level.");
			}
			else {
				XanBotMember member = await UserGetter.GetMemberFromDataIDStrictAsync(context.Server, args[0]);
				byte permLvl = byte.Parse(args[1]);
				if (member == null) {
					throw new CommandException(this, "The specified member could not be found. Are you searching by user ID?");
				}
				if (executingMember == member) {
					throw new CommandException(this, "You cannot alter your own permission level.");
				}
				if (executingMember.PermissionLevel <= permLvl) {
					throw new CommandException(this, string.Format("You cannot set the permission level of user `{0}` to a permission level equal to or higher than your own.", member.FullName));
				}
				if (executingMember.PermissionLevel <= member.PermissionLevel) {
					// Trying to edit the permission level of someone higher than or equal to themselves.
					throw new CommandException(this, "You cannot edit the permission level of someone at a rank greater than or equal to your own.");
				}
				member.PermissionLevel = permLvl;
				await ResponseUtil.RespondToAsync(originalMessage, string.Format("Set the permission level of user `{0}` to `{1}`", member.FullName, permLvl.ToString()));
			}
		}
	}
}
