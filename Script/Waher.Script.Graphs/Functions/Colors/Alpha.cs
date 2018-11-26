using System;
using System.Collections.Generic;
using SkiaSharp;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Graphs.Functions.Colors
{
	/// <summary>
	/// Sets the Alpha channel of a color.
	/// </summary>
	public class Alpha : FunctionTwoScalarVariables
	{
		/// <summary>
		/// Sets the Alpha channel of a color.
		/// </summary>
		/// <param name="Color">Color.</param>
		/// <param name="Alpha">Alpha channel value.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Alpha(ScriptNode Color, ScriptNode Alpha, int Start, int Length, Expression Expression)
			: base(Color, Alpha, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "Color", "Alpha" };
			}
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get
			{
				return "Alpha";
			}
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument1">Color argument.</param>
		/// <param name="Argument2">Alpha argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(IElement Argument1, IElement Argument2, Variables Variables)
		{
			return this.Evaluate(Argument1.AssociatedObjectValue, Argument2.AssociatedObjectValue);
		}

		private IElement Evaluate(object Argument1, object Argument2)
		{ 
			SKColor Color = Graph.ToColor(Argument1);
			double Alpha = Expression.ToDouble(Argument2);
			byte A;

			if (Alpha < 0)
				A = 0;
			else if (Alpha > 255)
				A = 255;
			else
				A = (byte)(Alpha + 0.5);

			return new ObjectValue(new SKColor(Color.Red, Color.Green, Color.Blue, A));
		}

		/// <summary>
		/// Evaluates the function on two scalar arguments.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(string Argument1, string Argument2, Variables Variables)
		{
			return this.Evaluate(Argument1, Argument2);
		}
	}
}
