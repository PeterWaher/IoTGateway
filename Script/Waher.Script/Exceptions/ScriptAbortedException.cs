using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Model;

namespace Waher.Script.Exceptions
{
	/// <summary>
	/// Exception thrown when a script has been aborted.
	/// </summary>
	public class ScriptAbortedException : ScriptException 
	{
		/// <summary>
		/// Exception thrown when a script has been aborted.
		/// </summary>
		public ScriptAbortedException()
			: this("Script evaluation aborted.")
		{
		}

		/// <summary>
		/// Exception thrown when a script has been aborted.
		/// </summary>
		/// <param name="Message">Message text.</param>
		public ScriptAbortedException(string Message)
			: base(Message)
		{
		}

		/// <summary>
		/// Exception thrown when a script has been aborted.
		/// </summary>
		/// <param name="Message">Message text.</param>
		/// <param name="InnerException">Inner exception.</param>
		public ScriptAbortedException(string Message, Exception InnerException)
			: base(Message, InnerException)
		{
		}

	}
}
