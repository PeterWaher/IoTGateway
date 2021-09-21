using System;
using System.Xml;

namespace Waher.Client.WPF.Controls.Report
{
	/// <summary>
	/// Abstract base class for report elements.
	/// </summary>
	public abstract class ReportElement
	{
		/// <summary>
		/// Exports element to XML
		/// </summary>
		/// <param name="Output">XML output</param>
		public abstract void ExportXml(XmlWriter Output);
	}
}
