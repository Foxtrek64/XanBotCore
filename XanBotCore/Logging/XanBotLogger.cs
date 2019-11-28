using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XanBotCore.DataPersistence;

namespace XanBotCore.Logging {

	/// <summary>
	/// A console logging utility. Offers the ability to format messages in a manner not unlike that of Minecraft's color code system. See https://minecraft.gamepedia.com/Formatting_codes#Color_codes</summary>
	/// </summary>
	public class XanBotLogger {

		/// <summary>The time when this class is initialized into memory. Used for the log file name. This value does not change.</summary>
		private static readonly string CURRENT_TIME = DateTime.UtcNow.ToFileTime().ToString();

		/// <summary>The current console log in a single string.</summary>
		private static string Log = ""; // To do: Not do this, since having extremely long string buffers can get laggy.

		/// <summary>The symbol recognized in messages for color codes. This is identical to Minecraft's color code system. See https://minecraft.gamepedia.com/Formatting_codes#Color_codes</summary>
		public static readonly char COLOR_CODE_SYM = '§';

		/// <summary>A map of byte code values to ConsoleColors</summary>
		public static readonly IReadOnlyDictionary<byte, ConsoleColor> ConsoleColorMap = new Dictionary<byte, ConsoleColor> {
			[0] = ConsoleColor.Black,
			[1] = ConsoleColor.DarkBlue,
			[2] = ConsoleColor.DarkGreen,
			[3] = ConsoleColor.DarkCyan,
			[4] = ConsoleColor.DarkRed,
			[5] = ConsoleColor.DarkMagenta,
			[6] = ConsoleColor.DarkYellow,
			[7] = ConsoleColor.DarkGray,
			[8] = ConsoleColor.Gray,
			[9] = ConsoleColor.Blue,
			[10] = ConsoleColor.Green,
			[11] = ConsoleColor.Cyan,
			[12] = ConsoleColor.Red,
			[13] = ConsoleColor.Magenta,
			[14] = ConsoleColor.Yellow,
			[15] = ConsoleColor.White
		};

		/// <summary>
		/// Returns a formatted timestamp: "[HH:MM:SS] "
		/// </summary>
		/// <returns>Returns a formatted timestamp: "[HH:MM:SS] "</returns>
		public static string GetFormattedTimestamp() {
			TimeSpan currentTime = DateTime.Now.TimeOfDay;
			return "[" + currentTime.Hours.ToString("D2") + ":" + currentTime.Minutes.ToString("D2") + ":" + currentTime.Seconds.ToString("D2") + "] ";
		}

		/// <summary>
		/// Returns a message formatted as an error message for display in text channels.
		/// </summary>
		public static string GetFormattedResponseErrMsg(Exception ex) {
			return "```diff\n-" + ex.GetType().ToString() + "-\n" + ex.Message + "\n" + ex.StackTrace + "```";
		}

		/// <summary>
		/// Writes errors to the console and plays a beep sound to alert the operator.
		/// </summary>
		public static void WriteException(Exception ex) {
			WriteLine("§c[" + ex.GetType().ToString() + " Thrown!] §e" + ex.Message + "\n§4" + ex.StackTrace, true);
		}

		/// <summary>
		/// Logs the string to the log file without showing in the console.
		/// </summary>
		/// <param name="message">The string to log.</param>
		public static void LogMessage(string message) {
			Log += GetFormattedTimestamp() + message + "\n";
			WriteLogFile();
		}

		/// <summary>
		/// Returns `true` if the message contains the color code symbol and, by extension, a color code.
		/// </summary>
		public static bool MessageHasColors(string message) {
			return message.Contains(COLOR_CODE_SYM);
		}

		/// <summary>
		/// Strips away all color formatting stuff from a message so that it's just plain text.
		/// </summary>
		public static string StripColorFormattingCode(string message) {
			string[] colorSegs = message.Split(COLOR_CODE_SYM);
			//colorSegs[0] will be empty if we have a color code at the start.
			string res = "";
			foreach (string coloredString in colorSegs) {
				if (coloredString.Length >= 1) {
					if (coloredString == colorSegs.First()) {
						if (message.Substring(0, 1) == COLOR_CODE_SYM.ToString()) {
							res += coloredString.Substring(1);
						}
						else {
							res += coloredString;
						}
					}
					else {
						res += coloredString.Substring(1);
					}
				}
				else {
					res += coloredString;
				}
			}
			return res;
		}


