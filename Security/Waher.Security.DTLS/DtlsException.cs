using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Security.DTLS
{
	/// <summary>
	/// Base class for all DTLS-based exceptions.
	/// </summary>
    public class DtlsException : Exception
    {
		/// <summary>
		/// Base class for all DTLS-based exceptions.
		/// </summary>
		/// <param name="Message">Message.</param>
		public DtlsException(string Message)
			: base(Message)
		{
		}
    }
}
