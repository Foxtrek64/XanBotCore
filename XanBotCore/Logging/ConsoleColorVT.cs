using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace XanBotCore.Logging {
	public class ConsoleColorVT {

		public const string COLOR_CODE_REGEX = @"\^#([0-9]|[a-f]|[A-F]){6};";

		public static readonly IReadOnlyDictionary<ConsoleColor, ConsoleColorVT> ConsoleColorMap = new Dictionary<ConsoleColor, ConsoleColorVT> {
			[ConsoleColor.Black] = new ConsoleColorVT(0, 0, 0),
			[ConsoleColor.DarkBlue] = new ConsoleColorVT(0, 0, 128),
			[ConsoleColor.DarkGreen] = new ConsoleColorVT(0, 128, 0),
			[ConsoleColor.DarkCyan] = new ConsoleColorVT(0, 128, 128),
			[ConsoleColor.DarkRed] = new ConsoleColorVT(128, 0, 0),
			[ConsoleColor.DarkMagenta] = new ConsoleColorVT(128, 0, 128),
			[ConsoleColor.DarkYellow] = new ConsoleColorVT(128, 128, 0),
			[ConsoleColor.DarkGray] = new ConsoleColorVT(128, 128, 128),
			[ConsoleColor.Gray] = new ConsoleColorVT(192, 192, 192),
			[ConsoleColor.Blue] = new ConsoleColorVT(0, 0, 255),
			[ConsoleColor.Green] = new ConsoleColorVT(0, 255, 0),
			[ConsoleColor.Cyan] = new ConsoleColorVT(0, 255, 255),
			[ConsoleColor.Red] = new ConsoleColorVT(255, 0, 0),
			[ConsoleColor.Magenta] = new ConsoleColorVT(255, 0, 255),
			[ConsoleColor.Yellow] = new ConsoleColorVT(255, 255, 0),
			[ConsoleColor.White] = new ConsoleColorVT(255, 255, 255)
		};


		public byte R { get; }
		public byte G { get; }
		public byte B { get; }

		/// <summary>
		/// Construct a new <see cref="ConsoleColorVT"/> from the specified color value.
		/// </summary>
		/// <param name="r">The R component (0 - 255)</param>
		/// <param name="g">The G component (0 - 255)</param>
		/// <param name="b">The B component (0 - 255)</param>
		public ConsoleColorVT(byte r, byte g, byte b) {
			R = r;
			G = g;
			B = b;
		}

		/// <summary>
		/// Returns a <see cref="ConsoleColorVT"/> created by the default color for the input <see cref="ConsoleColor"/>. This does NOT reflect changes made to the console's palette, and instead returns the MS-DOS default for the specific color.
		/// </summary>
		/// <param name="stockColor"></param>
		public static ConsoleColorVT FromConsoleColor(ConsoleColor stockColor) {
			return ConsoleColorMap[stockColor];
		}

		/// <summary>
		/// Translates a formatted piece of text into a <see cref="ConsoleColorVT"/>. The text should be formatted like: ^#FFFFFF; (where FFFFFF is a hex color code.)
		/// </summary>
		/// <param name="formatted">The formatted text.</param>
		/// <returns>A <see cref="ConsoleColorVT"/> using the specified hex code.</returns>
		public static ConsoleColorVT FromFormattedString(string formatted) {
			if (formatted.Length != 9 || !Regex.IsMatch(formatted, COLOR_CODE_REGEX)) {
				// Length 9 enforces that it's *just* the code.
				throw new ArgumentException("Expected text formatted like ^#......; (where ...... translates into a hex color code).");
			}

			// Now treat it properly
			string hexClip = formatted.Substring(2, 6);
			int value = int.Parse(hexClip, System.Globalization.NumberStyles.HexNumber);
			byte r = (byte)((value & 0xFF0000) >> 16);
			byte g = (byte)((value & 0x00FF00) >> 8);
			byte b = (byte)(value & 0x0000FF);

			return new ConsoleColorVT(r, g, b);
		}

		/// <summary>
		/// Applies this <see cref="ConsoleColorVT"/> to the foreground.
		/// </summary>
		public void ApplyToForeground() {
			Console.Write(ToString());
		}

		/// <summary>
		/// Applies this <see cref="ConsoleColorVT"/> to the background;
		/// </summary>
		public void ApplyToBackground() {
			Console.Write(ToStringBG());
		}

		/// <summary>
		/// Returns the formatted code so that it can be applied to the foreground by writing the result of this function to the console. Call <see cref="ToStringBG"/> to get the background format.
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			return string.Format("\u001b[38;2;{0};{1};{2}m", R, G, B);
		}

		/// <summary>
		/// Returns the formatted code so that it can be applied to the background by writing the result of this function to the console. Call <see cref="ToString"/> to get the foreground format.
		/// </summary>
		/// <returns></returns>
		public string ToStringBG() {
			return string.Format("\u001b[48;2;{0};{1};{2}m", R, G, B);
		}

		public static implicit operator ConsoleColorVT(ConsoleColor src) {
			return FromConsoleColor(src);
		}
	}
}
