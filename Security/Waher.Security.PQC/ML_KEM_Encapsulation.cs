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
		public ML_KEM_Encapsulation(byte[] SharedSecret, byte[] CipherText)
		{
			this.SharedSecret = SharedSecret;
			this.CipherText = CipherText;
		}

		/// <summary>
		/// Shared Secret K.
		/// </summary>
		public byte[] SharedSecret { get; }

		/// <summary>
		/// Cipher Text c.
		/// </summary>
		public byte[] CipherText { get; }
	}
}
