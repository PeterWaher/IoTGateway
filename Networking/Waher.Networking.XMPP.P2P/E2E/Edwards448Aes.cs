using System.Numerics;
using Waher.Security.EllipticCurves;

namespace Waher.Networking.XMPP.P2P.E2E
{
	/// <summary>
	/// Edwards448 Edwards Curve
	/// </summary>
	public class Edwards448Aes : EcAes256
    {
        /// <summary>
        /// Edwards448 Edwards Curve
        /// </summary>
        public Edwards448Aes()
			: this(new Edwards448())
		{
		}

        /// <summary>
        /// Edwards448 Edwards Curve
        /// </summary>
        /// <param name="Curve">Curve instance</param>
        public Edwards448Aes(Edwards448 Curve)
			: base(Curve)
		{
		}

        /// <summary>
        /// Edwards448 Edwards Curve
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        public Edwards448Aes(byte[] PublicKey)
			: base(PublicKey, new Edwards448())
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
			return new Edwards448Aes();
		}

        /// <summary>
        /// Creates a new endpoint given a private key.
        /// </summary>
        /// <param name="Secret">Secret.</param>
        /// <returns>Endpoint object.</returns>
        public override EcAes256 CreatePrivate(byte[] Secret)
		{
			return new Edwards448Aes(new Edwards448(Secret));
		}

        /// <summary>
        /// Creates a new endpoint given a public key.
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        /// <returns>Endpoint object.</returns>
        public override EcAes256 CreatePublic(byte[] PublicKey)
		{
			return new Edwards448Aes(PublicKey);
		}
	}
}
