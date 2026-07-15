using Waher.Networking.XMPP.P2P.SymmetricCiphers;
using Waher.Security.EllipticCurves;

namespace Waher.Networking.XMPP.P2P.E2E
{
	/// <summary>
	/// Brainpool P-512 Curve
	/// </summary>
	public class BrainpoolP512Endpoint : BrainpoolEndpoint
    {
        /// <summary>
        /// Brainpool P-512 Curve
        /// </summary>
        public BrainpoolP512Endpoint()
            : this(new BrainpoolP512())
        {
        }

        /// <summary>
        /// Brainpool P-512 Curve
        /// </summary>
        /// <param name="SymmetricCipher">Symmetric cipher to use by default.</param>
        public BrainpoolP512Endpoint(IE2eSymmetricCipher SymmetricCipher)
            : this(new BrainpoolP512(), SymmetricCipher)
        {
        }

        /// <summary>
        /// Brainpool P-512 Curve
        /// </summary>
        /// <param name="Curve">Curve instance</param>
        public BrainpoolP512Endpoint(BrainpoolP512 Curve)
            : this(Curve, new Aes256())
        {
        }

        /// <summary>
        /// Brainpool P-512 Curve
        /// </summary>
        /// <param name="Curve">Curve instance</param>
        /// <param name="SymmetricCipher">Symmetric cipher to use by default.</param>
        public BrainpoolP512Endpoint(BrainpoolP512 Curve, IE2eSymmetricCipher SymmetricCipher)
            : base(Curve, SymmetricCipher)
        {
        }

        /// <summary>
        /// Brainpool P-512 Curve
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        public BrainpoolP512Endpoint(byte[] PublicKey)
            : this(PublicKey, new Aes256())
        {
        }

        /// <summary>
        /// Brainpool P-512 Curve
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        /// <param name="SymmetricCipher">Symmetric cipher to use by default.</param>
        public BrainpoolP512Endpoint(byte[] PublicKey, IE2eSymmetricCipher SymmetricCipher)
            : base(PublicKey, new BrainpoolP512(), SymmetricCipher)
        {
        }

        /// <summary>
        /// Local name of the E2E encryption scheme
        /// </summary>
        public override string LocalName => "bp512";

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
			return new BrainpoolP512Endpoint(this.DefaultSymmetricCipher.CreteNew());
		}

        /// <summary>
        /// Creates a new endpoint given a private key.
        /// </summary>
        /// <param name="Secret">Secret.</param>
        /// <returns>Endpoint object.</returns>
        public override IE2eEndpoint CreatePrivate(byte[] Secret)
        {
            return new BrainpoolP512Endpoint(new BrainpoolP512(Secret), this.DefaultSymmetricCipher.CreteNew());
        }

        /// <summary>
        /// Creates a new endpoint given a public key.
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        /// <returns>Endpoint object.</returns>
        public override IE2eEndpoint CreatePublic(byte[] PublicKey)
        {
            return new BrainpoolP512Endpoint(PublicKey, this.DefaultSymmetricCipher.CreteNew());
        }
    }
}
