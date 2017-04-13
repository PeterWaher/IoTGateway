using System;
using System.Collections.Generic;
using System.Drawing;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Graphs.Functions.Colors
{
	public class Color : FunctionOneScalarVariable
	{
		public Color(ScriptNode Name, int Start, int Length, Expression Expression)
			: base(Name, Start, Length, Expression)
		{
		}

		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "Name" };
			}
		}

		public override string FunctionName
		{
			get
			{
				return "Color";
			}
		}

		public override IElement EvaluateScalar(string Argument, Variables Variables)
		{
			return new ObjectValue(System.Drawing.Color.FromName(Argument));
		}
	}
}
