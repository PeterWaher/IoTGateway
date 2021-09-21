using System;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Client.WPF.Controls.Report
{
	/// <summary>
	/// Contains information about a report object.
	/// </summary>
	public class ReportObject : ReportElement
	{
		private readonly object @object;
		private readonly byte[] binary;
		private readonly string contentType;

		/// <summary>
		/// Contains information about a report object.
		/// </summary>
		/// <param name="Object">Decoded Object</param>
		/// <param name="Binary">Binary representation</param>
		/// <param name="ContentType">Content-Type</param>
		public ReportObject(object Object, byte[] Binary, string ContentType)
		{
			this.@object = Object;
			this.binary = Binary;
			this.contentType = ContentType;
		}

		/// <summary>
		/// Contains information about a report object.
		/// </summary>
		/// <param name="Xml">XML Definition.</param>
		public ReportObject(XmlElement Xml)
		{
			this.binary = Convert.FromBase64String(Xml.InnerText);
			this.contentType = XML.Attribute(Xml, "contentType");
			this.@object = InternetContent.Decode(this.contentType, this.binary, null);
		}

		/// <summary>
		/// Decoded Object
		/// </summary>
		public object Object => this.@object;

		/// <summary>
		/// Binary representation
		/// </summary>
		public byte[] Binary => this.binary;

		/// <summary>
		/// Content-Type
		/// </summary>
		public string ContentType => this.contentType;

		/// <summary>
		/// Exports element to XML
		/// </summary>
		/// <param name="Output">XML output</param>
		public override void ExportXml(XmlWriter Output)
		{
			Output.WriteStartElement("Object");
			Output.WriteAttributeString("contentType", this.contentType);
			Output.WriteValue(Convert.ToBase64String(this.binary));
			Output.WriteEndElement();
		}
	}
}
