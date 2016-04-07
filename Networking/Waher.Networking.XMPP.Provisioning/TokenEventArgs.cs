using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Provisioning
{
	/// <summary>
	/// Event arguments for token callbacks.
	/// </summary>
	public class TokenEventArgs : IqResultEventArgs
	{
		private string token;

		internal TokenEventArgs(IqResultEventArgs e, object State, string Token)
			: base(e)
		{
			this.State = State;
			this.token = Token;
		}

		/// <summary>
		/// Token corresponding to a given certificate.
		/// </summary>
		public string Token
		{
			get { return this.token; }
		}
	}
}
