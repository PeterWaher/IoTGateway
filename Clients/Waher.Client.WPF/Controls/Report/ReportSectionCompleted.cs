using System;
using System.Xml;

namespace Waher.Client.WPF.Controls.Report
{
	/// <summary>
	/// Completion of a section.
	/// </summary>
	public class ReportSectionCompleted : ReportElement
	{
		/// <summary>
		/// Completion of a section.
		/// </summary>
		public ReportSectionCompleted()
		{
		}

		/// <summary>
		/// Exports element to XML
		/// </summary>
		/// <param name="Output">XML output</param>
		public override void ExportXml(XmlWriter Output)
		{
			Output.WriteElementString("SectionEnd", string.Empty);
		}
	}
}
