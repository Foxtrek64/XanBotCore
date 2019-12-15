using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XanBotCore.Logging;
using XanBotCore.Permissions;
using XanBotCore.ServerRepresentation;

namespace XanBotCore.UserObjects {

	/// <summary>
	/// Represents a wrapped member object that offers some extra data such as permission level and the <see cref="BotContext"/> that this member exists in.
	/// </summary>
	public class XanBotMember {
		/// <summary>
		/// A cache storing XanBotMembers referenced by user ID for usage in the get function.
		/// </summary>
		private static readonly Dictionary<ulong, XanBotMember> XMemberFromDUser = new Dictionary<ulong, XanBotMember>();

		/// <summary>
		/// The underlying DiscordUser of this XanBotMember.
		/// </summary>
		public DiscordUser BaseUser { get; }

		/// <summary>
		/// The bot context that this member lives in.
		/// </summary>
		public BotContext Context { get;}

		/// <summary>
		/// The underlying DiscordMember of this XanBotMember.
		/// </summary>
		public DiscordMember Member {
			// This needs to be acquired upon every reference. This is due to member updates.
			// The utilized version of DSharpPlus handles caching on its own so I won't worry about it here.
			get {
				try {
					return Context.Server.GetMemberAsync(BaseUser.Id).GetAwaiter().GetResult();
				} catch (NotFoundException) {
					return null;
				}
			}
		}

		private byte PermissionLevelInternal = PermissionRegistry.DefaultPermissionLevel;

		/// <summary>
		/// This user's registered permission level, which determines accessible commands.
		/// </summary>
		public byte PermissionLevel {
			get {
				if (BaseUser == XanBotCoreSystem.Client.CurrentUser) return 255;
				if (Member == null) return 0;
				return PermissionLevelInternal;
			}
			set {
				if (BaseUser == XanBotCoreSystem.Client.CurrentUser) return; // Stop if we're modifying the bot
				if (PermissionLevelInternal == value) return; //Stop if it's the same
				XanBotLogger.WriteLine("§aPermission Level of user \"§6" + FullName + "\"§a changed from §e" + PermissionLevelInternal + "§a to §e" + value + "§a.");
				PermissionLevelInternal = value;

				// It is IMPERATIVE that this is after PermissionLevelInternal = value (because the method references this property)
				PermissionRegistry.UpdatePermissionLevelOfMember(this, true);
			}
		}

		/// <summary>
		/// This user's username#discriminator e.g. Xan the Dragon#1760
		/// </summary>
		public string FullName {
			get {
				return Username + "#" + Discriminator;
			}
		}

		/// <summary>
		/// This user's username.
		/// </summary>
		public string Username => BaseUser.Username;

		/// <summary>
		/// This user's discriminator, not including the #.
		/// </summary>
		public string Discriminator => BaseUser.Discriminator;

		/// <summary>
		/// A reference to the underlying <see cref="DiscordMember.Nickname"/> property.
		/// </summary>
		public string Nickname => Member.Nickname;

		/// <summary>
		/// Returns <see cref="Nickname"/> if it is not empty nor null, and <see cref="Username"/> if it is.
		/// </summary>
		public string DisplayName {
			get {
				if (Nickname == default || Nickname == null || Nickname.Length == 0) {
					return Username;
				}
				return Nickname;
			}
		}

		/// <summary>
		/// A reference to <see cref="BaseUser"/>'s ID.
		/// </summary>
		public ulong Id => BaseUser.Id;

		/// <summary>
		/// A reference to <see cref="BaseUser"/>'s Mention property.
		/// </summary>
		public string Mention => BaseUser.Mention;


		/// <summary>
		/// Create a new XanBotMember from a DiscordUser. In standard cases this function would be impossible without a server reference, but this reference exists in the bot since it targets one server.
		/// </summary>
		/// <param name="user">The DiscordUser to use as the underlying user.</param>
		private XanBotMember(BotContext context, DiscordUser user) {
			try {
				BaseUser = user;
				Context = context;
				if (PermissionRegistry.AllowXanMaxPermissionLevel && user.Id == PermissionRegistry.BOT_CREATOR_ID) {
					XanBotLogger.WriteLine("§4Notice: A member object representing the bot's creator has been created in this server. PermissionRegistry.AllowXanMaxPermissionLevel is TRUE, which grants him permission level 255. Please set this value to FALSE and restart the bot if you do not want this to happen.");
					PermissionLevelInternal = 255;
				} else {
					PermissionLevelInternal = PermissionRegistry.GetPermissionLevelOfUser(user.Id, context);
				}
			}
			catch (Exception ex) {
				XanBotLogger.WriteException(ex);
			}
		}

