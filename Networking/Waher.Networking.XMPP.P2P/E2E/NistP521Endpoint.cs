using System.Numerics;
using Waher.Networking.XMPP.P2P.SymmetricCiphers;
using Waher.Security.EllipticCurves;

namespace Waher.Networking.XMPP.P2P.E2E
{
	/// <summary>
	/// NIST P-521 Curve
	/// </summary>
	public class NistP521Endpoint : NistEndpoint
    {
        /// <summary>
        /// NIST P-521 Curve
        /// </summary>
        public NistP521Endpoint()
            : this(new NistP521())
        {
        }

        /// <summary>
        /// NIST P-521 Curve
        /// </summary>
        /// <param name="SymmetricCipher">Symmetric cipher to use by default.</param>
        public NistP521Endpoint(IE2eSymmetricCipher SymmetricCipher)
            : this(new NistP521(), SymmetricCipher)
        {
        }

        /// <summary>
        /// NIST P-521 Curve
        /// </summary>
        /// <param name="Curve">Curve instance</param>
        public NistP521Endpoint(NistP521 Curve)
            : this(Curve, new Aes256())
        {
        }

        /// <summary>
        /// NIST P-521 Curve
        /// </summary>
        /// <param name="Curve">Curve instance</param>
        /// <param name="SymmetricCipher">Symmetric cipher to use by default.</param>
        public NistP521Endpoint(NistP521 Curve, IE2eSymmetricCipher SymmetricCipher)
            : base(Curve, SymmetricCipher)
        {
        }

        /// <summary>
        /// NIST P-521 Curve
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        public NistP521Endpoint(byte[] PublicKey)
            : this(PublicKey, new Aes256())
        {
        }

        /// <summary>
        /// NIST P-521 Curve
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        /// <param name="SymmetricCipher">Symmetric cipher to use by default.</param>
        public NistP521Endpoint(byte[] PublicKey, IE2eSymmetricCipher SymmetricCipher)
            : base(PublicKey, new NistP521(), SymmetricCipher)
        {
        }

        /// <summary>
        /// Local name of the E2E encryption scheme
        /// </summary>
        public override string LocalName => "p521";

		/// <summary>
		/// Security strength of End-to-End encryption scheme.
		/// </summary>
		public override int SecurityStrength => 256;

		/// <summary>
		/// Creates a new key.
		/// </summary>
		/// <param name="SecurityStrength">Overall desired security strength, if applicable.</param>
		/// <returns>New E2E endpoint.</returns>
		public override IE2eEndpoint Create(int SecurityStrength)
		{
			return new NistP521Endpoint(this.DefaultSymmetricCipher);
		}

        /// <summary>
        /// Creates a new endpoint given a private key.
        /// </summary>
        /// <param name="Secret">Secret.</param>
        /// <returns>Endpoint object.</returns>
        public override IE2eEndpoint CreatePrivate(byte[] Secret)
        {
            return new NistP521Endpoint(new NistP521(Secret), this.DefaultSymmetricCipher);
        }

        /// <summary>
        /// Creates a new endpoint given a public key.
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        /// <returns>Endpoint object.</returns>
        public override IE2eEndpoint CreatePublic(byte[] PublicKey)
        {
            return new NistP521Endpoint(PublicKey, this.DefaultSymmetricCipher);
        }
    }
}
