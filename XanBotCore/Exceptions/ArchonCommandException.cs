using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
		public new string Message { get; }

		/// <summary>
		/// The Archon Command that threw this error.
		/// </summary>
		public ArchonCommand Command { get; }

		public ArchonCommandException(ArchonCommand source, string errMsg) {
			Command = source;
			Message = errMsg;
		}

	}
}
