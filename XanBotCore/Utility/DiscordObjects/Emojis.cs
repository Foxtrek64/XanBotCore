using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		private ulong EmojiIdInternal = 0;

		/// <summary>
		/// The ID of this Emoji.
		/// </summary>
		public ulong EmojiId {
			get {
				return EmojiIdInternal;
			}
			set {
				if (value != EmojiIdInternal) {
					EmojiIdInternal = value;
					EmojiInternal = null;
				}
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
					DiscordEmoji emoji = DiscordEmoji.FromGuildEmote(XanBotCoreSystem.Client, EmojiId);
					EmojiInternal = emoji ?? throw new NullReferenceException("Attempt to create DiscordEmoji from ID " + EmojiId + " failed.");
				}
				return EmojiInternal;
			}
			set {
				EmojiInternal = value;
			}
		}

		/// <summary>
		/// Create a new ShallowEmoji from an Emoji ID.
		/// </summary>
		/// <param name="id">The Emoji ID</param>
		public ShallowEmoji(ulong id) {
			EmojiId = id;
			// Do NOT set Emoji here. There's a chance the Client may not have been set.
		}

		/// <summary>
		/// Create a new ShallowEmoji from an existing DiscordEmoji.
		/// </summary>
		/// <param name="emoji">The existing DiscordEmoji</param>
		public ShallowEmoji(DiscordEmoji emoji) {
			EmojiId = emoji.Id;
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
		/// Create a new ShallowEmoji from an Emoji name;
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
