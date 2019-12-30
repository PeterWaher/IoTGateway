using System;
using System.Collections.Generic;
using System.IO;

namespace Waher.Security
{
	/// <summary>
	/// Interface for digital signature algorithms.
	/// </summary>
	public interface ISignatureAlgorithm
	{
		/// <summary>
		/// Creates a signature of <paramref name="Data"/>.
		/// </summary>
		/// <param name="Data">Payload to sign.</param>
		/// <returns>Signature.</returns>
		byte[] Sign(byte[] Data);

		/// <summary>
		/// Creates a signature of <paramref name="Data"/>.
		/// </summary>
		/// <param name="Data">Payload to sign.</param>
		/// <returns>Signature.</returns>
		byte[] Sign(Stream Data);

		/// <summary>
		/// Verifies a signature of <paramref name="Data"/>.
		/// </summary>
		/// <param name="Data">Payload to sign.</param>
		/// <param name="PublicKey">Public Key of the entity that generated the signature.</param>
		/// <param name="Signature">Signature</param>
		/// <returns>If the signature is valid.</returns>
		bool Verify(byte[] Data, byte[] PublicKey, byte[] Signature);

		/// <summary>
		/// Verifies a signature of <paramref name="Data"/>.
		/// </summary>
		/// <param name="Data">Payload to sign.</param>
		/// <param name="PublicKey">Public Key of the entity that generated the signature.</param>
		/// <param name="Signature">Signature</param>
		/// <returns>If the signature is valid.</returns>
		bool Verify(Stream Data, byte[] PublicKey, byte[] Signature);
	}
}