		/// <summary>
		/// Create a <see cref="XanBotMember"/> from a <seealso cref="DiscordUser"/> and a <seealso cref="DiscordGuild"/>
		/// </summary>
		/// <param name="server">The server that this <see cref="XanBotMember"/> exists in.</param>
		/// <param name="user">The <seealso cref="DiscordUser"/> to create the member from.</param>
		/// <returns></returns>
		public static XanBotMember GetMemberFromUser(DiscordGuild server, DiscordUser user) {
			if (user == null) return null;
			if (XMemberFromDUser.TryGetValue(user.Id, out XanBotMember result)) {
				return result;
			}
			BotContext serverCtx = BotContextRegistry.GetContext(server);
			XanBotMember member = new XanBotMember(serverCtx, user);
			XMemberFromDUser[user.Id] = member;
			return member;
		}

		/// <summary>
		/// Create a <see cref="XanBotMember"/> from a <seealso cref="DiscordUser"/> and a <seealso cref="BotContext"/>
		/// </summary>
		/// <param name="context">The context that this <see cref="XanBotMember"/> exists in.</param>
		/// <param name="user">The <seealso cref="DiscordUser"/> to create the member from.</param>
		/// <returns></returns>
		public static XanBotMember GetMemberFromUser(BotContext context, DiscordUser user) {
			if (user == null) return null;
			if (XMemberFromDUser.TryGetValue(user.Id, out XanBotMember result)) {
				return result;
			}
			XanBotMember member = new XanBotMember(context, user);
			XMemberFromDUser[user.Id] = member;
			return member;
		}

		/// <summary>
		/// Returns a XanBotMember created from the data in a <seealso cref="ShallowXanBotMember"/>.
		/// </summary>
		/// <param name="shallow">The <seealso cref="ShallowXanBotMember"/> reference to create the member from.</param>
		/// <returns></returns>
		// This might be removed.
		public static XanBotMember GetMemberFromShallow(ShallowXanBotMember shallow) {
			try {
				DiscordUser user = XanBotCoreSystem.Client.GetUserAsync(shallow.UserId).GetAwaiter().GetResult();
				DiscordGuild server = XanBotCoreSystem.Client.GetGuildAsync(shallow.ServerId).GetAwaiter().GetResult();
				return GetMemberFromUser(server, user);
			} catch (Exception) {
				return null;
			}
		}

