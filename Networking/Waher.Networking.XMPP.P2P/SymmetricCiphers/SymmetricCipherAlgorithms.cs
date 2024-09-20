namespace Waher.Networking.XMPP.P2P.SymmetricCiphers
{
	/// <summary>
	/// Enumeration of symmetric cipher algorithms available in the library.
	/// </summary>
	public enum SymmetricCipherAlgorithms
	{
		/// <summary>
		/// AES-256
		/// </summary>
		Aes256,

		/// <summary>
		/// ChaCha20
		/// </summary>
		ChaCha20,

		/// <summary>
		/// AEAD-ChaCha20-Poly1305
		/// </summary>
		AeadChaCha20Poly1305
	}
}
