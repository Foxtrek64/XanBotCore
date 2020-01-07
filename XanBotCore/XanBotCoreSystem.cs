﻿using System;
using System.Threading;
using System.Threading.Tasks;
using XanBotCore.CommandSystem;
using XanBotCore.Logging;
using XanBotCore.Permissions;
using XanBotCore.ServerRepresentation;

namespace XanBotCore
{

    /// <summary>
    /// The heart of XanBotCore. This stores vital information about your discord bot, for instance, its Discord Client. It also provides methods to quickly connect a bot.
    /// </summary>
    public class XanBotCoreSystem
    {

        private static DiscordClient ClientInternal = null;
        private static VoiceNextExtension VoiceClientInternal = null;

        /// <summary>
        /// Runs when the bot is shutting down, before connections to Discord have been terminated.
        /// </summary>
        public static event ShutdownEvent OnBotShuttingDown;
        public delegate void ShutdownEvent();

        /// <summary>
        /// Whether or not the core has been instantiated and certain core values like <see cref="Client"/> exist.
        /// </summary>
        public static bool Created { get; private set; }

        /// <summary>
        /// The <see cref="DiscordClient"/> of the bot employing the use of XanBotCore.<para/>
        /// Throws a <see cref="InvalidOperationException"/> if the value for this is referenced before it is set -- It should be initialized immediately with the bot.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        public static DiscordClient Client
        {
            get
            {
                return ClientInternal ?? throw new InvalidOperationException("Failed to get Client -- XanBotCoreSystem.Client is null. Did you remember to set this value when you initialized your Discord bot?");
            }
            private set
            {
                ClientInternal = value;
            }
        }

        /// <summary>
        /// The <see cref="VoiceNextExtension"/> of the bot. This allows the bot to connect to voice channels to both send and receive audio.<para/>
        /// Throws a <see cref="InvalidOperationException"/> if the value for this is referenced before <see cref="Created"/> is true.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        public static VoiceNextExtension VoiceClient
        {
            get
            {
                // I do a non-null check since any BotContexts that initialize and get this will error if I don't.
                if (Created || VoiceClientInternal != null)
                    return VoiceClientInternal;
                throw new InvalidOperationException("Failed to get VoiceClient -- Cannot get this value before initialization.");
            }
            private set
            {
                VoiceClientInternal = value;
            }
        }

        /// <summary>
        /// True if all of the data has finished downloading (when <see cref="DiscordClient.GuildDownloadCompleted"/> is fired, this is set to true).
        /// </summary>
        public static bool HasFinishedGettingGuildData { get; private set; } = false;

