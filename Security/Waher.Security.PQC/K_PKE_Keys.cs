namespace Waher.Security.PQC
{
	/// <summary>
	/// K-PKE encryption and decryption keys, as defined in §5.1.
	/// </summary>
	public class K_PKE_Keys
	{
		/// <summary>
		/// K-PKE encryption and decryption keys, as defined in §5.1.
		/// </summary>
		/// <param name="Â">Internal matrix used to generate encryption key.</param>
		/// <param name="EncryptionKey">Public Encyrption key.</param>
		/// <param name="DecryptionKey">Private Decryption key.</param>
		public K_PKE_Keys(ushort[,][] Â, byte[] EncryptionKey, byte[] DecryptionKey)
		{
			this.Â = Â;
			this.EncryptionKey = EncryptionKey;
			this.DecryptionKey = DecryptionKey;
		}

		/// <summary>
		/// Matrix of encryption keys, as defined in §5.1.
		/// </summary>
		public ushort[,][] Â { get; }

		/// <summary>
		/// Encoded encryption key, as defined in §5.1.
		/// </summary>
		public byte[] EncryptionKey { get; }

		/// <summary>
		/// Encoded decryption key, as defined in §5.1.
		/// </summary>
		public byte[] DecryptionKey { get; }
	}
}
