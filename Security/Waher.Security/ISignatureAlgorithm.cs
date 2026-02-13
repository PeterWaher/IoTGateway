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
		/// <param name="BigEndian">Indicates if the signature should be in big-endian format.</param>
		/// <returns>Signature.</returns>
		byte[] Sign(byte[] Data, bool BigEndian);

		/// <summary>
		/// Creates a signature of <paramref name="Data"/>.
		/// </summary>
		/// <param name="Data">Payload to sign.</param>
		/// <param name="BigEndian">Indicates if the signature should be in big-endian format.</param>
		/// <returns>Signature.</returns>
		byte[] Sign(Stream Data, bool BigEndian);

		/// <summary>
		/// Verifies a signature of <paramref name="Data"/>.
		/// </summary>
		/// <param name="Data">Payload to sign.</param>
		/// <param name="PublicKey">Public Key of the entity that generated the signature.</param>
		/// <param name="BigEndian">Indicates if the signature is in big-endian format.</param>
		/// <param name="Signature">Signature</param>
		/// <returns>If the signature is valid.</returns>
		bool Verify(byte[] Data, byte[] PublicKey, bool BigEndian, byte[] Signature);

		/// <summary>
		/// Verifies a signature of <paramref name="Data"/>.
		/// </summary>
		/// <param name="Data">Payload to sign.</param>
		/// <param name="PublicKey">Public Key of the entity that generated the signature.</param>
		/// <param name="BigEndian">Indicates if the signature is in big-endian format.</param>
		/// <param name="Signature">Signature</param>
		/// <returns>If the signature is valid.</returns>
		bool Verify(Stream Data, byte[] PublicKey, bool BigEndian, byte[] Signature);
	}
}
