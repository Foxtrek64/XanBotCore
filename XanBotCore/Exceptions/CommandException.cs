using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XanBotCore.CommandSystem;
using XanBotCore.CommandSystem.Commands.ArchonCommands;

namespace XanBotCore.Exceptions {

	/// <summary>
	/// Represents an error that occurs due to malformed command usage, like incorrect input or invalid arguments.
	/// </summary>
	public class CommandException : Exception {

		/// <summary>
		/// The error message sent by the command.
		/// </summary>
		public new string Message { get; }

		/// <summary>
		/// The command that threw this error.
		/// </summary>
		public Command Command { get; }

		public CommandException(Command source, string errMsg) {
			Command = source;
			Message = errMsg;
		}

	}
}
