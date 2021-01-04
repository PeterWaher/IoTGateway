using System;
using System.Numerics;

namespace Waher.Script.Graphs3D.Functions.Plots
{
	/// <summary>
	/// Plots a three-dimensional surface.
	/// </summary>
	public class Surface3DPainter : IPainter3D
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
			bool TwoSided = Expression.ToDouble(Parameters[1]) != 0;
			Vector4[] Nodes = new Vector4[4];
			Vector4[] NodeNormals = new Vector4[4];

			c--;
			d--;

			if (Normals is null)
			{
				Vector4[,] N = new Vector4[c + 1, d + 1];
				Vector4 P1, P2, P3;
				int n;

				for (i = 0; i < c; i++)
				{
					for (j = 0; j < d; j++)
					{
						P1 = Points[i, j];
						P2 = Points[i + 1, j];
						if (P1 == P2)
							P2 = Points[i + 1, j + 1];
						P3 = Points[i, j + 1];
						if (P1 == P3)
							P3 = Points[i + 1, j + 1];

						N[i, j] = Graphs3D.Canvas3D.CalcNormal(P1, P2, P3);
					}

					if (Vector4.Distance(Points[i, d], Points[i, 0]) < 1e-10f)
						N[i, d] = N[i, 0];
					else
						N[i, d] = N[i, d - 1];
				}

				for (j = 0; j <= d; j++)
				{
					if (Vector4.Distance(Points[c, j], Points[0, j]) < 1e-10f)
						N[c, j] = N[0, j];
					else
						N[c, j] = N[c - 1, j];
				}

				Normals = new Vector4[c + 1, d + 1];

				for (i = 0; i <= c; i++)
				{
					for (j = 0; j <= d; j++)
					{
						P1 = N[i, j];
						n = 1;

						if (i > 0)
						{
							P1 += N[i - 1, j];
							n++;

							if (j > 0)
							{
								P1 += N[i - 1, j - 1];
								n++;
							}
						}

						if (j > 0)
						{
							P1 += N[i, j - 1];
							n++;
						}

						Normals[i, j] = P1 / n;
					}
				}
			}

			for (i = 0; i < c; i++)
			{
				for (j = 0; j < d; j++)
				{
					Nodes[0] = Points[i, j];
					Nodes[1] = Points[i + 1, j];
					Nodes[2] = Points[i + 1, j + 1];
					Nodes[3] = Points[i, j + 1];

					NodeNormals[0] = Normals[i, j];
					NodeNormals[1] = Normals[i + 1, j];
					NodeNormals[2] = Normals[i + 1, j + 1];
					NodeNormals[3] = Normals[i, j + 1];

					Canvas.Polygon(Nodes, NodeNormals, Shader, TwoSided);
				}
			}
		}
	}
}
