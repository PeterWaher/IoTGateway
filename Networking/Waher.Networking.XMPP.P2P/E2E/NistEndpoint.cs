using System.Numerics;
using Waher.Security.EllipticCurves;

namespace Waher.Networking.XMPP.P2P.E2E
{
	/// <summary>
	/// Abstract base class for NIST Curve endpoints
	/// </summary>
	public abstract class NistEndpoint : EllipticCurveEndpoint
    {
        /// <summary>
        /// Abstract base class for NIST Curve endpoints
        /// </summary>
        public NistEndpoint()
			: this(new NistP192())
		{
		}

        /// <summary>
        /// Abstract base class for NIST Curve endpoints
        /// </summary>
        /// <param name="Curve">Curve instance</param>
        public NistEndpoint(EllipticCurve Curve)
			: base(Curve)
		{
		}

        /// <summary>
        /// Abstract base class for NIST Curve endpoints
        /// </summary>
        /// <param name="PublicKey">Remote public key.</param>
        /// <param name="ReferenceCurve">Reference curve</param>
        public NistEndpoint(byte[] PublicKey, EllipticCurve ReferenceCurve)
			: base(PublicKey, ReferenceCurve)
		{
		}

        /// <summary>
        /// If endpoint is considered safe (i.e. there are no suspected backdoors)
        /// </summary>
        public override bool Safe => false;  // Ref: http://safecurves.cr.yp.to/

        /// <summary>
        /// If implementation is slow, compared to other options.
        /// </summary>
        public override bool Slow => true;

    }
}
