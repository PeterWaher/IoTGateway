using System;
using System.Text;
using System.Numerics;

namespace Waher.Security.EllipticCurves
{
	/// <summary>
	/// Represents a point on a curve.
	/// </summary>
	public struct PointOnCurve
	{
		private BigInteger x;
		private BigInteger y;
		private bool zero;

		/// <summary>
		/// Represents a point on a curve.
		/// </summary>
		/// <param name="X">X-coordinate</param>
		/// <param name="Y">Y-coordinate</param>
		public PointOnCurve(BigInteger X, BigInteger Y)
		{
			this.x = X;
			this.y = Y;
			this.zero = false;
		}

		/// <summary>
		/// Represents a point on a curve.
		/// </summary>
		/// <param name="X">X-coordinate</param>
		/// <param name="Y">Y-coordinate</param>
		/// <param name="Zero">Zero point (infinity).</param>
		private PointOnCurve(BigInteger X, BigInteger Y, bool Zero)
		{
			this.x = X;
			this.y = Y;
			this.zero = Zero;
		}

		/// <summary>
		/// Zero point on curve (infinity).
		/// </summary>
		public static PointOnCurve Zero = new PointOnCurve(BigInteger.Zero, BigInteger.Zero, true);

		/// <summary>
		/// X-coordinate
		/// </summary>
		public BigInteger X
		{
			get => this.x;
			internal set => this.x = value;
		}

		/// <summary>
		/// X-coordinate
		/// </summary>
		public BigInteger Y
		{
			get => this.y;
			internal set => this.y = value;
		}

		/// <summary>
		/// If the point represents zero (infinity).
		/// </summary>
		public bool IsZero
		{
			get => this.zero;
			internal set => this.zero = value;
		}

		/// <summary>
		/// <see cref="Object.ToString"/>
		/// </summary>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append('(');
			sb.Append(this.x.ToString());
			sb.Append(',');
			sb.Append(this.y.ToString());
			sb.Append(')');

			return sb.ToString();
		}
	}
}
