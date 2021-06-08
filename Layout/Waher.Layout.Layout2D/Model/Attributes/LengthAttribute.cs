using System;
using System.Xml;
using Waher.Content;
using Waher.Layout.Layout2D.Exceptions;
using Waher.Script;
using Waher.Script.Objects;
using Waher.Script.Units;

namespace Waher.Layout.Layout2D.Model.Attributes
{
	/// <summary>
	/// Length attribute
	/// </summary>
	public class LengthAttribute : Attribute<Length>
	{
		/// <summary>
		/// Length attribute
		/// </summary>
		/// <param name="AttributeName">Attribute name.</param>
		/// <param name="Value">Attribute value.</param>
		public LengthAttribute(string AttributeName, Length Value)
			: base(AttributeName, Value)
		{
		}

		/// <summary>
		/// Length attribute
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="AttributeName">Attribute name.</param>
		public LengthAttribute(XmlElement E, string AttributeName)
			: base(E, AttributeName, true)
		{
		}

		/// <summary>
		/// Length attribute
		/// </summary>
		/// <param name="AttributeName">Attribute name.</param>
		/// <param name="Expression">Expression.</param>
		public LengthAttribute(string AttributeName, Expression Expression)
			: base(AttributeName, Expression)
		{
		}

		/// <summary>
		/// Tries to convert script result to a value of type <see cref="float"/>.
		/// </summary>
		/// <param name="Result">Script result.</param>
		/// <param name="Value">Converted value.</param>
		/// <returns>If conversion was possible.</returns>
		public override bool TryConvert(object Result, out Length Value)
		{
			if (Result is double d)
			{
				Value = new Length((float)(d * 100), LengthUnit.Percent);
				return true;
			}
			else if (Result is PhysicalQuantity Q)
			{
				switch (Q.Unit.ToString().ToLower())
				{
					case "px":
						Value = new Length((float)Q.Magnitude, LengthUnit.Px);
						return true;

					case "pt":
						Value = new Length((float)Q.Magnitude, LengthUnit.Pt);
						return true;

					case "pc":
						Value = new Length((float)Q.Magnitude, LengthUnit.Pc);
						return true;

					case "cm":
						Value = new Length((float)Q.Magnitude, LengthUnit.Cm);
						return true;

					case "in":
						Value = new Length((float)Q.Magnitude, LengthUnit.In);
						return true;

					case "mm":
						Value = new Length((float)Q.Magnitude, LengthUnit.Mm);
						return true;

					case "em":
						Value = new Length((float)Q.Magnitude, LengthUnit.Em);
						return true;

					case "ex":
						Value = new Length((float)Q.Magnitude, LengthUnit.Ex);
						return true;

					case "ch":
						Value = new Length((float)Q.Magnitude, LengthUnit.Ch);
						return true;

					case "rem":
						Value = new Length((float)Q.Magnitude, LengthUnit.Rem);
						return true;

					case "vw":
						Value = new Length((float)Q.Magnitude, LengthUnit.Vw);
						return true;

					case "vh":
						Value = new Length((float)Q.Magnitude, LengthUnit.Vh);
						return true;

					case "vmin":
						Value = new Length((float)Q.Magnitude, LengthUnit.Vmin);
						return true;

					case "vmax":
						Value = new Length((float)Q.Magnitude, LengthUnit.Vmax);
						return true;

					default:
						if (Unit.TryConvert(Q.Magnitude, Q.Unit, pixels, out double Pixels))
						{
							Value = new Length((float)Pixels, LengthUnit.Px);
							return true;
						}
						else
						{
							Value = null;
							return false;
						}
				}
			}
			else
				return base.TryConvert(Result, out Value);
		}

		private static readonly Unit pixels = new Unit("px");

