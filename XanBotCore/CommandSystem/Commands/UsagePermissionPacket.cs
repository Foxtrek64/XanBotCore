using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XanBotCore.CommandSystem.Commands {

	/// <summary>
	/// Represents the status of whether or not a user can use a command, as well as an optional error message to associate with being unable to use the command.
	/// </summary>
	public struct UsagePermissionPacket {

		/// <summary>
		/// Whether or not this command is usable.
		/// </summary>
		public bool CanUse { get; }

		/// <summary>
		/// If <see cref="CanUse"/> is false, this is the error message to log.
		/// </summary>
		public string ErrorMessage { get; }

		/// <summary>
		/// Create a new <see cref="UsagePermissionPacket"/> with the specified info.
		/// </summary>
		/// <param name="canUse">Whether or not the user can use this command.</param>
		/// <param name="error">The error message to display if they cannot use this command.</param>
		public UsagePermissionPacket(bool canUse, string error = default) {
			CanUse = canUse;
			ErrorMessage = error;
		}

	}
}
