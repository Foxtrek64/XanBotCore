using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using XanBotCore.CommandSystem;

namespace XanBotCore.Logging {

	/// <summary>
	/// The core systems running Console input.<para/>
	/// Provides a special input system for the default Windows Console as well as utilities for logging to not interrupt operator input in the console (e.g. by moving the buffer)
	/// </summary>
	public class XanBotConsoleCore {
		/// <summary>The default output stream for the Console.</summary>
		private static readonly TextWriter CONSOLE_WRITER = Console.Out;

		/// <summary>A null output stream.</summary>
		private static readonly TextWriter NULL_WRITER = TextWriter.Null;

		/// <summary>The cache of commands used in the console for ease of access.</summary>
		private static readonly List<string> CommandCache = new List<string>();

		/// <summary>The current index in the command cache. Used when moving between cache entries with the up or down arrow keys.</summary>
		private static int CommandCachePosition = 0;

		/// <summary>The currently typed text in the console. Used when the console needs to be written to so that the input buffer display can be bumped down as new text is written into the console.</summary>
		private static string CurrentlyTypedConsoleText = "";
		//private static int LastLeft = 0;
		private static int LastTop = 0;

		/// <summary>
		/// Tells the console that something is about to be written and that the currently typed text should be temporarily removed so that the operation may continue without interruption.
		/// </summary>
		public static void BumpIncomingLogTextPre() {
			//LastLeft = Console.CursorLeft;
			LastTop = Console.CursorTop;
			Console.CursorLeft = 0;
			Console.Write(new string(' ', Console.BufferWidth));
			Console.CursorLeft = 0;
			Console.CursorTop = LastTop;
			Console.ForegroundColor = ConsoleColor.Green;
		}

		/// <summary>
		/// Tells the console that any writing has been completed and that it can put the current typed text back into the display area.
		/// </summary>
		public static void BumpIncomingLogTextPost() {
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.Write(CurrentlyTypedConsoleText);
		}

		/// <summary>
		/// Gets the contents of the operator's clipboard via a rather hacky reference to System.Windows.Clipboard and managed threads.<para/>
		/// TODO: See if there is a better way to access the user clipboard on Windows 7.
		/// </summary>
		/// <returns>The text on the operator's Clipboard.</returns>
		private static string GetClipboard() {
			//This, unfortunately, needs to go through a lot of hoops to work.
			//https://stackoverflow.com/questions/518701/clipboard-gettext-returns-null-empty-string

			string resultString = null;
			//IDataObject idat = null;
			Thread STAThread = new Thread(
				delegate () {
					try {
						resultString = Clipboard.GetText();
						//idat = Clipboard.GetDataObject();
					}
					catch (Exception ex) {
						XanBotLogger.WriteException(ex);
					}
				}
			);
			STAThread.SetApartmentState(ApartmentState.STA);
			STAThread.Start();
			STAThread.Join();

			//resultString = idat.GetData(DataFormats.UnicodeText, true).ToString();

			if (resultString?.Length == 0) return null;
			return resultString;
		}

