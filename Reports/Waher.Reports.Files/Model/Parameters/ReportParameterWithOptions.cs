using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Reports.Model.Attributes;
using Waher.Script;

namespace Waher.Reports.Files.Model.Parameters
{
	/// <summary>
	/// Represents a parameter with possible options on a command.
	/// </summary>
	public abstract class ReportParameterWithOptions : ReportParameter
	{
		private readonly ReportBooleanAttribute restrictToOptions;
		private readonly ParameterOption[] options;
		private readonly int nrOptions;

		/// <summary>
		/// Represents a parameter with possible options on a command.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public ReportParameterWithOptions(XmlElement Xml)
			: base(Xml)
		{
			this.restrictToOptions = new ReportBooleanAttribute(Xml, "restrictToOptions");

			List<ParameterOption> Options2 = new List<ParameterOption>();

			foreach (XmlNode N in Xml.ChildNodes)
			{
				if (!(N is XmlElement E2))
					continue;

				if (E2.LocalName == "Option")
					Options2.Add(new ParameterOption(E2));
			}

			this.options = Options2.ToArray();
			this.nrOptions = this.options.Length;
		}

		/// <summary>
		/// Evaluates the attributes of the report parameter with options.
		/// </summary>
		/// <param name="Variables">Report variables.</param>
		/// <returns>Parameter attributes</returns>
		public async Task<ReportParameterWithOptionsAttributes> GetReportParameterWithOptionsAttributes(Variables Variables)
		{
			ReportParameterAttributes Base = await this.GetReportParameterAttributes(Variables);
			KeyValuePair<string, string>[] Options = new KeyValuePair<string, string>[this.nrOptions];

			for (int i = 0; i < this.nrOptions; i++)
				Options[i] = await this.options[i].GetTag(Variables);

			return new ReportParameterWithOptionsAttributes()
			{
				Page = Base.Page,
				Name = Base.Name,
				Label = Base.Label,
				Description = Base.Description,
				Required = Base.Required,
				RestrictToOptions = await this.restrictToOptions.Evaluate(Variables, false),
				Options = Options 
			};
		}

	}
}
