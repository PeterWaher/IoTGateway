namespace Waher.Layout.Layout2D.Model
{
	/// <summary>
	/// Represents a calculated point.
	/// </summary>
	public struct CalculatedPoint
	{
		/// <summary>
		/// Represents a successfully calculated point.
		/// </summary>
		/// <param name="X">X-coordinate</param>
		/// <param name="Y">Y-coordinate</param>
		public CalculatedPoint(float X, float Y)
		{
			this.X = X;
			this.Y = Y;
			this.Ok = true;
		}

		/// <summary>
		/// Represents a successfully calculated point.
		/// </summary>
		/// <param name="X">X-coordinate</param>
		/// <param name="Y">Y-coordinate</param>
		/// <param name="Ok">If OK</param>
		private CalculatedPoint(float X, float Y, bool Ok)
		{
			this.X = X;
			this.Y = Y;
			this.Ok = Ok;
		}

		/// <summary>
		/// X-coordinate
		/// </summary>
		public float X;

		/// <summary>
		/// Y-coordinate
		/// </summary>
		public float Y;

		/// <summary>
		/// If point is successfully evaluated
		/// </summary>
		public bool Ok;

		/// <summary>
		/// No point calculated.
		/// </summary>
		public static readonly CalculatedPoint Empty = new CalculatedPoint(0, 0, false);
	}
}
