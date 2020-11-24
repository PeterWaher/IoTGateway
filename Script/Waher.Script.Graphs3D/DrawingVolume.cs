using System;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Graphs;

namespace Waher.Script.Graphs3D
{
	/// <summary>
	/// Contains information about the current drawing area.
	/// </summary>
	public class DrawingVolume
	{
		private readonly IElement minX;
		private readonly IElement maxX;
		private readonly IElement minY;
		private readonly IElement maxY;
		private readonly IElement minZ;
		private readonly IElement maxZ;
		private readonly int offsetX;
		private readonly int offsetY;
		private readonly int offsetZ;
		private readonly int width;
		private readonly int height;
		private readonly int depth;
		private readonly float origoX;
		private readonly float origoY;
		private readonly float origoZ;

		/// <summary>
		/// Contains information about the current drawing area.
		/// </summary>
		/// <param name="MinX">Smallest value of X.</param>
		/// <param name="MaxX">Largest value of X.</param>
		/// <param name="MinY">Smallest value of Y.</param>
		/// <param name="MaxY">Largest value of Y.</param>
		/// <param name="MinZ">Smallest value of Z.</param>
		/// <param name="MaxZ">Largest value of Z.</param>
		/// <param name="OffsetX">X-offset of drawing area, relative to the canvas origin.</param>
		/// <param name="OffsetY">Y-offset of drawing area, relative to the canvas origin.</param>
		/// <param name="OffsetZ">Z-offset of drawing area, relative to the canvas origin.</param>
		/// <param name="Width">Width (x) of drawing area.</param>
		/// <param name="Height">Height (y) of drawing area.</param>
		/// <param name="Depth">Depth (z) of drawing area.</param>
		/// <param name="OrigoX">X-coordinate of the origo.</param>
		/// <param name="OrigoY">Y-coordinate of the origo.</param>
		/// <param name="OrigoZ">Z-coordinate of the origo.</param>
		public DrawingVolume(IElement MinX, IElement MaxX, IElement MinY, IElement MaxY,
			IElement MinZ, IElement MaxZ, int OffsetX, int OffsetY, int OffsetZ,
			int Width, int Height, int Depth, float OrigoX, float OrigoY, float OrigoZ)
		{
			this.minX = MinX;
			this.maxX = MaxX;
			this.minY = MinY;
			this.maxY = MaxY;
			this.minZ = MinZ;
			this.maxZ = MaxZ;
			this.offsetX = OffsetX;
			this.offsetY = OffsetY;
			this.offsetZ = OffsetZ;
			this.width = Width;
			this.height = Height;
			this.depth = Depth;
			this.origoX = OrigoX;
			this.origoY = OrigoY;
			this.origoZ = OrigoZ;
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
		public IElement MaxX => this.maxX;

		/// <summary>
		/// Smallest value of Y.
		/// </summary>
		public IElement MinY => this.minY;

		/// <summary>
		/// Largest value of Y.
		/// </summary>
		public IElement MaxY => this.maxY;

		/// <summary>
		/// Smallest value of Z.
		/// </summary>
		public IElement MinZ => this.minZ;

		/// <summary>
		/// Largest value of Z.
		/// </summary>
		public IElement MaxZ => this.maxZ;

		/// <summary>
		/// X-offset of drawing area, relative to the canvas origin.
		/// </summary>
		public int OffsetX => this.offsetX;

		/// <summary>
		/// Y-offset of drawing area, relative to the canvas origin.
		/// </summary>
		public int OffsetY => this.offsetY;

		/// <summary>
		/// Z-offset of drawing area, relative to the canvas origin.
		/// </summary>
		public int OffsetZ => this.offsetZ;

		/// <summary>
		/// Width of drawing area.
		/// </summary>
		public int Width => this.width;

		/// <summary>
		/// Height of drawing area.
		/// </summary>
		public int Height => this.height;

		/// <summary>
		/// Depth of drawing area.
		/// </summary>
		public int Depth => this.depth;

		/// <summary>
		/// X-coordinate for the origo.
		/// </summary>
		public float OrigoX => this.origoX;

		/// <summary>
		/// Y-coordinate for the origo.
		/// </summary>
		public float OrigoY => this.origoY;

		/// <summary>
		/// Z-coordinate for the origo.
		/// </summary>
		public float OrigoZ => this.origoZ;

		/// <summary>
		/// Scales three matrices of equal size to point vectors in space.
		/// </summary>
		/// <param name="MatrixX">X-matrix.</param>
		/// <param name="MatrixY">Y-matrix.</param>
		/// <param name="MatrixZ">Z-matrix.</param>
		public Vector4[,] Scale(IMatrix MatrixX, IMatrix MatrixY, IMatrix MatrixZ)
		{
			return Graph3D.Scale(MatrixX, MatrixY, MatrixZ, this.minX, this.maxX,
				this.minY, this.maxY, this.minZ, this.maxZ,
				this.offsetX, this.offsetY, this.offsetZ,
				this.width, this.height, this.depth);
		}

		/// <summary>
		/// Scales a matrix with x-coordinates to fit a given volume.
		/// </summary>
		/// <param name="Matrix">Matrix.</param>
		public double[,] ScaleX(IMatrix Matrix)
		{
			return Graph3D.Scale(Matrix, this.minX, this.maxX, this.offsetX, this.width);
		}

		/// <summary>
		/// Scales a vector of x-coordinates to fit a given volume.
		/// </summary>
		/// <param name="Vector">Vector.</param>
		public double[] ScaleX(IVector Vector)
		{
			return Graph2D.Scale(Vector, this.minX, this.maxX, this.offsetX, this.width);
		}

		/// <summary>
		/// Scales a matrix with y-coordinates to fit a given volume.
		/// </summary>
		/// <param name="Matrix">Matrix.</param>
		public double[,] ScaleY(IMatrix Matrix)
		{
			return Graph3D.Scale(Matrix, this.minY, this.maxY, this.offsetY, this.height);
		}

		/// <summary>
		/// Scales a vector of y-coordinates to fit a given volume.
		/// </summary>
		/// <param name="Vector">Vector.</param>
		public double[] ScaleY(IVector Vector)
		{
			return Graph2D.Scale(Vector, this.minY, this.maxY, this.offsetY, this.height);
		}

		/// <summary>
		/// Scales a matrix with z-coordinates to fit a given volume.
		/// </summary>
		/// <param name="Matrix">Matrix.</param>
		public double[,] ScaleZ(IMatrix Matrix)
		{
			return Graph3D.Scale(Matrix, this.maxZ, this.minZ, this.offsetZ, this.depth);
		}

		/// <summary>
		/// Scales a vector of z-coordinates to fit a given volume.
		/// </summary>
		/// <param name="Vector">Vector.</param>
		public double[] ScaleZ(IVector Vector)
		{
			return Graph2D.Scale(Vector, this.maxZ, this.minZ, this.offsetZ, this.depth);
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

		/// <summary>
		/// Descales a scaled value along the Z-axis.
		/// </summary>
		/// <param name="Value">Scaled value.</param>
		public IElement DescaleZ(double Value)
		{
			return Graph.Descale(Value, this.maxZ, this.minZ, this.offsetZ, this.depth);
		}

	}
}
