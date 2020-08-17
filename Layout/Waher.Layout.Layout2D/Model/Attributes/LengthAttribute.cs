using System;
using System.Text;
using System.Xml;
using Waher.Content;
using Waher.Layout.Layout2D.Exceptions;
using Waher.Script;

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
