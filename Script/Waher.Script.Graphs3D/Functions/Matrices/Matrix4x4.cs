using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Graphs3D.Functions.Matrices
{
	/// <summary>
	/// Generates a <see cref="System.Numerics.Matrix4x4"/> object.
	/// </summary>
	public class Matrix4x4 : FunctionMultiVariate
	{
		/// <summary>
		/// Sixteen scalar parameters.
		/// </summary>
		protected static readonly ArgumentType[] argumentTypes16Scalar = new ArgumentType[] 
		{ 
			ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
			ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
			ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
			ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar
		};

		/// <summary>
		/// Generates a <see cref="System.Numerics.Matrix4x4"/> object.
		/// </summary>
		/// <param name="M11">Element for row 1, column 1.</param>
		/// <param name="M12">Element for row 1, column 2.</param>
		/// <param name="M13">Element for row 1, column 3.</param>
		/// <param name="M14">Element for row 1, column 4.</param>
		/// <param name="M21">Element for row 2, column 1.</param>
		/// <param name="M22">Element for row 2, column 2.</param>
		/// <param name="M23">Element for row 2, column 3.</param>
		/// <param name="M24">Element for row 2, column 4.</param>
		/// <param name="M31">Element for row 3, column 1.</param>
		/// <param name="M32">Element for row 3, column 2.</param>
		/// <param name="M33">Element for row 3, column 3.</param>
		/// <param name="M34">Element for row 3, column 4.</param>
		/// <param name="M41">Element for row 4, column 1.</param>
		/// <param name="M42">Element for row 4, column 2.</param>
		/// <param name="M43">Element for row 4, column 3.</param>
		/// <param name="M44">Element for row 4, column 4.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Matrix4x4(ScriptNode M11, ScriptNode M12, ScriptNode M13, ScriptNode M14,
			ScriptNode M21, ScriptNode M22, ScriptNode M23, ScriptNode M24,
			ScriptNode M31, ScriptNode M32, ScriptNode M33, ScriptNode M34,
			ScriptNode M41, ScriptNode M42, ScriptNode M43, ScriptNode M44, 
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { M11, M12, M13, M14, M21, M22, M23, M24, M31, M32, M33, M34, M41, M42, M43, M44 },
				  argumentTypes16Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "Matrix4x4";

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] 
		{ 
			"m11", "m12", "m13", "m14", 
			"m21", "m22", "m23", "m24", 
			"m31", "m32", "m33", "m34", 
			"m41", "m42", "m43", "m44" 
		};

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			return new ObjectValue(new System.Numerics.Matrix4x4(
				(float)Expression.ToDouble(Arguments[0].AssociatedObjectValue),
				(float)Expression.ToDouble(Arguments[1].AssociatedObjectValue),
				(float)Expression.ToDouble(Arguments[2].AssociatedObjectValue),
				(float)Expression.ToDouble(Arguments[3].AssociatedObjectValue),
				(float)Expression.ToDouble(Arguments[4].AssociatedObjectValue),
				(float)Expression.ToDouble(Arguments[5].AssociatedObjectValue),
				(float)Expression.ToDouble(Arguments[6].AssociatedObjectValue),
				(float)Expression.ToDouble(Arguments[7].AssociatedObjectValue),
				(float)Expression.ToDouble(Arguments[8].AssociatedObjectValue),
				(float)Expression.ToDouble(Arguments[9].AssociatedObjectValue),
				(float)Expression.ToDouble(Arguments[10].AssociatedObjectValue),
				(float)Expression.ToDouble(Arguments[11].AssociatedObjectValue),
				(float)Expression.ToDouble(Arguments[12].AssociatedObjectValue),
				(float)Expression.ToDouble(Arguments[13].AssociatedObjectValue),
				(float)Expression.ToDouble(Arguments[14].AssociatedObjectValue),
				(float)Expression.ToDouble(Arguments[15].AssociatedObjectValue)));
		}
	}
}