		/// <summary>
		/// Grant the specified <see cref="DiscordRole"/> to this member synchronously.
		/// </summary>
		/// <param name="role">The <see cref="DiscordRole"/> to give</param>
		/// <param name="reason">The reason to provide in the audit log</param>
		public void GrantRole(DiscordRole role, string reason = null) {
			if (role == null) throw new ArgumentNullException("role");
			Member.GrantRoleAsync(role, reason).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Grant the specified <see cref="DiscordRole"/> to this member.
		/// </summary>
		/// <param name="role">The <see cref="DiscordRole"/> to give</param>
		/// <param name="reason">The reason to provide in the audit log</param>
		public async Task GrantRoleAsync(DiscordRole role, string reason = null) {
			if (role == null) throw new ArgumentNullException("role");
			await Member.GrantRoleAsync(role, reason);
		}

		/// <summary>
		/// Remove the specified <see cref="DiscordRole"/> from this member synchronously.
		/// </summary>
		/// <param name="role">The <see cref="DiscordRole"/> to take</param>
		/// <param name="reason">The reason to provide in the audit log</param>
		public void RemoveRole(DiscordRole role, string reason = null) {
			if (role == null) throw new ArgumentNullException("role");
			Member.RevokeRoleAsync(role, reason).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Remove the specified <see cref="DiscordRole"/> from this member.
		/// </summary>
		/// <param name="role">The <see cref="DiscordRole"/> to take</param>
		/// <param name="reason">The reason to provide in the audit log</param>
		public async Task RemoveRoleAsync(DiscordRole role, string reason = null) {
			if (role == null) throw new ArgumentNullException("role");
			await Member.RevokeRoleAsync(role, reason);
		}

		/// <summary>
		/// If the user does not have the specified <see cref="DiscordRole"/>, it will give it to them. Likewise, if the user DOES have the specified <see cref="DiscordRole"/>, it will take it from them.<para/>
		/// Returns true if the user was given the role, and false if the role was taken away from the user.
		/// </summary>
		/// <param name="role">The <see cref="DiscordRole"/> to give or take</param>
		/// <param name="reason">The reason to provide in the audit log</param>
		public bool ToggleRole(DiscordRole role, string reason = null) {
			if (role == null) throw new ArgumentNullException("role");
			if (HasRole(role)) {
				RemoveRole(role, reason);
				return false;
			} else {
				GrantRole(role, reason);
				return true;
			}
		}

		/// <summary>
		/// If the user does not have the specified <see cref="DiscordRole"/>, it will give it to them. Likewise, if the user DOES have the specified <see cref="DiscordRole"/>, it will take it from them.<para/>
		/// Returns true if the user was given the role, and false if the role was taken away from the user.
		/// </summary>
		/// <param name="role">The <see cref="DiscordRole"/> to give or take</param>
		/// <param name="reason">The reason to provide in the audit log</param>
		public async Task<bool> ToggleRoleAsync(DiscordRole role, string reason = null) {
			if (role == null) throw new ArgumentNullException("role");
			if (HasRole(role)) {
				await RemoveRoleAsync(role, reason);
				return false;
			} else {
				await GrantRoleAsync(role, reason);
				return true;
			}
		}

		/// <summary>
		/// Returns whether or not this member has the specified <see cref="DiscordRole"/>
		/// </summary>
		/// <param name="role">The <see cref="DiscordRole"/> to look for</param>
		/// <returns></returns>
		public bool HasRole(DiscordRole role) {
			if (role == null) throw new ArgumentNullException("role");
			return Member.Roles.Contains(role);
		}

		/// <summary>
		/// DM this user.
		/// </summary>
		/// <param name="message">The string message to send.</param>
		/// <param name="embed">An optional embed to send.</param>
		/// <returns></returns>
		public async Task SendDMAsync(string message = null, DiscordEmbed embed = null) {
			DiscordChannel dm = await Member.CreateDmChannelAsync();
			await dm.SendMessageAsync(message, false, embed);
		}

		/// <summary>
		/// Calls <see cref="ToString(DisplayType)"/> with an argument of <see cref="DiscordUserExtensions.DisplayType.UserId"/>
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			return ToString(DisplayType.UserId);
		}

		/// <summary>
		/// Calls the extension method provided by <see cref="DiscordUserExtensions"/>: <see cref="DiscordUserExtensions.GetFormattedUser(DiscordUser, DisplayType)"/><para/>
		/// See <see cref="DisplayType"/> for more information.
		/// </summary>
		/// <param name="displayType"></param>
		/// <returns></returns>
		public string ToString(DisplayType displayType) {
			return BaseUser.GetFormattedUser(displayType);
		}

		public static implicit operator DiscordMember(XanBotMember src) {
			return src.Member;
		}

	}

	/// <summary>
	/// A "Shallow" <see cref="XanBotMember"/> which represents a member solely through user ID and server ID. This does not store any other data, and is used to<para/>
	/// create a <see cref="XanBotMember"/> object without a user (almost in an abstract form, in a sense).<para/>
	/// <para/>
	/// This is mainly used for data persistence where the permission registry needs to create user objects from user ID and server ID so that data<para/>
	/// can be loaded later on by an actual member object with proper data.
	/// </summary>
	public class ShallowXanBotMember {

		public static List<ShallowXanBotMember> ShallowMembers = new List<ShallowXanBotMember>();

		public ulong UserId { get; private set; }
		public ulong ServerId { get; private set; }

		private ShallowXanBotMember(ulong userId, ulong serverId) {
			UserId = userId;
			ServerId = serverId;
		}

		public bool CompareShallowMember(ulong userId, ulong serverId) {
			return UserId == userId && ServerId == serverId;
		}

		public bool DoesShallowRepresentDeep(XanBotMember deep) {
			return UserId == deep.BaseUser.Id && ServerId == deep.Context.ServerId;
		}

		public static ShallowXanBotMember GetShallowFromDeep(XanBotMember fullMemberObject) {
			foreach (ShallowXanBotMember shallow in ShallowMembers) {
				if (shallow.DoesShallowRepresentDeep(fullMemberObject)) {
					return shallow;
				}
			}
			ShallowXanBotMember member = new ShallowXanBotMember(fullMemberObject.BaseUser.Id, fullMemberObject.Context.ServerId);
			ShallowMembers.Add(member);
			return member;
		}

		public static ShallowXanBotMember GetShallowFromRaw(ulong userId, ulong serverId) {
			foreach (ShallowXanBotMember shallow in ShallowMembers) {
				if (shallow.CompareShallowMember(userId, serverId)) {
					return shallow;
				}
			}
			ShallowXanBotMember member = new ShallowXanBotMember(userId, serverId);
			ShallowMembers.Add(member);
			return member;
		}

	}
}
