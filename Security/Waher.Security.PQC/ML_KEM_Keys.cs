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
		/// <param name="Seed">Seed value, if seed is to be remembered.</param>
		public ML_KEM_Keys(K_PKE_Keys Keys, byte[] DecryptionKey, byte[] Seed)
		{
			this.Â = Keys.Â;
			this.EncapsulationKey = Keys.EncryptionKey;
			this.DecapsulationKey = DecryptionKey;
			this.Seed = Seed;
		}

		/// <summary>
		/// ML_KEM encryption and decryption keys, as defined in §6.1.
		/// </summary>
		/// <param name="EncapsulationKey">Encapsulation key.</param>
		public ML_KEM_Keys(byte[] EncapsulationKey)
		{
			this.Â = null;
			this.EncapsulationKey = EncapsulationKey;
			this.DecapsulationKey = null;
			this.Seed = null;
		}

		/// <summary>
		/// Matrix of encryption keys, as defined in §5.1.
		/// </summary>
		public ushort[,][] Â
		{
			get;
			internal set;
		}

		/// <summary>
		/// Encoded encapsulation key, as defined in §6.1.
		/// </summary>
		public byte[] EncapsulationKey { get; }

		/// <summary>
		/// Encoded decapsulation key, as defined in §6.1.
		/// </summary>
		public byte[] DecapsulationKey { get; }

		/// <summary>
		/// Seed that generated key, if available.
		/// </summary>
		public byte[] Seed { get; }

		/// <summary>
		/// If the key contains a decapsulation key.
		/// </summary>
		public bool HasDecapsulationKey => (this.DecapsulationKey?.Length ?? 0) > 0;

		/// <summary>
		/// Creates a key object instance from an encapsulation key.
		/// </summary>
		/// <param name="EncapsulationKey">Encapsulation key.</param>
		/// <returns>Object instance.</returns>
		public static ML_KEM_Keys FromEncapsulationKey(byte[] EncapsulationKey)
		{
			return new ML_KEM_Keys(EncapsulationKey);
		}
	}
}
