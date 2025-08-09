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
		/// <param name="Seed">Seed value, if seed is to be remembered.</param>
		public ML_DSA_Keys(uint[,][] Â, byte[] PublicKey, byte[] PrivateKey, byte[] Seed)
		{
			this.Â = Â;
			this.PublicKey = PublicKey;
			this.PrivateKey = PrivateKey;
			this.Seed = Seed;
		}

		/// <summary>
		/// Matrix of encryption keys, as defined in §6.1.
		/// </summary>
		public uint[,][] Â 
		{
			get;
			internal set;
		}

		/// <summary>
		/// Encoded public key, as defined in §6.1.
		/// </summary>
		public byte[] PublicKey { get; }

		/// <summary>
		/// Encoded private key, as defined in §6.1.
		/// </summary>
		public byte[] PrivateKey { get; }

		/// <summary>
		/// If the key contains a private key.
		/// </summary>
		public bool HasPrivateKey => (this.PrivateKey?.Length ?? 0) > 0;

		/// <summary>
		/// Seed that generated key, if available.
		/// </summary>
		public byte[] Seed { get; }

		/// <summary>
		/// Creates a key object instance from a public key.
		/// </summary>
		/// <param name="PublicKey">Public key.</param>
		/// <returns>Object instance.</returns>
		public static ML_DSA_Keys FromPublicKey(byte[] PublicKey)
		{
			return new ML_DSA_Keys(null, PublicKey, null, null);
		}

		/// <summary>
		/// Creates a key object instance from a private key.
		/// </summary>
		/// <param name="PrivateKey">Private key.</param>
		/// <returns>Object instance.</returns>
		public static ML_DSA_Keys FromPrivateKey(byte[] PrivateKey)
		{
			return new ML_DSA_Keys(null, null, PrivateKey, null);
		}
	}
}
