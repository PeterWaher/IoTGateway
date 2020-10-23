using System;
using System.Collections.Generic;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Content.Functions.Encoding
{
	/// <summary>
	/// Decode(Content,ContentType)
	/// </summary>
	public class Decode : FunctionTwoScalarVariables
	{
		/// <summary>
		/// Decode(Content,ContentType)
		/// </summary>
		/// <param name="Content">Content</param>
		/// <param name="ContentType">Content Type</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Decode(ScriptNode Content, ScriptNode ContentType, int Start, int Length, Expression Expression)
			: base(Content, ContentType, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "decode"; }
		}

		/// <summary>
		/// Evaluates the function on two scalar arguments.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(IElement Argument1, IElement Argument2, Variables Variables)
		{
			if (!(Argument1.AssociatedObjectValue is byte[] Bin))
				throw new ScriptRuntimeException("Binary data expected.", this);

			string ContentType = Argument2 is StringValue S2 ? S2.Value : Expression.ToString(Argument2.AssociatedObjectValue);

			return this.DoDecode(Bin, ContentType, System.Text.Encoding.UTF8);
		}

		private IElement DoDecode(byte[] Data, string ContentType, System.Text.Encoding Encoding)
		{
			object Decoded = InternetContent.Decode(ContentType, Data, Encoding, new KeyValuePair<string, string>[0], null);

			if (Decoded is string[][] Records)
			{
				int Rows = Records.Length;
				int MaxCols = 0;
				int i, c;

				foreach (string[] Rec in Records)
				{
					if (Rec is null)
						continue;

					c = Rec.Length;
					if (c > MaxCols)
						MaxCols = c;
				}

				LinkedList<IElement> Elements = new LinkedList<IElement>();

				foreach (string[] Rec in Records)
				{
					i = 0;

					if (!(Rec is null))
					{
						foreach (string s in Rec)
						{
							if (s is null || string.IsNullOrEmpty(s))
								Elements.AddLast(new ObjectValue(null));
							else if (CommonTypes.TryParse(s, out double dbl))
								Elements.AddLast(new DoubleNumber(dbl));
							else if (CommonTypes.TryParse(s, out bool b))
								Elements.AddLast(new BooleanValue(b));
							else if (XML.TryParse(s, out DateTime TP))
								Elements.AddLast(new DateTimeValue(TP));
							else if (TimeSpan.TryParse(s, out TimeSpan TS))
								Elements.AddLast(new ObjectValue(TS));
							else
								Elements.AddLast(new StringValue(s));

							i++;
						}
					}

					while (i++ < MaxCols)
						Elements.AddLast(new StringValue(string.Empty));
				}

				return Operators.Matrices.MatrixDefinition.Encapsulate(Elements, Rows, MaxCols, this);
			}
			else
				return new ObjectValue(Decoded);
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
			System.Text.Encoding Encoding = System.Text.Encoding.UTF8;
			byte[] Bin = Encoding.GetBytes(Argument1);

			return this.DoDecode(Bin, Argument2, Encoding);
		}

	}
}
