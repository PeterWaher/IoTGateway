using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Reports.Model.Attributes;
using Waher.Runtime.Collections;
using Waher.Script;

namespace Waher.Reports.Files.Model.Parameters
{
	/// <summary>
	/// Represents a parameter with possible options on a command.
	/// </summary>
	public abstract class ReportParameterWithOptions : ReportParameter
	{
		private readonly ReportBooleanAttribute restrictToOptions;
		private readonly IParameterOptions[] options;

		/// <summary>
		/// Represents a parameter with possible options on a command.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public ReportParameterWithOptions(XmlElement Xml)
			: base(Xml)
		{
			this.restrictToOptions = new ReportBooleanAttribute(Xml, "restrictToOptions");

			ChunkedList<IParameterOptions> Options2 = new ChunkedList<IParameterOptions>();

			foreach (XmlNode N in Xml.ChildNodes)
			{
				if (!(N is XmlElement E2))
					continue;

				switch (E2.LocalName)
				{
					case "Option":
						Options2.Add(new ParameterOption(E2));
						break;

					case "ScriptOptions":
						Options2.Add(new ParameterScriptOptions(E2));
						break;
				}
			}

			this.options = Options2.ToArray();
		}

		/// <summary>
		/// Evaluates the attributes of the report parameter with options.
		/// </summary>
		/// <param name="Variables">Report variables.</param>
		/// <returns>Parameter attributes</returns>
		public async Task<ReportParameterWithOptionsAttributes> GetReportParameterWithOptionsAttributes(Variables Variables)
		{
			ReportParameterAttributes Base = await this.GetReportParameterAttributes(Variables);
			ChunkedList<KeyValuePair<string, string>> Options = new ChunkedList<KeyValuePair<string, string>>();

			foreach (IParameterOptions Option in this.options)
				Options.AddRange(await Option.GetOptions(Variables));

			return new ReportParameterWithOptionsAttributes()
			{
				Page = Base.Page,
				Name = Base.Name,
				Label = Base.Label,
				Description = Base.Description,
				Required = Base.Required,
				RestrictToOptions = await this.restrictToOptions.Evaluate(Variables, false),
				Options = Options.ToArray()
			};
		}

	}
}
