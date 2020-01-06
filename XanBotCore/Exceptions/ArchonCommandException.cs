using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using XanBotCore.CommandSystem.Commands.ArchonCommands;

namespace XanBotCore.Exceptions {

	/// <summary>
	/// Represents an error that occurs due to malformed Archon Command usage, like incorrect input or invalid arguments.<para/>
	/// This is specific to Archon Commands.
	/// </summary>
	public class ArchonCommandException : Exception {

		/// <summary>
		/// The error message sent by the command.
		/// </summary>
		public override string Message { get; }

		/// <summary>
		/// The Archon Command that threw this error.
		/// </summary>
		public ArchonCommand Command { get; }

		/// <summary>
		/// True if the command message should be deleted, in which case the amount of time is specified by <see cref="ExpireTimeMilliseconds"/>
		/// </summary>
		public bool Expires { get; }

		/// <summary>
		/// The amount of milliseconds until the sent <see cref="DiscordMessage"/> reflecting this command is deleted, if applicable.
		/// </summary>
		public int ExpireTimeMilliseconds { get; } = 0;

		/// <summary>
		/// Create a new <see cref="ArchonCommandException"/> with the specified <see cref="ArchonCommand"/> <paramref name="source"/>, error message, and optional expire time in milliseconds (which deletes the message sent in the <see cref="DiscordChannel"/> the command was executed in, if applicable.)
		/// </summary>
		/// <param name="source">The <see cref="ArchonCommand"/> that threw this error. Should always be <see cref="this"/></param>
		/// <param name="errMsg">The error message to accompany which describes why this exception was thrown.</param>
		/// <param name="expireTimeMilliseconds">The amount of milliseconds until the message sent is deleted. This does nothing if this command was executed from the console.</param>
		public ArchonCommandException(ArchonCommand source, string errMsg, int? expireTimeMilliseconds = null) {
			Command = source;
			Message = errMsg;
			Expires = expireTimeMilliseconds != null;
			if (Expires) {
				ExpireTimeMilliseconds = expireTimeMilliseconds.Value;
			}
		}

		public ArchonCommandException(ArchonCommand source, Exception subExc, int? expireTimeMilliseconds = null) {
			Command = source;
			Message = $"**[{subExc.GetType().FullName} thrown!]** {subExc.Message}";
			Expires = expireTimeMilliseconds != null;
			if (Expires) {
				ExpireTimeMilliseconds = expireTimeMilliseconds.Value;
			}
		}
	}
}
