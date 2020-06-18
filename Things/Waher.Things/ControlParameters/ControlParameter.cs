using System;
using System.Threading.Tasks;
using System.Xml;

namespace Waher.Things.ControlParameters
{
	/// <summary>
	/// Abstract base class for control parameters.
	/// </summary>
	public abstract class ControlParameter
	{
		private readonly string name;
		private readonly string page;
		private readonly string label;
		private readonly string description;

		/// <summary>
		/// Abstract base class for control parameters.
		/// </summary>
		/// <param name="Name">Parameter name.</param>
		/// <param name="Page">On which page in the control dialog the parameter should appear.</param>
		/// <param name="Label">Label for parameter.</param>
		/// <param name="Description">Description for parameter.</param>
		public ControlParameter(string Name, string Page, string Label, string Description)
		{
			this.name = Name;
			this.page = Page;
			this.label = Label;
			this.description = Description;
		}

		/// <summary>
		/// Parameter Name
		/// </summary>
		public string Name
		{
			get { return this.name; }
		}

		/// <summary>
		/// On which page in the control dialog the parameter should appear.
		/// </summary>
		public string Page
		{
			get { return this.page; }
		}

		/// <summary>
		/// Label for parameter.
		/// </summary>
		public string Label
		{
			get { return this.label; }
		}

		/// <summary>
		/// Description for parameter.
		/// </summary>
		public string Description
		{
			get { return this.description; }
		}

		/// <summary>
		/// Sets the value of the control parameter.
		/// </summary>
		/// <param name="Node">Node reference, if available.</param>
		/// <param name="StringValue">String representation of value to set.</param>
		/// <returns>If the parameter could be set (true), or if the value could not be parsed or its value was invalid (false).</returns>
		public abstract Task<bool> SetStringValue(IThingReference Node, string StringValue);

		/// <summary>
		/// Gets the string value of the control parameter.
		/// </summary>
		/// <param name="Node">Node reference, if available.</param>
		/// <returns>String representation of the value.</returns>
		public abstract Task<string> GetStringValue(IThingReference Node);

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return
				this.GetType() == obj.GetType() &&
				this.name == ((ControlParameter)obj).name;
		}

		/// <summary>
		/// <see cref="Object.GetHashCode"/>
		/// </summary>
		public override int GetHashCode()
		{
			return this.name.GetHashCode();
		}

		/// <summary>
		/// Exports the field to a data form.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Node">Node reference, if available.</param>
		public virtual async Task ExportToForm(XmlWriter Output, IThingReference Node)
		{
			string StringValue = await this.GetStringValue(Node);

			Output.WriteStartElement("field");
			Output.WriteAttributeString("var", this.name);
			Output.WriteAttributeString("type", this.FormFieldType);
			Output.WriteAttributeString("label", this.label);

			Output.WriteElementString("desc", this.description);
			Output.WriteElementString("value", StringValue ?? string.Empty);

			await this.ExportValidationRules(Output, Node);

			Output.WriteElementString("xdd", "notSame", null, string.Empty);
			Output.WriteEndElement();
		}

		/// <summary>
		/// Data form field type.
		/// </summary>
		public virtual string FormFieldType
		{
			get { return "text-single"; }
		}

		/// <summary>
		/// Exports form validation rules for the parameter.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Node">Node reference, if available.</param>
		public abstract Task ExportValidationRules(XmlWriter Output, IThingReference Node);

	}
}
