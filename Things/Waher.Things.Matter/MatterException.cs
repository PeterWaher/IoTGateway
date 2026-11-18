using System;

namespace Waher.Things.Matter
{
	/// <summary>
	/// Base class for Matter-related exceptions.
	/// </summary>
	public class MatterException : Exception
	{
		/// <summary>
		/// Base class for Matter-related exceptions.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		public MatterException(string Message)
			: base(Message)
		{
		}

		/// <summary>
		/// Base class for Matter-related exceptions.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="InnerException">Inner exception.</param>
		public MatterException(string Message, Exception InnerException)
			: base(Message, InnerException)
		{
		}
	}

	/// <summary>
	/// Exception raised when encoded data cannot be decoded.
	/// </summary>
	public class MatterDecodingException : MatterException
	{
		/// <summary>
		/// Exception raised when encoded data cannot be decoded.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		public MatterDecodingException(string Message)
			: base(Message)
		{
		}
	}

	/// <summary>
	/// Exception raised when a communication timeout occurs.
	/// </summary>
	public class MatterTimeoutException : MatterException
	{
		/// <summary>
		/// Exception raised when a communication timeout occurs.
		/// </summary>
		/// <param name="Message">Exception message.</param>
		public MatterTimeoutException(string Message)
			: base(Message)
		{
		}
	}

	/// <summary>
	/// Exception raised when a security check fails (authentication, attestation, certificate validation, etc.).
	/// </summary>
	public class MatterSecurityException : MatterException
	{
		/// <summary>
		/// Exception raised when a security check fails (authentication, attestation, certificate validation, etc.).
		/// </summary>
		/// <param name="Message">Exception message.</param>
		public MatterSecurityException(string Message)
			: base(Message)
		{
		}

		/// <summary>
		/// Exception raised when a security check fails (authentication, attestation, certificate validation, etc.).
		/// </summary>
		/// <param name="Message">Exception message.</param>
		/// <param name="InnerException">Inner exception.</param>
		public MatterSecurityException(string Message, Exception InnerException)
			: base(Message, InnerException)
		{
		}
	}
}
