using System;
using System.Numerics;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Graphs3D.Functions.Plots
{
	/// <summary>
	/// Plots a three-dimensional vertical bars chart.
	/// </summary>
	public class VerticalBars3DPainter : IPainter3D
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
			I3DShader Shader = Graph3D.ToShader(Parameters[0]);
			Vector3 P1, P2;
			float dx, dz;
			int i0, j0;
			double[] v = DrawingVolume.ScaleY(new DoubleVector(0));
			float OrigoY = (float)v[0];

			for (i = 0; i < c; i++)
			{
				i0 = i == 0 ? i : i - 1;

				for (j = 0; j < d; j++)
				{
					j0 = j == 0 ? j : j - 1;

					P1 = Graphs3D.Canvas3D.ToVector3(Points[i0, j0]);
					P2 = Graphs3D.Canvas3D.ToVector3(Points[i0 + 1, j0 + 1]);

					dx = P2.X - P1.X;
					dz = P2.Z - P1.Z;

					P1 = Graphs3D.Canvas3D.ToVector3(Points[i, j]);
					dx /= 2;
					dz /= 2;

					Canvas.Box(P1.X - dx, OrigoY, P1.Z - dz, P1.X + dx, P1.Y, P1.Z + dz, Shader);
				}
			}
		}
	}
}
