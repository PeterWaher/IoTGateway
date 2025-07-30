namespace Waher.Security.PQC
{
	/// <summary>
	/// ML_KEM encapsulation keys, as defined in §7.2.
	/// </summary>
	public class ML_KEM_Encapsulation
	{
		/// <summary>
		/// ML_KEM encapsulation keys, as defined in §7.2.
		/// </summary>
		/// <param name="SharedSecret">Shared Secret K.</param>
		/// <param name="CipherText">Cipher Text c.</param>
		public ML_KEM_Encapsulation(byte[] SharedSecret, K_PKE_Encryption CipherText)
			: this(SharedSecret, CipherText, null)
		{
		}

		/// <summary>
		/// ML_KEM encapsulation keys, as defined in §7.2.
		/// </summary>
		/// <param name="SharedSecret">Shared Secret K.</param>
		/// <param name="CipherText">Cipher Text c.</param>
		/// <param name="r">Intermediate value r.</param>
		public ML_KEM_Encapsulation(byte[] SharedSecret, K_PKE_Encryption CipherText, 
			byte[] r)
		{
			this.SharedSecret = SharedSecret;
			this.CipherText = CipherText.CipherText;
			this.r = r;
			this.t = CipherText.t;
			this.y = CipherText.y;
			this.e1 = CipherText.e1;
			this.e2 = CipherText.e2;
			this.y2 = CipherText.y2;
			this.u0 = CipherText.u0;
			this.u1 = CipherText.u1;
			this.u2 = CipherText.u2;
			this.u = CipherText.u;
			this.μ = CipherText.μ;
			this.v0 = CipherText.v0;
			this.v1 = CipherText.v1;
			this.v2 = CipherText.v2;
			this.v = CipherText.v;
		}

		/// <summary>
		/// Shared Secret K.
		/// </summary>
		public byte[] SharedSecret { get; }

		/// <summary>
		/// Cipher Text c.
		/// </summary>
		public byte[] CipherText { get; }

		/// <summary>
		/// Intermediate value r.
		/// </summary>
		public byte[] r { get; }

		/// <summary>
		/// Intermediate value t.
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
