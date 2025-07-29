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
		/// <param name="A">Internal matrix used to generate encryption key.</param>
		/// <param name="EncryptionKey">Public Encyrption key.</param>
		/// <param name="DecryptionKey">Private Decryption key.</param>
		/// <param name="ρ">ρ used to generate internal matrix.</param>
		/// <param name="σ">σ used to generate samples and errors.</param>
		public K_PKE_Keys(ushort[,][] A, byte[] EncryptionKey, byte[] DecryptionKey,
			byte[] ρ, byte[] σ)
			: this(A, EncryptionKey, DecryptionKey, ρ, σ, null, null, null, null, null)
		{
		}

		/// <summary>
		/// K-PKE encryption and decryption keys, as defined in §5.1.
		/// </summary>
		/// <param name="Â">Internal matrix used to generate encryption key.</param>
		/// <param name="EncryptionKey">Public Encyrption key.</param>
		/// <param name="DecryptionKey">Private Decryption key.</param>
		/// <param name="ρ">ρ used to generate internal matrix.</param>
		/// <param name="σ">σ used to generate samples and errors.</param>
		/// <param name="s">Intermediate s vector, null by default.</param>
		/// <param name="e">Intermediate e vector, null by default.</param>
		/// <param name="ŝ">Intermediate ŝ vector, null by default.</param>
		/// <param name="ê">Intermediate ê vector, null by default.</param>
		/// <param name="t">Intermediate t vector, null by default.</param>
		public K_PKE_Keys(ushort[,][] Â, byte[] EncryptionKey, byte[] DecryptionKey,
			byte[] ρ, byte[] σ, ushort[][] s, ushort[][] e, ushort[][] ŝ, ushort[][] ê,
			ushort[][] t)
		{
			this.Â = Â;
			this.EncryptionKey = EncryptionKey;
			this.DecryptionKey = DecryptionKey;
			this.ρ = ρ;
			this.σ = σ;
			this.s = s;
			this.t = t;
			this.e = e;
			this.ŝ = ŝ;
			this.ê = ê;
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
		/// Encoded encryption key, as defined in §5.1.
		/// </summary>
		public byte[] EncryptionKey { get; }

		/// <summary>
		/// Encoded decryption key, as defined in §5.1.
		/// </summary>
		public byte[] DecryptionKey { get; }
	}
}
