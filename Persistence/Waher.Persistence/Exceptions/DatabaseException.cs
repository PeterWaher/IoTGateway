using System;
using System.Collections.Generic;

namespace Waher.Persistence.Exceptions
{
	/// <summary>
	/// Base class for database exceptions.
	/// </summary>
	public class DatabaseException : Exception
	{
		/// <summary>
		/// Base class for database exceptions.
		/// </summary>
		/// <param name="Message">Exception message</param>
		public DatabaseException(string Message)
			: base(Message)
		{
		}

		/// <summary>
		/// Base class for database exceptions.
		/// </summary>
		/// <param name="Message">Exception message</param>
		/// <param name="InnerException">Inner exception.</param>
		public DatabaseException(string Message, Exception InnerException)
			: base(Message, InnerException)
		{
		}
	}
}
