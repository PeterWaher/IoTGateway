using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using Waher.Security.EllipticCurves;

namespace Waher.Networking.XMPP.P2P.E2E
{
	/// <summary>
	/// NIST P-192 Curve
	/// </summary>
	public class NistP192Aes : EcAes256
    {
		/// <summary>
		/// NIST P-192 Curve
		/// </summary>
		/// <param name="X">X-coordinate of remote public key.</param>
		/// <param name="Y">Y-coordinate of remote public key.</param>
		/// <param name="LocalEndpoint">Local security endpoint, if available.</param>
		public NistP192Aes(byte[] X, byte[] Y, EndpointSecurity LocalEndpoint)
			: base(X, Y, LocalEndpoint)
		{
		}

		/// <summary>
		/// Security strength of End-to-End encryption scheme.
		/// </summary>
		public override int SecurityStrength => 96;

		/// <summary>
		/// Name of elliptic curve
		/// </summary>
		public override string CurveName => this.localEndpoint.p192.CurveName;

		/// <summary>
		/// Elliptic Curve
		/// </summary>
		protected override CurvePrimeField Curve => this.localEndpoint.p192;

		/// <summary>
		/// Previous Elliptic Curve
		/// </summary>
		protected override CurvePrimeField PrevCurve => this.localEndpoint.p192Old;
	}
}
