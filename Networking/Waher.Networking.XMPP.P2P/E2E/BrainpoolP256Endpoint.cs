using Waher.Networking.XMPP.P2P.SymmetricCiphers;
using Waher.Security.EllipticCurves;

namespace Waher.Networking.XMPP.P2P.E2E
{
	/// <summary>
	/// Brainpool P-256 Curve
	/// </summary>
	public class BrainpoolP256Endpoint : BrainpoolEndpoint
    {
        /// <summary>
        /// Brainpool P-256 Curve
        /// </summary>
        public BrainpoolP256Endpoint()
            : this(new BrainpoolP256())
        {
        }

        /// <summary>
        /// Brainpool P-256 Curve
        /// </summary>
        /// <param name="SymmetricCipher">Symmetric cipher to use by default.</param>
        public BrainpoolP256Endpoint(IE2eSymmetricCipher SymmetricCipher)
            : this(new BrainpoolP256(), SymmetricCipher)
        {
        }

        /// <summary>
        /// Brainpool P-256 Curve
        /// </summary>
        /// <param name="Curve">Curve instance</param>
        public BrainpoolP256Endpoint(BrainpoolP256 Curve)
            : this(Curve, new Aes256())
        {
        }

        /// <summary>
        /// Brainpool P-256 Curve
        /// </summary>
        /// <param name="Curve">Curve instance</param>
        /// <param name="SymmetricCipher">Symmetric cipher to use by default.</param>
        public BrainpoolP256Endpoint(BrainpoolP256 Curve, IE2eSymmetricCipher SymmetricCipher)
            : base(Curve, SymmetricCipher)
        {
        }

        /// <summary>
        /// Brainpool P-256 Curve
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        public BrainpoolP256Endpoint(byte[] PublicKey)
            : this(PublicKey, new Aes256())
        {
        }

        /// <summary>
        /// Brainpool P-256 Curve
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        /// <param name="SymmetricCipher">Symmetric cipher to use by default.</param>
        public BrainpoolP256Endpoint(byte[] PublicKey, IE2eSymmetricCipher SymmetricCipher)
            : base(PublicKey, new BrainpoolP256(), SymmetricCipher)
        {
        }

        /// <summary>
        /// Local name of the E2E encryption scheme
        /// </summary>
        public override string LocalName => "bp256";

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
			return new BrainpoolP256Endpoint(this.DefaultSymmetricCipher.CreteNew());
		}

        /// <summary>
        /// Creates a new endpoint given a private key.
        /// </summary>
        /// <param name="Secret">Secret.</param>
        /// <returns>Endpoint object.</returns>
        public override IE2eEndpoint CreatePrivate(byte[] Secret)
        {
            return new BrainpoolP256Endpoint(new BrainpoolP256(Secret), this.DefaultSymmetricCipher.CreteNew());
        }

        /// <summary>
        /// Creates a new endpoint given a public key.
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        /// <returns>Endpoint object.</returns>
        public override IE2eEndpoint CreatePublic(byte[] PublicKey)
        {
            return new BrainpoolP256Endpoint(PublicKey, this.DefaultSymmetricCipher.CreteNew());
        }
    }
}
