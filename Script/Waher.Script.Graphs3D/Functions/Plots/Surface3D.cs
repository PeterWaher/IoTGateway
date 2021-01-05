using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Graphs;
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

			return new Graph3D(X, Y, Z, null, new Surface3DPainter(), false, false, false, this,
				Color is null ? Graph.DefaultColor : Color.AssociatedObjectValue,
				TwoSided is null ? true : TwoSided.AssociatedObjectValue);
		}
	}
}