		/// <summary>
		/// Tries to parse a string value
		/// </summary>
		/// <param name="StringValue">String value for attribute.</param>
		/// <param name="Value">Parsed value, if successful.</param>
		/// <returns>If the value could be parsed.</returns>
		public override bool TryParse(string StringValue, out Length Value)
		{
			LengthUnit Unit;
			int c = StringValue.Length;
			int i = c;
			char ch = StringValue[--i];

			if (ch == '%')
				Unit = LengthUnit.Percent;
			else
			{
				char ch2 = StringValue[--i];

				switch (ch)
				{
					case 'x':
						switch (ch2)
						{
							case 'p':
								Unit = LengthUnit.Px;
								break;

							case 'e':
								Unit = LengthUnit.Ex;
								break;

							case 'a':
								if (c < 4 ||
									StringValue[--i] != 'm' ||
									StringValue[--i] != 'v')
								{
									throw InvalidLengthException(StringValue);
								}

								Unit = LengthUnit.Vmax;
								break;

							default:
								throw InvalidLengthException(StringValue);
						}
						break;

					case 't':
						if (ch2 != 'p')
							throw InvalidLengthException(StringValue);

						Unit = LengthUnit.Pt;
						break;

					case 'c':
						if (ch2 != 'p')
							throw InvalidLengthException(StringValue);

						Unit = LengthUnit.Pc;
						break;

					case 'm':
						switch (ch2)
						{
							case 'c':
								Unit = LengthUnit.Cm;
								break;

							case 'm':
								Unit = LengthUnit.Mm;
								break;

							case 'e':
								if (c >= 3 && StringValue[i - 1] == 'r')
								{
									Unit = LengthUnit.Rem;
									i--;
								}
								else
									Unit = LengthUnit.Em;
								break;

							default:
								throw InvalidLengthException(StringValue);
						}
						break;

					case 'n':
						if (c >= 4 && StringValue[i - 1] == 'm')
						{
							i--;

							if (StringValue[--i] == 'v')
								Unit = LengthUnit.Vmin;
							else
								throw InvalidLengthException(StringValue);
						}
						else
							Unit = LengthUnit.In;
						break;

					case 'h':
						switch (ch2)
						{
							case 'c':
								Unit = LengthUnit.Ch;
								break;

							case 'v':
								Unit = LengthUnit.Vh;
								break;

							default:
								throw InvalidLengthException(StringValue);
						}
						break;

					case 'w':
						if (ch2 != 'v')
							throw InvalidLengthException(StringValue);

						Unit = LengthUnit.Vw;
						break;

					default:
						throw InvalidLengthException(StringValue);
				}
			}

			while (i >= 0 && ((ch = StringValue[i]) <= ' ' || ch == 160))
				i--;

			StringValue = StringValue.Substring(0, i);

			if (!CommonTypes.TryParse(StringValue, out float d))
				throw InvalidLengthException(StringValue);

			Value = new Length(d, Unit);

			return true;
		}

		private static Exception InvalidLengthException(string AttributeValue)
		{
			return new LayoutSyntaxException("Invalid length: " + AttributeValue);
		}

		/// <summary>
		/// Converts a value to a string.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>String representation.</returns>
		public override string ToString(Length Value)
		{
			string s = CommonTypes.Encode(Value.Value);

			switch (Value.Unit)
			{
				case LengthUnit.Px: return s + " px";
				case LengthUnit.Pt: return s + " pt";
				case LengthUnit.Pc: return s + " pc";
				case LengthUnit.Cm: return s + " cm";
				case LengthUnit.In: return s + " in";
				case LengthUnit.Mm: return s + " mm";
				case LengthUnit.Em: return s + " em";
				case LengthUnit.Ex: return s + " ex";
				case LengthUnit.Ch: return s + " ch";
				case LengthUnit.Rem: return s + " rem";
				case LengthUnit.Vw: return s + " vw";
				case LengthUnit.Vh: return s + " vh";
				case LengthUnit.Vmin: return s + " vmin";
				case LengthUnit.Vmax: return s + " vmax";
				case LengthUnit.Percent: return s + " %";
				default: return s;
			}
		}

		/// <summary>
		/// Copies the attribute object if undefined, or defined by an expression.
		/// Returns a reference to itself, if preset (set by a constant value).
		/// </summary>
		/// <returns>Attribute reference.</returns>
		public LengthAttribute CopyIfNotPreset()
		{
			if (this.HasPresetValue)
				return this;
			else
				return new LengthAttribute(this.Name, this.Expression);
		}

	}
}
