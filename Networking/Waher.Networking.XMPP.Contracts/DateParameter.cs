using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Networking.XMPP.Contracts.HumanReadable;
using Waher.Script;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Date contractual parameter
	/// </summary>
	public class DateParameter : RangeParameter<DateTime>
	{
		/// <summary>
		/// Serializes the parameter, in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output</param>
		/// <param name="UsingTemplate">If the XML is for creating a contract using a template.</param>
		public override void Serialize(StringBuilder Xml, bool UsingTemplate)
		{
			Xml.Append("<dateParameter name=\"");
			Xml.Append(XML.Encode(this.Name));

			if (this.Value.HasValue)
			{
				Xml.Append("\" value=\"");
				Xml.Append(XML.Encode(this.Value.Value, true));
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
					Xml.Append(XML.Encode(this.Min.Value, true));
					Xml.Append("\" minIncluded=\"");
					Xml.Append(XML.Encode(CommonTypes.Encode(this.MinIncluded)));
				}

				if (this.Max.HasValue)
				{
					Xml.Append("\" max=\"");
					Xml.Append(XML.Encode(this.Max.Value, true));
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

					Xml.Append("</dateParameter>");
				}
			}
		}

		/// <summary>
		/// Checks if the parameter value is valid.
		/// </summary>
		/// <param name="Variables">Collection of parameter values.</param>
		/// <returns>If parameter value is valid.</returns>
		public override Task<bool> IsParameterValid(Variables Variables)
		{
			if (this.Value.HasValue && this.Value.Value.TimeOfDay != TimeSpan.Zero)
				return Task.FromResult(false);

			return base.IsParameterValid(Variables);
		}

		/// <summary>
		/// Sets the parameter value.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of the correct type.</exception>
		public override void SetValue(object Value)
		{
			if (Value is DateTime d)
				this.Value = d.Date;
			else
				throw new ArgumentException("Invalid parameter type.", nameof(Value));
		}

	}
}
