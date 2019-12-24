using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XanBotCore.ServerRepresentation;
using XanBotCore.UserObjects;

namespace XanBotCore.PassiveHandlers {

	/// <summary>
	/// Represents a passive handler. Passive handlers are similar to commands, but are not run exclusively by a specific command.
	/// </summary>
	public abstract class PassiveHandler : IComparable<PassiveHandler> {

		/// <summary>
		/// The name of this passive handler. Can be anything.
		/// </summary>
		public abstract string Name { get; }

		/// <summary>
		/// The description of this passive handler.
		/// </summary>
		public abstract string Description { get; }

		/// <summary>
		/// Try running this passive handler. Returns true if the passive handler should intercept the message and prevent other passive handlers from picking it up.
		/// </summary>
		/// <param name="executingMember">The <see cref="XanBotMember"/> who will be executing this handler.</param>
		/// <param name="originalMessage">The <see cref="DiscordMessage"/> the member sent.</param>
		/// <returns></returns>
		public abstract Task<bool> RunHandlerAsync(BotContext context, XanBotMember executingMember, DiscordMessage originalMessage);

		/// <summary>
		/// A method that is run before bot shutdown. Use this method to save any necessary data and release any resources in use by the handler.
		/// </summary>
		public virtual void Dispose() { }

		public int CompareTo(PassiveHandler other) {
			return Name.CompareTo(other.Name);
		}
	}
}
