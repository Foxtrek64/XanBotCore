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
		/// Return this <see cref="DiscordMember"/>'s name formatted as: [Nickname] Username#Discriminator. If the user has no nickname, it returns <see cref="GetFullName(DiscordUser)"/> instead (which has no nickname)
		/// </summary>
		/// <returns></returns>
		public static string GetFullName(this DiscordMember member) {
			if (member.Nickname != null && member.Nickname != default && member.Nickname != "") {
				return "[" + member.Nickname + "] " + member.Username + "#" + member.Discriminator;
			}
			return GetFullName((DiscordUser)member);
		}

		/// <summary>
		/// Formats this <see cref="DiscordUser"/>'s display name based on the specified <see cref="DisplayType"/>
		/// </summary>
		/// <param name="showAs">The <see cref="DisplayType"/> used to determine how the name is formatted.</param>
		/// <returns></returns>
		public static string GetFormattedUser(this DiscordUser user, DisplayType showAs = DisplayType.UserId) {
			string extra = "";
			if (showAs == DisplayType.UserId) {
				extra = $" (UserID `{user.Id}`)";
			} else if (showAs == DisplayType.Mention) {
				extra = $" ({user.Mention})";
			} else if (showAs == DisplayType.UserIdAndMention) {
				extra = $" (UserID `{user.Id}` | {user.Mention})";
			}

			return GetFullName(user) + extra;
		}

		public static string GetFormattedMember(this DiscordMember member, DisplayType showAs = DisplayType.NicknameUserId) {
			string extra = "";
			if (showAs == DisplayType.NicknameUserId) {
				extra = $" (UserID `{member.Id})`";
			} else if (showAs == DisplayType.NicknameMention) {
				extra = $" ({member.Mention})";
			} else if (showAs == DisplayType.NicknameUserIdAndMention) {
				extra = $" (UserID `{member.Id}` | {member.Mention})";
			} else {
				return GetFormattedUser(member, showAs);
			}

			return GetFullName(member) + extra;
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
		/// Username#Discriminator (UserID `id_here`)
		/// </summary>
		UserId,

		/// <summary>
		/// Username#Disciminator (&lt;@id_here&gt;)
		/// </summary>
		Mention,

		/// <summary>
		/// Username#Discriminator (UserID `id_here` | &lt;@id_here&gt;)
		/// </summary>
		UserIdAndMention,

		/// <summary>
		/// CAN ONLY BE USED ON MEMBERS, NOT USERS.<para/>
		/// [Nickname] Username#Discriminator (UserID `id_here`)
		/// </summary>
		NicknameUserId,

		/// <summary>
		/// CAN ONLY BE USED ON MEMBERS, NOT USERS.<para/>
		/// [Nickname] Username#Discriminator (&lt;@id_here&gt;)
		/// </summary>
		NicknameMention,

		/// <summary>
		/// CAN ONLY BE USED ON MEMBERS, NOT USERS.<para/>
		/// [Nickname] Username#Discriminator (UserID `id_here` | &lt;@id_here&gt;)
		/// </summary>
		NicknameUserIdAndMention,
	}
}
