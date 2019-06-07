using System.Numerics;
using Waher.Networking.XMPP.P2P.SymmetricCiphers;
using Waher.Security.EllipticCurves;

namespace Waher.Networking.XMPP.P2P.E2E
{
	/// <summary>
	/// Edwards448 Edwards Curve
	/// </summary>
	public class Edwards448Endpoint : EllipticCurveEndpoint
    {
        /// <summary>
        /// Edwards448 Edwards Curve
        /// </summary>
        public Edwards448Endpoint()
            : this(new Edwards448())
        {
        }

        /// <summary>
        /// Edwards448 Edwards Curve
        /// </summary>
        /// <param name="SymmetricCipher">Symmetric cipher to use by default.</param>
        public Edwards448Endpoint(IE2eSymmetricCipher SymmetricCipher)
            : this(new Edwards448(), SymmetricCipher)
        {
        }

        /// <summary>
        /// Edwards448 Edwards Curve
        /// </summary>
        /// <param name="Edwards">Edwards instance</param>
        public Edwards448Endpoint(Edwards448 Edwards)
            : this(Edwards, new Aes256())
        {
        }

        /// <summary>
        /// Edwards448 Edwards Curve
        /// </summary>
        /// <param name="Edwards">Edwards instance</param>
        /// <param name="SymmetricCipher">Symmetric cipher to use by default.</param>
        public Edwards448Endpoint(Edwards448 Edwards, IE2eSymmetricCipher SymmetricCipher)
            : base(Edwards, SymmetricCipher)
        {
        }

        /// <summary>
        /// Edwards448 Edwards Curve
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        public Edwards448Endpoint(byte[] PublicKey)
            : this(PublicKey, new Aes256())
        {
        }

        /// <summary>
        /// Edwards448 Edwards Curve
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        /// <param name="SymmetricCipher">Symmetric cipher to use by default.</param>
        public Edwards448Endpoint(byte[] PublicKey, IE2eSymmetricCipher SymmetricCipher)
            : base(PublicKey, new Edwards448(), SymmetricCipher)
        {
        }

        /// <summary>
        /// Local name of the E2E encryption scheme
        /// </summary>
        public override string LocalName => "ed448";

		/// <summary>
		/// Security strength of End-to-End encryption scheme.
		/// </summary>
		public override int SecurityStrength => 224;

		/// <summary>
		/// Creates a new key.
		/// </summary>
		/// <param name="SecurityStrength">Overall desired security strength, if applicable.</param>
		/// <returns>New E2E endpoint.</returns>
		public override IE2eEndpoint Create(int SecurityStrength)
		{
			return new Edwards448Endpoint(this.DefaultSymmetricCipher.CreteNew());
		}

        /// <summary>
        /// Creates a new endpoint given a private key.
        /// </summary>
        /// <param name="Secret">Secret.</param>
        /// <returns>Endpoint object.</returns>
        public override IE2eEndpoint CreatePrivate(byte[] Secret)
		{
			return new Edwards448Endpoint(new Edwards448(Secret), this.DefaultSymmetricCipher.CreteNew());
		}

        /// <summary>
        /// Creates a new endpoint given a public key.
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        /// <returns>Endpoint object.</returns>
        public override IE2eEndpoint CreatePublic(byte[] PublicKey)
		{
			return new Edwards448Endpoint(PublicKey, this.DefaultSymmetricCipher.CreteNew());
		}
	}
}
