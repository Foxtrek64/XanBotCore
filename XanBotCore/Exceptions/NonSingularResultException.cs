using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XanBotCore.Exceptions {

	/// <summary>
	/// A generic exception that is thrown when there is not exclusively one return value possible from a function, for instance, in a function that is intended to get a user object from string.<para/>
	/// If the return value is of a definite type, consider using <see cref="NonSingularResultException{T}"/> instead.
	/// </summary>
	public class NonSingularResultException : Exception {

		/// <summary>
		/// The message accompanying this NonSingularResultException.
		/// </summary>
		public new string Message { get; }

		/// <summary>
		/// An array of the potential return values.
		/// </summary>
		public object[] PotentialReturnValues { get; }

		/// <summary>
		/// An array of the potential return values, where each object has had its ToString method called.
		/// </summary>
		public string[] PotentialReturnValuesString { get; }

		public NonSingularResultException(string message = null, params object[] potentialReturnValues) {
			Message = message;
			PotentialReturnValues = potentialReturnValues;

			string[] retAsString = new string[potentialReturnValues.Length];
			for (int idx = 0; idx < potentialReturnValues.Length; idx++) {
				object obj = potentialReturnValues[idx];
				retAsString[idx] = obj.ToString();
			}
			PotentialReturnValuesString = retAsString;
		}

	}

	/// <summary>
	/// An exception that is thrown when there is not exclusively one return value possible from a function, for instance, in a function that is intended to get a user object from string.
	/// </summary>
	public class NonSingularResultException<T> : NonSingularResultException {
		/// <summary>
		/// The message accompanying this NonSingularResultException.
		/// </summary>
		public new string Message { get; }

		/// <summary>
		/// An array of the potential return values.
		/// </summary>
		public new T[] PotentialReturnValues { get; }

		/// <summary>
		/// An array of the potential return values, where each object has had its ToString method called.
		/// </summary>
		public new string[] PotentialReturnValuesString { get; }

		public NonSingularResultException(string message = null, params T[] potentialReturnValues) {
			Message = message;
			PotentialReturnValues = potentialReturnValues;

			string[] retAsString = new string[potentialReturnValues.Length];
			for (int idx = 0; idx < potentialReturnValues.Length; idx++) {
				object obj = potentialReturnValues[idx];
				retAsString[idx] = obj.ToString();
			}
			PotentialReturnValuesString = retAsString;
		}
	}
}
