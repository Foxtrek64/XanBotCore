using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XanBotCore.Exceptions;
using XanBotCore.UserObjects;

namespace XanBotCore.Utility {

	/// <summary>
	/// Offers utilities intended for getting users from certain queries e.g. username, discriminator, or GUID.
	/// </summary>
	public class UserGetter {

		/// <summary>
		/// Attempts to get a member via three distinct methods: First, it will attempt to cast <paramref name="data"/> into a ulong and see if it is a user GUID. If this fails, it will attempt to find it as a nickname or username.<para/>
		/// This will throw a NonSingularResultException if the query can return more than one user. 
		/// </summary>
		/// <param name="server">The Discord server to target.</param>
		/// <param name="data">The query to get a XanBotMember from. This can be a ulong as a string, a user ping (&lt;@ulong&gt;), a server nickname, a username (or optionally username#discriminator)</param>
		/// <returns></returns>
		public static async Task<XanBotMember> GetMemberFromData(DiscordGuild server, string data) {
			// Wait! If it's a ping, it will start with <@ and end with >
			string newdata = data;
			if (data.StartsWith("<@") && data.EndsWith(">")) {
				newdata = data.Substring(2, data.Length - 3);
			}
			if (ulong.TryParse(newdata, out ulong uuid)) {
				DiscordUser user = await XanBotCoreSystem.Client.GetUserAsync(uuid);

				// Catch case: Someone's username is a bunch of numbers OR they link a user ID that isn't in the server.
				if (user != null) {
					XanBotMember member = XanBotMember.GetMemberFromUser(server, user);
					if (member != null) {
						return member;
					}
				}
			}
			List<XanBotMember> potentialReturns = new List<XanBotMember>();
			string dataLower = data.ToLower();
			foreach (DiscordUser user in server.Members.Values) {
				XanBotMember member = XanBotMember.GetMemberFromUser(server, user);
				string fullName = member.FullName.ToLower();
				string nickName = member.Nickname ?? "";
				nickName = nickName.ToLower();
				if (fullName.Length >= dataLower.Length && dataLower == fullName.Substring(0, dataLower.Length)) {
					potentialReturns.Add(member);
				} else if (nickName.Length >= dataLower.Length && dataLower == nickName.Substring(0, dataLower.Length)) {
					potentialReturns.Add(member);
				}
				// Do NOT break if there are multiple. This is necessary for the values in a potential NonSingularResultException
			}
			XanBotMember[] potentialReturnsArray = potentialReturns.ToArray();
			if (potentialReturnsArray.Length == 0) {
				return null;
			} else if (potentialReturnsArray.Length == 1) {
				return potentialReturnsArray[0];
			} else {
				throw new NonSingularResultException(string.Format("More than one member of the server was found with the search query `{0}`!", data), potentialReturnsArray);
			}
		}

		/// <summary>
		/// Strictly gets a user from their user ID. Unlike <see cref="GetMemberFromData(DiscordGuild, string)"/>, this code will return null if a name or discriminator is passed in.
		/// </summary>
		/// <param name="data">The query to get a XanBotMember from. This can either be a ulong as a string or a user ping (&lt;@ulong&gt;)</param>
		/// <returns></returns>
		public static async Task<XanBotMember> GetMemberFromDataIDStrict(DiscordGuild server, string data) {
			string newdata = data;
			if (data.StartsWith("<@") && data.EndsWith(">")) {
				newdata = data.Substring(2, data.Length - 3);
			}
			if (ulong.TryParse(newdata, out ulong uuid)) {
				DiscordUser user = await XanBotCoreSystem.Client.GetUserAsync(uuid);

				// Catch case: Someone's username is a bunch of numbers OR they link a user ID that isn't in the server.
				if (user != null) {
					XanBotMember member = XanBotMember.GetMemberFromUser(server, user);
					if (member != null) {
						return member;
					}
				}
			}
			return null;
		}

	}
}
