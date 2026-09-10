using System;
using Waher.Security;
using Waher.Security.E2EE;

namespace Waher.Networking.XMPP.P2P.SymmetricCiphers
{
	/// <summary>
	/// Static class providing methods for creating symmetric cipher algorithms.
	/// </summary>
	public static class SymmetricCipher
	{
		/// <summary>
		/// Creates an instance of a symmetric cipher algorithm.
		/// </summary>
		/// <param name="Algorithm">Algorithm to create.</param>
		/// <returns>Algorithm instance.</returns>
		/// <exception cref="ArgumentException">If algorithm is not recognized.</exception>
		public static IE2eSymmetricCipher Create(SymmetricCipherAlgorithms Algorithm)
		{
			return Algorithm switch
			{
				SymmetricCipherAlgorithms.Aes256 => new Aes256(),
				SymmetricCipherAlgorithms.ChaCha20 => new ChaCha20(),
				SymmetricCipherAlgorithms.AeadChaCha20Poly1305 => new AeadChaCha20Poly1305(),
				_ => throw new ArgumentException("Unrecognized algorithm: " + Algorithm.ToString(), nameof(Algorithm)),
			};
		}
	}
}
