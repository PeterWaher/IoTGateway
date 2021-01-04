using System;
using System.Numerics;
using SkiaSharp;
using Waher.Script.Graphs;

namespace Waher.Script.Graphs3D.Functions.Plots
{
	/// <summary>
	/// Plots a three-dimensional line mesh.
	/// </summary>
	public class LineMesh3DPainter : IPainter3D
	{
		/// <summary>
		/// Draws the graph on a canvas.
		/// </summary>
		/// <param name="Canvas">Canvas to draw on.</param>
		/// <param name="Points">Points to draw.</param>
		/// <param name="Normals">Optional normals.</param>
		/// <param name="Parameters">Graph-specific parameters.</param>
		/// <param name="PrevPoints">Points of previous graph of same type (if available), null (if not available).</param>
		/// <param name="PrevNormals">Optional normals of previous graph of same type (if available), null (if not available).</param>
		/// <param name="PrevParameters">Parameters of previous graph of same type (if available), null (if not available).</param>
		/// <param name="DrawingVolume">Current drawing volume.</param>
		public void DrawGraph(Graphs3D.Canvas3D Canvas, Vector4[,] Points, Vector4[,] Normals,
			object[] Parameters, Vector4[,] PrevPoints, Vector4[,] PrevNormals,
			object[] PrevParameters, DrawingVolume DrawingVolume)
		{
			int i, c = Points.GetLength(0);
			int j, d = Points.GetLength(1);
			SKColor Color = Graph.ToColor(Parameters[0]);
			Vector4[] Nodes = new Vector4[4];

			c--;
			d--;

			for (i = 0; i < c; i++)
			{
				for (j = 0; j < d; j++)
				{
					Nodes[0] = Points[i, j];
					Nodes[1] = Points[i + 1, j];
					Nodes[2] = Points[i + 1, j + 1];
					Nodes[3] = Points[i, j + 1];

					if (Normals is null)
						Canvas.PolyLine(Nodes, Color);
				}
			}
		}
	}
}
