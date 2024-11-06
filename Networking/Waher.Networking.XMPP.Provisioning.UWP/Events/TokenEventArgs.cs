using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Provisioning.Events
{
	/// <summary>
	/// Event arguments for token callbacks.
	/// </summary>
	public class TokenEventArgs : IqResultEventArgs
	{
		private readonly string token;

		internal TokenEventArgs(IqResultEventArgs e, object State, string Token)
			: base(e)
		{
			this.State = State;
			this.token = Token;
		}

		/// <summary>
		/// Token corresponding to a given certificate.
		/// </summary>
		public string Token => this.token;
	}
}
