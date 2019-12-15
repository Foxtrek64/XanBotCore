using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using XanBotCore.Exceptions;
using XanBotCore.ServerRepresentation;
using XanBotCore.UserObjects;
using XanBotCore.Utility;

namespace XanBotCore.CommandSystem.Commands.ArchonCommands {
	public class ArchonCommandHelp : ArchonCommand {
		public override string Name => "help";

		public override string Description => "Returns the list of Archon Commands that are registered, or lists information on the input command. Identical to the stock help command, but it instead targets Archon Commands.";

		public override string Syntax => Name + " [archonCommandName]";

		public override async Task ExecuteCommandAsync(BotContext context, XanBotMember executingMember, DiscordMessage originalMessage, string[] args, string allArgs) {
			if (args.Length == 0) {
				string text = "Current Archon Commands:\n```\n";
				foreach (ArchonCommand cmd in CommandMarshaller.ArchonCommands) {
					text += cmd.Name + "\n";
				}
				text += "```\n";

				await ResponseUtil.RespondToAsync(originalMessage, text);
			} else if (args.Length == 1) {
				string command = args[0];
				foreach (ArchonCommand cmd in CommandMarshaller.ArchonCommands) {
					if (cmd.Name.ToLower() == command.ToLower()) {
						string text = string.Format("**Archon Command:** `{0}` \n{1}\n\n**Usage:** `{2}`", cmd.Name, cmd.Description, cmd.Syntax);
						await ResponseUtil.RespondToAsync(originalMessage, text);
						return;
					}
				}
				throw new ArchonCommandException(this, $"Archon Command `{command}` does not exist.");
			} else {
				throw new ArchonCommandException(this, "Invalid argument count. Expected no arguments, or one argument which is the name of the archon command you wish to get details on.");
			}
		}
	}
}
