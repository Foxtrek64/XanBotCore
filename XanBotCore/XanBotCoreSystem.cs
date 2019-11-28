using DSharpPlus;
using DSharpPlus.Net.WebSocket;
using DSharpPlus.VoiceNext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XanBotCore.Logging;
using XanBotCore.ServerRepresentation;
using DSharpPlus.EventArgs;
using XanBotCore.CommandSystem;
using XanBotCore.Permissions;

namespace XanBotCore {

	/// <summary>
	/// The heart of XanBotCore. This stores vital information about your discord bot, for instance, its Discord Client. It also provides methods to quickly connect a bot.
	/// </summary>
	public class XanBotCoreSystem {

		private static DiscordClient ClientInternal = null;
		private static VoiceNextExtension VoiceClientInternal = null;

		/// <summary>
		/// Whether or not the core has been instantiated and certain core values like <see cref="Client"/> exist.
		/// </summary>
		public static bool Created { get; private set; }

		/// <summary>
		/// The <see cref="DiscordClient"/> of the bot employing the use of XanBotCore.<para/>
		/// Throws a <see cref="InvalidOperationException"/> if the value for this is referenced before it is set -- It should be initialized immediately with the bot.
		/// </summary>
		public static DiscordClient Client {
			get {
				return ClientInternal ?? throw new InvalidOperationException("Failed to get Client -- XanBotCoreSystem.Client is null. Did you remember to set this value when you initialized your Discord bot?");
			}
			set {
				ClientInternal = value;
			}
		}

		/// <summary>
		/// The <see cref="VoiceNextExtension"/> of the bot. This allows the bot to connect to voice channels to both send and receive audio.<para/>
		/// Throws a <see cref="InvalidOperationException"/> if the value for this is referenced before it is set -- It should be initialized immediately with the bot.
		/// </summary>
		public static VoiceNextExtension VoiceClient {
			get {
				return VoiceClientInternal ?? throw new InvalidOperationException("Failed to get VoiceClient -- XanBotCoreSystem.VoiceClient is null. Did you remember to set this value when you initialized your Discord bot?");
			}
			set {
				VoiceClientInternal = value;
			}
		}

		/// <summary>
		/// Automatically creates <see cref="Client"/> and connects it, creates <see cref="VoiceClient"/> (if requested), instantiates all bot contexts automatically, starts the <see cref="XanBotConsoleCore"/> update loop, and binds the windows console CTRL+C/CTRL+BREAK to <see cref="Exit(int)>"/>
		/// </summary>
		/// <param name="botToken">The token of this discord bot.</param>
		/// <param name="createVoiceNextClient">If true, <see cref="VoiceClient"/> will be instantiated and allow this bot to connect to voice channels.</param>
		/// <param name="isTargettingWindows7">Whether or not this bot is running on Windows 7. This is necessary for some init code.</param>
		/// <returns></returns>
		public static async Task InitializeBotAsync(string botToken, bool createVoiceNextClient = false, bool isTargettingWindows7 = false) {
			if (IsDebugMode) {
				XanBotLogger.WriteLine("§eInitializing in Debug Mode...");
			} else {
				XanBotLogger.WriteLine("Initializing...");
			}
			if (isTargettingWindows7) {
				Client = new DiscordClient(new DiscordConfiguration {
					Token = botToken,
					TokenType = TokenType.Bot,
					WebSocketClientFactory = new WebSocketClientFactoryDelegate(WebSocket4NetClient.CreateNew), // Remove if this bot is running on machine with Windows 10.
				});
			} else {
				Client = new DiscordClient(new DiscordConfiguration {
					Token = botToken,
					TokenType = TokenType.Bot,
					WebSocketClientFactory = new WebSocketClientFactoryDelegate(WebSocketClient.CreateNew), // Remove if this bot is running on machine with Windows 10.
				});
			}
			XanBotLogger.WriteDebugLine("Created DiscordClient.");

			if (createVoiceNextClient) {
				VoiceClient = Client.UseVoiceNext(new VoiceNextConfiguration {
					AudioFormat = new AudioFormat(48000, 2, VoiceApplication.LowLatency)
				});
				XanBotLogger.WriteDebugLine("Created VoiceNextClient.");
			}

			XanBotLogger.WriteLine("Connecting to Discord...");
			await Client.ConnectAsync();

			// Client is connected. Create contexts!
			XanBotLogger.WriteLine("Initializing User-Defined Bot Contexts...");
			BotContextRegistry.InitializeAllContexts();

			XanBotLogger.WriteLine("Connecting CommandMarshaller to chat events...");
#pragma warning disable CS1998
			Client.MessageCreated += async (evt) => {
				if (evt.Author == Client.CurrentUser) return;
				if (evt.Author.IsBot) return;
				if (CommandMarshaller.IsCommand(evt.Message.Content)) {
					XanBotLogger.WriteDebugLine("Message was sent and was detected as a command.");
					CommandMarshaller.HandleMessageCommand(evt.Message);
				} else {
					XanBotLogger.WriteDebugLine("Message was sent but was not a command. Throwing it into the passive handlers.");
					CommandMarshaller.RunPassiveHandlersForMessage(evt.Message);
				}
			};
#pragma warning restore CS1998

			XanBotLogger.WriteLine("Setting up console...");
			Console.CancelKeyPress += new ConsoleCancelEventHandler(OnCtrlCExit);
			XanBotConsoleCore.StartUpdateConsoleTask();

			Created = true;
			XanBotLogger.WriteLine("§aInitialization complete!"); // This fixes an occasional formatting issue where it renders in the typing text color.
		}

		/// <summary>
		/// Disconnects this discord bot from Discord. Should be used before shutdown.
		/// </summary>
		/// <returns></returns>
		public static void Destroy() {
			Client.DisconnectAsync().GetAwaiter().GetResult();
			Client.Dispose();
		}

		internal static bool WantsToExit { get; set; } = false;

		/// <summary>
		/// The method that safely exits the bot.
		/// </summary>
		/// <param name="code"></param>
		public static void Exit(int code = 0) {
			WantsToExit = true;
			XanBotLogger.WriteLine("Bot shutdown requested. Tying up loose ends...");

			PermissionRegistry.SaveAllUserPermissionsToFile();

			XanBotLogger.WriteLine("Finalizing shutdown.");
			Destroy();
			Environment.Exit(code);
		}

		/// <summary>
		/// The proxy for the exit function that fires when the bot presses CTRL+C or CTRL+BREAK
		/// </summary>
		/// <param name="src"></param>
		/// <param name="evt"></param>
		public static void OnCtrlCExit(object src, EventArgs evt) {
			Exit();
		}

		// n.b. These two would be const instead of static readonly, but this threw a bunch of "unreachable code" warnings on stuff that changed depending on debug mode or not.
		// Rather than use #pragma warning disable CS0162 over and over again as a bandaid solution, I just did this.
#if DEBUG
		/// <summary>
		/// Whether or not this application is running in debug mode or not.<para/>
		/// CURRENT STATE: TRUE
		/// </summary>
		public static readonly bool IsDebugMode = true;
#else
		/// <summary>
		/// Whether or not this application is running in debug mode or not.<para/>
		/// /// CURRENT STATE: FALSE
		/// </summary>
		public static readonly bool IsDebugMode = false;
#endif
	}
}
