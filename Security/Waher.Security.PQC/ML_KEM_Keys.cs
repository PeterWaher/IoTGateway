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
			this.ρ = Keys.ρ;
			this.σ = Keys.σ;
			this.s = Keys.s;
			this.t = Keys.t;
			this.e = Keys.e;
			this.ŝ = Keys.ŝ;
			this.ê = Keys.ê;
		}

		/// <summary>
		/// Matrix of encryption keys, as defined in §5.1.
		/// </summary>
		public ushort[,][] Â { get; }

		/// <summary>
		/// ρ used to generate internal matrix.
		/// </summary>
		public byte[] ρ { get; }

		/// <summary>
		/// σ used to generate samples and errors.
		/// </summary>
		public byte[] σ { get; }

		/// <summary>
		/// Intermediate s vector, null by default.
		/// </summary>
		public ushort[][] s { get; }

		/// <summary>
		/// Intermediate ŝ vector, null by default.
		/// </summary>
		public ushort[][] ŝ { get; }

		/// <summary>
		/// Intermediate t vector, null by default.
		/// </summary>
		public ushort[][] t { get; }

		/// <summary>
		/// Intermediate e vector, null by default.
		/// </summary>
		public ushort[][] e { get; }

		/// <summary>
		/// Intermediate ê vector, null by default.
		/// </summary>
		public ushort[][] ê { get; }

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
