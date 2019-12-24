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
	class CommandHelp : Command {
		public override string Name { get; } = "help";

		public override string Description { get; } = "Lists every command or returns information on a command.\n\nSome commands may show something called \"Arguments\" as part of their documentation. This is the text like `<someArg>` or `[someArg]`.\n"
						+ "Any text shown in greater than/less than (these: `<>`) is a **required argument.** This means that the command will not work unless you include text in place of it. For instance, say there's a command: `{0}say <message>` --  `message` is a required argument, so you need to have something after `{0}say`, like `{0}say Hello!`, in order for it to work. Do not include the `< >` in your text. (So `{0}say <Hello!>` is wrong.)\n\n"
						+ "Any text shown in square brackets (these: `[]`) is an **optional argument.** This means that the command will still work if you don't include text there. A great example is this command -- `{0}help` can **optionally** take in the name of a specific command to get more information on that command. If you don't do that, it shows the list of every command. Do not include the `[ ]` in your text. (Same as above)\n\n"
						+ "Command arguments are split by spaces. The way this is handled is identical to that of the Windows Command Line. For example: The command `{0}cmd abc Cool Text! 123` will be interpreted with four arguments: `abc`, `Cool`, `Text!`, and `123`. You can put quotes around arguments to join them, like `{0}cmd abc \"Cool Text!\" 123` which will evaluate into *three* arguments: `abc`, `Cool Text!`, and `123`";

		public override string Syntax => Name + " [commandName]";

		public override byte RequiredPermissionLevel { get; } = PermissionRegistry.PERMISSION_LEVEL_STANDARD_USER;

		public override async Task ExecuteCommandAsync(BotContext context, XanBotMember executingMember, DiscordMessage originalMessage, string[] args, string allArgs) {
			if (args.Length == 0) {
				string text = "Commands in yellow with a `+` before them are commands you can use. Commands in red with a `-` before them are commands you cannot use. ";
				text += "\nSay **`{0}help command_name_here`** to get more documentation on a specific command. Say **`{0}help help`** to get information on how commands work.";
				text += "```diff\n";
				foreach (Command cmd in CommandMarshaller.Commands) {
					int spaces = 34;
					string usagePrefix = "+ ";
					if (executingMember != null) {
						usagePrefix = cmd.CanUseCommand(executingMember) ? "+ " : "- ";
					}
					text += usagePrefix + cmd.Name;
					spaces -= (cmd.Name.Length + 2);
					for (int i = 0; i < spaces; i++) {
						text += " ";
					}
					text += $"Requires Permission Level {cmd.RequiredPermissionLevel} (or higher).";
					text += "\n";
				}

				if (context != null && context.ContextSpecificCommands.Length > 0) {
					text += "\nCommands specific to this server:\n\n";
					foreach (Command cmd in context.ContextSpecificCommands) {
						int spaces = 34;
						string usagePrefix = "+";
						if (executingMember != null) {
							usagePrefix = cmd.CanUseCommand(executingMember) ? "+ " : "- ";
						}
						text += usagePrefix + cmd.Name;
						spaces -= (cmd.Name.Length + 2);
						for (int i = 0; i < spaces; i++) {
							text += " ";
						}
						text += $"Requires Permission Level {cmd.RequiredPermissionLevel} (or higher).";
						text += "\n";
					}
				}

				text += "```\n";

				text = string.Format(text, CommandMarshaller.CommandPrefix);

				await ResponseUtil.RespondToAsync(originalMessage, text);
			} else if (args.Length == 1) {
				string command = args[0];
				string cmdLower = command.ToLower();
				foreach (Command cmd in CommandMarshaller.Commands) {
					if (cmd.Name.ToLower() == cmdLower) {
						await ResponseUtil.RespondToAsync(originalMessage, GetFormattedCommandHelpInfo(cmd, cmd.Name));
						return;
					} else if (cmd.AlternateNames != null) {
						foreach (string alt in cmd.AlternateNames) {
							if (alt.ToLower() == cmdLower) {
								await ResponseUtil.RespondToAsync(originalMessage, GetFormattedCommandHelpInfo(cmd, alt));
								return;
							}
						}
					}
				}
				if (context.ContextSpecificCommands.Length > 0) {
					foreach (Command cmd in context.ContextSpecificCommands) {
						if (cmd.Name.ToLower() == cmdLower) {
							await ResponseUtil.RespondToAsync(originalMessage, GetFormattedCommandHelpInfo(cmd, cmd.Name));
							return;
						} else if (cmd.AlternateNames != null) {
							foreach (string alt in cmd.AlternateNames) {
								if (alt.ToLower() == cmdLower) {
									await ResponseUtil.RespondToAsync(originalMessage, GetFormattedCommandHelpInfo(cmd, alt));
									return;
								}
							}
						}
					}
				}
				throw new CommandException(this, "Command `" + command + "` does not exist.");
			} else {
				throw new CommandException(this, "Invalid argument count. Expected no arguments, or one argument which is the name of the command you wish to get details on.");
			}
		}

		public string GetFormattedCommandHelpInfo(Command cmd, string indexedBy) {
			int locatedGraves = 0;
			foreach (char c in cmd.Syntax.ToCharArray()) {
				if (c == '`') locatedGraves++;
			}

			string text = string.Format(
				"**Command:** `{0}` \n{1}\n\n**Usage:** `{2}",
				cmd.Name,
				string.Format(cmd.Description, CommandMarshaller.CommandPrefix),
				string.Format(cmd.Syntax, CommandMarshaller.CommandPrefix)
			);
			if (locatedGraves % 2 == 0) {
				// Effectively what this does is prevents formatting issues from if users specify custom code formatting.
				// They are expected to terminate the syntax code block manually in syntax.
				// If they use an odd number of graves, it means that they have overridden it and we need to not include a trailing grave since that will show up.
				text += '`';
			}

			string alsoIndexedBy = null;
			if (cmd.AlternateNames != null) {
				if (indexedBy.ToLower() == cmd.Name.ToLower()) {
					alsoIndexedBy = StrArrayToFormatedList(cmd.AlternateNames);
				} else {
					alsoIndexedBy = "`" + cmd.Name + "`";
					if (cmd.AlternateNames.Length > 1) {
						alsoIndexedBy += ", "; // If it's 1, then that means there's no other names to show so it's just gonna be the command's name.
						foreach (string alt in cmd.AlternateNames) {
							if (alt.ToLower() != indexedBy.ToLower()) {
								alsoIndexedBy += "`" + alt + "`";
								if (alt != cmd.AlternateNames.Last()) {
									alsoIndexedBy += ", ";
								}
							}
						}
					}
				}
			}

			if (alsoIndexedBy != null) {
				text += "\n**Can also be run with:** " + alsoIndexedBy;
			}

			return text;
		}

		private static string StrArrayToFormatedList(string[] list) {
			string o = "";
			foreach (string s in list) {
				o += "`" + s + "`";
				if (s != list.Last()) {
					o += ", ";
				}
			}
			return o;
		}
	}
}
