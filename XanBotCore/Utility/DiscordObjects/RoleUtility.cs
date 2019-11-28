using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XanBotCore.Utility.DiscordObjects {

	/// <summary>
	/// Offers some handy utilities for roles, primarily used in events pertaning to member roles being updated.
	/// </summary>
	public class RoleUtility {

		/// <summary>
		/// Returns a formatted string showing the attribute differences for the specified role.<para/>
		/// Intended for use in a <see cref="DiscordMessage"/> or <see cref="DiscordEmbed"/>
		/// </summary>
		/// <param name="oldRole"></param>
		/// <param name="newRole"></param>
		/// <returns></returns>
		public static string GetRoleDiffs(DiscordRole oldRole, DiscordRole newRole) {
			StringBuilder diffs = new StringBuilder();
			if (oldRole.Name != newRole.Name) diffs.AppendLine($"**Name:** {oldRole.Name} => {newRole.Name}");
			if (oldRole.Color.Value != newRole.Color.Value) diffs.AppendLine($"**Color:** {oldRole.Color.ToString()} => {newRole.Color.ToString()}");
			if (oldRole.IsHoisted != newRole.IsHoisted) diffs.AppendLine($"**Display Role Separately:** {oldRole.IsHoisted} => {newRole.IsHoisted}");
			if (oldRole.IsMentionable != newRole.IsMentionable) diffs.AppendLine($"**Mentionable:** {oldRole.IsMentionable} => {newRole.IsMentionable}");
			if (oldRole.Position != newRole.Position) diffs.AppendLine($"**Position:** {oldRole.Position} => {newRole.Position}");

			string ret = diffs.ToString();
			if (ret.Length == 0) return "No changes";
			return ret;
		}

		/// <summary>
		/// Returns a formatted string showing the permission differences for the specified role.<para/>
		/// Intended for use in a <see cref="DiscordMessage"/> or <see cref="DiscordEmbed"/>
		/// </summary>
		/// <param name="oldRole"></param>
		/// <param name="newRole"></param>
		/// <returns></returns>
		public static string GetRolePermissionDiffs(DiscordRole oldRole, DiscordRole newRole) {
			StringBuilder diffs = new StringBuilder();
			foreach (DSharpPlus.Permissions permission in Enum.GetValues(typeof(DSharpPlus.Permissions))) {
				PermissionLevel oldLvl = oldRole.CheckPermission(permission);
				PermissionLevel newLvl = newRole.CheckPermission(permission);
				if (oldLvl != newLvl) {
					diffs.AppendLine($"**{permission.ToPermissionString()}**: {oldLvl.ToString()} => {newLvl.ToString()}");
				}
			}
			string ret = diffs.ToString();
			if (ret.Length == 0) return "No changes";
			return ret;
		}

		/// <summary>
		/// Gets the differences between two role lists, which are intended to correlate to a member's roles.
		/// </summary>
		/// <param name="rolesBefore"></param>
		/// <param name="rolesAfter"></param>
		/// <returns></returns>
		public static string GetMemberRolesDiff(List<DiscordRole> rolesBefore, List<DiscordRole> rolesAfter) {
			StringBuilder diffs = new StringBuilder();
			foreach (DiscordRole roleBefore in rolesBefore) {
				if (!rolesAfter.Contains(roleBefore)) {
					// Role list after does not contain a role we had before.
					diffs.AppendLine($"- {roleBefore.ToString()}");
				} else {
					// It DOES, so we'll remove it from the after list.
					rolesAfter.Remove(roleBefore);
				}
			}

			// Since we removed roles from the after list that were present in the before list, the only roles left behind in the after list are new ones.
			foreach (DiscordRole newRole in rolesAfter) {
				diffs.AppendLine($"+ {newRole.ToString()}");
			}

			string ret = diffs.ToString();
			if (ret.Length == 0) return "No changes";
			return $"```diff\n{ret}\n```";
		}

	}
}
