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
#if WINDOWS_UWP
			HashAlgorithmProvider Provider = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
			CryptographicHash Hash = Provider.CreateHash();

			Hash.Append(CryptographicBuffer.CreateFromByteArray(Data));
			
			byte[] Result;
			CryptographicBuffer.CopyToByteArray(Hash.GetValueAndReset(), out Result);

			return Result;
#else
			using (MD5 MD5 = MD5.Create())
			{
				return MD5.ComputeHash(Data);
			}
#endif
		}

	}
}
