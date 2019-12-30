using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XanBotCore.Exceptions {

	/// <summary>
	/// A counterpart to <see cref="KeyNotFoundException"/> that instead signifies that the specified value could not be found.
	/// </summary>
	public class ValueNotFoundException : KeyNotFoundException {

		public ValueNotFoundException() : base() { }

		public ValueNotFoundException(string message) : base(message) { }

	}
}
