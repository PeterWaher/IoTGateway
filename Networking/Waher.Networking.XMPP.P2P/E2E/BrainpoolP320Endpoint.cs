using Waher.Networking.XMPP.P2P.SymmetricCiphers;
using Waher.Security.EllipticCurves;

namespace Waher.Networking.XMPP.P2P.E2E
{
	/// <summary>
	/// Brainpool P-320 Curve
	/// </summary>
	public class BrainpoolP320Endpoint : BrainpoolEndpoint
    {
        /// <summary>
        /// Brainpool P-320 Curve
        /// </summary>
        public BrainpoolP320Endpoint()
            : this(new BrainpoolP320())
        {
        }

        /// <summary>
        /// Brainpool P-320 Curve
        /// </summary>
        /// <param name="SymmetricCipher">Symmetric cipher to use by default.</param>
        public BrainpoolP320Endpoint(IE2eSymmetricCipher SymmetricCipher)
            : this(new BrainpoolP320(), SymmetricCipher)
        {
        }

        /// <summary>
        /// Brainpool P-320 Curve
        /// </summary>
        /// <param name="Curve">Curve instance</param>
        public BrainpoolP320Endpoint(BrainpoolP320 Curve)
            : this(Curve, new Aes256())
        {
        }

        /// <summary>
        /// Brainpool P-320 Curve
        /// </summary>
        /// <param name="Curve">Curve instance</param>
        /// <param name="SymmetricCipher">Symmetric cipher to use by default.</param>
        public BrainpoolP320Endpoint(BrainpoolP320 Curve, IE2eSymmetricCipher SymmetricCipher)
            : base(Curve, SymmetricCipher)
        {
        }

        /// <summary>
        /// Brainpool P-320 Curve
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        public BrainpoolP320Endpoint(byte[] PublicKey)
            : this(PublicKey, new Aes256())
        {
        }

        /// <summary>
        /// Brainpool P-320 Curve
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        /// <param name="SymmetricCipher">Symmetric cipher to use by default.</param>
        public BrainpoolP320Endpoint(byte[] PublicKey, IE2eSymmetricCipher SymmetricCipher)
            : base(PublicKey, new BrainpoolP320(), SymmetricCipher)
        {
        }

        /// <summary>
        /// Local name of the E2E encryption scheme
        /// </summary>
        public override string LocalName => "bp320";

		/// <summary>
		/// Security strength of End-to-End encryption scheme.
		/// </summary>
		public override int SecurityStrength => 160;

		/// <summary>
		/// Creates a new key.
		/// </summary>
		/// <param name="SecurityStrength">Overall desired security strength, if applicable.</param>
		/// <returns>New E2E endpoint.</returns>
		public override IE2eEndpoint Create(int SecurityStrength)
		{
			return new BrainpoolP320Endpoint(this.DefaultSymmetricCipher.CreteNew());
		}

        /// <summary>
        /// Creates a new endpoint given a private key.
        /// </summary>
        /// <param name="Secret">Secret.</param>
        /// <returns>Endpoint object.</returns>
        public override IE2eEndpoint CreatePrivate(byte[] Secret)
        {
            return new BrainpoolP320Endpoint(new BrainpoolP320(Secret), this.DefaultSymmetricCipher.CreteNew());
        }

        /// <summary>
        /// Creates a new endpoint given a public key.
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        /// <returns>Endpoint object.</returns>
        public override IE2eEndpoint CreatePublic(byte[] PublicKey)
        {
            return new BrainpoolP320Endpoint(PublicKey, this.DefaultSymmetricCipher.CreteNew());
        }
    }
}
