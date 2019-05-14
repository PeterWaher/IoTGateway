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
		private BigInteger z;
		private BigInteger t;
        private bool nonzero;
        private bool homogeneous;

        /// <summary>
        /// Represents a point on a curve.
        /// </summary>
        /// <param name="P">Point to copy.</param>
        public PointOnCurve(PointOnCurve P)
        {
            this.x = P.x;
            this.y = P.y;
            this.z = P.z;
            this.t = P.t;
            this.nonzero = P.nonzero;
            this.homogeneous = P.homogeneous;
        }

        /// <summary>
        /// Represents a point on a curve.
        /// </summary>
        /// <param name="X">X-coordinate</param>
        /// <param name="Y">Y-coordinate</param>
        public PointOnCurve(BigInteger X, BigInteger Y)
		{
			this.x = X;
			this.y = Y;
            this.z = BigInteger.One;
            this.t = BigInteger.Zero;
			this.nonzero = !X.IsZero || !Y.IsZero;
            this.homogeneous = false;
        }

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
        /// Z-coordinate
        /// </summary>
        public BigInteger Z
        {
            get => this.z;
            internal set
            {
                this.z = value;
                this.homogeneous = true;
            }
        }

        /// <summary>
        /// T-coordinate
        /// </summary>
        public BigInteger T
        {
            get => this.t;
            internal set
            {
                this.t = value;
                this.homogeneous = true;
            }
        }

        /// <summary>
        /// If the point is not zero (infinity).
        /// </summary>
        public bool NonZero
		{
			get => this.nonzero;
			internal set => this.nonzero = value;
		}

        /// <summary>
        /// If the point is in homogeneous coordinates.
        /// </summary>
        public bool IsHomogeneous
        {
            get => this.homogeneous;
            internal set => this.homogeneous = value;
        }

        /// <summary>
        /// If the X-coordinate is zero.
        /// </summary>
        public bool IsXZero => this.x.IsZero;

		/// <summary>
		/// If the Y-coordinate is zero.
		/// </summary>
		public bool IsYZero => this.y.IsZero;

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

            if (this.homogeneous)
            {
                sb.Append(',');
                sb.Append(this.z.ToString());
                sb.Append(',');
                sb.Append(this.t.ToString());
            }

            sb.Append(')');

			return sb.ToString();
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is PointOnCurve P &&
				this.x.Equals(P.x) &&
				this.y.Equals(P.y) &&
				this.z.Equals(P.z) &&
				this.t.Equals(P.t) &&
				this.nonzero.Equals(P.nonzero) &&
                this.homogeneous.Equals(P.homogeneous);
		}

		/// <summary>
		/// <see cref="Object.GetHashCode"/>
		/// </summary>
		public override int GetHashCode()
		{
			return
				this.x.GetHashCode() ^
				this.y.GetHashCode() ^
				this.z.GetHashCode() ^
				this.t.GetHashCode() ^
				this.nonzero.GetHashCode() ^
                this.homogeneous.GetHashCode();
		}

        /// <summary>
        /// Copies the value from point <paramref name="C"/>.
        /// </summary>
        /// <param name="C">Point to copy from.</param>
        public void CopyFrom(PointOnCurve C)
        {
            this.x = C.x;
            this.y = C.y;
            this.z = C.z;
            this.t = C.t;
            this.nonzero = C.nonzero;
            this.homogeneous = C.homogeneous;
        }

        /// <summary>
        /// Normalizes a point, if in homogeneous coorinates.
        /// </summary>
        /// <param name="Curve">Curve</param>
        public void Normalize(CurvePrimeField Curve)
        {
            if (this.homogeneous)
            {
                this.x = Curve.Divide(this.x, this.z);
                this.y = Curve.Divide(this.y, this.z);
                this.z = BigInteger.One;
                this.t = BigInteger.Zero;
                this.homogeneous = false;
            }

            if (this.x.Sign < 0)
                this.x += Curve.Prime;

            if (this.y.Sign < 0)
                this.y += Curve.Prime;
        }
    }
}
