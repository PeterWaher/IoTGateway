using System;

namespace Waher.Networking.XMPP.Contracts.EventArguments
{
	/// <summary>
	/// Event arguments for signature validation events.
	/// </summary>
	public class ValidateSignatureEventArgs : EventArgs
	{
		/// <summary>
		/// Event arguments for signature validation events.
		/// </summary>
		/// <param name="LegalId">Legal identity used to create the signature. If empty, current approved legal identities will be used to validate the signature.</param>
		/// <param name="Data">Binary data to sign.</param>
		/// <param name="Signature">Digital signature of data</param>
		public ValidateSignatureEventArgs(string LegalId, byte[] Data, byte[] Signature)
			: base()
		{
			this.LegalId = LegalId;
			this.Data = Data;
			this.Signature = Signature;
		}

		/// <summary>
		/// Legal identity used to create the signature. If empty, current approved legal 
		/// identities will be used to validate the signature.
		/// </summary>
		public string LegalId { get; }

		/// <summary>
		/// Binary data to sign.
		/// </summary>
		public byte[] Data { get; }

		/// <summary>
		/// Digital signature of data
		/// </summary>
		public byte[] Signature { get; }

		/// <summary>
		/// If signature is valid (true), invalid (false), or if signature validation was
		/// not performed (null).
		/// </summary>
		public bool? Valid { get; set; }

		/// <summary>
		/// Legal Identity associated with the <see cref="LegalId"/>
		/// </summary>
		public LegalIdentity Identity { get; set; }
	}
}
