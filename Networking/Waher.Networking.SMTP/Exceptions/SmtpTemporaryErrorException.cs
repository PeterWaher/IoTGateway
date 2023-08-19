namespace Waher.Networking.SMTP.Exceptions
{
	/// <summary>
	/// Base class for temporary SMTP-related exceptions.
	/// </summary>
	public class SmtpTemporaryErrorException : SmtpException
	{
		/// <summary>
		/// Base class for temporary SMTP-related exceptions.
		/// </summary>
		/// <param name="Message">Error message</param>
		/// <param name="Code">SMTP return code.</param>
		public SmtpTemporaryErrorException(string Message, int Code)
			: base(Message, Code)
		{
		}
	}
}
