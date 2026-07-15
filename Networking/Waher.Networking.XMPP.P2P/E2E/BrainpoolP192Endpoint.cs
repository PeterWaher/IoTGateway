using Waher.Networking.XMPP.P2P.SymmetricCiphers;
using Waher.Security.EllipticCurves;

namespace Waher.Networking.XMPP.P2P.E2E
{
	/// <summary>
	/// Brainpool P-192 Curve
	/// </summary>
	public class BrainpoolP192Endpoint : BrainpoolEndpoint
    {
        /// <summary>
        /// Brainpool P-192 Curve
        /// </summary>
        public BrainpoolP192Endpoint()
            : this(new BrainpoolP192())
        {
        }

        /// <summary>
        /// Brainpool P-192 Curve
        /// </summary>
        /// <param name="SymmetricCipher">Symmetric cipher to use by default.</param>
        public BrainpoolP192Endpoint(IE2eSymmetricCipher SymmetricCipher)
            : this(new BrainpoolP192(), SymmetricCipher)
        {
        }

        /// <summary>
        /// Brainpool P-192 Curve
        /// </summary>
        /// <param name="Curve">Curve instance</param>
        public BrainpoolP192Endpoint(BrainpoolP192 Curve)
            : this(Curve, new Aes256())
        {
        }

        /// <summary>
        /// Brainpool P-192 Curve
        /// </summary>
        /// <param name="Curve">Curve instance</param>
        /// <param name="SymmetricCipher">Symmetric cipher to use by default.</param>
        public BrainpoolP192Endpoint(BrainpoolP192 Curve, IE2eSymmetricCipher SymmetricCipher)
            : base(Curve, SymmetricCipher)
        {
        }

        /// <summary>
        /// Brainpool P-192 Curve
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        public BrainpoolP192Endpoint(byte[] PublicKey)
            : this(PublicKey, new Aes256())
        {
        }

        /// <summary>
        /// Brainpool P-192 Curve
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        /// <param name="SymmetricCipher">Symmetric cipher to use by default.</param>
        public BrainpoolP192Endpoint(byte[] PublicKey, IE2eSymmetricCipher SymmetricCipher)
            : base(PublicKey, new BrainpoolP192(), SymmetricCipher)
        {
        }

		/// <summary>
		/// Local name of the E2E encryption scheme
		/// </summary>
		public override string LocalName => "bp192";

		/// <summary>
		/// Security strength of End-to-End encryption scheme.
		/// </summary>
		public override int SecurityStrength => 96;

		/// <summary>
		/// Creates a new key.
		/// </summary>
		/// <param name="SecurityStrength">Overall desired security strength, if applicable.</param>
		/// <returns>New E2E endpoint.</returns>
		public override IE2eEndpoint Create(int SecurityStrength)
		{
			return new BrainpoolP192Endpoint(this.DefaultSymmetricCipher.CreteNew());
		}

        /// <summary>
        /// Creates a new endpoint given a private key.
        /// </summary>
        /// <param name="Secret">Secret.</param>
        /// <returns>Endpoint object.</returns>
        public override IE2eEndpoint CreatePrivate(byte[] Secret)
		{
			return new BrainpoolP192Endpoint(new BrainpoolP192(Secret), this.DefaultSymmetricCipher.CreteNew());
		}

        /// <summary>
        /// Creates a new endpoint given a public key.
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        /// <returns>Endpoint object.</returns>
        public override IE2eEndpoint CreatePublic(byte[] PublicKey)
		{
			return new BrainpoolP192Endpoint(PublicKey, this.DefaultSymmetricCipher.CreteNew());
		}
	}
}
