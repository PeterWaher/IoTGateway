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
	/// Converts a color into grayscale.
	/// </summary>
	public class GrayScale : FunctionOneScalarVariable
	{
		/// <summary>
		/// Converts a color into grayscale.
		/// </summary>
		/// <param name="Color">Color to convert.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public GrayScale(ScriptNode Color, int Start, int Length, Expression Expression)
			: base(Color, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "Color" };
			}
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get
			{
				return "GrayScale";
			}
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(IElement Argument, Variables Variables)
		{
			SKColor Color = Graph.ToColor(Argument.AssociatedObjectValue);
			byte Intensity = (byte)(0.3 * Color.Red + 0.59 * Color.Green + 0.11 * Color.Blue + 0.5);

			return new ObjectValue(new SKColor(Intensity, Intensity, Intensity, Color.Alpha));
		}
	}
}
