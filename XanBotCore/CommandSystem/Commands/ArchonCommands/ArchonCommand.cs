using DSharpPlus.Entities;
using System;
using XanBotCore.ServerRepresentation;
using XanBotCore.UserObjects;

namespace XanBotCore.CommandSystem.Commands.ArchonCommands {

	/// <summary>
	/// Archon Commands represent high power commands (e.g. editing bot behaviors on a low level). Hence their name, they are intended to be locked to leading administrators and extremely close trustees.
	/// </summary>
	public abstract class ArchonCommand : IComparable<ArchonCommand> {
		/// <summary>
		/// The name of this Archon Command. This is also used to execute it. Should not include spaces or special characters. Should be all lowercase.
		/// </summary>
		public abstract string Name { get; }

		/// <summary>
		/// The description of this Archon Command.
		/// </summary>
		public abstract string Description { get; }

		/// <summary>
		/// The usage syntax of this Archon Command.
		/// </summary>
		public abstract string Syntax { get; }

		/// <summary>
		/// Executes the command. This does not check if the user can run it. Check if the user is authorized before using this method.
		/// </summary>
		/// <param name="context">The <see cref="BotContext"/> this command is running in.</param>
		/// <param name="executingMember">The member executing this command.</param>
		/// <param name="originalMessage">The DiscordMessage that was responsible for invoking this command.</param>
		/// <param name="args">The arguments of the command split via shell32.DLL's handling system.</param>
		/// <param name="allArgs">Every argument passed into this command as its raw string. This is used to preserve quotes and other characters stripped by shell32.</param>
		public abstract void ExecuteCommand(BotContext context, XanBotMember executingMember, DiscordMessage originalMessage, string[] args, string allArgs);

		public int CompareTo(ArchonCommand other) {
			if (other == null) return 1;
			return Name.CompareTo(other.Name);
		}
	}
}
