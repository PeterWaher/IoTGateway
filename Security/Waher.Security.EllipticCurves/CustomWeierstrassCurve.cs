using System.Numerics;

namespace Waher.Security.EllipticCurves
{
    /// <summary>
    /// Custom Weierstrass curves (y²=x³+ax+b) over a prime field.
    /// </summary>
    public class CustomWeierstrassCurve : WeierstrassCurve 
	{
		private readonly string curveName;

		/// <summary>
		/// Custom Weierstrass curves (y²=x³+ax+b) over a prime field.
		/// </summary>
		/// <param name="CurveName">Curve name</param>
		/// <param name="Prime">Prime base of field.</param>
		/// <param name="BasePoint">Base-point.</param>
		/// <param name="a">Coefficient in the Weierstrass equation.</param>
		/// <param name="b">Coefficient in the Weierstrass equation.</param>
		/// <param name="Order">Order of base-point.</param>
		/// <param name="Cofactor">Cofactor of curve.</param>
		public CustomWeierstrassCurve(string CurveName, BigInteger Prime, 
			PointOnCurve BasePoint, BigInteger a, BigInteger b, 
			BigInteger Order, int Cofactor)
            : this(CurveName, Prime, BasePoint, a, b, Order, Cofactor, (byte[])null)
		{
		}

		/// <summary>
		/// Custom Weierstrass curves (y²=x³+ax+b) over a prime field.
		/// </summary>
		/// <param name="CurveName">Curve name</param>
		/// <param name="Prime">Prime base of field.</param>
		/// <param name="BasePoint">Base-point.</param>
		/// <param name="a">Coefficient in the Weierstrass equation.</param>
		/// <param name="b">Coefficient in the Weierstrass equation.</param>
		/// <param name="Order">Order of base-point.</param>
		/// <param name="Cofactor">Cofactor of curve.</param>
		/// <param name="Secret">Secret.</param>
		public CustomWeierstrassCurve(string CurveName, BigInteger Prime, 
			PointOnCurve BasePoint, BigInteger a, BigInteger b, 
			BigInteger Order, int Cofactor, byte[] Secret)
			: base(Prime, BasePoint, a, b, Order, Cofactor, Secret)
		{
			this.curveName = CurveName;
		}

		/// <summary>
		/// Custom Weierstrass curves (y²=x³+ax+b) over a prime field.
		/// </summary>
		/// <param name="CurveName">Curve name</param>
		/// <param name="Prime">Prime base of field.</param>
		/// <param name="BasePoint">Base-point.</param>
		/// <param name="a">Coefficient in the Weierstrass equation.</param>
		/// <param name="b">Coefficient in the Weierstrass equation.</param>
		/// <param name="Order">Order of base-point.</param>
		/// <param name="Cofactor">Cofactor of curve.</param>
		/// <param name="Secret">Secret.</param>
		public CustomWeierstrassCurve(string CurveName, BigInteger Prime, 
			PointOnCurve BasePoint, BigInteger a, BigInteger b, BigInteger Order, 
			int Cofactor, uint[] Secret)
			: base(Prime, BasePoint, a, b, Order, Cofactor, Secret)
		{
			this.curveName = CurveName;
		}

		/// <summary>
		/// Name of curve.
		/// </summary>
		public override string CurveName => this.curveName;
	}
}
