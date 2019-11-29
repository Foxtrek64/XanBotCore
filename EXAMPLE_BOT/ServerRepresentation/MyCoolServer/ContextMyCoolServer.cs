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
		public override Command[] ContextSpecificCommands => new Command[] {
			new CommandSayHello()
		};
		public override PassiveHandler[] ContextSpecificHandlers => new PassiveHandler[] {
			new HandlerYouCantSayTheNWord()
		};
		public override void AfterContextInitialized() {
			XanBotLogger.WriteLine("Wow, my cool context finished initializing!");
		}
	}
}
