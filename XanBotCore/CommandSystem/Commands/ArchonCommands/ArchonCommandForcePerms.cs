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
	class ArchonCommandForcePerms : ArchonCommand {

		private static readonly ConsoleColorVT ORANGE = new ConsoleColorVT(255, 127, 0);

		public override string Name => "forceperms";

		public override string Description => "Forces the permission level of a given user.";

		public override string Syntax => Name + " <userID> <permLvl>";

		public override async Task ExecuteCommandAsync(BotContext context, XanBotMember executingMember, DiscordMessage originalMessage, string[] args, string allArgs) {
			if (args.Length != 2) throw new ArchonCommandException(this, "Expected 2 args.");
			if (context == null) {
				XanBotLogger.WriteLine(ORANGE + "For lack of any proper context (since this was run in the console), this member's permission level will be updated in ALL contexts.");
				XanBotLogger.WriteLine(ORANGE + "Are you sure you want to do this? [y/N]");
				ConsoleKeyInfo key = Console.ReadKey(true);
				if (key.KeyChar != 'y') {
					XanBotLogger.WriteLine(ORANGE + "Cancelling operation.");
					return;
				}
				XanBotLogger.WriteLine("§aProcessing...");

				foreach (BotContext ctx in BotContextRegistry.AllContexts) {
					if (!ctx.Virtual) {
						await SetPermissionInContext(ctx, args);
					}
				}
				XanBotLogger.WriteLine("§aDone.");
				return;
			}

			await SetPermissionInContext(context, args);
		}

		private async Task SetPermissionInContext(BotContext context, string[] args) {
			XanBotMember person = await UserGetter.GetMemberFromDataIDStrictAsync(context.Server, args[0]);
			if (person == null) throw new ArchonCommandException(this, "Invalid user.");
			if (person == context.Bot) throw new ArchonCommandException(this, "My permission level is immutable and cannot be lowered from 255.");

			if (byte.TryParse(args[1], out byte newLvl)) {
				person.PermissionLevel = newLvl;
			} else {
				throw new ArchonCommandException(this, "Invalid permission level");
			}
		}
	}
}
