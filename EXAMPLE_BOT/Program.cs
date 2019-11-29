using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XanBotCore;
using XanBotCore.Logging;
using XanBotCore.Permissions;

namespace EXAMPLE_BOT {
	class Program {

		private const string BOT_TOKEN = "asdfg";

		static void Main(string[] args) {
			Console.Title = "Optional console title here.";

			try {
				MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
				XanBotCoreSystem.Exit();
			} catch (Exception ex) {
				XanBotLogger.WriteException(ex);
				XanBotLogger.WriteLine("§1Press any key to quit.");
				Console.ReadKey(true);
				XanBotCoreSystem.Exit(1);
			}
		}

		public static async Task MainAsync(string[] args) {
			PermissionRegistry.AllowXanMaxPermissionLevel = false; // If you're feeling generous, you can set this to true.
			// Its default value is true, but here I'm setting it to false so that people know they can do that.

			// Start the bot init cycle.
			// This method as-is creates a voice client (first boolean argument) and says that the bot is targeting Windows 7 (second bool).
			await XanBotCoreSystem.InitializeBotAsync(BOT_TOKEN, true, true);
			// Do any other stuff that runs after the bot has connected to discord here.

			await Task.Delay(-1);
		}
	}
}
