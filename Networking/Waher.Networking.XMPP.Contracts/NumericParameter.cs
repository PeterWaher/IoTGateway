using System;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Networking.XMPP.Contracts.HumanReadable;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Numerical contractual parameter
	/// </summary>
	public class NumericalParameter : RangeParameter<decimal>
	{
		/// <summary>
		/// Serializes the parameter, in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output</param>
		/// <param name="UsingTemplate">If the XML is for creating a contract using a template.</param>
		public override void Serialize(StringBuilder Xml, bool UsingTemplate)
		{
			Xml.Append("<numericalParameter name=\"");
			Xml.Append(XML.Encode(this.Name));

			if (this.Value.HasValue)
			{
				Xml.Append("\" value=\"");
				Xml.Append(CommonTypes.Encode(this.Value.Value));
			}

			if (UsingTemplate)
				Xml.Append("\"/>");
			else
			{
				if (!string.IsNullOrEmpty(this.Guide))
				{
					Xml.Append("\" guide=\"");
					Xml.Append(XML.Encode(this.Guide.Normalize(NormalizationForm.FormC)));
				}

				if (!string.IsNullOrEmpty(this.Expression))
				{
					Xml.Append("\" exp=\"");
					Xml.Append(XML.Encode(this.Expression.Normalize(NormalizationForm.FormC)));
				}

				if (this.Min.HasValue)
				{
					Xml.Append("\" min=\"");
					Xml.Append(CommonTypes.Encode(this.Min.Value));
					Xml.Append("\" minIncluded=\"");
					Xml.Append(XML.Encode(CommonTypes.Encode(this.MinIncluded)));
				}

				if (this.Max.HasValue)
				{
					Xml.Append("\" max=\"");
					Xml.Append(CommonTypes.Encode(this.Max.Value));
					Xml.Append("\" maxIncluded=\"");
					Xml.Append(XML.Encode(CommonTypes.Encode(this.MaxIncluded)));
				}

				if (this.Descriptions is null || this.Descriptions.Length == 0)
					Xml.Append("\"/>");
				else
				{
					Xml.Append("\">");

					foreach (HumanReadableText Description in this.Descriptions)
						Description.Serialize(Xml, "description", false);

					Xml.Append("</numericalParameter>");
				}
			}
		}

		/// <summary>
		/// Sets the parameter value.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of the correct type.</exception>
		public override void SetValue(object Value)
		{
			if (Value is double d)
				this.Value = (decimal)d;
			else if (Value is float f)
				this.Value = (decimal)f;
			else if (Value is decimal dec)
				this.Value = dec;
			else if (Value is int i)
				this.Value = i;
			else if (Value is long l)
				this.Value = l;
			else if (Value is short s)
				this.Value = s;
			else if (Value is sbyte sb)
				this.Value = sb;
			else if (Value is uint ui)
				this.Value = ui;
			else if (Value is ulong ul)
				this.Value = ul;
			else if (Value is ushort us)
				this.Value = us;
			else if (Value is byte ub)
				this.Value = ub;
			else if (Value is string str && CommonTypes.TryParse(str, out dec))
				this.Value = dec;
			else
				throw new ArgumentException("Invalid parameter type.", nameof(Value));
		}

		/// <summary>
		/// Imports parameter values from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <returns>If import was successful.</returns>
		public override async Task<bool> Import(XmlElement Xml)
		{
			if (!await base.Import(Xml))
				return false;

			this.Value = Xml.HasAttribute("value") ? XML.Attribute(Xml, "value", 0.0m) : (decimal?)null;
			this.Min = Xml.HasAttribute("min") ? XML.Attribute(Xml, "min", 0.0m) : (decimal?)null;
			this.MinIncluded = XML.Attribute(Xml, "minIncluded", true);
			this.Max = Xml.HasAttribute("max") ? XML.Attribute(Xml, "max", 0.0m) : (decimal?)null;
			this.MaxIncluded = XML.Attribute(Xml, "maxIncluded", true);

			return true;
		}

	}
}