		/// <summary>
		/// Writes a color coded console message, like text in Minecraft.
		/// </summary>
		public static void WriteMessageFromColors(string message) {
			string[] colorSegs = message.Split(COLOR_CODE_SYM);
			//colorSegs[0] will be empty if we have a color code at the start.
			ConsoleColor defColor = Console.ForegroundColor;
			XanBotConsoleCore.BumpIncomingLogTextPre();
			foreach (string coloredString in colorSegs) {
				if (coloredString.Length > 1) {
					byte code = 255;
					try { code = Convert.ToByte(coloredString.First().ToString(), 16); } catch (Exception) { }
					bool success = ConsoleColorMap.TryGetValue(code, out ConsoleColor color);
					if (!success) {
						//WriteLine("ERROR: Failed to parse text color! Unknown color code " + coloredString.First());
						return;
					}

					Console.ForegroundColor = color;
					Console.Write(coloredString.Substring(1));
				}
			}
			Console.ForegroundColor = defColor;
			XanBotConsoleCore.BumpIncomingLogTextPost();
		}

		/// <summary>
		/// Log some text on a single line, and make a newline afterwards.
		/// </summary>
		/// <param name="message">The text to log.</param>
		/// <param name="alertSound">If true, this message will cause the console to beep.</param>
		/// <param name="isDebugModeOnly">If true, this will only log if the bot is in debug mode.</param>
		public static void WriteLine(string message = "", bool alertSound = false, bool isDebugModeOnly = false) {
			if (isDebugModeOnly && !XanBotCoreSystem.IsDebugMode) return;
			ClearConsoleIfNecessary();
			if (alertSound) Console.Beep();
			message += "\n";
			string timestamp = GetFormattedTimestamp();
			string logMessage = message;
			if (MessageHasColors(logMessage)) logMessage = StripColorFormattingCode(logMessage);
			Log += timestamp + logMessage;
			WriteMessageFromColors(COLOR_CODE_SYM + "2" + timestamp + COLOR_CODE_SYM + "a" + message);
			WriteLogFile();
		}

		/// <summary>
		/// An alias method that calls <see cref="WriteLine"/> with its <paramref name="isDebugModeOnly"/> parameter set to true.<para/>
		/// This also enforces the text to be darkgray by default. It is possible to override this normally.
		/// </summary>
		/// <param name="message">The message to display.</param>
		/// <param name="alertSound">If true, this will only log if the bot is in debug mode.</param>
		public static void WriteDebugLine(string message = "", bool alertSound = false) {
			message = "§7" + message;
			WriteLine(message, alertSound, true);
		}

		/// <summary>
		/// Write the currently cached program log to the log file at <paramref name="dir"/>, or the current log file if null.
		/// </summary>
		/// <param name="dir">A file path for a log file. This should ideally be a text document with the .log extension.</param>
		public static void WriteLogFile(string dir = null) {
			if (dir == null) {
				dir = ".\\latest-" + CURRENT_TIME + ".log";
			}
			Stream logFileStream = File.AppendText(dir).BaseStream;
			char[] chars = Log.ToCharArray();
			logFileStream.Write(Encoding.UTF8.GetBytes(chars), 0, chars.Length);
			logFileStream.Close();
			Log = ""; //Reset it.
		}

		/// <summary>
		/// In case the console runs out of buffer space, clear it out. This is where log files come in handy because the console has limited display.
		/// </summary>
		public static void ClearConsoleIfNecessary() { 
			if (Console.CursorTop >= Console.BufferHeight - 50) {
				Console.Clear();
				Console.ForegroundColor = ConsoleColor.DarkCyan;
				Console.WriteLine("-- CONSOLE CLEARED TO RESET BUFFER --");
				LogMessage("-- CONSOLE CLEARED TO RESET BUFFER --");
				Console.ForegroundColor = ConsoleColor.Green;
			}
		}


		/// <summary>
		/// Gets the latest log file in its current state as a FileStream. Used by <seealso cref="CommandSystem.Commands.CommandShutdown"/> to post the latest log on bot shutdown.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static FileStream GetLatestLogFileStream(string path = null) {
			if (path == null) {
				path = ".\\latest-" + CURRENT_TIME + ".log";
			}
			return File.OpenRead(path);
		}
	}
}
