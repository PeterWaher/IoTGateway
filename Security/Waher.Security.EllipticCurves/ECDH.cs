using System;

namespace Waher.Security.EllipticCurves
{
    /// <summary>
    /// Implements the Elliptic Curve Diffie-Hellman (ECDH) algorithm.
    /// </summary>
    public static class ECDH
    {
		/// <summary>
		/// Gets a shared key using the Elliptic Curve Diffie-Hellman (ECDH) algorithm.
		/// </summary>
		/// <param name="LocalPrivateKey">Local private key.</param>
		/// <param name="RemotePublicKey">Public key of the remote party.</param>
		/// <param name="RemoteBigEndian">Indicates if the remote public key is in big-endian format.</param>
		/// <param name="Curve">Elliptic curve used.</param>
		/// <returns>Shared secret.</returns>
		public static PointOnCurve GetSharedPoint(byte[] LocalPrivateKey, byte[] RemotePublicKey,
			bool RemoteBigEndian, EllipticCurve Curve)
		{
			PointOnCurve PublicKey = Curve.Decode(RemotePublicKey, RemoteBigEndian);
			return Curve.ScalarMultiplication(LocalPrivateKey, PublicKey, true);
		}

		/// <summary>
		/// Gets a shared key using the Elliptic Curve Diffie-Hellman (ECDH) algorithm.
		/// </summary>
		/// <param name="LocalPrivateKey">Local private key.</param>
		/// <param name="RemotePublicKey">Public key of the remote party.</param>
		/// <param name="RemoteBigEndian">Indicates if the remote public key is in big-endian format.</param>
		/// <param name="HashFunction">A Hash function is applied to the derived key to generate the shared secret.
		/// The derived key, as a byte array of equal size as the order of the prime field, ordered by most significant byte first,
		/// is passed on to the hash function before being returned as the shared key.</param>
		/// <param name="Curve">Elliptic curve used.</param>
		/// <returns>Shared secret.</returns>
		public static byte[] GetSharedKey(byte[] LocalPrivateKey, byte[] RemotePublicKey, 
            bool RemoteBigEndian, HashFunctionArray HashFunction, EllipticCurve Curve)
        {
			PointOnCurve P = GetSharedPoint(LocalPrivateKey, RemotePublicKey, RemoteBigEndian, Curve);

            byte[] B = P.X.ToByteArray();	// Little endian

            if (B.Length != Curve.OrderBytes)
                Array.Resize(ref B, Curve.OrderBytes);

            Array.Reverse(B);   // Most significant byte first.

            return HashFunction(B);
        }
    }
}
