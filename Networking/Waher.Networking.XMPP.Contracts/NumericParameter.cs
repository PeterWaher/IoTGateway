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
		/// String representation of value.
		/// </summary>
		public override string StringValue
		{
			get => this.Value.HasValue ? CommonTypes.Encode(this.Value.Value) : string.Empty;
			set
			{
				if (CommonTypes.TryParse(value, out decimal d))
					this.Value = d;
				else
					this.Value = null;
			}
		}

		/// <summary>
		/// Parameter type name, corresponding to the local name of the parameter element in XML.
		/// </summary>
		public override string ParameterType => "numericalParameter";

		/// <summary>
		/// Serializes the parameter, in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output</param>
		/// <param name="UsingTemplate">If the XML is for creating a contract using a template.</param>
		public override void Serialize(StringBuilder Xml, bool UsingTemplate)
		{
			Xml.Append("<numericalParameter name=\"");
			Xml.Append(XML.Encode(this.Name));

			if (this.Value.HasValue && this.CanSerializeValue)
			{
				Xml.Append("\" value=\"");
				Xml.Append(CommonTypes.Encode(this.Value.Value));
			}
			else if (this.CanSerializeProtectedValue)
			{
				Xml.Append("\" protected=\"");
				Xml.Append(Convert.ToBase64String(this.ProtectedValue));
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

				if (this.Protection != ProtectionLevel.Normal)
				{
					Xml.Append("\" protection=\"");
					Xml.Append(this.Protection.ToString());
				}

				if (this.Descriptions is null || this.Descriptions.Length == 0)
					Xml.Append("\"/>");
				else
				{
					Xml.Append("\">");

					foreach (HumanReadableText Description in this.Descriptions)
						Description.Serialize(Xml, "description", null);

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
			this.Value = ToDecimal(Value);
		}

		/// <summary>
		/// Converts an object value to a <see cref="System.Decimal"/> value.
		/// </summary>
		/// <param name="Value">Object value.</param>
		/// <returns><see cref="System.Decimal"/> value</returns>
		/// <exception cref="ArgumentException">If the object value cannot be converted to a <see cref="System.Decimal"/> value.</exception>
		public static decimal ToDecimal(object Value)
		{
			if (Value is double d)
				return (decimal)d;
			else if (Value is float f)
				return (decimal)f;
			else if (Value is decimal dec)
				return dec;
			else if (Value is int i)
				return i;
			else if (Value is long l)
				return l;
			else if (Value is short s)
				return s;
			else if (Value is sbyte sb)
				return sb;
			else if (Value is uint ui)
				return ui;
			else if (Value is ulong ul)
				return ul;
			else if (Value is ushort us)
				return us;
			else if (Value is byte ub)
				return ub;
			else if (Value is string str && CommonTypes.TryParse(str, out dec))
				return dec;
			else
				throw new ArgumentException("Invalid parameter type.", nameof(Value));
		}

		/// <summary>
		/// Sets the minimum value allowed by the parameter.
		/// </summary>
		/// <param name="Value">Minimum value.</param>
		/// <param name="Inclusive">If the value is included in the range. If null, keeps the original value.</param>
		public override void SetMinValue(object Value, bool? Inclusive)
		{
			this.Min = ToDecimal(Value);

			if (Inclusive.HasValue)
				this.MinIncluded = Inclusive.Value;
		}

		/// <summary>
		/// Sets the maximum value allowed by the parameter.
		/// </summary>
		/// <param name="Value">Maximum value.</param>
		/// <param name="Inclusive">If the value is included in the range. If null, keeps the original value.</param>
		public override void SetMaxValue(object Value, bool? Inclusive)
		{
			this.Max = ToDecimal(Value);

			if (Inclusive.HasValue)
				this.MaxIncluded = Inclusive.Value;
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
