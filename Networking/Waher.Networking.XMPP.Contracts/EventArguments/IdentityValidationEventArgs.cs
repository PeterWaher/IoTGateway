using System;

namespace Waher.Networking.XMPP.Contracts.EventArguments
{
	/// <summary>
	/// Event arguments for identity validation responses
	/// </summary>
	public class IdentityValidationEventArgs : EventArgs
	{
		private readonly IdentityStatus status;
		private readonly object state;

		/// <summary>
		/// Event arguments for identity validation responses
		/// </summary>
		/// <param name="Status">Validation status</param>
		/// <param name="State">State object</param>
		public IdentityValidationEventArgs(IdentityStatus Status, object State)
			: base()
		{
			this.status = Status;
			this.state = State;
		}

		/// <summary>
		/// Validation status of legal identity.
		/// </summary>
		public IdentityStatus Status => this.status;

		/// <summary>
		/// State object.
		/// </summary>
		public object State => this.state;
	}
}
