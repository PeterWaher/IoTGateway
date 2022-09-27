using System.Threading.Tasks;

namespace Waher.Networking.XMPP.HTTPX
{
	/// <summary>
	/// Event handler for Token response events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task TokenResponseEventHandler(object Sender, TokenResponseEventArgs e);

	/// <summary>
	/// Event arguments for Token responses.
	/// </summary>
	public class TokenResponseEventArgs : IqResultEventArgs
	{
		private readonly string token;

		/// <summary>
		/// Event arguments for Token responses.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		///	<param name="Token">Received token, if any.</param>
		public TokenResponseEventArgs(IqResultEventArgs e, string Token)
			: base(e)
		{
			this.token = Token;
		}

		/// <summary>
		/// Token, if any.
		/// </summary>
		public string Token => this.token;
	}
}
