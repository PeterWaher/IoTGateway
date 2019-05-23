using System.Numerics;
using Waher.Security.EllipticCurves;

namespace Waher.Networking.XMPP.P2P.E2E
{
	/// <summary>
	/// Curve25519 Montgomery Curve
	/// </summary>
	public class Curve25519Aes : EcAes256
    {
        /// <summary>
        /// Curve25519 Montgomery Curve
        /// </summary>
        public Curve25519Aes()
			: this(new Curve25519())
		{
		}

        /// <summary>
        /// Curve25519 Montgomery Curve
        /// </summary>
        /// <param name="Curve">Curve instance</param>
        public Curve25519Aes(Curve25519 Curve)
			: base(Curve)
		{
		}

        /// <summary>
        /// Curve25519 Montgomery Curve
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        public Curve25519Aes(byte[] PublicKey)
			: base(PublicKey, new Curve25519())
		{
		}

		/// <summary>
		/// Local name of the E2E encryption scheme
		/// </summary>
		public override string LocalName => "x25519";

		/// <summary>
		/// Security strength of End-to-End encryption scheme.
		/// </summary>
		public override int SecurityStrength => 128;

		/// <summary>
		/// Creates a new key.
		/// </summary>
		/// <param name="SecurityStrength">Overall desired security strength, if applicable.</param>
		/// <returns>New E2E endpoint.</returns>
		public override IE2eEndpoint Create(int SecurityStrength)
		{
			return new Curve25519Aes();
		}

        /// <summary>
        /// Creates a new endpoint given a private key.
        /// </summary>
        /// <param name="Secret">Secret.</param>
        /// <returns>Endpoint object.</returns>
        public override EcAes256 CreatePrivate(byte[] Secret)
		{
			return new Curve25519Aes(new Curve25519(Secret));
		}

        /// <summary>
        /// Creates a new endpoint given a public key.
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        /// <returns>Endpoint object.</returns>
        public override EcAes256 CreatePublic(byte[] PublicKey)
		{
			return new Curve25519Aes(PublicKey);
		}
	}
}
