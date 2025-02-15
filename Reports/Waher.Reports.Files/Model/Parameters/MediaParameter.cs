using System.Threading.Tasks;
using System.Xml;
using Waher.Networking.XMPP.Concentrator;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.DataForms.FieldTypes;
using Waher.Networking.XMPP.DataForms.Layout;
using Waher.Reports.Model.Attributes;
using Waher.Runtime.Language;
using Waher.Script;

namespace Waher.Reports.Files.Model.Parameters
{
	/// <summary>
	/// Represents a media-valued parameter.
	/// </summary>
	public class MediaParameter : ReportParameter
	{
		private readonly ReportStringAttribute url;
		private readonly ReportStringAttribute contentType;
		private readonly ReportUInt16Attribute width;
		private readonly ReportUInt16Attribute height;

		/// <summary>
		/// Represents a media-valued parameter.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public MediaParameter(XmlElement Xml)
			: base(Xml)
		{
			this.url = new ReportStringAttribute(Xml, "url");
			this.contentType = new ReportStringAttribute(Xml, "contentType");
			this.width = new ReportUInt16Attribute(Xml, "width");
			this.height = new ReportUInt16Attribute(Xml, "height");
		}

		/// <summary>
		/// Populates a data form with parameters for the object.
		/// </summary>
		/// <param name="Parameters">Data form to host all editable parameters.</param>
		/// <param name="Language">Current language.</param>
		/// <param name="Variables">Report variables.</param>
		public override async Task PopulateForm(DataForm Parameters, Language Language, Variables Variables)
		{
			ReportParameterAttributes Attributes = await this.GetReportParameterAttributes(Variables);
			string Url = await this.url.Evaluate(Variables);
			string ContentType = await this.contentType.Evaluate(Variables);
			ushort? Width = this.width.IsEmpty ? null : (ushort?)await this.width.Evaluate(Variables);
			ushort? Height = this.height.IsEmpty ? null : (ushort?)await this.height.Evaluate(Variables);

			Media Media = new Media(Url, ContentType, Width, Height);
			MediaField Field = new MediaField(Attributes.Name, Attributes.Label, Media);

			Parameters.Add(Field);

			Page Page = Parameters.GetPage(Attributes.Page);
			Page.Add(Field);
		}

		/// <summary>
		/// Sets the parameters of the object, based on contents in the data form.
		/// </summary>
		/// <param name="Parameters">Data form with parameter values.</param>
		/// <param name="Language">Current language.</param>
		/// <param name="OnlySetChanged">If only changed parameters are to be set.</param>
		/// <param name="Variables">Report variables.</param>
		/// <param name="Result">Result set to return to caller.</param>
		/// <returns>Any errors encountered, or null if parameters was set properly.</returns>
		public override Task SetParameter(DataForm Parameters, Language Language, bool OnlySetChanged, Variables Variables,
			SetEditableFormResult Result)
		{
			return Task.CompletedTask;
		}

	}
}