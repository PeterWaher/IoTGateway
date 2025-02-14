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
using Waher.Script.Constants;

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
		private readonly ReportAction[] content;

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
			List<ReportAction> Content = new List<ReportAction>();
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
							if (!(N2 is XmlElement E2))
								continue;

							switch (N2.LocalName)
							{
								case "Boolean":
									ParseReportParameter(E2, out string Page, out string Name,
										out string Label, out string Description, out bool Required);

									bool? DefaultBooleanValue;

									if (E2.HasAttribute("default"))
										DefaultBooleanValue = XML.Attribute(E2, "default", false);
									else
										DefaultBooleanValue = null;

									Parameters.Add(new BooleanParameter(Page, Name, Label,
										Description, Required, DefaultBooleanValue));
									break;

								case "Color":
									ParseReportParameterWithOptions(E2, out Page, out Name,
										out Label, out Description, out Required,
										out bool RestrictToOptions, out ParameterOption[] Options);

									bool AlphaChannel = XML.Attribute(E2, "alpha", false);
									string DefaultStringValue;

									if (E2.HasAttribute("default"))
										DefaultStringValue = XML.Attribute(E2, "default");
									else
										DefaultStringValue = null;

									Parameters.Add(new ColorParameter(Page, Name, Label, Description,
										Required, RestrictToOptions, Options, DefaultStringValue, AlphaChannel));
									break;

								case "Date":
									ParseReportParameterWithOptions(E2, out Page, out Name,
										out Label, out Description, out Required,
										out RestrictToOptions, out Options);

									DateTime? DefaultDateTimeValue;
									DateTime? MinDateTimeValue;
									DateTime? MaxDateTimeValue;

									if (E2.HasAttribute("default"))
										DefaultDateTimeValue = XML.Attribute(E2, "default", DateTime.MinValue);
									else
										DefaultDateTimeValue = null;

									if (E2.HasAttribute("min"))
										MinDateTimeValue = XML.Attribute(E2, "min", DateTime.MinValue);
									else
										MinDateTimeValue = null;

									if (E2.HasAttribute("max"))
										MaxDateTimeValue = XML.Attribute(E2, "max", DateTime.MinValue);
									else
										MaxDateTimeValue = null;

									Parameters.Add(new DateParameter(Page, Name, Label, Description,
										Required, RestrictToOptions, Options, DefaultDateTimeValue,
										MinDateTimeValue, MaxDateTimeValue));
									break;

								case "DateTime":
									ParseReportParameterWithOptions(E2, out Page, out Name,
										out Label, out Description, out Required,
										out RestrictToOptions, out Options);

									if (E2.HasAttribute("default"))
										DefaultDateTimeValue = XML.Attribute(E2, "default", DateTime.MinValue);
									else
										DefaultDateTimeValue = null;

									if (E2.HasAttribute("min"))
										MinDateTimeValue = XML.Attribute(E2, "min", DateTime.MinValue);
									else
										MinDateTimeValue = null;

									if (E2.HasAttribute("max"))
										MaxDateTimeValue = XML.Attribute(E2, "max", DateTime.MinValue);
									else
										MaxDateTimeValue = null;

									Parameters.Add(new DateTimeParameter(Page, Name, Label, Description,
										Required, RestrictToOptions, Options, DefaultDateTimeValue,
										MinDateTimeValue, MaxDateTimeValue));
									break;

								case "Double":
									ParseReportParameterWithOptions(E2, out Page, out Name,
										out Label, out Description, out Required,
										out RestrictToOptions, out Options);

									double? DefaultDoubleValue;
									double? MinDoubleValue;
									double? MaxDoubleValue;

									if (E2.HasAttribute("default"))
										DefaultDoubleValue = XML.Attribute(E2, "default", Double.MinValue);
									else
										DefaultDoubleValue = null;

									if (E2.HasAttribute("min"))
										MinDoubleValue = XML.Attribute(E2, "min", 0.0);
									else
										MinDoubleValue = null;

									if (E2.HasAttribute("max"))
										MaxDoubleValue = XML.Attribute(E2, "max", 0.0);
									else
										MaxDoubleValue = null;

									Parameters.Add(new DoubleParameter(Page, Name, Label, Description,
										Required, RestrictToOptions, Options, DefaultDoubleValue,
										MinDoubleValue, MaxDoubleValue));
									break;

								case "Fixed":
									ParseReportParameter(E2, out Page, out Name,
										out Label, out Description, out Required);

									List<string> Values = new List<string>();

									foreach (XmlNode N3 in E2.ChildNodes)
									{
										if (N3.LocalName == "Value")
											Values.Add(N3.InnerText);
									}

									Parameters.Add(new FixedParameter(Page, Name, Label, Description,
										Required, Values.ToArray()));
									break;

								case "Int8":
									ParseReportParameterWithOptions(E2, out Page, out Name,
										out Label, out Description, out Required,
										out RestrictToOptions, out Options);

									sbyte? DefaultInt8Value;
									sbyte? MinInt8Value;
									sbyte? MaxInt8Value;

									if (E2.HasAttribute("default"))
										DefaultInt8Value = (sbyte)XML.Attribute(E2, "default", 0);
									else
										DefaultInt8Value = null;

									if (E2.HasAttribute("min"))
										MinInt8Value = (sbyte)XML.Attribute(E2, "min", 0);
									else
										MinInt8Value = null;

									if (E2.HasAttribute("max"))
										MaxInt8Value = (sbyte)XML.Attribute(E2, "max", 0);
									else
										MaxInt8Value = null;

									Parameters.Add(new Int8Parameter(Page, Name, Label, Description,
										Required, RestrictToOptions, Options, DefaultInt8Value,
										MinInt8Value, MaxInt8Value));
									break;

								case "Int16":
									ParseReportParameterWithOptions(E2, out Page, out Name,
										out Label, out Description, out Required,
										out RestrictToOptions, out Options);

									short? DefaultInt16Value;
									short? MinInt16Value;
									short? MaxInt16Value;

									if (E2.HasAttribute("default"))
										DefaultInt16Value = (short)XML.Attribute(E2, "default", 0);
									else
										DefaultInt16Value = null;

									if (E2.HasAttribute("min"))
										MinInt16Value = (short)XML.Attribute(E2, "min", 0);
									else
										MinInt16Value = null;

									if (E2.HasAttribute("max"))
										MaxInt16Value = (short)XML.Attribute(E2, "max", 0);
									else
										MaxInt16Value = null;

									Parameters.Add(new Int16Parameter(Page, Name, Label, Description,
										Required, RestrictToOptions, Options, DefaultInt16Value,
										MinInt16Value, MaxInt16Value));
									break;

								case "Int32":
									ParseReportParameterWithOptions(E2, out Page, out Name,
										out Label, out Description, out Required,
										out RestrictToOptions, out Options);

									int? DefaultInt32Value;
									int? MinInt32Value;
									int? MaxInt32Value;

									if (E2.HasAttribute("default"))
										DefaultInt32Value = XML.Attribute(E2, "default", 0);
									else
										DefaultInt32Value = null;

									if (E2.HasAttribute("min"))
										MinInt32Value = XML.Attribute(E2, "min", 0);
									else
										MinInt32Value = null;

									if (E2.HasAttribute("max"))
										MaxInt32Value = XML.Attribute(E2, "max", 0);
									else
										MaxInt32Value = null;

									Parameters.Add(new Int32Parameter(Page, Name, Label, Description,
										Required, RestrictToOptions, Options, DefaultInt32Value,
										MinInt32Value, MaxInt32Value));
									break;

								case "Int64":
									ParseReportParameterWithOptions(E2, out Page, out Name,
										out Label, out Description, out Required,
										out RestrictToOptions, out Options);

									long? DefaultInt64Value;
									long? MinInt64Value;
									long? MaxInt64Value;

									if (E2.HasAttribute("default"))
										DefaultInt64Value = XML.Attribute(E2, "default", 0L);
									else
										DefaultInt64Value = null;

									if (E2.HasAttribute("min"))
										MinInt64Value = XML.Attribute(E2, "min", 0L);
									else
										MinInt64Value = null;

									if (E2.HasAttribute("max"))
										MaxInt64Value = XML.Attribute(E2, "max", 0L);
									else
										MaxInt64Value = null;

									Parameters.Add(new Int64Parameter(Page, Name, Label, Description,
										Required, RestrictToOptions, Options, DefaultInt64Value,
										MinInt64Value, MaxInt64Value));
									break;

								case "Jid":
									ParseReportParameterWithOptions(E2, out Page, out Name,
										out Label, out Description, out Required,
										out RestrictToOptions, out Options);

									if (E2.HasAttribute("default"))
										DefaultStringValue = XML.Attribute(E2, "default");
									else
										DefaultStringValue = null;

									Parameters.Add(new JidParameter(Page, Name, Label, Description,
										Required, RestrictToOptions, Options, DefaultStringValue));
									break;

								case "Jids":
									ParseReportParameterWithOptionsAndDefaultValues(E2,
										out Page, out Name, out Label, out Description, out Required,
										out RestrictToOptions, out Options, out string[] DefaultValues,
										out ushort? MinCount, out ushort? MaxCount);

									Parameters.Add(new JidsParameter(Page, Name, Label, Description, Required,
										RestrictToOptions, Options, DefaultValues, MinCount, MaxCount));
									break;

								case "Media":
									ParseReportParameter(E2, out Page, out Name, out Label,
										out Description, out Required);

									string Url = XML.Attribute(E2, "url");
									string ContentType = XML.Attribute(E2, "contentType");
									ushort? Width;
									ushort? Height;

									if (E2.HasAttribute("width"))
										Width = (ushort)XML.Attribute(E2, "width", 0);
									else
										Width = null;

									if (E2.HasAttribute("height"))
										Height = (ushort)XML.Attribute(E2, "height", 0);
									else
										Height = null;

									Parameters.Add(new MediaParameter(Page, Name, Label, Description, Required,
										Url, ContentType, Width, Height));
									break;

								case "Password":
									ParseReportParameter(E2, out Page, out Name,
										out Label, out Description, out Required);

									if (E2.HasAttribute("default"))
										DefaultStringValue = XML.Attribute(E2, "default");
									else
										DefaultStringValue = null;

									Parameters.Add(new PasswordParameter(Page, Name, Label, Description,
										Required, DefaultStringValue));
									break;

								case "String":
									ParseReportParameterWithOptions(E2, out Page, out Name,
										out Label, out Description, out Required,
										out RestrictToOptions, out Options);

									if (E2.HasAttribute("default"))
										DefaultStringValue = XML.Attribute(E2, "default");
									else
										DefaultStringValue = null;

									string Pattern = XML.Attribute(E2, "pattern");

									Parameters.Add(new StringParameter(Page, Name, Label, Description,
										Required, RestrictToOptions, Options, DefaultStringValue, Pattern));
									break;

								case "Text":
									ParseReportParameterWithOptionsAndDefaultValues(E2,
										out Page, out Name, out Label, out Description, out Required,
										out RestrictToOptions, out Options, out DefaultValues,
										out MinCount, out MaxCount);

									ContentType = XML.Attribute(E2, "contentType");

									Parameters.Add(new TextParameter(Page, Name, Label, Description, Required,
										RestrictToOptions, Options, ContentType, DefaultValues, MinCount, MaxCount));
									break;

								case "Time":
									ParseReportParameterWithOptions(E2, out Page, out Name,
										out Label, out Description, out Required,
										out RestrictToOptions, out Options);

									TimeSpan? DefaultTimeSpanValue;
									TimeSpan? MinTimeSpanValue;
									TimeSpan? MaxTimeSpanValue;

									if (E2.HasAttribute("default"))
										DefaultTimeSpanValue = XML.Attribute(E2, "default", TimeSpan.Zero);
									else
										DefaultTimeSpanValue = null;

									if (E2.HasAttribute("min"))
										MinTimeSpanValue = XML.Attribute(E2, "min", TimeSpan.Zero);
									else
										MinTimeSpanValue = null;

									if (E2.HasAttribute("max"))
										MaxTimeSpanValue = XML.Attribute(E2, "max", TimeSpan.Zero);
									else
										MaxTimeSpanValue = null;

									Parameters.Add(new TimeParameter(Page, Name, Label, Description,
										Required, RestrictToOptions, Options, DefaultTimeSpanValue,
										MinTimeSpanValue, MaxTimeSpanValue));
									break;

								case "Uri":
									ParseReportParameterWithOptions(E2, out Page, out Name,
										out Label, out Description, out Required,
										out RestrictToOptions, out Options);

									if (E2.HasAttribute("default"))
										DefaultStringValue = XML.Attribute(E2, "default");
									else
										DefaultStringValue = null;

									Parameters.Add(new UriParameter(Page, Name, Label, Description,
										Required, RestrictToOptions, Options, DefaultStringValue));
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
						throw new NotSupportedException("Unrecognized report element: " + N.LocalName);
				}
			}

			this.privileges = Privileges.ToArray();
			this.parameters = Parameters.ToArray();
			this.content = Content.ToArray();
		}

		private static void ParseReportParameter(XmlElement E, out string Page,
			out string Name, out string Label, out string Description, out bool Required)
		{
			Page = XML.Attribute(E, "page");
			Name = XML.Attribute(E, "name");
			Label = XML.Attribute(E, "label");
			Description = XML.Attribute(E, "description");
			Required = XML.Attribute(E, "required", false);
		}

		private static void ParseReportParameterWithOptions(XmlElement E, out string Page,
			out string Name, out string Label, out string Description, out bool Required,
			out bool RestrictToOptions, out ParameterOption[] Options)
		{
			ParseReportParameter(E, out Page, out Name, out Label, out Description, out Required);

			RestrictToOptions = XML.Attribute(E, "restrictToOptions", false);

			List<ParameterOption> Options2 = new List<ParameterOption>();
			string Label2;
			string Value;

			foreach (XmlNode N in E.ChildNodes)
			{
				if (!(N is XmlElement E2))
					continue;

				if (E2.LocalName == "Option")
				{
					Label2 = XML.Attribute(E2, "label");
					Value = XML.Attribute(E2, "value");

					Options2.Add(new ParameterOption(Label2, Value));
				}
			}

			Options = Options2.ToArray();
		}

		private static void ParseReportParameterWithOptionsAndDefaultValues(XmlElement E, out string Page,
			out string Name, out string Label, out string Description, out bool Required,
			out bool RestrictToOptions, out ParameterOption[] Options, out string[] DefaultValues,
			out ushort? MinCount, out ushort? MaxCount)
		{
			ParseReportParameterWithOptions(E, out Page, out Name, out Label, out Description, out Required,
				out RestrictToOptions, out Options);

			if (E.HasAttribute("minCount"))
				MinCount = (ushort)XML.Attribute(E, "minCount", 0);
			else
				MinCount = null;

			if (E.HasAttribute("maxCount"))
				MaxCount = (ushort)XML.Attribute(E, "maxCount", 0);
			else
				MaxCount = null;

			List<string> Values = new List<string>();

			foreach (XmlNode N in E.ChildNodes)
			{
				if (N.LocalName == "DefaultValue")
					Values.Add(N.InnerText);
			}

			DefaultValues = Values.ToArray();
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
