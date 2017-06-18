using System;
using System.Collections.Generic;
using SkiaSharp;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Graphs.Functions.Colors
{
	public class GrayScale : FunctionOneScalarVariable
	{
		public GrayScale(ScriptNode Color, int Start, int Length, Expression Expression)
			: base(Color, Start, Length, Expression)
		{
		}

		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "Color" };
			}
		}

		public override string FunctionName
		{
			get
			{
				return "GrayScale";
			}
		}

		public override IElement EvaluateScalar(IElement Argument, Variables Variables)
		{
			SKColor Color = Graph.ToColor(Argument.AssociatedObjectValue);
			byte Intensity = (byte)(0.3 * Color.Red + 0.59 * Color.Green + 0.11 * Color.Blue + 0.5);

			return new ObjectValue(new SKColor(Intensity, Intensity, Intensity, Color.Alpha));
		}
	}
}
