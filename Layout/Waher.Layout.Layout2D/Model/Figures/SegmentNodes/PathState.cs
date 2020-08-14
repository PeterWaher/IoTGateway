using System;
using System.Collections.Generic;
using SkiaSharp;

namespace Waher.Layout.Layout2D.Model.Figures.SegmentNodes
{
	/// <summary>
	/// Current state tracing the path.
	/// </summary>
	public class PathState
	{
		private readonly Path pathDef;
		private readonly SKPath path;
		private float x0, y0;
		private float x, y;
		private float xPrev, yPrev;
		private float? angle;
		private bool drawingSpline = false;
		private List<SKPoint> vertices = null;
		private bool nonSpline = false;

		/// <summary>
		/// Current state tracing the path.
		/// </summary>
		/// <param name="PathDef">Path definition</param>
		/// <param name="Path">Path being drawn (null when measuring).</param>
		public PathState(Path PathDef, SKPath Path)
		{
			this.pathDef = PathDef;
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
				if (this.drawingSpline)
					this.FlushSpline();

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
		/// Sets the start coordinate of a new contour.
		/// </summary>
		/// <param name="X">X-coordinate</param>
		/// <param name="Y">Y-coordinate</param>
		public void Set0(float X, float Y)
		{
			this.Set(X, Y);
			this.x0 = X;
			this.y0 = Y;
			this.nonSpline = false;
		}

		/// <summary>
		/// Sets the start coordinate of a new contour, relative to the last point.
		/// </summary>
		/// <param name="DeltaX">X-coordinate</param>
		/// <param name="DeltaY">Y-coordinate</param>
		/// <returns>Absolute coordinates.</returns>
		public SKPoint Add0(float DeltaX, float DeltaY)
		{
			SKPoint P = this.Add(DeltaX, DeltaY);

			this.nonSpline = false;
			this.x0 = P.X;
			this.y0 = P.Y;

			return P;
		}

		/// <summary>
		/// Closes the current contour, using a line.
		/// </summary>
		public void CloseLine()
		{
			this.Set(this.x0, this.y0);
			this.path?.Close();
			this.nonSpline = false;
		}

		/// <summary>
		/// Closes the current contour, using a spline, creating a closed smooth loop.
		/// </summary>
		public void CloseLoop()
		{
			if (this.drawingSpline)
			{
				if (this.nonSpline)
				{
					this.AddSplineVertex(this.x0, this.y0);
					this.FlushSpline();
					this.path?.Close();
					this.nonSpline = false;
				}
				else
				{
					Loop.CreateLoop(this.path, this.vertices.ToArray());
					this.vertices.Clear();
				}

				this.drawingSpline = false;
			}
			else
				this.CloseLine();
		}

		/// <summary>
		/// Sets a new coordinate
		/// </summary>
		/// <param name="X">X-coordinate</param>
		/// <param name="Y">Y-coordinate</param>
		public void Set(float X, float Y)
		{
			if (this.drawingSpline)
				this.FlushSpline();

			this.nonSpline = true;
			if (X != this.x || Y != this.y)
			{
				this.xPrev = this.x;
				this.x = X;

				this.yPrev = this.y;
				this.y = Y;

				this.angle = null;

				this.pathDef.IncludePoint(X, Y);
			}
		}

		/// <summary>
		/// Sets a new coordinate
		/// </summary>
		/// <returns>Absolute coordinates.</returns>
		public SKPoint Add(float DeltaX, float DeltaY)
		{
			DeltaX += this.x;
			DeltaY += this.y;

			this.Set(DeltaX, DeltaY);

			return new SKPoint(DeltaX, DeltaY);
		}

		/// <summary>
		/// Moves forward
		/// </summary>
		/// <param name="Distance"></param>
		/// <returns>Absolute coordinates.</returns>
		public SKPoint Forward(float Distance)
		{
			if (this.drawingSpline)
				this.FlushSpline();

			if (Distance != 0)
			{
				float Radians = this.Angle;
				float Dx = (float)(Distance * Math.Cos(Radians));
				float Dy = (float)(Distance * Math.Sin(Radians));

				return this.Add(Dx, Dy);
			}
			else
				return new SKPoint(this.x, this.y);
		}

		/// <summary>
		/// Moves forward
		/// </summary>
		/// <param name="Distance"></param>
		/// <returns>Absolute coordinates.</returns>
		public SKPoint Backward(float Distance)
		{
			return this.Forward(-Distance);
		}

		/// <summary>
		/// Turns the current direction left.
		/// </summary>
		/// <param name="DeltaDegrees">Angle, in degrees.</param>
		public void TurnLeft(float DeltaDegrees)
		{
			if (this.drawingSpline)
				this.FlushSpline();

			this.angle = this.Angle - DeltaDegrees * LayoutElement.DegreesToRadians;
		}

		/// <summary>
		/// Turns the current direction right.
		/// </summary>
		/// <param name="DeltaDegrees">Angle, in degrees.</param>
		public void TurnRight(float DeltaDegrees)
		{
			if (this.drawingSpline)
				this.FlushSpline();

			this.angle = this.Angle + DeltaDegrees * LayoutElement.DegreesToRadians;
		}

		/// <summary>
		/// Closes an ongoing spline curve (if one open).
		/// </summary>
		public void FlushSpline()
		{
			if (this.drawingSpline)
			{
				this.drawingSpline = false;

				Spline.CreateSpline(this.path, this.vertices.ToArray());
				this.vertices.Clear();
				this.nonSpline = false;
			}
		}

		/// <summary>
		/// Sets a new spline vertex
		/// </summary>
		/// <param name="X">X-coordinate</param>
		/// <param name="Y">Y-coordinate</param>
		public void SetSplineVertex(float X, float Y)
		{
			if (this.path is null)
				this.Set(X, Y);
			else
			{
				if (this.vertices is null)
					this.vertices = new List<SKPoint>() { new SKPoint(this.x, this.y) };

				this.vertices.Add(new SKPoint(X, Y));
				this.drawingSpline = true;

				if (X != this.x || Y != this.y)
				{
					this.xPrev = this.x;
					this.x = X;

					this.yPrev = this.y;
					this.y = Y;

					this.angle = null;

					this.pathDef.IncludePoint(X, Y);
				}
			}
		}

		/// <summary>
		/// Sets a new spline vertex, relative to the last coordinate
		/// </summary>
		/// <param name="DeltaX">X-coordinate</param>
		/// <param name="DeltaY">Y-coordinate</param>
		public void AddSplineVertex(float DeltaX, float DeltaY)
		{
			this.SetSplineVertex(this.x + DeltaX, this.y + DeltaY);
		}
	}
}
