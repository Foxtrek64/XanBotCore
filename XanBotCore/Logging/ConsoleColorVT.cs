﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using XanBotCore.Utility;

namespace XanBotCore.Logging
{
    public class ConsoleColorVT
    {

        public const string COLOR_CODE_REGEX = @"\^#([0-9]|[a-f]|[A-F]){6};";

        public static IReadOnlyDictionary<ConsoleColor, ConsoleColorVT> ConsoleColorMap => Colors;

        private static readonly Dictionary<ConsoleColor, ConsoleColorVT> Colors = new Dictionary<ConsoleColor, ConsoleColorVT>
        {
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


        public byte R { get; } = 0;
        public byte G { get; } = 0;
        public byte B { get; } = 0;

        /// <summary>
        /// Construct a new <see cref="ConsoleColorVT"/> from the specified color value.
        /// </summary>
        /// <param name="r">The R component (0 - 255)</param>
        /// <param name="g">The G component (0 - 255)</param>
        /// <param name="b">The B component (0 - 255)</param>
        public ConsoleColorVT(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        /// <summary>
        /// Returns a <see cref="ConsoleColorVT"/> created by the default color for the input <see cref="ConsoleColor"/>. This does NOT reflect changes made to the console's palette, and instead returns the MS-DOS default for the specific color.
        /// </summary>
        /// <param name="stockColor"></param>
        public static ConsoleColorVT FromConsoleColor(ConsoleColor stockColor)
        {
            return ConsoleColorMap[stockColor];
        }

        /// <summary>
        /// Translates a formatted piece of text into a <see cref="ConsoleColorVT"/>. The text should be formatted like: ^#FFFFFF; (where FFFFFF is a hex color code.)
        /// </summary>
        /// <param name="formatted">The formatted text.</param>
        /// <returns>A <see cref="ConsoleColorVT"/> using the specified hex code.</returns>
        /// <exception cref="ArgumentException"/>
        public static ConsoleColorVT FromFormattedString(string formatted)
        {
            if (formatted.Length != 9 || !Regex.IsMatch(formatted, COLOR_CODE_REGEX))
            {
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
        public void ApplyToForeground()
        {
            Console.Write(ToString());
        }

        /// <summary>
        /// Applies this <see cref="ConsoleColorVT"/> to the background;
        /// </summary>
        public void ApplyToBackground()
        {
            Console.Write(ToStringBG());
        }

        /// <summary>
        /// Attempts to return this <see cref="ConsoleColorVT"/> as a <see cref="ConsoleColor"/> based on its color. If this <see cref="ConsoleColorVT"/> does not match up with a <see cref="ConsoleColor"/>, this will return null.
        /// </summary>
        /// <returns></returns>
        public ConsoleColor? AsConsoleColor()
        {
            if (Colors.Values.Contains(this))
                return Colors.KeyOf(this);
            return null;
        }

        /// <summary>
        /// Returns the formatted code so that it can be applied to the foreground by writing the result of this function to the console. Call <see cref="ToStringBG"/> to get the background format.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("\u001b[38;2;{0};{1};{2}m", R, G, B);
        }


        /// <summary>
        /// Returns the formatted code so that it can be applied to the foreground.
        /// </summary>
        /// <param name="asVT">If false, this will return the result of <see cref="ToStringNonVT"/>. Otherwise, this will return the result of <see cref="ToString"/></param>
        /// <exception cref="NotSupportedException"/>
        public string ToString(bool asVT)
        {
            return asVT ? ToString() : ToStringNonVT();
        }

        /// <summary>
        /// Returns the formatted code so that it can be applied to the background by writing the result of this function to the console. Call <see cref="ToString"/> to get the foreground format.
        /// </summary>
        /// <returns></returns>
        public string ToStringBG()
        {
            return string.Format("\u001b[48;2;{0};{1};{2}m", R, G, B);
        }


        /// <summary>
        /// A tostring method that uses legacy coloring when translating this ConsoleColorVT to a string. This will throw a <see cref="NotSupportedException"/> if this <see cref="ConsoleColorVT"/> is not using a color possible via stock console colors.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"/>
        public string ToStringNonVT()
        {
            if (ConsoleColorMap.Values.Contains(this))
            {
                return "§" + ((Dictionary<byte, ConsoleColor>)XanBotLogger.ConsoleColorMap).KeyOf(AsConsoleColor().Value).ToString("X");
            }
            throw new NotSupportedException("This ConsoleColorVT does not have a color identical to a stock Windows console.");
        }

        public static implicit operator ConsoleColorVT(ConsoleColor src)
        {
            return FromConsoleColor(src);
        }

        public static bool operator ==(ConsoleColorVT alpha, ConsoleColorVT bravo)
        {
            if (ReferenceEquals(alpha, null))
                return ReferenceEquals(bravo, null);
            if (ReferenceEquals(bravo, null))
                return false;
            return alpha.Equals(bravo);
        }

        public static bool operator !=(ConsoleColorVT alpha, ConsoleColorVT bravo)
        {
            if (ReferenceEquals(alpha, null))
                return !ReferenceEquals(bravo, null);
            if (ReferenceEquals(bravo, null))
                return true;
            return !alpha.Equals(bravo);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;
            if (obj is ConsoleColorVT other)
            {
                return (R == other.R) && (G == other.G) && (B == other.B);
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 13;
            hash = (hash * 7) ^ R;
            hash = (hash * 13) + G;
            hash = (hash * 7) | B;
            return hash;
        }
    }
}
