using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using Waher.Content.Xsl;
using Waher.Networking.XMPP.Concentrator;
using Waher.Networking.XMPP.DataForms;
using Waher.Runtime.Language;
using Waher.Script;

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
		private readonly string title;
		private readonly string[] privileges;
		private readonly ReportParameter[] parameters;

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

			List<ReportParameter> Parameters = new List<ReportParameter>();
			List<string> Privileges = new List<string>();
			this.title = string.Empty;

			foreach (XmlNode N in Doc.DocumentElement.ChildNodes)
			{
				switch (N.LocalName)
				{
					case "Title":
						this.title = N.InnerText;
						break;

					case "Privilege":
						Privileges.Add(N.InnerText);
						break;

					case "Parameters":
						foreach (XmlNode N2 in N.ChildNodes)
						{
							switch (N2.LocalName)
							{
								case "Boolean":
								case "Color":
								case "Date":
								case "DateTime":
								case "Double":
								case "Fixed":
								case "Int8":
								case "Int16":
								case "Int32":
								case "Int64":
								case "Jid":
								case "Jids":
								case "Media":
								case "Password":
								case "String":
								case "Text":
								case "Time":
								case "Uri":
									// TODO
									break;
							}
						}
						break;
				}
			}

			this.privileges = Privileges.ToArray();
			this.parameters = Parameters.ToArray();
		}

		/// <summary>
		/// File name of report.
		/// </summary>
		public string FileName => this.fileName;

		/// <summary>
		/// Title of report.
		/// </summary>
		public string Title => this.title;

		/// <summary>
		/// Privileges required to execute the report.
		/// </summary>
		public string[] Privileges => this.privileges;

		/// <summary>
		/// Report parameters.
		/// </summary>
		public ReportParameter[] Parameters => this.parameters;

		/// <summary>
		/// Populates a data form with parameters for the object.
		/// </summary>
		/// <param name="Parameters">Data form to host all editable parameters.</param>
		/// <param name="Language">Current language.</param>
		/// <param name="Values">Variable values</param>
		public async Task PopulateForm(DataForm Parameters, Language Language, Variables Values)
		{
			object Value;

			foreach (ReportParameter Parameter in this.parameters)
			{
				if (Values.TryGetVariable(Parameter.Name, out Variable v))
					Value = v.ValueObject;
				else
					Value = null;

				await Parameter.PopulateForm(Parameters, Language, Value);
			}
		}

		/// <summary>
		/// Sets the parameters of the object, based on contents in the data form.
		/// </summary>
		/// <param name="Parameters">Data form with parameter values.</param>
		/// <param name="Language">Current language.</param>
		/// <param name="OnlySetChanged">If only changed parameters are to be set.</param>
		/// <param name="Values">Variable values.</param>
		/// <returns>Any errors encountered, or null if parameters was set properly.</returns>
		public async Task<SetEditableFormResult> SetParameters(DataForm Parameters, Language Language, 
			bool OnlySetChanged, Variables Values)
		{
			SetEditableFormResult Result = new SetEditableFormResult();

			foreach (ReportParameter Parameter in this.parameters)
				await Parameter.SetParameter(Parameters, Language, OnlySetChanged, Values, Result);

			return Result;
		}
	}
}
