namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Parameter protection levels
	/// </summary>
	public enum ProtectionLevel
	{
		/// <summary>
		/// Normal parameter. Value follows contract in the clear.
		/// </summary>
		Normal,

		/// <summary>
		/// The creator of a contract creates keys for the encryption and decryption 
		/// of encrypted parameter values. The creator is responsible for sharing the 
		/// keys with the other parts of the contract, using end-to-end encrypted 
		/// communication. Encypted values are transmitted in encrypted form.
		/// </summary>
		Encrypted,

		/// <summary>
		/// A transient parameter is sent out-of-scope of the signature.
		/// Instead, a GUID is used as a parameter value.
		/// This GUID can be used to peers to validate the signature of the contract.
		/// Transient parameter values are only available in-transit, and are not 
		/// persisted.
		/// </summary>
		Transient
	}
}
