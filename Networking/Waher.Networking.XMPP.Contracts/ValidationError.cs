using System.Collections.Generic;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Contains information about a validation error.
	/// </summary>
	public class ValidationError
	{
		/// <summary>
		/// Contains information about a validation error.
		/// </summary>
		/// <param name="ErrorType">Error Type</param>
		/// <param name="ErrorMessage">Human-readable error message.</param>
		/// <param name="ErrorLanguage">Language code.</param>
		/// <param name="ErrorCode">Machine-readable Error code</param>
		/// <param name="Service">Service reporting the error.</param>
		/// <param name="Tags">Tags annotating the error message.</param>
		public ValidationError(ValidationErrorType ErrorType, string ErrorMessage, string ErrorLanguage, 
			string ErrorCode, object Service, params KeyValuePair<string, object>[] Tags)
		{
			this.ErrorType = ErrorType;
			this.ErrorMessage = ErrorMessage;
			this.ErrorLanguage = ErrorLanguage;
			this.ErrorCode = ErrorCode;
			this.Service = Service;
			this.Tags = Tags;
		}

		/// <summary>
		/// Type of error.
		/// </summary>
		public ValidationErrorType ErrorType { get; }

		/// <summary>
		/// Error message that can be sent to origin
		/// </summary>
		public string ErrorMessage { get; }

		/// <summary>
		/// Language ISO code of <see cref="ErrorMessage"/>.
		/// </summary>
		public string ErrorLanguage { get; }

		/// <summary>
		/// Machine-readable error code (service-specific).
		/// </summary>
		public string ErrorCode { get; }

		/// <summary>
		/// Service reporting the error.
		/// </summary>
		public object Service { get; }

		/// <summary>
		/// Tags annotating the error message.
		/// </summary>
		public KeyValuePair<string, object>[] Tags { get; }
	}
}
