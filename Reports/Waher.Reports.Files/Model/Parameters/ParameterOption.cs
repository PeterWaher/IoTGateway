using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Reports.Model.Attributes;
using Waher.Script;

namespace Waher.Reports.Files.Model.Parameters
{
	/// <summary>
	/// Represents a string-valued option.
	/// </summary>
	public class ParameterOption
	{
		private readonly ReportStringAttribute label;
		private readonly ReportStringAttribute value;

		/// <summary>
		/// Represents a string-valued option.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public ParameterOption(XmlElement Xml)
			: base()
		{
			this.label = new ReportStringAttribute(Xml, "label");
			this.value = new ReportStringAttribute(Xml, "value");
		}

		/// <summary>
		/// Evaluates report option attributes.
		/// </summary>
		/// <param name="Variables">Report variables.</param>
		/// <returns>Label-Value pair.</returns>
		public async Task<KeyValuePair<string, string>> GetTag(Variables Variables)
		{
			return new KeyValuePair<string, string>(
				await this.label.Evaluate(Variables),
				await this.value.Evaluate(Variables));
		}
	}
}