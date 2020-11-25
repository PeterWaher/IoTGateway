using System;
using System.Collections.Generic;
using System.Numerics;
using SkiaSharp;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Graphs3D.Functions.Plots
{
	/// <summary>
	/// Plots a three-dimensional vertical bars chart.
	/// </summary>
	public class VerticalBars3D : FunctionMultiVariate
	{
		/// <summary>
		/// Plots a three-dimensional vertical bars chart.
		/// </summary>
		/// <param name="X">X-axis.</param>
		/// <param name="Y">Y-axis.</param>
		/// <param name="Z">Z-axis.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public VerticalBars3D(ScriptNode X, ScriptNode Y, ScriptNode Z, int Start, int Length, Expression Expression)
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
		public VerticalBars3D(ScriptNode X, ScriptNode Y, ScriptNode Z, ScriptNode Color, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { X, Y, Z, Color },
				  new ArgumentType[] { ArgumentType.Matrix, ArgumentType.Matrix, ArgumentType.Matrix, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "verticalbars3d"; }
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

			return new Graph3D(X, Y, Z, null, this.DrawGraph, false, true, false, this,
				Color is null ? SKColors.Red : Color.AssociatedObjectValue);
		}

		private void DrawGraph(Graphs3D.Canvas3D Canvas, Vector4[,] Points, Vector4[,] Normals,
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
