using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Script.Exceptions
{
	/// <summary>
	/// Base class for script exceptions.
	/// </summary>
	public class ScriptException : Exception
	{
		/// <summary>
		/// Base class for script exceptions.
		/// </summary>
		/// <param name="Message">Message text.</param>
		public ScriptException(string Message)
			: base(Message)
		{
		}

		/// <summary>
		/// Base class for script exceptions.
		/// </summary>
		/// <param name="Message">Message text.</param>
		/// <param name="InnerException">Inner exception.</param>
		public ScriptException(string Message, Exception InnerException)
			: base(Message, InnerException)
		{
		}
	}
}
