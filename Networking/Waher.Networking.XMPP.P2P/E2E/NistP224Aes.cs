using System.Numerics;
using Waher.Security.EllipticCurves;

namespace Waher.Networking.XMPP.P2P.E2E
{
	/// <summary>
	/// NIST P-224 Curve
	/// </summary>
	public class NistP224Aes : EcAes256
	{
		/// <summary>
		/// NIST P-224 Curve
		/// </summary>
		public NistP224Aes()
			: this(new NistP224())
		{
		}

		/// <summary>
		/// NIST P-224 Curve
		/// </summary>
		/// <param name="Curve">Curve instance</param>
		public NistP224Aes(NistP224 Curve)
			: base(Curve)
		{
		}

        /// <summary>
        /// NIST P-224 Curve
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        public NistP224Aes(byte[] PublicKey)
            : base(PublicKey, new NistP224())
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
			return new NistP224Aes();
		}

        /// <summary>
        /// Creates a new endpoint given a private key.
        /// </summary>
        /// <param name="Secret">Secret.</param>
        /// <returns>Endpoint object.</returns>
        public override EcAes256 CreatePrivate(byte[] Secret)
        {
            return new NistP224Aes(new NistP224(Secret));
        }

        /// <summary>
        /// Creates a new endpoint given a public key.
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        /// <returns>Endpoint object.</returns>
        public override EcAes256 CreatePublic(byte[] PublicKey)
        {
            return new NistP224Aes(PublicKey);
        }
    }
}
