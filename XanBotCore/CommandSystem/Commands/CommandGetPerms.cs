using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using XanBotCore.Exceptions;
using XanBotCore.Permissions;
using XanBotCore.ServerRepresentation;
using XanBotCore.UserObjects;
using XanBotCore.Utility;

namespace XanBotCore.CommandSystem.Commands {
	class CommandGetPerms : Command {

		public override string Name { get; } = "getperms";

		public override string Description { get; } = "Gets the current user's permissions, or if a user is specified, the permissions of the specified user.";

		public override string Syntax {
			get {
				return Name + " [username/nickname/userGUID]";
			}
		}

		public override byte RequiredPermissionLevel { get; } = PermissionRegistry.PERMISSION_LEVEL_STANDARD_USER;

		public override void ExecuteCommand(BotContext context, XanBotMember executingMember, DiscordMessage originalMessage, string[] args, string allArgs) {
			if (args.Length == 0) {
				// User wants to get their own permissions.
				ResponseUtil.RespondTo(originalMessage, string.Format("Your permission level is `{0}`", executingMember.PermissionLevel));
			} else if (args.Length >= 1) {

				try {
					XanBotMember member = UserGetter.GetMemberFromData(context.Server, allArgs).GetAwaiter().GetResult();
					if (member == null) {
						throw new CommandException(this, "The specified user is not a member of this server.");
					}
					ResponseUtil.RespondTo(originalMessage, string.Format("The permission level of `{0}` is `{1}`", member.FullName, member.PermissionLevel));
				}
				catch (NonSingularResultException err) {
					string msg = err.Message;
					msg += "\nThe potential users are:\n";
					foreach (object obj in err.PotentialReturnValues) {
						XanBotMember member = (XanBotMember)obj;
						msg += member?.FullName + "(User GUID: " + member?.BaseUser.Id + ")\n";
					}
					msg += "\nYou can directly copy/paste the user's GUID into this command to get that specific user.";
					ResponseUtil.RespondTo(originalMessage, msg);
				}
			}
			
		}
	}
}
