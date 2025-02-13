using System.Threading.Tasks;
using Waher.Networking.XMPP.Concentrator;
using Waher.Networking.XMPP.DataForms;
using Waher.Runtime.Language;
using Waher.Script;

namespace Waher.Reports.Files.Model
{
	/// <summary>
	/// Abstract base class for report parameters.
	/// </summary>
	public abstract class ReportParameter
	{
		/// <summary>
		/// Abstract base class for report parameters.
		/// </summary>
		/// <param name="Page">Parameter Page</param>
		/// <param name="Name">Parameter name.</param>
		/// <param name="Label">Parameter label.</param>
		/// <param name="Description">Parameter description.</param>
		/// <param name="Required">If parameter is required.</param>
		public ReportParameter(string Page, string Name, string Label, string Description,
			bool Required)
		{
			this.Page = Page;
			this.Name = Name;
			this.Label = Label;
			this.Description = Description;
			this.Required = Required;
		}

		/// <summary>
		/// Parameter page.
		/// </summary>
		public string Page { get; }

		/// <summary>
		/// Parameter name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Parameter label.
		/// </summary>
		public string Label { get; }

		/// <summary>
		/// Parameter description.
		/// </summary>
		public string Description { get; }

		/// <summary>
		/// If parameter is required.
		/// </summary>
		public bool Required { get; }

		/// <summary>
		/// Populates a data form with parameters for the object.
		/// </summary>
		/// <param name="Parameters">Data form to host all editable parameters.</param>
		/// <param name="Language">Current language.</param>
		/// <param name="Value">Value for parameter.</param>
		public abstract Task PopulateForm(DataForm Parameters, Language Language, object Value);

		/// <summary>
		/// Sets the parameters of the object, based on contents in the data form.
		/// </summary>
		/// <param name="Parameters">Data form with parameter values.</param>
		/// <param name="Language">Current language.</param>
		/// <param name="OnlySetChanged">If only changed parameters are to be set.</param>
		/// <param name="Values">Collection of parameter values.</param>
		/// <param name="Result">Result set to return to caller.</param>
		/// <returns>Any errors encountered, or null if parameters was set properly.</returns>
		public abstract Task SetParameter(DataForm Parameters, Language Language, bool OnlySetChanged, Variables Values,
		  SetEditableFormResult Result);
	}
}