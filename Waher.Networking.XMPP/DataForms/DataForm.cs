using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Networking.XMPP.DataForms.DataTypes;
using Waher.Networking.XMPP.DataForms.FieldTypes;
using Waher.Networking.XMPP.DataForms.ValidationMethods;

namespace Waher.Networking.XMPP.DataForms
{
	public enum FormType
	{
		/// <summary>
		/// Data Form
		/// </summary>
		Form,

		/// <summary>
		/// Form cancellation
		/// </summary>
		Cancel,

		/// <summary>
		/// Form Result
		/// </summary>
		Result,

		/// <summary>
		/// Form submission
		/// </summary>
		Submit,

		/// <summary>
		/// Undefined form type.
		/// </summary>
		Undefined
	}

	/// <summary>
	/// Implements support for data forms. Data Forms are defined in the following XEPs:
	/// 
	/// XEP-0004: Data Forms:
	/// http://xmpp.org/extensions/xep-0004.html
	/// 
	/// XEP-0122: Data Forms Validation:
	/// http://xmpp.org/extensions/xep-0122.html
	/// 
	/// XEP-0331: Data Forms - Color Field Types
	/// http://xmpp.org/extensions/xep-0331.html
	/// </summary>
	public class DataForm
	{
		private FormType type;
		private Field[] fields;
		private string[] instructions;
		private string title = string.Empty;

