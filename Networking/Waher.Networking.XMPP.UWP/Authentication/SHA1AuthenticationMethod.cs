using System;
using System.Collections.Generic;
using System.Text;
#if WINDOWS_UWP
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
#else
using System.Security.Cryptography;
#endif

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
#if WINDOWS_UWP
			HashAlgorithmProvider Provider = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);
			CryptographicHash Hash = Provider.CreateHash();

			Hash.Append(CryptographicBuffer.CreateFromByteArray(Data));

			byte[] Result;
			CryptographicBuffer.CopyToByteArray(Hash.GetValueAndReset(), out Result);

			return Result;
#else
			using (SHA1 SHA1 = SHA1.Create())
			{
				return SHA1.ComputeHash(Data);
			}
#endif
		}

	}
}
