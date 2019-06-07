using System.Numerics;
using Waher.Networking.XMPP.P2P.SymmetricCiphers;
using Waher.Security.EllipticCurves;

namespace Waher.Networking.XMPP.P2P.E2E
{
	/// <summary>
	/// NIST P-224 Curve
	/// </summary>
	public class NistP224Endpoint : NistEndpoint
    {
        /// <summary>
        /// NIST P-224 Curve
        /// </summary>
        public NistP224Endpoint()
            : this(new NistP224())
        {
        }

        /// <summary>
        /// NIST P-224 Curve
        /// </summary>
        /// <param name="SymmetricCipher">Symmetric cipher to use by default.</param>
        public NistP224Endpoint(IE2eSymmetricCipher SymmetricCipher)
            : this(new NistP224(), SymmetricCipher)
        {
        }

        /// <summary>
        /// NIST P-224 Curve
        /// </summary>
        /// <param name="Curve">Curve instance</param>
        public NistP224Endpoint(NistP224 Curve)
            : this(Curve, new Aes256())
        {
        }

        /// <summary>
        /// NIST P-224 Curve
        /// </summary>
        /// <param name="Curve">Curve instance</param>
        /// <param name="SymmetricCipher">Symmetric cipher to use by default.</param>
        public NistP224Endpoint(NistP224 Curve, IE2eSymmetricCipher SymmetricCipher)
            : base(Curve, SymmetricCipher)
        {
        }

        /// <summary>
        /// NIST P-224 Curve
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        public NistP224Endpoint(byte[] PublicKey)
            : this(PublicKey, new Aes256())
        {
        }

        /// <summary>
        /// NIST P-224 Curve
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        /// <param name="SymmetricCipher">Symmetric cipher to use by default.</param>
        public NistP224Endpoint(byte[] PublicKey, IE2eSymmetricCipher SymmetricCipher)
            : base(PublicKey, new NistP224(), SymmetricCipher)
        {
        }

        /// <summary>
        /// Local name of the E2E encryption scheme
        /// </summary>
        public override string LocalName => "p224";

		/// <summary>
		/// Security strength of End-to-End encryption scheme.
		/// </summary>
		public override int SecurityStrength => 112;

		/// <summary>
		/// Creates a new key.
		/// </summary>
		/// <param name="SecurityStrength">Overall desired security strength, if applicable.</param>
		/// <returns>New E2E endpoint.</returns>
		public override IE2eEndpoint Create(int SecurityStrength)
		{
			return new NistP224Endpoint(this.DefaultSymmetricCipher.CreteNew());
		}

        /// <summary>
        /// Creates a new endpoint given a private key.
        /// </summary>
        /// <param name="Secret">Secret.</param>
        /// <returns>Endpoint object.</returns>
        public override IE2eEndpoint CreatePrivate(byte[] Secret)
        {
            return new NistP224Endpoint(new NistP224(Secret), this.DefaultSymmetricCipher.CreteNew());
        }

        /// <summary>
        /// Creates a new endpoint given a public key.
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        /// <returns>Endpoint object.</returns>
        public override IE2eEndpoint CreatePublic(byte[] PublicKey)
        {
            return new NistP224Endpoint(PublicKey, this.DefaultSymmetricCipher.CreteNew());
        }
    }
}