		/// <summary>
		/// Implements support for data forms. Data Forms are defined in the following XEPs:
		/// 
		/// XEP-0004: Data Forms:
		/// http://xmpp.org/extensions/xep-0004.html
		/// 
		/// XEP-0122: Data Forms Validation:
		/// http://xmpp.org/extensions/xep-0122.html
		/// </summary>
		/// <param name="X">Data Form definition.</param>
		public DataForm(XmlElement X)
		{
			List<string> Instructions = new List<string>();
			List<Field> Fields = new List<Field>();

			switch (XmppClient.XmlAttribute(X, "type").ToLower())
			{
				case "cancel":
					this.type = FormType.Cancel;
					break;

				case "form":
					this.type = FormType.Form;
					break;

				case "result":
					this.type = FormType.Result;
					break;

				case "submit":
					this.type = FormType.Submit;
					break;

				default:
					this.type = FormType.Undefined;
					break;
			}

			foreach (XmlNode N in X.ChildNodes)
			{
				switch (N.LocalName)
				{
					case "instructions":
						Instructions.Add(N.InnerText.Trim());
						break;

					case "title":
						this.title = N.InnerText.Trim();
						break;

					case "field":
						XmlElement E = (XmlElement)N;
						string Label = XmppClient.XmlAttribute(E, "label");
						string Type = XmppClient.XmlAttribute(E, "type");
						string Var = XmppClient.XmlAttribute(E, "var");
						List<string> ValueStrings = null;
						List<KeyValuePair<string, string>> OptionStrings = null;
						string Description = string.Empty;
						string DataTypeName = null;
						DataType DataType = null;
						ValidationMethod ValidationMethod = null;
						bool Required = false;

						foreach (XmlNode N2 in N.ChildNodes)
						{
							switch (N2.LocalName)
							{
								case "desc":
									Description = N2.InnerText;
									break;

								case "required":
									Required = true;
									break;

								case "value":
									if (ValueStrings == null)
										ValueStrings = new List<string>();

									ValueStrings.Add(N2.InnerText);
									break;

								case "option":
									if (OptionStrings == null)
										OptionStrings = new List<KeyValuePair<string, string>>();

									string OptionLabel = XmppClient.XmlAttribute((XmlElement)N2, "label");
									string OptionValue = string.Empty;

									foreach (XmlNode N3 in N2.ChildNodes)
									{
										if (N3.LocalName == "value")
										{
											OptionValue = N3.InnerText;
											break;
										}
									}

									OptionStrings.Add(new KeyValuePair<string, string>(OptionLabel, OptionValue));
									break;

								case "validate":
									DataTypeName = XmppClient.XmlAttribute((XmlElement)N2, "datatype");

									foreach (XmlNode N3 in N2.ChildNodes)
									{
										switch (N3.LocalName)
										{
											case "basic":
												ValidationMethod = new BasicValidation();
												break;

											case "open":
												ValidationMethod = new OpenValidation();
												break;

											case "range":
												XmlElement E3 = (XmlElement)N3;

												ValidationMethod = new RangeValidation(
													XmppClient.XmlAttribute(E3, "min"),
													XmppClient.XmlAttribute(E3, "max"));
												break;

											case "regex":
												ValidationMethod = new RegexValidation(N3.InnerText);
												break;

											case "list-range":
												E3 = (XmlElement)N3;

												ValidationMethod = new ListRangeValidation(ValidationMethod,
													XmppClient.XmlAttribute(E3, "min", 0),
													XmppClient.XmlAttribute(E3, "max", int.MaxValue));
												break;
										}
									}
									break;
							}
						}

						if (string.IsNullOrEmpty(DataTypeName))
						{
							if (Type == "boolean")
								DataTypeName = "xs:boolean";
							else
								DataTypeName = "xs:string";
						}

						switch (DataTypeName.ToLower())
						{
							case "xs:boolean":
								DataType = new BooleanDataType(DataTypeName);
								break;

							case "xs:string":
							default:
								DataType = new StringDataType(DataTypeName);
								break;

							case "anyURI":
								DataType = new AnyUriDataType(DataTypeName);
								break;

							case "xs:byte":
								DataType = new ByteDataType(DataTypeName);
								break;

							case "xs:date":
								DataType = new DateDataType(DataTypeName);
								break;

							case "xs:dateTime":
								DataType = new DateTimeDataType(DataTypeName);
								break;

							case "xs:decimal":
								DataType = new DecimalDataType(DataTypeName);
								break;

							case "xs:double":
								DataType = new DoubleDataType(DataTypeName);
								break;

							case "xs:int":
								DataType = new IntDataType(DataTypeName);
								break;

							case "xs:integer":
								DataType = new IntegerDataType(DataTypeName);
								break;

							case "xs:language":
								DataType = new LanguageDataType(DataTypeName);
								break;

							case "xs:long":
								DataType = new LongDataType(DataTypeName);
								break;

							case "xs:short":
								DataType = new ShortDataType(DataTypeName);
								break;

							case "xs:time":
								DataType = new TimeDataType(DataTypeName);
								break;

							case "xdc:Color":
								DataType = new ColorDataType(DataTypeName);
								break;

							case "xdc:ColorAlpha":
								DataType = new ColorAlphaDataType(DataTypeName);
								break;
						}

						if (ValidationMethod == null)
							ValidationMethod = new BasicValidation();

						switch (Type)
						{
							case "boolean":
								Fields.Add(new BooleanField(Var, Label, Required,
									ValueStrings == null ? null : ValueStrings.ToArray(),
									OptionStrings == null ? null : OptionStrings.ToArray(),
									Description, DataType, ValidationMethod));
								break;

							case "fixed":
								Fields.Add(new FixedField(Var, Label, Required,
									ValueStrings == null ? null : ValueStrings.ToArray(),
									OptionStrings == null ? null : OptionStrings.ToArray(),
									Description, DataType, ValidationMethod));
								break;

							case "hidden":
								Fields.Add(new HiddenField(Var, Label, Required,
									ValueStrings == null ? null : ValueStrings.ToArray(),
									OptionStrings == null ? null : OptionStrings.ToArray(),
									Description, DataType, ValidationMethod));
								break;

							case "jid-multi":
								Fields.Add(new JidMultiField(Var, Label, Required,
									ValueStrings == null ? null : ValueStrings.ToArray(),
									OptionStrings == null ? null : OptionStrings.ToArray(),
									Description, DataType, ValidationMethod));
								break;

							case "jid-single":
								Fields.Add(new JidSingleField(Var, Label, Required,
									ValueStrings == null ? null : ValueStrings.ToArray(),
									OptionStrings == null ? null : OptionStrings.ToArray(),
									Description, DataType, ValidationMethod));
								break;

							case "list-multi":
								Fields.Add(new ListMultiField(Var, Label, Required,
									ValueStrings == null ? null : ValueStrings.ToArray(),
									OptionStrings == null ? null : OptionStrings.ToArray(),
									Description, DataType, ValidationMethod));
								break;

							case "list-single":
								Fields.Add(new ListSingleField(Var, Label, Required,
									ValueStrings == null ? null : ValueStrings.ToArray(),
									OptionStrings == null ? null : OptionStrings.ToArray(),
									Description, DataType, ValidationMethod));
								break;

							case "text-multi":
								Fields.Add(new TextMultiField(Var, Label, Required,
									ValueStrings == null ? null : ValueStrings.ToArray(),
									OptionStrings == null ? null : OptionStrings.ToArray(),
									Description, DataType, ValidationMethod));
								break;

							case "text-private":
								Fields.Add(new TextPrivateField(Var, Label, Required,
									ValueStrings == null ? null : ValueStrings.ToArray(),
									OptionStrings == null ? null : OptionStrings.ToArray(),
									Description, DataType, ValidationMethod));
								break;

							case "text-single":
							default:
								Fields.Add(new TextSingleField(Var, Label, Required,
									ValueStrings == null ? null : ValueStrings.ToArray(),
									OptionStrings == null ? null : OptionStrings.ToArray(),
									Description, DataType, ValidationMethod));
								break;
						}
						break;

					case "reported":
						// TODO
						break;

					case "item":
						// TODO
						break;
				}
			}

			this.instructions = Instructions.ToArray();
			this.fields = Fields.ToArray();
		}

		/// <summary>
		/// Form type
		/// </summary>
		public FormType Type { get { return this.type; } }

		/// <summary>
		/// Fields in the form.
		/// </summary>
		public Field[] Fields { get { return this.fields; } }

		/// <summary>
		/// Form Instructions
		/// </summary>
		public string[] Instructions { get { return this.instructions; } }

		/// <summary>
		/// Title of the form.
		/// </summary>
		public string Title { get { return this.title; } }

	}
}
