namespace Waher.Security.PQC
{
	/// <summary>
	/// ML_KEM encryption and decryption keys, as defined in §6.1.
	/// </summary>
	public class ML_KEM_Keys
	{
		/// <summary>
		/// ML_KEM encryption and decryption keys, as defined in §6.1.
		/// </summary>
		/// <param name="Keys">K-PKE keys.</param>
		/// <param name="DecryptionKey">Ammended decryption key.</param>
		public ML_KEM_Keys(K_PKE_Keys Keys, byte[] DecryptionKey)
		{
			this.Â = Keys.Â;
			this.EncapsulationKey = Keys.EncryptionKey;
			this.DecapsulationKey = DecryptionKey;
		}

		/// <summary>
		/// Matrix of encryption keys, as defined in §5.1.
		/// </summary>
		public ushort[,][] Â { get; }

		/// <summary>
		/// Encoded encapsulation key, as defined in §6.1.
		/// </summary>
		public byte[] EncapsulationKey { get; }

		/// <summary>
		/// Encoded decapsulation key, as defined in §6.1.
		/// </summary>
		public byte[] DecapsulationKey { get; }
	}
}
