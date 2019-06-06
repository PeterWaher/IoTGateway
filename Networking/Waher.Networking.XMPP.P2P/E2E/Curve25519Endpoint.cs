using System.Numerics;
using Waher.Networking.XMPP.P2P.SymmetricCiphers;
using Waher.Security.EllipticCurves;

namespace Waher.Networking.XMPP.P2P.E2E
{
	/// <summary>
	/// Curve25519 Montgomery Curve
	/// </summary>
	public class Curve25519Endpoint : EllipticCurveEndpoint
    {
        /// <summary>
        /// Curve25519 Montgomery Curve
        /// </summary>
        public Curve25519Endpoint()
            : this(new Curve25519())
        {
        }

        /// <summary>
        /// Curve25519 Montgomery Curve
        /// </summary>
        /// <param name="SymmetricCipher">Symmetric cipher to use by default.</param>
        public Curve25519Endpoint(IE2eSymmetricCipher SymmetricCipher)
            : this(new Curve25519(), SymmetricCipher)
        {
        }

        /// <summary>
        /// Curve25519 Montgomery Curve
        /// </summary>
        /// <param name="Curve">Curve instance</param>
        public Curve25519Endpoint(Curve25519 Curve)
			: this(Curve, new AeadChaCha20Poly1305())
		{
		}

        /// <summary>
        /// Curve25519 Montgomery Curve
        /// </summary>
        /// <param name="Curve">Curve instance</param>
        /// <param name="SymmetricCipher">Symmetric cipher to use by default.</param>
        public Curve25519Endpoint(Curve25519 Curve, IE2eSymmetricCipher SymmetricCipher)
            : base(Curve, SymmetricCipher)
        {
        }

        /// <summary>
        /// Curve25519 Montgomery Curve
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        public Curve25519Endpoint(byte[] PublicKey)
			: this(PublicKey, new AeadChaCha20Poly1305())
		{
		}

        /// <summary>
        /// Curve25519 Montgomery Curve
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        /// <param name="SymmetricCipher">Symmetric cipher to use by default.</param>
        public Curve25519Endpoint(byte[] PublicKey, IE2eSymmetricCipher SymmetricCipher)
            : base(PublicKey, new Curve25519(), SymmetricCipher)
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
        /// If signatures are supported.
        /// </summary>
        public override bool SupportsSignatures => false;

        /// <summary>
        /// Creates a new key.
        /// </summary>
        /// <param name="SecurityStrength">Overall desired security strength, if applicable.</param>
        /// <returns>New E2E endpoint.</returns>
        public override IE2eEndpoint Create(int SecurityStrength)
		{
			return new Curve25519Endpoint(this.DefaultSymmetricCipher);
		}

        /// <summary>
        /// Creates a new endpoint given a private key.
        /// </summary>
        /// <param name="Secret">Secret.</param>
        /// <returns>Endpoint object.</returns>
        public override IE2eEndpoint CreatePrivate(byte[] Secret)
		{
			return new Curve25519Endpoint(new Curve25519(Secret), this.DefaultSymmetricCipher);
		}

        /// <summary>
        /// Creates a new endpoint given a public key.
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        /// <returns>Endpoint object.</returns>
        public override IE2eEndpoint CreatePublic(byte[] PublicKey)
		{
			return new Curve25519Endpoint(PublicKey, this.DefaultSymmetricCipher);
		}
	}
}
