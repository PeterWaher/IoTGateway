using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using Waher.Content.Xml;
using Waher.Content.Xsl;
using Waher.Networking.XMPP.Concentrator;
using Waher.Networking.XMPP.DataForms;
using Waher.Reports.Files.Model.Actions;
using Waher.Reports.Files.Model.Parameters;
using Waher.Runtime.Language;
using Waher.Script;
using Waher.Things;

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
		private readonly string @namespace;
		private readonly string origin;
		private readonly string[] privileges;
		private readonly ReportParameter[] parameters;
		private readonly ReportAction[] content;

		/// <summary>
		/// Contains a parsed report, as defined in a report file.
		/// </summary>
		/// <param name="FileName">File name of report.</param>
		public ReportFile(string FileName)
		{
			this.fileName = FileName;

			XmlDocument Doc = XML.LoadFromFile(FileName, true);

			XSL.Validate(Path.GetFileName(FileName), Doc, ReportFileLocalName, ReportFileNamespace, schema);

			List<ReportParameter> Parameters = new List<ReportParameter>();
			List<string> Privileges = new List<string>();
			List<ReportAction> Content = new List<ReportAction>();
			this.title = string.Empty;
			this.@namespace = XML.Attribute(Doc.DocumentElement, "namespace");
			this.origin = XML.Attribute(Doc.DocumentElement, "originVariable", "origin");

			foreach (XmlNode N in Doc.DocumentElement.ChildNodes)
			{
				if (!(N is XmlElement E))
					continue;

				switch (E.LocalName)
				{
					case "Title":
						this.title = E.InnerText;
						break;

					case "Privilege":
						Privileges.Add(E.InnerText);
						break;

					case "Parameters":
						foreach (XmlNode N2 in E.ChildNodes)
						{
							if (!(N2 is XmlElement E2))
								continue;

							switch (E2.LocalName)
							{
								case "Boolean":
									Parameters.Add(new BooleanParameter(E2));
									break;

								case "Color":
									Parameters.Add(new ColorParameter(E2));
									break;

								case "Date":
									Parameters.Add(new DateParameter(E2));
									break;

								case "DateTime":
									Parameters.Add(new DateTimeParameter(E2));
									break;

								case "Double":
									Parameters.Add(new DoubleParameter(E2));
									break;

								case "Fixed":
									Parameters.Add(new FixedParameter(E2));
									break;

								case "Int8":
									Parameters.Add(new Int8Parameter(E2));
									break;

								case "Int16":
									Parameters.Add(new Int16Parameter(E2));
									break;

								case "Int32":
									Parameters.Add(new Int32Parameter(E2));
									break;

								case "Int64":
									Parameters.Add(new Int64Parameter(E2));
									break;

								case "Jid":
									Parameters.Add(new JidParameter(E2));
									break;

								case "Jids":
									Parameters.Add(new JidsParameter(E2));
									break;

								case "Media":
									Parameters.Add(new MediaParameter(E2));
									break;

								case "Password":
									Parameters.Add(new PasswordParameter(E2));
									break;

								case "String":
									Parameters.Add(new StringParameter(E2));
									break;

								case "Text":
									Parameters.Add(new TextParameter(E2));
									break;

								case "Time":
									Parameters.Add(new TimeParameter(E2));
									break;

								case "Uri":
									Parameters.Add(new UriParameter(E2));
									break;

								default:
									throw new NotSupportedException("Unrecognized parameter type: " + N2.LocalName);
							}
						}
						break;

					case "Content":
						this.ParseActions(Content, N);
						break;

					default:
						throw new NotSupportedException("Unrecognized report element: " + E.LocalName);
				}
			}

			this.privileges = Privileges.ToArray();
			this.parameters = Parameters.ToArray();
			this.content = Content.ToArray();
		}

		internal ReportAction[] ParseActions(XmlNode Container)
		{
			List<ReportAction> Actions = new List<ReportAction>();
			this.ParseActions(Actions, Container, out _, out _);
			return Actions.ToArray();
		}

		internal ReportAction[] ParseActions(XmlNode Container,
			out ReportAction[] Catch, out ReportAction[] Finally)
		{
			List<ReportAction> Actions = new List<ReportAction>();
			this.ParseActions(Actions, Container, out Catch, out Finally);
			return Actions.ToArray();
		}

		private void ParseActions(List<ReportAction> Actions, XmlNode Container)
		{
			this.ParseActions(Actions, Container, out _, out _);
		}

		private void ParseActions(List<ReportAction> Actions, XmlNode Container,
			out ReportAction[] Catch, out ReportAction[] Finally)
		{
			List<ReportAction> Catch2 = null;
			List<ReportAction> Finally2 = null;

			foreach (XmlNode N in Container.ChildNodes)
			{
				if (!(N is XmlElement E))
					continue;

				switch (E.LocalName)
				{
					case "CreateTable":
						Actions.Add(new CreateTable(E, this));
						break;

					case "TableRecords":
						Actions.Add(new TableRecords(E, this));
						break;

					case "TableComplete":
						Actions.Add(new TableComplete(E, this));
						break;

					case "Script":
						Actions.Add(new ReportScript(E, this));
						break;

					case "Object":
						Actions.Add(new ReportObject(E, this));
						break;

					case "Message":
						Actions.Add(new Message(E, this));
						break;

					case "Status":
						Actions.Add(new Status(E, this));
						break;

					case "Section":
						Actions.Add(new Section(E, this));
						break;

					case "Conditional":
						Actions.Add(new Conditional(E, this));
						break;

					case "While":
						Actions.Add(new While(E, this));
						break;

					case "Do":
						Actions.Add(new Do(E, this));
						break;

					case "ForEach":
						Actions.Add(new ForEach(E, this));
						break;

					case "Try":
						Actions.Add(new Try(E, this));
						break;

					case "Catch":
						Catch2 ??= new List<ReportAction>();
						this.ParseActions(Catch2, N);
						break;

					case "Finally":
						Finally2 ??= new List<ReportAction>();
						this.ParseActions(Finally2, N);
						break;

					default:
						throw new NotSupportedException("Unrecognized report action: " + E.LocalName);
				}
			}

			Catch = Catch2?.ToArray();
			Finally = Finally2?.ToArray();
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
		/// Content of report.
		/// </summary>
		public ReportAction[] Content => this.content;

		/// <summary>
		/// Populates a data form with parameters for the object.
		/// </summary>
		/// <param name="Parameters">Data form to host all editable parameters.</param>
		/// <param name="Language">Current language.</param>
		/// <param name="Variables">Report variables.</param>
		/// <param name="Origin">Origin of request.</param>
		public async Task PopulateForm(DataForm Parameters, Language Language, 
			Variables Variables, IRequestOrigin Origin)
		{
			if (!string.IsNullOrEmpty(this.origin))
				Variables[this.origin] = Origin;

			foreach (ReportParameter Parameter in this.parameters)
				await Parameter.PopulateForm(Parameters, Language, Variables);
		}

		/// <summary>
		/// Sets the parameters of the object, based on contents in the data form.
		/// </summary>
		/// <param name="Parameters">Data form with parameter values.</param>
		/// <param name="Language">Current language.</param>
		/// <param name="OnlySetChanged">If only changed parameters are to be set.</param>
		/// <param name="Variables">Variable values.</param>
		/// <param name="Origin">Origin of request.</param>
		/// <returns>Any errors encountered, or null if parameters was set properly.</returns>
		public async Task<SetEditableFormResult> SetParameters(DataForm Parameters, Language Language,
			bool OnlySetChanged, Variables Variables, IRequestOrigin Origin)
		{
			SetEditableFormResult Result = new SetEditableFormResult();

			if (!string.IsNullOrEmpty(this.origin))
				Variables[this.origin] = Origin;

			foreach (ReportParameter Parameter in this.parameters)
				await Parameter.SetParameter(Parameters, Language, OnlySetChanged, Variables, Result);

			return Result;
		}

		/// <summary>
		/// Executes the report action.
		/// </summary>
		/// <param name="State">State of the report execution.</param>
		/// <returns>If the action was executed.</returns>
		public async Task<bool> Execute(ReportState State)
		{
			if (!string.IsNullOrEmpty(this.@namespace))
				State.Namespace = await State.Language.GetNamespaceAsync(this.@namespace);

			if (!string.IsNullOrEmpty(this.origin))
				State.Variables[this.origin] = State.Origin;

			if (!string.IsNullOrEmpty(this.title))
				await State.Query.SetTitle(this.title);

			foreach (ReportAction Action in this.content)
				await Action.Execute(State);

			return true;
		}

	}
}
