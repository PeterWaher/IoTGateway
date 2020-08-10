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
		private float x, y;
		private float xPrev, yPrev;
		private float? angle;

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
		public float X => this.x;

		/// <summary>
		/// Current Y-coordinte
		/// </summary>
		public float Y => this.y;

		/// <summary>
		/// Previous X-coordinate
		/// </summary>
		public float XPrev => this.xPrev;

		/// <summary>
		/// Previous Y-coordinate
		/// </summary>
		public float YPrev => this.yPrev;

		/// <summary>
		/// Calculates the current direction angle, in radians
		/// </summary>
		public float Angle
		{
			get
			{
				if (!this.angle.HasValue)
				{
					float dx = this.x - this.xPrev;
					float dy = this.y - this.yPrev;

					if (dx == 0 && dy == 0)
						this.angle = 0;
					else
						this.angle = (float)Math.Atan2(dy, dx);
				}

				return this.angle.Value;
			}
		}

		/// <summary>
		/// Sets a new coordinate
		/// </summary>
		/// <param name="X">X-coordinate</param>
		/// <param name="Y">Y-coordinate</param>
		public void Set(float X, float Y)
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
		public void Add(float DeltaX, float DeltaY)
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
		public void Forward(float Distance)
		{
			if (Distance != 0)
			{
				float Radians = this.Angle;
				float Dx = (float)(Distance * Math.Cos(Radians));
				float Dy = (float)(Distance * Math.Sin(Radians));

				this.Add(Dx, Dy);
			}
		}

		/// <summary>
		/// Moves forward
		/// </summary>
		/// <param name="Distance"></param>
		public void Backward(float Distance)
		{
			this.Forward(-Distance);
		}

		/// <summary>
		/// Turns the current direction left.
		/// </summary>
		/// <param name="DeltaDegrees">Angle, in degrees.</param>
		public void TurnLeft(float DeltaDegrees)
		{
			this.angle = this.Angle - DeltaDegrees * LayoutElement.DegreesToRadians;
		}

		/// <summary>
		/// Turns the current direction right.
		/// </summary>
		/// <param name="DeltaDegrees">Angle, in degrees.</param>
		public void TurnRight(float DeltaDegrees)
		{
			this.angle = this.Angle + DeltaDegrees * LayoutElement.DegreesToRadians;
		}
	}
}
