using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Layout.Layout2D.Model.Figures.SegmentNodes
{
	/// <summary>
	/// Current state tracing the path.
	/// </summary>
	public class PathState
	{
		private readonly Path path;
		private double x, y;
		private double xPrev, yPrev;
		private double? angle;

		/// <summary>
		/// Current state tracing the path.
		/// </summary>
		public PathState(Path Path)
		{
			this.path = Path;
		}

		/// <summary>
		/// Current X-coordinate
		/// </summary>
		public double X => this.x;

		/// <summary>
		/// Current Y-coordinte
		/// </summary>
		public double Y => this.y;

		/// <summary>
		/// Previous X-coordinate
		/// </summary>
		public double XPrev => this.xPrev;

		/// <summary>
		/// Previous Y-coordinate
		/// </summary>
		public double YPrev => this.yPrev;

		/// <summary>
		/// Calculates the current direction angle, in radians
		/// </summary>
		public double Angle
		{
			get
			{
				if (!this.angle.HasValue)
				{
					double dx = this.x - this.xPrev;
					double dy = this.y - this.yPrev;

					if (dx == 0 && dy == 0)
						this.angle = 0;
					else
						this.angle = Math.Atan2(dy, dx);
				}

				return this.angle.Value;
			}
		}

		/// <summary>
		/// Sets a new coordinate
		/// </summary>
		/// <param name="X">X-coordinate</param>
		/// <param name="Y">Y-coordinate</param>
		public void Set(double X, double Y)
		{
			if (X != this.x || Y != this.y)
			{
				this.xPrev = this.x;
				this.x = X;

				this.yPrev = this.y;
				this.y = Y;

				this.angle = null;

				this.path.IncludePoint(X, Y);
			}
		}

		/// <summary>
		/// Sets a new coordinate
		/// </summary>
		public void Add(double DeltaX, double DeltaY)
		{
			if (DeltaX != 0 || DeltaY != 0)
			{
				this.xPrev = this.x;
				this.x += DeltaX;

				this.yPrev = this.y;
				this.y += DeltaY;

				this.angle = null;
			}
		}

		/// <summary>
		/// Moves forward
		/// </summary>
		/// <param name="Distance"></param>
		public void Forward(double Distance)
		{
			if (Distance != 0)
			{
				double Radians = this.Angle;
				double Dx = Distance * Math.Cos(Radians);
				double Dy = Distance * Math.Sin(Radians);

				this.Add(Dx, Dy);
			}
		}

		/// <summary>
		/// Moves forward
		/// </summary>
		/// <param name="Distance"></param>
		public void Backward(double Distance)
		{
			this.Forward(-Distance);
		}

		/// <summary>
		/// Turns the current direction left.
		/// </summary>
		/// <param name="DeltaDegrees">Angle, in degrees.</param>
		public void TurnLeft(double DeltaDegrees)
		{
			this.angle = this.Angle - DeltaDegrees * LayoutElement.DegreesToRadians;
		}

		/// <summary>
		/// Turns the current direction right.
		/// </summary>
		/// <param name="DeltaDegrees">Angle, in degrees.</param>
		public void TurnRight(double DeltaDegrees)
		{
			this.angle = this.Angle + DeltaDegrees * LayoutElement.DegreesToRadians;
		}
	}
}
