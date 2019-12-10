using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XanBotCore.UserObjects {
	public static class DiscordUserExtensions {

		/// <summary>
		/// Return this <see cref="DiscordUser"/>'s name formatted as: Username#Discriminator
		/// </summary>
		/// <returns></returns>
		public static string GetFullName(this DiscordUser user) {
			return user.Username + "#" + user.Discriminator;
		}

		/// <summary>
		/// Formats this <see cref="DiscordUser"/>'s display name based on the specified <see cref="DisplayType"/>
		/// </summary>
		/// <param name="showAs">The <see cref="DisplayType"/> used to determine how the name is formatted.</param>
		/// <returns></returns>
		public static string GetFormattedUser(this DiscordUser user, DisplayType showAs = DisplayType.UserId) {
			string extra = "";
			if (showAs == DisplayType.UserId) {
				extra = $" (UserID {user.Id})";
			} else if (showAs == DisplayType.Mention) {
				extra = $" ({user.Mention})";
			} else if (showAs == DisplayType.UserIdAndMention) {
				extra = $" (UserID {user.Id} | {user.Mention})";
			}

			return GetFullName(user) + extra;
		}
	}

	/// <summary>
	/// A method of displaying a formatted user.
	/// </summary>
	public enum DisplayType {
		/// <summary>
		/// Username#Discriminator
		/// </summary>
		None,

		/// <summary>
		/// Username#Discriminator (UserID id_here)
		/// </summary>
		UserId,

		/// <summary>
		/// Username#Disciminator (&lt;@id_here&gt;)
		/// </summary>
		Mention,

		/// <summary>
		/// Username#Discriminator (UserID id_here | &lt;@id_here&gt;)
		/// </summary>
		UserIdAndMention
	}
}
