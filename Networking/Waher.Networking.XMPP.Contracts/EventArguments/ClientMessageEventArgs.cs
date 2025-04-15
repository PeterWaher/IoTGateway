using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Contracts.EventArguments
{
	/// <summary>
	/// Identity Review event arguments.
	/// </summary>
	public class ClientMessageEventArgs : IdentityReviewEventArgs
	{
		private readonly string code;
		private readonly ValidationErrorType type;

		/// <summary>
		/// Identity Review event arguments.
		/// </summary>
		/// <param name="e">Message event arguments.</param>
		/// <param name="LegalId">Identifier of associated Legal ID.</param>
		/// <param name="Code">Machine-readable code corresponding to the first error message.</param>
		/// <param name="Type">Type of first error message</param>
		public ClientMessageEventArgs(MessageEventArgs e, string LegalId,
			string Code, ValidationErrorType Type)
			: base(e, LegalId)
		{
			this.code = Code;
			this.type = Type;
		}

		/// <summary>
		/// Machine-readable code corresponding to the first error message.
		/// </summary>
		public string Code => this.code;

		/// <summary>
		/// Type of first error message
		/// </summary>
		public ValidationErrorType ValidationErrorType => this.type;
	}
}
