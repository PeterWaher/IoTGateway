using System;
using SkiaSharp;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Graphs
{
	/// <summary>
	/// Contains information about the current drawing area.
	/// </summary>
	public class DrawingArea
	{
		private readonly IElement minX;
		private readonly IElement maxX;
		private readonly IElement minY;
		private readonly IElement maxY;
		private readonly int offsetX;
		private readonly int offsetY;
		private readonly int width;
		private readonly int height;
		private readonly float origoX;
		private readonly float origoY;

		/// <summary>
		/// Contains information about the current drawing area.
		/// </summary>
		/// <param name="MinX">Smallest value of X.</param>
		/// <param name="MaxX">Largest value of X.</param>
		/// <param name="MinY">Smallest value of Y.</param>
		/// <param name="MaxY">Largest value of Y.</param>
		/// <param name="OffsetX">X-offset of drawing area, relative to the canvas origin.</param>
		/// <param name="OffsetY">Y-offset of drawing area, relative to the canvas origin.</param>
		/// <param name="Width">Width of drawing area.</param>
		/// <param name="Height">Height of drawing area.</param>
		/// <param name="OrigoX">X-coordinate of the origo.</param>
		/// <param name="OrigoY">Y-coordinate of the origo.</param>
		public DrawingArea(IElement MinX, IElement MaxX, IElement MinY, IElement MaxY, int OffsetX, int OffsetY, int Width, int Height,
			float OrigoX, float OrigoY)
		{
			this.minX = MinX;
			this.maxX = MaxX;
			this.minY = MinY;
			this.maxY = MaxY;
			this.offsetX = OffsetX;
			this.offsetY = OffsetY;
			this.width = Width;
			this.height = Height;
			this.origoX = OrigoX;
			this.origoY = OrigoY;
		}

		/// <summary>
		/// Smallest value of X.
		/// </summary>
		public IElement MinX
		{
			get
			{
				return this.minX;
			}
		}

		/// <summary>
		/// Largest value of X.
		/// </summary>
		public IElement MaxX
		{
			get
			{
				return this.maxX;
			}
		}

		/// <summary>
		/// Smallest value of Y.
		/// </summary>
		public IElement MinY
		{
			get
			{
				return this.minY;
			}
		}

		/// <summary>
		/// Largest value of Y.
		/// </summary>
		public IElement MaxY
		{
			get
			{
				return this.maxY;
			}
		}

		/// <summary>
		/// X-offset of drawing area, relative to the canvas origin.
		/// </summary>
		public int OffsetX
		{
			get
			{
				return this.offsetX;
			}
		}

		/// <summary>
		/// Y-offset of drawing area, relative to the canvas origin.
		/// </summary>
		public int OffsetY
		{
			get
			{
				return this.offsetY;
			}
		}

		/// <summary>
		/// Width of drawing area.
		/// </summary>
		public int Width
		{
			get
			{
				return this.width;
			}
		}

		/// <summary>
		/// Height of drawing area.
		/// </summary>
		public int Height
		{
			get
			{
				return this.height;
			}
		}

		/// <summary>
		/// X-coordinate for the origo.
		/// </summary>
		public float OrigoX
		{
			get
			{
				return this.origoX;
			}
		}

		/// <summary>
		/// Y-coordinate for the origo.
		/// </summary>
		public float OrigoY
		{
			get
			{
				return this.origoY;
			}
		}

		/// <summary>
		/// Scales two vectors of equal size to points in a rectangular area.
		/// </summary>
		/// <param name="VectorX">X-vector.</param>
		/// <param name="VectorY">Y-vector.</param>
		public SKPoint[] Scale(IVector VectorX, IVector VectorY)
		{
			return Graph.Scale(VectorX, VectorY, this.minX, this.maxX, this.minY, this.maxY, this.offsetX, this.offsetY, this.width, this.height);
		}

		/// <summary>
		/// Scales a vector to fit a given area.
		/// </summary>
		/// <param name="Vector">Vector.</param>
		public double[] ScaleX(IVector Vector)
		{
			return Graph.Scale(Vector, this.minX, this.maxX, this.offsetX, this.width);
		}

		/// <summary>
		/// Scales a vector to fit a given area.
		/// </summary>
		/// <param name="Vector">Vector.</param>
		public double[] ScaleY(IVector Vector)
		{
			return Graph.Scale(Vector, this.minY, this.maxY, this.offsetY, this.height);
		}


		/// <summary>
		/// Descales a scaled value along the X-axis.
		/// </summary>
		/// <param name="Value">Scaled value.</param>
		public IElement DescaleX(double Value)
		{
			return Graph.Descale(Value, this.minX, this.maxX, this.offsetX, this.width);
		}

		/// <summary>
		/// Descales a scaled value along the Y-axis.
		/// </summary>
		/// <param name="Value">Scaled value.</param>
		public IElement DescaleY(double Value)
		{
			return Graph.Descale(Value, this.minY, this.maxY, this.offsetY, this.height);
		}

	}
}
