using System;

namespace Waher.Networking.SMTP.Exceptions
{
	/// <summary>
	/// Base class for SMTP-related exceptions.
	/// </summary>
	public class SmtpException : Exception
	{
		private readonly int code;

		/// <summary>
		/// Base class for SMTP-related exceptions.
		/// </summary>
		/// <param name="Message">Error message</param>
		/// <param name="Code">SMTP return code.</param>
		public SmtpException(string Message, int Code)
			: base(Message)
		{
			this.code = Code;
		}

		/// <summary>
		/// SMTP return code.
		/// </summary>
		public int Code => this.code;
	}
}
