using System;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Client.WPF.Controls.Report
{
	/// <summary>
	/// Contains information about a report object.
	/// </summary>
	/// <remarks>
	/// Contains information about a report object.
	/// </remarks>
	/// <param name="Object">Decoded Object</param>
	/// <param name="Binary">Binary representation</param>
	/// <param name="ContentType">Content-Type</param>
	public class ReportObject(object Object, byte[] Binary, string ContentType) 
		: ReportElement
	{
		private readonly object @object = Object;
		private readonly byte[] binary = Binary;
		private readonly string contentType = ContentType;

		/// <summary>
		/// Contains information about a report object.
		/// </summary>
		/// <param name="Xml">XML Definition.</param>
		public static async Task<ReportObject> CreateAsync(XmlElement Xml)
		{
			byte[] Binary = Convert.FromBase64String(Xml.InnerText);
			string ContentType = XML.Attribute(Xml, "contentType");
			ContentResponse Object = await InternetContent.DecodeAsync(ContentType, Binary, null);
			Object.AssertOk();

			return new ReportObject(Object.Decoded, Binary, ContentType);
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
