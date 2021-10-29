using System;
using System.Text;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Networking.XMPP.Contracts.HumanReadable;
using Waher.Script;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Numerical contractual parameter
	/// </summary>
	public class NumericalParameter : Parameter
	{
		private double? value;
		private double? min = null;
		private double? max = null;
		private bool minIncluded = true;
		private bool maxIncluded = true;

		/// <summary>
		/// Parameter value
		/// </summary>
		public double? Value
		{
			get => this.value;
			set => this.value = value;
		}

		/// <summary>
		/// Optional minimum value.
		/// </summary>
		public double? Min
		{
			get => this.min;
			set => this.min = value;
		}

		/// <summary>
		/// Optional maximum value.
		/// </summary>
		public double? Max
		{
			get => this.max;
			set => this.max = value;
		}

		/// <summary>
		/// If the optional minimum value is included in the allowed range.
		/// </summary>
		public bool MinIncluded
		{
			get => this.minIncluded;
			set => this.minIncluded = value;
		}

		/// <summary>
		/// If the optional maximum value is included in the allowed range.
		/// </summary>
		public bool MaxIncluded
		{
			get => this.maxIncluded;
			set => this.maxIncluded = value;
		}

		/// <summary>
		/// Parameter value.
		/// </summary>
		public override object ObjectValue => this.value;

		/// <summary>
		/// Serializes the parameter, in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output</param>
		public override void Serialize(StringBuilder Xml)
		{
			Xml.Append("<numericalParameter name=\"");
			Xml.Append(XML.Encode(this.Name));

			if (this.value.HasValue)
			{
				Xml.Append("\" value=\"");
				Xml.Append(CommonTypes.Encode(this.value.Value));
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

			if (this.min.HasValue)
			{
				Xml.Append("\" min=\"");
				Xml.Append(CommonTypes.Encode(this.min.Value));
				Xml.Append("\" minIncluded=\"");
				Xml.Append(XML.Encode(CommonTypes.Encode(this.minIncluded)));
			}

			if (this.max.HasValue)
			{
				Xml.Append("\" max=\"");
				Xml.Append(CommonTypes.Encode(this.max.Value));
				Xml.Append("\" maxIncluded=\"");
				Xml.Append(XML.Encode(CommonTypes.Encode(this.maxIncluded)));
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

		/// <summary>
		/// Checks if the parameter value is valid.
		/// </summary>
		/// <param name="Variables">Collection of parameter values.</param>
		/// <returns>If parameter value is valid.</returns>
		public override bool IsParameterValid(Variables Variables)
		{
			double Diff;

			if (!(this.value.HasValue))
				return false;

			if (this.min.HasValue)
			{
				Diff = this.value.Value - this.min.Value;

				if (Diff < 0 || (Diff == 0 && !this.minIncluded))
					return false;
			}

			if (this.max.HasValue)
			{
				Diff = this.value.Value - this.max.Value;

				if (Diff > 0 || (Diff == 0 && !this.maxIncluded))
					return false;
			}

			return base.IsParameterValid(Variables);
		}

		/// <summary>
		/// Populates a variable collection with the value of the parameter.
		/// </summary>
		/// <param name="Variables">Variable collection.</param>
		public override void Populate(Variables Variables)
		{
			Variables[this.Name] = this.value;
		}

	}
}
