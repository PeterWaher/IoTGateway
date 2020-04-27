using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Security.DTLS
{
	/// <summary>
	/// Pre-shared key.
	/// </summary>
    public class PresharedKey : IDtlsCredentials
    {
		private readonly byte[] identity;
		private readonly byte[] key;

		/// <summary>
		/// Pre-shared key.
		/// </summary>
		/// <param name="Identity">Identity.</param>
		/// <param name="Key">Secret key.</param>
		public PresharedKey(string Identity, string Key)
			: this(Encoding.UTF8.GetBytes(Identity), Encoding.UTF8.GetBytes(Key))
		{
		}

		/// <summary>
		/// Pre-shared key.
		/// </summary>
		/// <param name="Identity">Identity.</param>
		/// <param name="Key">Secret binary key.</param>
		public PresharedKey(string Identity, byte[] Key)
			: this(Encoding.UTF8.GetBytes(Identity), Key)
		{
		}

		/// <summary>
		/// Pre-shared key.
		/// </summary>
		/// <param name="Identity">UTF-8 encoded Identity.</param>
		/// <param name="Key">Secret binary key.</param>
		public PresharedKey(byte[] Identity, byte[] Key)
		{
			this.identity = Identity;
			this.key = Key;
		}

		/// <summary>
		/// UTF-8 encoded Identity.
		/// </summary>
		public byte[] Identity
		{
			get { return this.identity; }
		}

		/// <summary>
		/// Secret binary key.
		/// </summary>
		public byte[] Key
		{
			get { return this.key; }
		}
    }
}
