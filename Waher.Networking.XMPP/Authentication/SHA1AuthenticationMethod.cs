using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Waher.Networking.XMPP.Authentication
{
	/// <summary>
	/// Base class for all SHA-1-hashed authentication methods.
	/// </summary>
	public abstract class SHA1AuthenticationMethod : HashedAuthenticationMethod
	{
		/// <summary>
		/// Base class for all SHA-1-hashed authentication methods.
		/// </summary>
		public SHA1AuthenticationMethod()
			: base()
		{
		}

		/// <summary>
		/// Hash function
		/// </summary>
		/// <param name="Data">Data to hash.</param>
		/// <returns>Hash of data.</returns>
		public override byte[] H(byte[] Data)
		{
			using (SHA1 SHA1 = SHA1.Create())
			{
				return SHA1.ComputeHash(Data);
			}
		}

	}
}
