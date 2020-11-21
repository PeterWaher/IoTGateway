using System;
using System.Collections.Generic;
using System.Numerics;
using SkiaSharp;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Script.Graphs3D.Functions.Plots
{
	/// <summary>
	/// Plots a three-dimensional surface.
	/// </summary>
	public class Surface3D : FunctionMultiVariate
	{
		/// <summary>
		/// Plots a three-dimensional surface.
		/// </summary>
		/// <param name="X">X-axis.</param>
		/// <param name="Y">Y-axis.</param>
		/// <param name="Z">Z-axis.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Surface3D(ScriptNode X, ScriptNode Y, ScriptNode Z, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { X, Y, Z },
				  new ArgumentType[] { ArgumentType.Matrix, ArgumentType.Matrix, ArgumentType.Matrix },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Plots a three-dimensional surface.
		/// </summary>
		/// <param name="X">X-axis.</param>
		/// <param name="Y">Y-axis.</param>
		/// <param name="Z">Z-axis.</param>
		/// <param name="Color">Color or shader.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Surface3D(ScriptNode X, ScriptNode Y, ScriptNode Z, ScriptNode Color, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { X, Y, Z, Color },
				  new ArgumentType[] { ArgumentType.Matrix, ArgumentType.Matrix, ArgumentType.Matrix, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Plots a three-dimensional surface.
		/// </summary>
		/// <param name="X">X-axis.</param>
		/// <param name="Y">Y-axis.</param>
		/// <param name="Z">Z-axis.</param>
		/// <param name="Color">Color or shader.</param>
		/// <param name="TwoSided">If the surface is two-sided.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Surface3D(ScriptNode X, ScriptNode Y, ScriptNode Z, ScriptNode Color, ScriptNode TwoSided, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { X, Y, Z, Color, TwoSided },
				  new ArgumentType[] { ArgumentType.Matrix, ArgumentType.Matrix, ArgumentType.Matrix, ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "surface3d"; }
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get { return new string[] { "x", "y", "z", "color", "twoSided" }; }
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			if (!(Arguments[0] is IMatrix X))
				throw new ScriptRuntimeException("Expected matrix for X argument.", this);

			if (!(Arguments[1] is IMatrix Y))
				throw new ScriptRuntimeException("Expected matrix for Y argument.", this);

			if (!(Arguments[2] is IMatrix Z))
				throw new ScriptRuntimeException("Expected matrix for Z argument.", this);

			int Rows = X.Rows;
			int Columns = X.Columns;
			if (Y.Rows != Rows || Y.Columns != Columns || Z.Rows != Rows || Z.Columns != Columns)
				throw new ScriptRuntimeException("Matrix dimension mismatch.", this);

			IElement Color = Arguments.Length <= 3 ? null : Arguments[3];
			IElement TwoSided = Arguments.Length <= 4 ? null : Arguments[4];

			return new Graph3D(X, Y, Z, null, this.DrawGraph, false, false, false, this,
				Color is null ? SKColors.Red : Color.AssociatedObjectValue,
				TwoSided is null ? true : TwoSided.AssociatedObjectValue);
		}

		private void DrawGraph(Graphs3D.Canvas3D Canvas, Vector4[,] Points, Vector4[,] Normals,
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
