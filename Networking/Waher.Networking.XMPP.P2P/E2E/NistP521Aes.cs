using System.Numerics;
using Waher.Security.EllipticCurves;

namespace Waher.Networking.XMPP.P2P.E2E
{
	/// <summary>
	/// NIST P-521 Curve
	/// </summary>
	public class NistP521Aes : EcAes256
	{
		/// <summary>
		/// NIST P-521 Curve
		/// </summary>
		public NistP521Aes()
			: this(new NistP521())
		{
		}

		/// <summary>
		/// NIST P-521 Curve
		/// </summary>
		/// <param name="Curve">Curve instance</param>
		public NistP521Aes(NistP521 Curve)
			: base(Curve)
		{
		}

		/// <summary>
		/// NIST P-521 Curve
		/// </summary>
		/// <param name="X">X-coordinate of remote public key.</param>
		/// <param name="Y">Y-coordinate of remote public key.</param>
		public NistP521Aes(byte[] X, byte[] Y)
			: base(X, Y, new NistP521())
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
			return new NistP521Aes();
		}

		/// <summary>
		/// Creates a new endpoint.
		/// </summary>
		/// <param name="D">Private key.</param>
		/// <returns>Endpoint object.</returns>
		public override EcAes256 Create(BigInteger D)
		{
			return new NistP521Aes(new NistP521(D));
		}

		/// <summary>
		/// Creates a new endpoint.
		/// </summary>
		/// <param name="X">X-coordinate of remote public key.</param>
		/// <param name="Y">Y-coordinate of remote public key.</param>
		/// <returns>Endpoint object.</returns>
		public override EcAes256 Create(byte[] X, byte[] Y)
		{
			return new NistP521Aes(X, Y);
		}
	}
}
