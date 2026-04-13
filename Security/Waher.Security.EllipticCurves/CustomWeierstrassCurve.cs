using System.Numerics;

namespace Waher.Security.EllipticCurves
{
    /// <summary>
    /// Custom Weierstrass curves (y²=x³+ax+b) over a prime field.
    /// </summary>
    public class CustomWeierstrassCurve : WeierstrassCurve 
	{
		private readonly string curveName;
		private readonly HashFunctionArray hashFunction;
		private readonly HashFunctionStream hashFunctionStream;

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
		/// <param name="HashFunction">Hash function to use in signatures for 
		/// binary blocks of data in memory.</param>
		/// <param name="HashFunctionStream">Hash function to use in signatures 
		/// for data streams.</param>
		public CustomWeierstrassCurve(string CurveName, BigInteger Prime,
			PointOnCurve BasePoint, BigInteger a, BigInteger b, 
			BigInteger Order, int Cofactor, HashFunctionArray HashFunction,
			HashFunctionStream HashFunctionStream)
            : this(CurveName, Prime, BasePoint, a, b, Order, Cofactor, 
				  (byte[])null, HashFunction, HashFunctionStream)
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
		/// <param name="HashFunction">Hash function to use in signatures for 
		/// binary blocks of data in memory.</param>
		/// <param name="HashFunctionStream">Hash function to use in signatures 
		/// for data streams.</param>
		public CustomWeierstrassCurve(string CurveName, BigInteger Prime, 
			PointOnCurve BasePoint, BigInteger a, BigInteger b, 
			BigInteger Order, int Cofactor, byte[] Secret, HashFunctionArray HashFunction,
			HashFunctionStream HashFunctionStream)
			: base(Prime, BasePoint, a, b, Order, Cofactor, Secret)
		{
			this.curveName = CurveName;
			this.hashFunction = HashFunction;
			this.hashFunctionStream = HashFunctionStream;
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
		/// <param name="HashFunction">Hash function to use in signatures for 
		/// binary blocks of data in memory.</param>
		/// <param name="HashFunctionStream">Hash function to use in signatures 
		/// for data streams.</param>
		public CustomWeierstrassCurve(string CurveName, BigInteger Prime, 
			PointOnCurve BasePoint, BigInteger a, BigInteger b, BigInteger Order, 
			int Cofactor, uint[] Secret, HashFunctionArray HashFunction,
			HashFunctionStream HashFunctionStream)
			: base(Prime, BasePoint, a, b, Order, Cofactor, Secret)
		{
			this.curveName = CurveName;
			this.hashFunction = HashFunction;
			this.hashFunctionStream = HashFunctionStream;
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
		/// <param name="HashFunction">Hash function to use in signatures.</param>
		public CustomWeierstrassCurve(string CurveName, BigInteger Prime,
			PointOnCurve BasePoint, BigInteger a, BigInteger b,
			BigInteger Order, int Cofactor, HashFunction HashFunction)
			: this(CurveName, Prime, BasePoint, a, b, Order, Cofactor, (byte[])null, 
				  Hashes.GetHashFunctionArray(HashFunction),
				  Hashes.GetHashFunctionStream(HashFunction))
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
		/// <param name="HashFunction">Hash function to use in signatures.</param>
		public CustomWeierstrassCurve(string CurveName, BigInteger Prime,
			PointOnCurve BasePoint, BigInteger a, BigInteger b,
			BigInteger Order, int Cofactor, byte[] Secret, HashFunction HashFunction)
			: this(CurveName, Prime, BasePoint, a, b, Order, Cofactor, Secret,
				  Hashes.GetHashFunctionArray(HashFunction),
				  Hashes.GetHashFunctionStream(HashFunction))
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
		/// <param name="HashFunction">Hash function to use in signatures.</param>
		public CustomWeierstrassCurve(string CurveName, BigInteger Prime,
			PointOnCurve BasePoint, BigInteger a, BigInteger b, BigInteger Order,
			int Cofactor, uint[] Secret, HashFunction HashFunction)
			: this(CurveName, Prime, BasePoint, a, b, Order, Cofactor, Secret,
				  Hashes.GetHashFunctionArray(HashFunction),
				  Hashes.GetHashFunctionStream(HashFunction))
		{
		}

		/// <summary>
		/// Name of curve.
		/// </summary>
		public override string CurveName => this.curveName;

		/// <summary>
		/// Hash function to use in signatures for binary blocks of data in memory.
		/// </summary>
		public override HashFunctionArray HashFunction => this.hashFunction;

		/// <summary>
		/// Hash function to use in signatures for data streams.
		/// </summary>
		public override HashFunctionStream HashFunctionStream => this.hashFunctionStream;
	}
}
