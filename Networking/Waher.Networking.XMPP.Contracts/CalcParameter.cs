using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Xml;
using Waher.Networking.XMPP.Contracts.HumanReadable;
using Waher.Script;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Calculation contractual parameter
	/// </summary>
	public class CalcParameter : Parameter
	{
		private object value;
		
		/// <summary>
		/// Parameter value.
		/// </summary>
		public override object ObjectValue => this.value;

		/// <summary>
		/// Serializes the parameter, in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output</param>
		/// <param name="UsingTemplate">If the XML is for creating a contract using a template.</param>
		public override void Serialize(StringBuilder Xml, bool UsingTemplate)
		{
			if (!UsingTemplate)
			{
				Xml.Append("<calcParameter name=\"");
				Xml.Append(XML.Encode(this.Name.Normalize(NormalizationForm.FormC)));

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

				if (this.Descriptions is null || this.Descriptions.Length == 0)
					Xml.Append("\"/>");
				else
				{
					Xml.Append("\">");

					foreach (HumanReadableText Description in this.Descriptions)
						Description.Serialize(Xml, "description", false);

					Xml.Append("</calcParameter>");
				}
			}
		}

		/// <summary>
		/// Checks if the parameter value is valid.
		/// </summary>
		/// <param name="Variables">Collection of parameter values.</param>
		/// <returns>If parameter value is valid.</returns>
		public override async Task<bool> IsParameterValid(Variables Variables)
		{
			if (string.IsNullOrEmpty(this.Expression))
			{
				this.value = null;
				return false;
			}
			else
			{
				try
				{
					this.value = await this.Parsed.EvaluateAsync(Variables);
					Variables[this.Name] = this.value;
				}
				catch (Exception)
				{
					this.value = null;
					return false;
				}

				return true;
			}
		}

		/// <summary>
		/// Populates a variable collection with the value of the parameter.
		/// </summary>
		/// <param name="Variables">Variable collection.</param>
		public override void Populate(Variables Variables)
		{
			// Do nothing.
		}

		/// <summary>
		/// Sets the parameter value.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of the correct type.</exception>
		public override void SetValue(object Value)
		{
			// Ignore
		}

	}
}