        /// <summary>
        /// Automatically creates <see cref="Client"/> and connects it, creates <see cref="VoiceClient"/> (if requested), instantiates all bot contexts automatically, starts the <see cref="XanBotConsoleCore"/> update loop, and binds the windows console CTRL+C/CTRL+BREAK to <see cref="Exit(int)>"/>
        /// </summary>
        /// <param name="botToken">The token of this discord bot.</param>
        /// <param name="createVoiceNextClient">If true, <see cref="VoiceClient"/> will be instantiated and allow this bot to connect to voice channels.</param>
        /// <param name="isTargettingWindows7">Whether or not this bot is running on Windows 7. This is necessary for some init code.</param>
        /// <param name="yieldUntilGuildsDownloaded">If true, this task will yield until <see cref="DiscordClient.GuildDownloadCompleted"/> is fired.</param>
        /// <returns></returns>
        public static async Task InitializeBotAsync(string botToken, bool createVoiceNextClient = false, bool isTargettingWindows7 = false, bool yieldUntilGuildsDownloaded = false)
        {
            Console.ForegroundColor = ConsoleColor.Green;

            XanBotLogger.IsPathLocked = true;
            if (IsDebugMode)
            {
                XanBotLogger.WriteLine("§eInitializing in Debug Mode...");
            }
            else
            {
                XanBotLogger.WriteLine("§2Initializing...");
            }
            if (isTargettingWindows7)
            {
                Client = new DiscordClient(new DiscordConfiguration
                {
                    Token = botToken,
                    TokenType = TokenType.Bot,
                    WebSocketClientFactory = new WebSocketClientFactoryDelegate(WebSocket4NetClient.CreateNew), // Remove if this bot is running on machine with Windows 10.
                });
            }
            else
            {
                Client = new DiscordClient(new DiscordConfiguration
                {
                    Token = botToken,
                    TokenType = TokenType.Bot
                });
            }
            XanBotLogger.WriteDebugLine("Created DiscordClient.");

            if (createVoiceNextClient)
            {
                VoiceClient = Client.UseVoiceNext(new VoiceNextConfiguration
                {
                    AudioFormat = new AudioFormat(48000, 2, VoiceApplication.LowLatency)
                });
                XanBotLogger.WriteDebugLine("Created VoiceNextClient.");
            }

            XanBotLogger.WriteLine("§2Connecting to Discord...");
            await Client.ConnectAsync();

            // Client is connected. Create contexts!
            XanBotLogger.WriteLine("§2Initializing User-Defined Bot Contexts...");
            BotContextRegistry.InitializeAllContexts();

            XanBotLogger.WriteLine("§2Connecting CommandMarshaller to chat events...");
            Client.MessageCreated += async evt =>
            {
                //XanBotLogger.WriteLine("Hey the message created event fired");
                if (evt.Author == Client.CurrentUser)
                    return;
                if (evt.Author.IsBot)
                    return;
                if (CommandMarshaller.IsCommand(evt.Message.Content))
                {
                    XanBotLogger.WriteDebugLine("Message was sent and was detected as a command.");
                    await CommandMarshaller.HandleMessageCommand(evt.Message);
                }
                else
                {
                    XanBotLogger.WriteDebugLine("Message was sent but was not a command. Throwing it into the passive handlers.");
                    await CommandMarshaller.RunPassiveHandlersForMessage(evt.Message);
                }
            };

#pragma warning disable CS1998
            if (yieldUntilGuildsDownloaded)
            {
                XanBotLogger.WriteLine("§2Downloading server data from Discord...");
                ManualResetEvent completedSignal = new ManualResetEvent(false);
                Client.GuildDownloadCompleted += async evt =>
                {
                    HasFinishedGettingGuildData = true;
                    completedSignal?.Set();
                };
                completedSignal.WaitOne();
                completedSignal.Dispose();
                completedSignal = null;
            }
            else
            {
                Client.GuildDownloadCompleted += async evt =>
                {
                    HasFinishedGettingGuildData = true;
                };
            }
#pragma warning restore CS1998

            XanBotLogger.WriteLine("§2Setting up frontend console...");
            Console.CancelKeyPress += new ConsoleCancelEventHandler(OnCtrlCExit);
            XanBotConsoleCore.StartUpdateConsoleTask();

            Created = true;
            XanBotLogger.WriteLine("§aInitialization complete!"); // This fixes an occasional formatting issue where it renders in the typing text color.
        }

        /// <summary>
        /// Disconnects this discord bot from Discord. Should be used before shutdown.
        /// </summary>
        /// <returns></returns>
        public static void Destroy()
        {
            Client.DisconnectAsync().GetAwaiter().GetResult();
            Client.Dispose();
        }

        internal static bool WantsToExit { get; set; } = false;

        /// <summary>
        /// The method that safely exits the bot.
        /// </summary>
        /// <param name="code"></param>
        public static void Exit(int code = 0)
        {
            WantsToExit = true;
            XanBotLogger.WriteLine("Bot shutdown requested. Tying up loose ends...");

            PermissionRegistry.SaveAllUserPermissionsToFile();

            XanBotLogger.WriteLine("Finalizing shutdown.");

            PerformExitOperations();
            Destroy();
            Environment.Exit(code);
        }



        /// <summary>
        /// Safely fires <see cref="OnBotShuttingDown"/>.
        /// </summary>
        private static void PerformExitOperations()
        {
            try
            {
                OnBotShuttingDown();
            }
            catch (Exception) { }
        }

        /// <summary>
        /// The proxy for the exit function that fires when the bot presses CTRL+C or CTRL+BREAK
        /// </summary>
        /// <param name="src"></param>
        /// <param name="evt"></param>
        public static void OnCtrlCExit(object src, EventArgs evt)
        {
            Exit();
        }

        // n.b. These two would be const instead of static readonly, but this threw a bunch of "unreachable code" warnings on stuff that changed depending on debug mode or not.
        // Rather than use #pragma warning disable CS0162 over and over again as a bandaid solution, I just did this.
#if DEBUG
        /// <summary>
        /// Whether or not this application is running in debug mode or not.<para/>
        /// /// CURRENT STATE: TRUE ///
        /// </summary>
        public static bool IsDebugMode { get; } = true;
#else
		/// <summary>
		/// Whether or not this application is running in debug mode or not.<para/>
		/// /// CURRENT STATE: FALSE ///
		/// </summary>
		public static bool IsDebugMode { get; } = false;
#endif
    }
}
