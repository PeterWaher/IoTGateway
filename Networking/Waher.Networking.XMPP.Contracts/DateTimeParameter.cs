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
	/// Date and Time contractual parameter
	/// </summary>
	public class DateTimeParameter : RangeParameter<DateTime>
	{
		/// <summary>
		/// String representation of value.
		/// </summary>
		public override string StringValue
		{
			get => this.Value.HasValue ? XML.Encode(this.Value.Value, false) : string.Empty;
			set
			{
				if (XML.TryParse(value, out DateTime TP))
					this.Value = TP;
				else
					this.Value = null;
			}
		}

		/// <summary>
		/// Parameter type name, corresponding to the local name of the parameter element in XML.
		/// </summary>
		public override string ParameterType => "dateTimeParameter";

		/// <summary>
		/// Serializes the parameter, in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output</param>
		/// <param name="UsingTemplate">If the XML is for creating a contract using a template.</param>
		public override void Serialize(StringBuilder Xml, bool UsingTemplate)
		{
			Xml.Append("<dateTimeParameter");

			if (!UsingTemplate)
			{
				if (!string.IsNullOrEmpty(this.Expression))
				{
					Xml.Append(" exp=\"");
					Xml.Append(XML.Encode(this.Expression.Normalize(NormalizationForm.FormC)));
					Xml.Append('"');
				}

				if (!string.IsNullOrEmpty(this.Guide))
				{
					Xml.Append(" guide=\"");
					Xml.Append(XML.Encode(this.Guide.Normalize(NormalizationForm.FormC)));
					Xml.Append('"');
				}

				if (this.Max.HasValue)
				{
					Xml.Append(" max=\"");
					Xml.Append(XML.Encode(this.Max.Value, false));
					Xml.Append("\" maxIncluded=\"");
					Xml.Append(XML.Encode(CommonTypes.Encode(this.MaxIncluded)));
					Xml.Append('"');
				}

				if (this.Min.HasValue)
				{
					Xml.Append(" min=\"");
					Xml.Append(XML.Encode(this.Min.Value, false));
					Xml.Append("\" minIncluded=\"");
					Xml.Append(XML.Encode(CommonTypes.Encode(this.MinIncluded)));
					Xml.Append('"');
				}
			}

			Xml.Append(" name=\"");
			Xml.Append(XML.Encode(this.Name));
			Xml.Append('"');

			if (this.CanSerializeProtectedValue)
			{
				Xml.Append(" protected=\"");
				Xml.Append(Convert.ToBase64String(this.ProtectedValue));
				Xml.Append('"');
			}

			if (!UsingTemplate && this.Protection != ProtectionLevel.Normal)
			{
				Xml.Append(" protection=\"");
				Xml.Append(this.Protection.ToString());
				Xml.Append('"');
			}

			if (this.Value.HasValue && this.CanSerializeValue)
			{
				Xml.Append(" value=\"");
				Xml.Append(XML.Encode(this.Value.Value, false));
				Xml.Append('"');
			}

			if (UsingTemplate || this.Descriptions is null || this.Descriptions.Length == 0)
				Xml.Append("/>");
			else
			{
				Xml.Append('>');

				foreach (HumanReadableText Description in this.Descriptions)
					Description.Serialize(Xml, "description", null);

				Xml.Append("</dateTimeParameter>");
			}
		}

		/// <summary>
		/// Sets the parameter value.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of the correct type.</exception>
		public override void SetValue(object Value)
		{
			if (Value is DateTime d)
				this.Value = d;
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
			if (Value is DateTime d)
			{
				this.Min = d;

				if (Inclusive.HasValue)
					this.MinIncluded = Inclusive.Value;
			}
			else
				throw new ArgumentException("Invalid parameter type.", nameof(Value));
		}

		/// <summary>
		/// Sets the maximum value allowed by the parameter.
		/// </summary>
		/// <param name="Value">Maximum value.</param>
		/// <param name="Inclusive">If the value is included in the range. If null, keeps the original value.</param>
		public override void SetMaxValue(object Value, bool? Inclusive)
		{
			if (Value is DateTime d)
			{
				this.Max = d;

				if (Inclusive.HasValue)
					this.MaxIncluded = Inclusive.Value;
			}
			else
				throw new ArgumentException("Invalid parameter type.", nameof(Value));
		}

		/// <summary>
		/// Imports parameter values from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <returns>If import was successful.</returns>
		public override Task<bool> Import(XmlElement Xml)
		{
			this.Value = Xml.HasAttribute("value") ? XML.Attribute(Xml, "value", DateTime.MinValue) : (DateTime?)null;
			this.Min = Xml.HasAttribute("min") ? XML.Attribute(Xml, "min", DateTime.MinValue) : (DateTime?)null;
			this.MinIncluded = XML.Attribute(Xml, "minIncluded", true);
			this.Max = Xml.HasAttribute("max") ? XML.Attribute(Xml, "max", DateTime.MinValue) : (DateTime?)null;
			this.MaxIncluded = XML.Attribute(Xml, "maxIncluded", true);

			return base.Import(Xml);
		}

	}
}
