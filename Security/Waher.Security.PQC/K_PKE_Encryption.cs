namespace Waher.Security.PQC
{
	/// <summary>
	/// K-PKE encryption resuls, as defined in §5.2.
	/// </summary>
	public class K_PKE_Encryption
	{
		/// <summary>
		/// K-PKE encryption resuls, as defined in §5.2.
		/// </summary>
		/// <param name="CipherText">Ciphertext (32*(dᵤk+dᵥ) bytes)</param>
		public K_PKE_Encryption(byte[] CipherText)
			: this(CipherText, null, null, null, null, null, null, null, null, null, null,
				  null, null, null, null)
		{
		}

		/// <summary>
		/// K-PKE encryption resuls, as defined in §5.2.
		/// </summary>
		/// <param name="CipherText">Ciphertext (32*(dᵤk+dᵥ) bytes)</param>
		/// <param name="t">Intermediate t vector, null by default.</param>
		/// <param name="y">Intermediate y vector, null by default.</param>
		/// <param name="e1">Intermediate e1 vector, null by default.</param>
		/// <param name="e2">Intermediate e2 vector, null by default.</param>
		/// <param name="y2">Intermediate y2 vector, null by default.</param>
		/// <param name="u0">Intermediate u0 vector, null by default.</param>
		/// <param name="u1">Intermediate u1 vector, null by default.</param>
		/// <param name="u2">Intermediate u2 vector, null by default.</param>
		/// <param name="u">Intermediate u vector, null by default.</param>
		/// <param name="v0">Intermediate v0 vector, null by default.</param>
		/// <param name="v1">Intermediate v1 vector, null by default.</param>
		/// <param name="v2">Intermediate v2 vector, null by default.</param>
		/// <param name="v">Intermediate v vector, null by default.</param>
		public K_PKE_Encryption(byte[] CipherText, ushort[][] t, ushort[][] y, ushort[][] e1,
			ushort[] e2, ushort[][] y2, ushort[][] u0, ushort[][] u1, ushort[][] u2, 
			ushort[][] u, ushort[] μ, ushort[] v0, ushort[] v1, ushort[] v2, ushort[] v)
		{
			this.CipherText = CipherText;
			this.t = t;
			this.y = y;
			this.e1 = e1;
			this.e2 = e2;
			this.y2 = y2;
			this.u0 = u0;
			this.u1 = u1;
			this.u2 = u2;
			this.u = u;
			this.μ = μ;
			this.v0 = v0;
			this.v1 = v1;
			this.v2 = v2;
			this.v = v;
		}

		/// <summary>
		/// Ciphertext (32*(dᵤk+dᵥ) bytes)
		/// </summary>
		public byte[] CipherText { get; }

		/// <summary>
		/// Intermediate t vector, null by default.
		/// </summary>
		public ushort[][] t { get; }

		/// <summary>
		/// Intermediate y vector, null by default.
		/// </summary>
		public ushort[][] y { get; }

		/// <summary>
		/// Intermediate e1 vector, null by default.
		/// </summary>
		public ushort[][] e1 { get; }

		/// <summary>
		/// Intermediate e2 vector, null by default.
		/// </summary>
		public ushort[] e2 { get; }

		/// <summary>
		/// Intermediate y2 vector, null by default.
		/// </summary>
		public ushort[][] y2 { get; }

		/// <summary>
		/// Intermediate u0 vector, null by default.
		/// </summary>
		public ushort[][] u0 { get; }

		/// <summary>
		/// Intermediate u1 vector, null by default.
		/// </summary>
		public ushort[][] u1 { get; }

		/// <summary>
		/// Intermediate u2 vector, null by default.
		/// </summary>
		public ushort[][] u2 { get; }

		/// <summary>
		/// Intermediate u vector, null by default.
		/// </summary>
		public ushort[][] u { get; }

		/// <summary>
		/// Intermediate μ vector, null by default.
		/// </summary>
		public ushort[] μ { get; }

		/// <summary>
		/// Intermediate v0 vector, null by default.
		/// </summary>
		public ushort[] v0 { get; }

		/// <summary>
		/// Intermediate v1 vector, null by default.
		/// </summary>
		public ushort[] v1 { get; }

		/// <summary>
		/// Intermediate v2 vector, null by default.
		/// </summary>
		public ushort[] v2 { get; }

		/// <summary>
		/// Intermediate v vector, null by default.
		/// </summary>
		public ushort[] v { get; }
	}
}
