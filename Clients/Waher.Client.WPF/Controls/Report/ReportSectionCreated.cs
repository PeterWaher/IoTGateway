using System;
using System.Xml;

namespace Waher.Client.WPF.Controls.Report
{
	/// <summary>
	/// Creation of a section.
	/// </summary>
	public class ReportSectionCreated : ReportElement
	{
		private readonly string header;

		/// <summary>
		/// Creation of a section.
		/// </summary>
		/// <param name="Header">Header</param>
		public ReportSectionCreated(string Header)
		{
			this.header = Header;
		}

		/// <summary>
		/// Creation of a section.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		public ReportSectionCreated(XmlElement Xml)
		{
			this.header = Xml.InnerText;
		}

		/// <summary>
		/// Section header
		/// </summary>
		public string Header
		{
			get { return this.header; }
		}

		/// <summary>
		/// Exports element to XML
		/// </summary>
		/// <param name="Output">XML output</param>
		public override void ExportXml(XmlWriter Output)
		{
			Output.WriteElementString("SectionStart", this.header);
		}
	}
}
