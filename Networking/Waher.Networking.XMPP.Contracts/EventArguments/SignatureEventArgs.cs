using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Delegate for signature callback methods.
	/// </summary>
	/// <param name="Sender">Sender</param>
	/// <param name="e">Event arguments</param>
	public delegate Task SignatureEventHandler(object Sender, SignatureEventArgs e);

	/// <summary>
	/// Event arguments for signature responses
	/// </summary>
	public class SignatureEventArgs : KeyEventArgs
	{
		private readonly byte[] signature;

		/// <summary>
		/// Event arguments for signature responses
		/// </summary>
		/// <param name="Key">Key algorithm used.</param>
		/// <param name="Signature">Digital signature</param>
		/// <param name="State">State object.</param>
		public SignatureEventArgs(IE2eEndpoint Key, byte[] Signature, object State)
			: base(new KeyEventArgs(Key, State))
		{
			this.signature = Signature;
		}

		/// <summary>
		/// Event arguments for signature responses
		/// </summary>
		/// <param name="e">Event arguments of request response.</param>
		/// <param name="Signature">Digital signature</param>
		public SignatureEventArgs(KeyEventArgs e, byte[] Signature)
			: base(e)
		{
			this.signature = Signature;
		}

		/// <summary>
		/// Digital signature
		/// </summary>
		public byte[] Signature => this.signature;
	}
}
