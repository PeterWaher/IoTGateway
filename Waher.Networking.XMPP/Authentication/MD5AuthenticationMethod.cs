using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Waher.Networking.XMPP.Authentication
{
	/// <summary>
	/// Base class for all MD5-hashed authentication methods.
	/// </summary>
	public abstract class MD5AuthenticationMethod : HashedAuthenticationMethod
	{
		/// <summary>
		/// Base class for all MD5-hashed authentication methods.
		/// </summary>
		public MD5AuthenticationMethod()
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
			using (MD5 MD5 = MD5.Create())
			{
				return MD5.ComputeHash(Data);
			}
		}

	}
}
