using System;
using System.Text;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Networking.XMPP.Contracts.HumanReadable;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Duration contractual parameter
	/// </summary>
	public class DurationParameter : RangeParameter<Duration>
	{
		/// <summary>
		/// Serializes the parameter, in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output</param>
		public override void Serialize(StringBuilder Xml)
		{
			Xml.Append("<durationParameter name=\"");
			Xml.Append(XML.Encode(this.Name));

			if (this.Value.HasValue)
			{
				Xml.Append("\" value=\"");
				Xml.Append(this.Value.Value.ToString());
			}

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
				Xml.Append(this.Min.Value.ToString());
				Xml.Append("\" minIncluded=\"");
				Xml.Append(XML.Encode(CommonTypes.Encode(this.MinIncluded)));
			}

			if (this.Max.HasValue)
			{
				Xml.Append("\" max=\"");
				Xml.Append(this.Max.Value.ToString());
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

				Xml.Append("</durationParameter>");
			}
		}

		/// <summary>
		/// Sets the parameter value.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of the correct type.</exception>
		public override void SetValue(object Value)
		{
			if (Value is Duration D)
				this.Value = D;
			else
				throw new ArgumentException("Invalid parameter type.", nameof(Value));
		}

	}
}
