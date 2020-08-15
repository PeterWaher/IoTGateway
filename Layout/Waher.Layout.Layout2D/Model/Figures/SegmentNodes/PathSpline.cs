using System;
using System.Collections.Generic;
using SkiaSharp;

namespace Waher.Layout.Layout2D.Model.Figures.SegmentNodes
{
	/// <summary>
	/// Represents a spline curve in a path
	/// </summary>
	public class PathSpline : IDirectedElement
	{
		private readonly List<SKPoint> vertices;

		/// <summary>
		/// Represents a spline curve in a path
		/// </summary>
		/// <param name="X0">Starting X-coordinate.</param>
		/// <param name="Y0">Starting Y-coordinate.</param>
		public PathSpline(float X0, float Y0)
		{
			this.vertices = new List<SKPoint>() { new SKPoint(X0, Y0) };
		}

		/// <summary>
		/// Adds a vertex
		/// </summary>
		/// <param name="X">X-Coordinate</param>
		/// <param name="Y">Y-Coordinte</param>
		public void Add(float X, float Y)
		{
			this.vertices.Add(new SKPoint(X, Y));
		}

		/// <summary>
		/// Returns an array of vertices of the spline curve.
		/// </summary>
		/// <returns>Vertices</returns>
		public SKPoint[] ToArray()
		{
			return this.vertices.ToArray();
		}

		/// <summary>
		/// Tries to get start position and initial direction.
		/// </summary>
		/// <param name="X">X-coordinate.</param>
		/// <param name="Y">Y-coordinate.</param>
		/// <param name="Direction">Initial direction.</param>
		/// <returns>If a start position was found.</returns>
		public bool TryGetStart(out float X, out float Y, out float Direction)
		{
			int c = this.vertices.Count;
			if (c<2)
			{
				X = Y = Direction = 0;
				return false;
			}

			SKPoint P0 = this.vertices[0];
			SKPoint P1 = this.vertices[1];
			float dx = P1.X - P0.X;
			float dy = P1.Y - P0.Y;

			X = P0.X;
			Y = P0.Y;
			Direction = LayoutElement.CalcDirection(dx, dy);

			return true;
		}

		/// <summary>
		/// Tries to get end position and terminating direction.
		/// </summary>
		/// <param name="X">X-coordinate.</param>
		/// <param name="Y">Y-coordinate.</param>
		/// <param name="Direction">Terminating direction.</param>
		/// <returns>If a terminating position was found.</returns>
		public bool TryGetEnd(out float X, out float Y, out float Direction)
		{
			int c = this.vertices.Count;
			if (c < 2)
			{
				X = Y = Direction = 0;
				return false;
			}

			SKPoint P0 = this.vertices[c - 2];
			SKPoint P1 = this.vertices[c - 1];
			float dx = P1.X - P0.X;
			float dy = P1.Y - P0.Y;

			X = P1.X;
			Y = P1.Y;
			Direction = LayoutElement.CalcDirection(dx, dy);

			return true;
		}
	}
}
