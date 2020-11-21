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
	/// Plots a three-dimensional surface mesh.
	/// </summary>
	public class PolygonMesh3D : FunctionMultiVariate
	{
		/// <summary>
		/// Plots a three-dimensional surface mesh.
		/// </summary>
		/// <param name="X">X-axis.</param>
		/// <param name="Y">Y-axis.</param>
		/// <param name="Z">Z-axis.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public PolygonMesh3D(ScriptNode X, ScriptNode Y, ScriptNode Z, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { X, Y, Z },
				  new ArgumentType[] { ArgumentType.Matrix, ArgumentType.Matrix, ArgumentType.Matrix },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Plots a three-dimensional surface mesh.
		/// </summary>
		/// <param name="X">X-axis.</param>
		/// <param name="Y">Y-axis.</param>
		/// <param name="Z">Z-axis.</param>
		/// <param name="Shader">Color or shader.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public PolygonMesh3D(ScriptNode X, ScriptNode Y, ScriptNode Z, ScriptNode Shader, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { X, Y, Z, Shader },
				  new ArgumentType[] { ArgumentType.Matrix, ArgumentType.Matrix, ArgumentType.Matrix, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Plots a three-dimensional surface mesh.
		/// </summary>
		/// <param name="X">X-axis.</param>
		/// <param name="Y">Y-axis.</param>
		/// <param name="Z">Z-axis.</param>
		/// <param name="Shader">Color or shader.</param>
		/// <param name="TwoSided">If the surface is two-sided.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public PolygonMesh3D(ScriptNode X, ScriptNode Y, ScriptNode Z, ScriptNode Shader, ScriptNode TwoSided, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { X, Y, Z, Shader, TwoSided },
				  new ArgumentType[] { ArgumentType.Matrix, ArgumentType.Matrix, ArgumentType.Matrix, ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "polygonmesh3d"; }
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get { return new string[] { "x", "y", "z", "shader", "twoSided" }; }
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

			IElement Shader = Arguments.Length <= 3 ? null : Arguments[3];
			IElement TwoSided = Arguments.Length <= 4 ? null : Arguments[4];

			return new Graph3D(X, Y, Z, null, this.DrawGraph, false, false, false, this,
				Shader is null ? SKColors.Red : Shader.AssociatedObjectValue,
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

					Canvas.Polygon(Nodes, Shader, TwoSided);
				}
			}
		}

	}
}
