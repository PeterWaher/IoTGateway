using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Authentication
{
	/// <summary>
	/// Base class for all hashed authentication methods.
	/// </summary>
	public abstract class HashedAuthenticationMethod : AuthenticationMethod
	{
		/// <summary>
		/// Base class for all hashed authentication methods.
		/// </summary>
		public HashedAuthenticationMethod()
			: base()
		{
		}

		/// <summary>
		/// Hash function
		/// </summary>
		/// <param name="Data">Data to hash.</param>
		/// <returns>Hash of data.</returns>
		public abstract byte[] H(byte[] Data);

		/// <summary>
		/// Hash function
		/// </summary>
		/// <param name="s">String to hash.</param>
		/// <returns>Hash of string.</returns>
		public virtual byte[] H(string s)
		{
			return this.H(System.Text.Encoding.UTF8.GetBytes(s));
		}

		/// <summary>
		/// See RFC 2104 for a description of the HMAC algorithm:
		/// http://tools.ietf.org/html/rfc2104
		/// </summary>
		/// <param name="Key">Key</param>
		/// <param name="Text">Text</param>
		/// <returns>HMAC(Key,Text)</returns>
		protected byte[] HMAC(byte[] Key, byte[] Text)
		{
			byte[] A1 = Key;
			if (A1.Length > 64)
				A1 = H(A1);

			if (A1.Length < 64)
				Array.Resize<byte>(ref A1, 64);

			byte[] A3 = (byte[])A1.Clone();
			int i;

			for (i = 0; i < 64; i++)
			{
				A1[i] ^= 0x5c;    // opad
				A3[i] ^= 0x36;    // ipad
			}

			return H(CONCAT(A1, H(CONCAT(A3, Text))));
		}

		protected byte[] KD(string k, string s)
		{
			return H(CONCAT(k, ":", s));
		}

	}
}
