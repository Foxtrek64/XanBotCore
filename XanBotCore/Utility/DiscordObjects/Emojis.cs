using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XanBotCore.Logging;

namespace XanBotCore.Utility.DiscordObjects {

	/// <summary>
	/// A storage class for a reference to Emojis usable by this bot. <see cref="CustomEmojis"/> is intended for bot-wide emojis.<para/>
	/// If a specific server has emojis that need to be used by the bot in that server, consider defining the emojis in a class designed specifically for that server's <see cref="ServerRepresentation.BotContext"/>
	/// </summary>
	public class Emojis {

		/// <summary>
		/// A registry of custom, bot-wide emojis. 
		/// </summary>
		public static Dictionary<string, ShallowEmoji> CustomEmojis = new Dictionary<string, ShallowEmoji>();

	}

	/// <summary>
	/// A "shallow emoji" which stores an Emoji by ID. It can be implicitly casted into DiscordEmoji. It will not create a DiscordEmoji until one is requested either via referencing its Emoji property or by performing an implicit cast.
	/// </summary>
	public class ShallowEmoji {

		private string EmojiNameInternal = null;
		private ulong EmojiIdInternal = 0;

		/// <summary>
		/// The ID of this Emoji.
		/// </summary>
		public ulong EmojiId {
			get {
				return EmojiIdInternal;
			}
		}

		/// <summary>
		/// The name of this emoji, including the :s
		/// </summary>
		public string EmojiName {
			get {
				return EmojiNameInternal;
			}
		}

		private DiscordEmoji EmojiInternal = null;

		/// <summary>
		/// A DiscordEmoji created from information stored by this ShallowEmoji. Get will attempt to create a new Emoji if an existing one is not stored.<para/>
		/// Get will throw an InvalidOperationException if the client does not exist.<para/>
		/// Get will throw NullReferenceException if an Emoji could not be created from the stored ID.
		/// </summary>
		public DiscordEmoji Emoji {
			get {
				if (EmojiInternal == null) {
					if (EmojiIdInternal != 0) {
						DiscordEmoji emoji = DiscordEmoji.FromGuildEmote(XanBotCoreSystem.Client, EmojiId);
						EmojiInternal = emoji ?? throw new NullReferenceException("Attempt to create DiscordEmoji from ID " + EmojiId + " failed as the returned emoji was null.");
					} else if (EmojiNameInternal != null) {
						while (!XanBotCoreSystem.HasFinishedGettingGuildData) {
							Task.Delay(500).GetAwaiter().GetResult(); // I told you it was nasty.
						}
						foreach (DiscordGuild server in XanBotCoreSystem.Client.Guilds.Values) {
							foreach (DiscordEmoji e in server.Emojis.Values) {
								XanBotLogger.WriteDebugLine($"Comparing emoji {e.Name} to {EmojiNameInternal}");
								if (e.Name == EmojiNameInternal) {
									XanBotLogger.WriteDebugLine("Got match!");
									EmojiInternal = e;
									EmojiIdInternal = e.Id;
									return EmojiInternal;
								}
							}
						}
					} else {
						throw new NullReferenceException("Attempted to create DiscordEmoji where the ID was 0 (presumed null) and the Name was null.");
					}
				}
				return EmojiInternal;
			}
			set {
				EmojiInternal = value;
				if (value != null) {
					EmojiNameInternal = value.Name;
					EmojiIdInternal = value.Id;
				} else {
					EmojiNameInternal = null;
					EmojiIdInternal = 0;
				}
			}
		}

		/// <summary>
		/// Create a new ShallowEmoji from an Emoji ID.
		/// </summary>
		/// <param name="id">The Emoji ID</param>
		public ShallowEmoji(ulong id) {
			EmojiIdInternal = id;
			// Do NOT set Emoji here. There's a chance the Client may not have been set.
		}

		/// <summary>
		/// Create a new ShallowEmoji from an Emoji Name including the :s. Example is :customEmoji:<para/>
		/// This method is not advised since it has to search every guild's emojis by name, and referencing <see cref="Emoji"/> blocks the current thread if the guild information has not been completely downloaded.
		/// </summary>
		/// <param name="nameWithColons">The name of the emoji, including the surrounding :s</param>
		public ShallowEmoji(string nameWithColons) {
			EmojiNameInternal = nameWithColons;
		}

		/// <summary>
		/// Create a new ShallowEmoji from an existing DiscordEmoji.
		/// </summary>
		/// <param name="emoji">The existing DiscordEmoji</param>
		public ShallowEmoji(DiscordEmoji emoji) {
			EmojiIdInternal = emoji.Id;
			Emoji = emoji;
		}

		public static implicit operator DiscordEmoji(ShallowEmoji input) {
			return input.Emoji;
		}

		public static explicit operator ShallowEmoji(DiscordEmoji input) {
			return new ShallowEmoji(input);
		}

		public override string ToString() {
			// DiscordEmoji.ToString() will format it for being typed into a chat message.
			return ((DiscordEmoji)this).ToString();
		}

	}

	/// <summary>
	/// Identical to <see cref="ShallowEmoji"/> but it references a real unicode emoji, as instantiation of emojis requires a <see cref="DiscordClient"/>.
	/// </summary>
	public class ShallowDiscordEmoji {

		private string NameInternal;
		public string Name {
			get {
				return NameInternal;
			}
			private set {
				string name = value;
				if (!name.StartsWith(":")) {
					name = ":" + name;
				}
				if (!name.EndsWith(":")) {
					name += ":";
				}
				NameInternal = name;
			}
		}

		private DiscordEmoji EmojiInternal = null;

		/// <summary>
		/// A DiscordEmoji created from information stored by this ShallowDiscordEmoji. Get will attempt to create a new Emoji if an existing one is not stored.<para/>
		/// Get will throw an InvalidOperationException if the client does not exist.<para/>
		/// Get will throw NullReferenceException if an Emoji could not be created from the stored ID.
		/// </summary>
		public DiscordEmoji Emoji {
			get {
				if (EmojiInternal == null) {
					XanBotLogger.WriteDebugLine($"ShallowDiscordEmoji instantiating {Name}");
					DiscordEmoji emoji = DiscordEmoji.FromName(XanBotCoreSystem.Client, Name);
					EmojiInternal = emoji ?? throw new NullReferenceException("Attempt to create DiscordEmoji from name " + Name + " failed.");
				}
				return EmojiInternal;
			}
			set {
				EmojiInternal = value;
			}
		}

		/// <summary>
		/// Create a new ShallowEmoji from an Emoji name
		/// </summary>
		public ShallowDiscordEmoji(string name) {
			Name = name;
			// Do NOT set Emoji here. There's a chance the Client may not have been set.
		}

		/// <summary>
		/// Create a new ShallowEmoji from an existing DiscordEmoji.
		/// </summary>
		/// <param name="emoji">The existing DiscordEmoji</param>
		public ShallowDiscordEmoji(DiscordEmoji emoji) {
			Emoji = emoji;
			Name = emoji.Name;
		}

		public static implicit operator DiscordEmoji(ShallowDiscordEmoji input) {
			return input.Emoji;
		}

		public static explicit operator ShallowDiscordEmoji(DiscordEmoji input) {
			return new ShallowDiscordEmoji(input);
		}

		public override string ToString() {
			// DiscordEmoji.ToString() will format it for being typed into a chat message.
			return ((DiscordEmoji)this).ToString();
		}
	}
}
