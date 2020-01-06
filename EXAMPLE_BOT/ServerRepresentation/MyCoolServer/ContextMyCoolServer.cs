using EXAMPLE_BOT.ServerRepresentation.MyCoolServer.Commands;
using EXAMPLE_BOT.ServerRepresentation.MyCoolServer.PassiveHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XanBotCore.CommandSystem;
using XanBotCore.Logging;
using XanBotCore.PassiveHandlers;
using XanBotCore.ServerRepresentation;

namespace EXAMPLE_BOT.ServerRepresentation.MyCoolServer {
	class ContextMyCoolServer : BotContext {

		// Refer to BotContext.cs itself for docs.

		public override string Name => "My cool server's context";

		public override string DataPersistenceName => "ctxMyServer";

		public override ulong ServerId => 0;


		// Optional stuff:
		protected override Command[] ContextSpecificCommands => new Command[] {
			new CommandSayHello()
		};

		protected override PassiveHandler[] ContextSpecificHandlers => new PassiveHandler[] {
			new HandlerYouCantSayTheNWord()
		};

		// Ensures that the users (ulong keys) have the specified permission level (byte value) when their member is instantiated for this server.
		public override Dictionary<ulong, byte> DefaultUserPermissions => new Dictionary<ulong, byte>() {
			[694201337] = 0,
		};

		public override void AfterContextInitialization() {
			XanBotLogger.WriteLine("Wow, my cool context finished initializing!");
		}
	}
}
