using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Script;
using Waher.Networking.XMPP.Contracts.HumanReadable;
using System;
using System.Xml;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Role-reference contractual parameter
	/// </summary>
	public class RoleParameter : Parameter
	{
		private string role = string.Empty;
		private string property = string.Empty;
		private string value = string.Empty;
		private int index = 0;
		private bool required = false;

		/// <summary>
		/// Parameter value.
		/// </summary>
		public override object ObjectValue => this.value;

		/// <summary>
		/// Name of role the parameter references.
		/// </summary>
		public string Role
		{
			get => this.role;
			set => this.role = value;
		}

		/// <summary>
		/// 1-based Role index. 1=First signatory having the rolw, 2=second, etc.
		/// </summary>
		public int Index
		{
			get => this.index;
			set => this.index = value;
		}

		/// <summary>
		/// Name of property of signatory having the corresponding role.
		/// </summary>
		public string Property
		{
			get => this.property;
			set => this.property = value;
		}

		/// <summary>
		/// If parameter is required to exist for contract to be valid.
		/// </summary>
		public bool Required
		{
			get => this.required;
			set => this.required = value;
		}

		/// <summary>
		/// Role parameter value.
		/// </summary>
		public string Value => this.value;

		/// <summary>
		/// Serializes the parameter, in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output</param>
		/// <param name="UsingTemplate">If the XML is for creating a contract using a template.</param>
		public override void Serialize(StringBuilder Xml, bool UsingTemplate)
		{
			if (!UsingTemplate)
			{
				Xml.Append("<roleParameter name=\"");
				Xml.Append(XML.Encode(this.Name.Normalize(NormalizationForm.FormC)));
				Xml.Append("\" role=\"");
				Xml.Append(XML.Encode(this.role.Normalize(NormalizationForm.FormC)));
				Xml.Append("\" index=\"");
				Xml.Append(this.index.ToString());
				Xml.Append("\" property=\"");
				Xml.Append(XML.Encode(this.property.Normalize(NormalizationForm.FormC)));
				Xml.Append("\" required=\"");
				Xml.Append(CommonTypes.Encode(this.required));

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

					Xml.Append("</roleParameter>");
				}
			}
		}

		/// <summary>
		/// Checks if the parameter value is valid.
		/// </summary>
		/// <param name="Variables">Collection of parameter values.</param>
		/// <returns>Error message, if parameter value is not valid, null if valid.</returns>
		public override Task<bool> IsParameterValid(Variables Variables)
		{
			return Task.FromResult(true);
		}

		/// <summary>
		/// Populates a variable collection with the value of the parameter.
		/// </summary>
		/// <param name="Variables">Variable collection.</param>
		public override void Populate(Variables Variables)
		{
			Variables[this.Name] = this.value;
		}

		/// <summary>
		/// Sets the parameter value.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of the correct type.</exception>
		public override void SetValue(object Value)
		{
			this.value = Value?.ToString() ?? string.Empty;
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

			this.Role = XML.Attribute(Xml, "role");
			this.Index = XML.Attribute(Xml, "index", 0);
			this.Property = XML.Attribute(Xml, "property");
			this.Required = XML.Attribute(Xml, "required", false);

			return true;
		}

	}
}
