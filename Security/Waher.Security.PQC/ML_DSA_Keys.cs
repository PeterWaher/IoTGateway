namespace Waher.Security.PQC
{
	/// <summary>
	/// ML-DSA public and private keys, as defined in §6.1.
	/// </summary>
	public class ML_DSA_Keys
	{
		/// <summary>
		/// ML-DSA public and private keys, as defined in §6.1.
		/// </summary>
		/// <param name="Â">Internal matrix used to generate encryption key.</param>
		/// <param name="PublicKey">Public Encryption key.</param>
		/// <param name="PrivateKey">Private Decryption key.</param>
		public ML_DSA_Keys(uint[,][] Â, byte[] PublicKey, byte[] PrivateKey)
		{
			this.Â = Â;
			this.PublicKey = PublicKey;
			this.PrivateKey = PrivateKey;
		}

		/// <summary>
		/// Matrix of encryption keys, as defined in §6.1.
		/// </summary>
		public uint[,][] Â { get; }

		/// <summary>
		/// Encoded public key, as defined in §6.1.
		/// </summary>
		public byte[] PublicKey { get; }

		/// <summary>
		/// Encoded private key, as defined in §6.1.
		/// </summary>
		public byte[] PrivateKey { get; }
	}
}
