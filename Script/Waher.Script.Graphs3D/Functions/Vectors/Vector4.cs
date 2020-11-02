using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Graphs3D.Functions.Vectors
{
	/// <summary>
	/// Generates a <see cref="System.Numerics.Vector4"/> object.
	/// </summary>
	public class Vector4 : FunctionMultiVariate
	{
		/// <summary>
		/// Generates a <see cref="System.Numerics.Vector4"/> object.
		/// </summary>
		/// <param name="X">Homogeneous X-coordinate.</param>
		/// <param name="Y">Homogeneous Y-coordinate.</param>
		/// <param name="Z">Homogeneous Z-coordinate.</param>
		/// <param name="W">Homogeneous W-coordinate.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Vector4(ScriptNode X, ScriptNode Y, ScriptNode Z, ScriptNode W,
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { X, Y, Z, W },
				  argumentTypes4Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "Vector4";

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "x", "y", "z", "w" };

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			return new ObjectValue(new System.Numerics.Vector4(
				(float)Expression.ToDouble(Arguments[0].AssociatedObjectValue),
				(float)Expression.ToDouble(Arguments[1].AssociatedObjectValue),
				(float)Expression.ToDouble(Arguments[2].AssociatedObjectValue),
				(float)Expression.ToDouble(Arguments[3].AssociatedObjectValue)));
		}
	}
}