		/// <summary>
		/// Should be called from a while loop in the bot's core thread.<para/>
		/// This runs an update cycle on the console, which listens for operator input and updates display based on said input, and initiates commands if the user presses return in the console.
		/// </summary>
		private static void UpdateConsole() {
			CurrentlyTypedConsoleText = "";
			Console.ForegroundColor = ConsoleColor.Cyan;

			ConsoleKeyInfo kInfo = Console.ReadKey(false);
			char c = kInfo.KeyChar;

			while (c != '\n' && c != '\r') {
				// Didn't press enter.

				// Up arrow. Go up one in the command cache if possible.
				if (kInfo.Key == ConsoleKey.UpArrow) {
					CommandCachePosition = Math.Max(CommandCachePosition - 1, 0);
					CurrentlyTypedConsoleText = CommandCache[CommandCachePosition];
					Console.CursorLeft = 0;
					Console.Write(new string(' ', Console.BufferWidth - 1));
					Console.CursorLeft = 0;
					Console.Write(CurrentlyTypedConsoleText);
				}
				// Down arrow. Go down one in the command cache if possible, or display empty text. To do: Make it display whatever was there if we typed something, hit up, then hit down again.
				else if (kInfo.Key == ConsoleKey.DownArrow) {
					if (CommandCachePosition + 1 == CommandCache.Count) {
						//We'd be going after the last index.
						CurrentlyTypedConsoleText = "";
					}
					else {
						CurrentlyTypedConsoleText = CommandCache[CommandCachePosition + 1];
					}
					CommandCachePosition = Math.Min(CommandCachePosition + 1, CommandCache.Count - 1);

					Console.CursorLeft = 0;
					Console.Write(new string(' ', Console.BufferWidth - 1));
					Console.CursorLeft = 0;
					Console.Write(CurrentlyTypedConsoleText);
				}

				// Backspace. Erase text.
				if (c == '\b') {
					if (CurrentlyTypedConsoleText.Length > 0) {
						//Backspace. Remove the char.
						Console.SetOut(CONSOLE_WRITER);
						Console.CursorLeft = Math.Max(Console.CursorLeft - 1, 0);
						Console.Write(' ');
						Console.CursorLeft = 0;
						CurrentlyTypedConsoleText = CurrentlyTypedConsoleText.Substring(0, CurrentlyTypedConsoleText.Length - 1);
						Console.Write(new string(' ', Console.BufferWidth - 1));
						Console.CursorLeft = 0;
						Console.Write(CurrentlyTypedConsoleText);
					}
					// Used to beep if you tried to erase without anything typed. Got annoying. Disabled it.
					//else {
					//	Console.Beep();
					//}
				}

				// If we're trying to type and there's enough space...
				if (CurrentlyTypedConsoleText.Length < Console.BufferWidth - 1) {
					if (c != '\b' && (kInfo.Modifiers & ConsoleModifiers.Control) == 0) {
						// Ctrl isn't behind held and it's not a backspace. Append the currently typed text.
						CurrentlyTypedConsoleText += c;
						if (CurrentlyTypedConsoleText.Length == Console.BufferWidth - 1) {
							// If the length of the current text is now the buffer width, don't let it type anymore.
							// I would allow multi-line editing but this was quite problematic and I just didn't bother.
							Console.SetOut(NULL_WRITER);
						}
					}
				}
				else {
					// Beep if we can't type any more text.
					Console.Beep();
				}


				// Holding Ctrl...
				if ((kInfo.Modifiers & ConsoleModifiers.Control) != 0) {
					if (c == 0x16) { //0x16 SYN aka CTRL+V was input. Paste.
						string clipboard = GetClipboard();
						if (clipboard != null) {
							// Try to add the text to the current typed stuff.
							string resultText = CurrentlyTypedConsoleText + clipboard;
							if (resultText.Length < Console.BufferWidth - 1) {
								// Append the text to the currently typed text.
								CurrentlyTypedConsoleText += clipboard;
								Console.CursorLeft = 0;
								Console.Write(new string(' ', Console.BufferWidth - 1));
								Console.CursorLeft = 0;
								Console.Write(CurrentlyTypedConsoleText);
							}
							else {
								// If the text is too long after adding the pasted content, beep and don't add it.
								Console.Beep();
							}
							// Old clipboard test. Failed.
							//Stream consoleOut = Console.OpenStandardOutput();
							//consoleOut.Write(Encoding.Default.GetBytes(clipboard), 0, clipboard.Length);
							//Console.CursorLeft--;
							//Console.Out.Write(clipboard);
						}
						else {
							// Beep if nothing is on the clipboard that can be pasted.
							Console.Beep();
						}
					}
				}

				// Prep for loop repeat.
				kInfo = Console.ReadKey(false);
				c = kInfo.KeyChar;
			}

			// If we make it here, we've pressed enter. Log the input text in the console.
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			Console.Write("CONSOLE ISSUED >> ");
			Console.ForegroundColor = ConsoleColor.DarkGreen;
			Console.Write(CurrentlyTypedConsoleText);
			Console.ForegroundColor = ConsoleColor.Green;

			string cmd = CurrentlyTypedConsoleText;
			CurrentlyTypedConsoleText = "";
			Console.CursorLeft = 0;
			Console.CursorTop++;
			try {
				XanBotLogger.LogMessage("CONSOLE ISSUED >> " + cmd);
				CommandMarshaller.HandleCommand(cmd).GetAwaiter().GetResult();
				CommandCache.Add(cmd);
				CommandCachePosition = CommandCache.Count;
			}
			catch (Exception ex) {
				// In this case, the command failed. We should clear the typed console text.
				// Write the exception to the console.
				XanBotLogger.WriteException(ex);
			}
		}

		/// <summary>
		/// Starts the update console task, which handles console input.
		/// </summary>
		/// <returns></returns>
		public static void StartUpdateConsoleTask() {
			try {
				Task.Run(() => {
					while (!XanBotCoreSystem.WantsToExit) {
						UpdateConsole();
					}
				});
			} catch (Exception ex) {
				// Will probably be a TaskCanceledException
				XanBotLogger.WriteException(ex);
				XanBotCoreSystem.Exit(1);
			}
		}
	}
}
