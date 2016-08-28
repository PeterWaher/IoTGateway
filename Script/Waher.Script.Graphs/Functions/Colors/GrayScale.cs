using System;
using System.Collections.Generic;
using System.Drawing;
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
			Color Color = Graph.ToColor(Argument.AssociatedObjectValue);
			int Intensity = (int)(0.3 * Color.R + 0.59 * Color.G + 0.11 * Color.B + 0.5);

			return new ObjectValue(Color.FromArgb(Color.A, Intensity, Intensity, Intensity));
		}
	}
}
