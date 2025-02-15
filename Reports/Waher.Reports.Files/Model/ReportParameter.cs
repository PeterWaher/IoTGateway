using System.Threading.Tasks;
using System.Xml;
using Waher.Networking.XMPP.Concentrator;
using Waher.Networking.XMPP.DataForms;
using Waher.Reports.Files.Model.Parameters;
using Waher.Reports.Model.Attributes;
using Waher.Runtime.Language;
using Waher.Script;

namespace Waher.Reports.Files.Model
{
	/// <summary>
	/// Abstract base class for report parameters.
	/// </summary>
	public abstract class ReportParameter
	{
		private readonly ReportStringAttribute page;
		private readonly ReportStringAttribute name;
		private readonly ReportStringAttribute label;
		private readonly ReportStringAttribute description;
		private readonly ReportBooleanAttribute required;

		/// <summary>
		/// Abstract base class for report parameters.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public ReportParameter(XmlElement Xml)
		{
			this.page = new ReportStringAttribute(Xml, "page");
			this.name = new ReportStringAttribute(Xml, "name");
			this.label = new ReportStringAttribute(Xml, "label");
			this.description = new ReportStringAttribute(Xml, "description");
			this.required = new ReportBooleanAttribute(Xml, "required");
		}

		/// <summary>
		/// Name of the parameter.
		/// </summary>
		/// <param name="Variables">Report variables.</param>
		/// <returns>Parameter name</returns>
		public Task<string> GetName(Variables Variables)
		{
			return this.name.Evaluate(Variables, string.Empty);
		}

		/// <summary>
		/// If parameter is required.
		/// </summary>
		/// <param name="Variables">Report variables.</param>
		/// <returns>If required.</returns>
		public Task<bool> IsRequired(Variables Variables)
		{
			return this.required.Evaluate(Variables, false);
		}

		/// <summary>
		/// Evaluates the attributes of the report parameter.
		/// </summary>
		/// <param name="Variables">Report variables.</param>
		/// <returns>Parameter attributes</returns>
		public async Task<ReportParameterAttributes> GetReportParameterAttributes(Variables Variables)
		{
			return new ReportParameterAttributes()
			{
				Page = await this.page.Evaluate(Variables, string.Empty),
				Name = await this.name.Evaluate(Variables, string.Empty),
				Label = await this.label.Evaluate(Variables, string.Empty),
				Description = await this.description.Evaluate(Variables, string.Empty),
				Required = await this.required.Evaluate(Variables, false)
			};
		}

		/// <summary>
		/// Populates a data form with parameters for the object.
		/// </summary>
		/// <param name="Parameters">Data form to host all editable parameters.</param>
		/// <param name="Language">Current language.</param>
		/// <param name="Variables">Report variables.</param>
		public abstract Task PopulateForm(DataForm Parameters, Language Language, Variables Variables);

		/// <summary>
		/// Sets the parameters of the object, based on contents in the data form.
		/// </summary>
		/// <param name="Parameters">Data form with parameter values.</param>
		/// <param name="Language">Current language.</param>
		/// <param name="OnlySetChanged">If only changed parameters are to be set.</param>
		/// <param name="Variables">Report variables.</param>
		/// <param name="Result">Result set to return to caller.</param>
		/// <returns>Any errors encountered, or null if parameters was set properly.</returns>
		public abstract Task SetParameter(DataForm Parameters, Language Language, bool OnlySetChanged, Variables Variables,
		  SetEditableFormResult Result);
	}
}