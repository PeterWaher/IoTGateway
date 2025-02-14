using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Reports.Model.Attributes;
using Waher.Script;

namespace Waher.Reports.Files.Model.Actions
{
	/// <summary>
	/// Defines a column in a table.
	/// </summary>
	public class TableRecord
	{
		private readonly ReportObjectAttribute[] elements;
		private readonly int nrElements;

		/// <summary>
		/// Defines a column in a table.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public TableRecord(XmlElement Xml)
		{
			List<ReportObjectAttribute> Elements = new List<ReportObjectAttribute>();

			foreach (XmlNode N in Xml.ChildNodes)
			{
				if (N is XmlElement E && E.LocalName == "Element")
					Elements.Add(new ReportObjectAttribute(E, null));
			}

			this.elements = Elements.ToArray();
			this.nrElements = this.elements.Length;
		}

		/// <summary>
		/// Number of elements in record.
		/// </summary>
		public int NrElements => this.nrElements;

		/// <summary>
		/// Evaluates the record definition.
		/// </summary>
		/// <param name="State">State of the report execution.</param>
		/// <returns>Record</returns>
		public async Task<object[]> Evaluate(ReportState State)
		{
			object[] Result = new object[this.nrElements];
			int i;

			for (i = 0; i < this.nrElements; i++)
				Result[i] = await this.elements[i].Evaluate(State.Variables);

			return Result;
		}
	}
}
