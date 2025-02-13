using System.IO;
using System.Xml;
using System.Xml.Schema;
using Waher.Content.Xsl;

namespace Waher.Reports.Files.Model
{
	/// <summary>
	/// Contains a parsed report, as defined in a report file.
	/// </summary>
	public class ReportFile
	{
		private const string ReportFileLocalName = "Report";
		private const string ReportFileNamespace = "http://waher.se/Schema/ReportFile.xsd";

		private static readonly XmlSchema schema = XSL.LoadSchema(typeof(ReportFileNode).Namespace + ".Schema.ReportFile.xsd");

		private readonly string fileName;

		/// <summary>
		/// Contains a parsed report, as defined in a report file.
		/// </summary>
		/// <param name="FileName">File name of report.</param>
		public ReportFile(string FileName)
		{
			this.fileName = FileName;

			XmlDocument Doc = new XmlDocument()
			{
				PreserveWhitespace = true
			};
			Doc.LoadXml(FileName);

			XSL.Validate(Path.GetFileName(FileName), Doc, ReportFileLocalName, ReportFileNamespace, schema);
		}

		/// <summary>
		/// File name of report.
		/// </summary>
		public string FileName => this.fileName;
	}
}
