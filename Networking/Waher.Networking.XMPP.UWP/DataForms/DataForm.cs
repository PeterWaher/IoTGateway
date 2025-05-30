﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.XMPP.BitsOfBinary;
using Waher.Networking.XMPP.DataForms.DataTypes;
using Waher.Networking.XMPP.DataForms.FieldTypes;
using Waher.Networking.XMPP.DataForms.Layout;
using Waher.Networking.XMPP.DataForms.ValidationMethods;
using Waher.Security;

namespace Waher.Networking.XMPP.DataForms
{
	/// <summary>
	/// Implements support for data forms. Data Forms are defined in the following XEPs:
	/// 
	/// XEP-0004: Data Forms:
	/// http://xmpp.org/extensions/xep-0004.html
	/// 
	/// XEP-0122: Data Forms Validation:
	/// http://xmpp.org/extensions/xep-0122.html
	/// 
	/// XEP-0141: Data Forms Layout
	/// http://xmpp.org/extensions/xep-0141.html
	/// 
	/// XEP-0221: Data Forms Media Element
	/// http://xmpp.org/extensions/xep-0221.html
	/// 
	/// XEP-0331: Data Forms - Color Field Types
	/// http://xmpp.org/extensions/xep-0331.html
	/// 
	/// XEP-0336: Data Forms - Dynamic Forms
	/// http://xmpp.org/extensions/xep-0336.html
	/// 
	/// XEP-0348: Signing Forms: 
	/// http://xmpp.org/extensions/xep-0348.html
	/// </summary>
	public class DataForm
	{
		private Dictionary<string, Field> fieldsByVar = new Dictionary<string, Field>();
		private EventHandlerAsync<DataForm> onSubmit;
		private EventHandlerAsync<DataForm> onCancel;
		private readonly XmppClient client;
		private FormType type;
		private Field[] fields;
		private Field[] header;
		private Field[][] records;
		private Page[] pages;
		private string[] instructions;
		private string title = string.Empty;
		private object state = null;
		private readonly string from;
		private readonly string to;
		private readonly bool containsPostBackFields = false;
		private bool hasPages = false;
		private bool hasMedia = false;

		/// <summary>
		/// Implements support for data forms. Data Forms are defined in the following XEPs:
		/// 
		/// XEP-0004: Data Forms:
		/// http://xmpp.org/extensions/xep-0004.html
		/// 
		/// XEP-0122: Data Forms Validation:
		/// http://xmpp.org/extensions/xep-0122.html
		/// 
		/// XEP-0141: Data Forms Layout
		/// http://xmpp.org/extensions/xep-0141.html
		/// 
		/// XEP-0221: Data Forms Media Element
		/// http://xmpp.org/extensions/xep-0221.html
		/// 
		/// XEP-0331: Data Forms - Color Field Types
		/// http://xmpp.org/extensions/xep-0331.html
		/// 
		/// XEP-0336: Data Forms - Dynamic Forms
		/// http://xmpp.org/extensions/xep-0336.html
		/// 
		/// XEP-0348: Signing Forms: 
		/// http://xmpp.org/extensions/xep-0348.html
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		/// <param name="X">Data Form definition.</param>
		/// <param name="OnSubmit">Method called when the form is submitted.</param>
		/// <param name="OnCancel">Method called when the form is cancelled.</param>
		/// <param name="From">From where the form came.</param>
		/// <param name="To">To where the form was sent.</param>
		public DataForm(XmppClient Client, XmlElement X, EventHandlerAsync<DataForm> OnSubmit, EventHandlerAsync<DataForm> OnCancel, 
			string From, string To)
		{
			List<string> Instructions = new List<string>();
			List<Field> Fields = new List<Field>();
			List<Field[]> Records = new List<Field[]>();
			List<Page> Pages = null;

			this.client = Client;
			this.onSubmit = OnSubmit;
			this.onCancel = OnCancel;
			this.from = From;
			this.to = To;

			switch (XML.Attribute(X, "type").ToLower())
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
						Field Field = this.ParseField((XmlElement)N, out Media Media);
						Fields.Add(Field);

						if (Field.PostBack)
							this.containsPostBackFields = true;

						if (!string.IsNullOrEmpty(Field.Var))
							this.fieldsByVar[Field.Var] = Field;

						if (!(Media is null))
						{
							Field = new MediaField(this, Guid.NewGuid().ToString(), string.Empty, false,
								null, null, string.Empty, new StringDataType(), new BasicValidation(), Media, string.Empty, false, true, false);
							Fields.Add(Field);
							this.fieldsByVar[Field.Var] = Field;
							this.hasMedia = true;
						}
						break;

					case "reported":
						List<Field> Header = new List<Field>();

						foreach (XmlNode N2 in N.ChildNodes)
						{
							if (N2.LocalName == "field")
							{
								Field = this.ParseField((XmlElement)N2, out Media);
								Header.Add(Field);
							}
						}

						this.header = Header.ToArray();
						break;

					case "item":
						List<Field> Record = new List<Field>();

						foreach (XmlNode N2 in N.ChildNodes)
						{
							if (N2.LocalName == "field")
							{
								Field = this.ParseField((XmlElement)N2, out Media);
								Record.Add(Field);
							}
						}

						Records.Add(Record.ToArray());
						break;

					case "page":
						if (Pages is null)
							Pages = new List<Page>();

						Pages.Add(new Page(this, (XmlElement)N));
						break;
				}
			}

			this.instructions = Instructions.ToArray();
			this.fields = Fields.ToArray();
			this.records = Records.ToArray();

			if (this.header is null)
				this.header = Array.Empty<Field>();

			if (this.hasPages = (!(Pages is null) && Pages.Count > 0))
				this.pages = Pages.ToArray();
			else if (this.fields.Length > 0)
				this.pages = new Page[] { new Page(this, this.title, this.fields) };
			else
				this.pages = new Page[] { new Page(this, this.title, new ReportedReference(this)) };

			if (this.hasMedia)
			{
				Dictionary<string, byte[]> Bob = new Dictionary<string, byte[]>(StringComparer.CurrentCultureIgnoreCase);

				foreach (XmlNode N in X.ParentNode.ChildNodes)
				{
					if (N is XmlElement E && E.LocalName == "data" && E.NamespaceURI == BobClient.Namespace)
					{
						string Cid = XML.Attribute(E, "cid");
						byte[] Bin = Convert.FromBase64String(E.InnerText);

						Bob["cid:" + Cid] = Bin;
					}
				}

				foreach (Field F in this.fields)
				{
					if (F is MediaField MediaField && !(MediaField.Media is null))
					{
						foreach (KeyValuePair<string, Uri> Uri in MediaField.Media.URIs)
						{
							switch (Uri.Value.Scheme.ToLower())
							{
								case "cid":
									if (Bob.TryGetValue(Uri.Value.ToString(), out byte[] Bin))
										MediaField.Media.Binary = Bin;
									break;

								case "data":
									string s = Uri.Value.ToString();
									string ContentType;

									int i = s.IndexOf(':');
									if (i > 0)
										s = s.Substring(i + 1);

									i = s.IndexOf(';');
									if (i < 0)
										ContentType = Uri.Key;
									else
									{
										ContentType = s.Substring(0, i);
										s = s.Substring(i + 1);
									}

									i = s.IndexOf(',');
									if (i < 0)
										break;

									if (s.Substring(0, i) != "base64")
										break;

									s = s.Substring(i + 1);

									Bin = Convert.FromBase64String(s);
									MediaField.Media.Binary = Bin;
									MediaField.Media.ContentType = ContentType;
									break;

								case "http":
								case "https":
									MediaField.Media.URL = Uri.Value.ToString();
									break;
							}
						}
					}
				}
			}
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
		/// XEP-0141: Data Forms Layout
		/// http://xmpp.org/extensions/xep-0141.html
		/// 
		/// XEP-0221: Data Forms Media Element
		/// http://xmpp.org/extensions/xep-0221.html
		/// 
		/// XEP-0331: Data Forms - Color Field Types
		/// http://xmpp.org/extensions/xep-0331.html
		/// 
		/// XEP-0336: Data Forms - Dynamic Forms
		/// http://xmpp.org/extensions/xep-0336.html
		/// 
		/// XEP-0348: Signing Forms: 
		/// http://xmpp.org/extensions/xep-0348.html
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		/// <param name="Type">Type of data form.</param>
		/// <param name="From">From where the form came.</param>
		/// <param name="To">To where the form was sent.</param>
		/// <param name="Fields">Data form fields.</param>
		public DataForm(XmppClient Client, FormType Type, string From, string To, params Field[] Fields)
			: this(Client, Type, null, null, From, To, Fields)
		{
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
		/// XEP-0141: Data Forms Layout
		/// http://xmpp.org/extensions/xep-0141.html
		/// 
		/// XEP-0221: Data Forms Media Element
		/// http://xmpp.org/extensions/xep-0221.html
		/// 
		/// XEP-0331: Data Forms - Color Field Types
		/// http://xmpp.org/extensions/xep-0331.html
		/// 
		/// XEP-0336: Data Forms - Dynamic Forms
		/// http://xmpp.org/extensions/xep-0336.html
		/// 
		/// XEP-0348: Signing Forms: 
		/// http://xmpp.org/extensions/xep-0348.html
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		/// <param name="OnSubmit">Method called when the form is submitted.</param>
		/// <param name="OnCancel">Method called when the form is cancelled.</param>
		/// <param name="From">From where the form came.</param>
		/// <param name="To">To where the form was sent.</param>
		/// <param name="Fields">Data form fields.</param>
		public DataForm(XmppClient Client, EventHandlerAsync<DataForm> OnSubmit, EventHandlerAsync<DataForm> OnCancel, string From, string To, params Field[] Fields)
			: this(Client, FormType.Form, OnSubmit, OnCancel, From, To, Fields)
		{
		}

		private DataForm(XmppClient Client, FormType Type, EventHandlerAsync<DataForm> OnSubmit, EventHandlerAsync<DataForm> OnCancel, string From, string To, params Field[] Fields)
		{
			this.client = Client;
			this.onSubmit = OnSubmit;
			this.onCancel = OnCancel;
			this.type = Type;
			this.from = From;
			this.to = To;

			foreach (Field F in Fields)
			{
				F.Form = this;

				if (!string.IsNullOrEmpty(F.Var))
					this.fieldsByVar[F.Var] = F;
			}

			this.title = string.Empty;
			this.instructions = Array.Empty<string>();
			this.fields = Fields;
			this.records = Array.Empty<Field[]>();
			this.header = Array.Empty<Field>();
			this.hasPages = false;
			this.pages = new Page[] { new Page(this, this.title, this.fields) };
		}

		/// <summary>
		/// Parses a data form in XML format.
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="Xml">XML representation of data form.</param>
		/// <param name="OnSubmit">Method to call when user submits form.</param>
		/// <param name="OnCancel">Method to call when user cancels form.</param>
		/// <param name="From">Sender address.</param>
		/// <param name="To">Recipient address.</param>
		/// <returns>Parsed data form.</returns>
		/// <exception cref="Exception">Exception thrown if XML does not represent an XMPP Data Form.</exception>
		public static DataForm Parse(XmppClient Client, string Xml, EventHandlerAsync<DataForm> OnSubmit, EventHandlerAsync<DataForm> OnCancel, string From, string To)
		{
			XmlDocument Doc = new XmlDocument();
			Doc.LoadXml(Xml);

			if (Doc.DocumentElement.LocalName != "x")
				throw new Exception("Not an XMPP Data Form XML document.");

			return new DataForm(Client, Doc.DocumentElement, OnSubmit, OnCancel, From, To);
		}

		private Field ParseField(XmlElement E, out Media Media)
		{
			string Label = XML.Attribute(E, "label");
			string Type = XML.Attribute(E, "type");
			string Var = XML.Attribute(E, "var");
			List<string> ValueStrings = null;
			List<KeyValuePair<string, string>> OptionStrings = null;
			string Description = string.Empty;
			string DataTypeName = null;
			DataType DataType;
			ValidationMethod ValidationMethod = null;
			Field Field;
			string Error = null;
			string ContentType = null;
			bool Required = false;
			bool PostBack = false;
			bool ReadOnly = false;
			bool NotSame = false;

			Media = null;

			foreach (XmlNode N2 in E.ChildNodes)
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
						if (ValueStrings is null)
							ValueStrings = new List<string>();

						ValueStrings.Add(N2.InnerText);
						break;

					case "option":
						if (OptionStrings is null)
							OptionStrings = new List<KeyValuePair<string, string>>();

						string OptionLabel = XML.Attribute((XmlElement)N2, "label");
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
						DataTypeName = XML.Attribute((XmlElement)N2, "datatype");

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
										XML.Attribute(E3, "min"),
										XML.Attribute(E3, "max"));
									break;

								case "regex":
									ValidationMethod = new RegexValidation(N3.InnerText);
									break;

								case "list-range":
									E3 = (XmlElement)N3;

									ValidationMethod = new ListRangeValidation(ValidationMethod,
										XML.Attribute(E3, "min", 0),
										XML.Attribute(E3, "max", int.MaxValue));
									break;
							}
						}
						break;

					case "media":
						Media = new Media((XmlElement)N2);
						break;

					case "postBack":
						PostBack = true;
						break;

					case "readOnly":
						ReadOnly = true;
						break;

					case "notSame":
						NotSame = true;
						break;

					case "error":
						Error = N2.InnerText;
						break;

					case "contentType":
						ContentType = N2.InnerText;
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
					DataType = BooleanDataType.Instance;
					break;

				case "xs:string":
				default:
					DataType = StringDataType.Instance;
					break;

				case "xs:anyuri":
					DataType = AnyUriDataType.Instance;
					break;

				case "xs:byte":
					DataType = ByteDataType.Instance;
					break;

				case "xs:date":
					DataType = DateDataType.Instance;
					break;

				case "xs:datetime":
					DataType = DateTimeDataType.Instance;
					break;

				case "xs:decimal":
					DataType = DecimalDataType.Instance;
					break;

				case "xs:double":
					DataType = DoubleDataType.Instance;
					break;

				case "xs:int":
					DataType = IntDataType.Instance;
					break;

				case "xs:integer":
					DataType = IntegerDataType.Instance;
					break;

				case "xs:language":
					DataType = LanguageDataType.Instance;
					break;

				case "xs:long":
					DataType = LongDataType.Instance;
					break;

				case "xs:short":
					DataType = ShortDataType.Instance;
					break;

				case "xs:time":
					DataType = TimeDataType.Instance;
					break;

				case "xdc:Color":
					DataType = ColorDataType.Instance;
					break;

				case "xdc:ColorAlpha":
					DataType = ColorAlphaDataType.Instance;
					break;
			}

			if (ValidationMethod is null)
				ValidationMethod = new BasicValidation();

			switch (Type)
			{
				case "boolean":
					Field = new BooleanField(this, Var, Label, Required,
						ValueStrings?.ToArray(), OptionStrings?.ToArray(),
						Description, DataType, ValidationMethod, Error, PostBack, ReadOnly, NotSame);
					break;

				case "fixed":
					Field = new FixedField(this, Var, Label, Required,
						ValueStrings?.ToArray(), OptionStrings?.ToArray(),
						Description, DataType, ValidationMethod, Error, PostBack, ReadOnly, NotSame);
					break;

				case "hidden":
					Field = new HiddenField(this, Var, Label, Required,
						ValueStrings?.ToArray(), OptionStrings?.ToArray(),
						Description, DataType, ValidationMethod, Error, PostBack, ReadOnly, NotSame);
					break;

				case "jid-multi":
					Field = new JidMultiField(this, Var, Label, Required,
						ValueStrings?.ToArray(), OptionStrings?.ToArray(),
						Description, DataType, ValidationMethod, Error, PostBack, ReadOnly, NotSame);
					break;

				case "jid-single":
					Field = new JidSingleField(this, Var, Label, Required,
						ValueStrings?.ToArray(), OptionStrings?.ToArray(),
						Description, DataType, ValidationMethod, Error, PostBack, ReadOnly, NotSame);
					break;

				case "list-multi":
					Field = new ListMultiField(this, Var, Label, Required,
						ValueStrings?.ToArray(), OptionStrings?.ToArray(),
						Description, DataType, ValidationMethod, Error, PostBack, ReadOnly, NotSame);
					break;

				case "list-single":
					Field = new ListSingleField(this, Var, Label, Required,
						ValueStrings?.ToArray(), OptionStrings?.ToArray(),
						Description, DataType, ValidationMethod, Error, PostBack, ReadOnly, NotSame);
					break;

				case "text-multi":
					Field = new TextMultiField(this, Var, Label, Required,
						ValueStrings?.ToArray(), OptionStrings?.ToArray(),
						Description, DataType, ValidationMethod, Error, PostBack, ReadOnly, NotSame, ContentType);
					break;

				case "text-private":
					Field = new TextPrivateField(this, Var, Label, Required,
						ValueStrings?.ToArray(), OptionStrings?.ToArray(),
						Description, DataType, ValidationMethod, Error, PostBack, ReadOnly, NotSame);
					break;

				case "text-single":
					Field = new TextSingleField(this, Var, Label, Required,
						ValueStrings?.ToArray(), OptionStrings?.ToArray(),
						Description, DataType, ValidationMethod, Error, PostBack, ReadOnly, NotSame);
					break;

				default:
					if (Media is null)
					{
						Field = new TextSingleField(this, Var, Label, Required,
							ValueStrings?.ToArray(), OptionStrings?.ToArray(),
							Description, DataType, ValidationMethod, Error, PostBack, ReadOnly, NotSame);
					}
					else
					{
						Field = new MediaField(this, Var, Label, Required,
							ValueStrings?.ToArray(), OptionStrings?.ToArray(),
							Description, DataType, ValidationMethod, Media, Error, PostBack, ReadOnly, NotSame);

						Media = null;
						this.hasMedia = true;
					}
					break;
			}

			return Field;
		}

		/// <summary>
		/// Access to Fields in the form, given their variable name. If not found, null is returned.
		/// </summary>
		/// <param name="Var">Name of field.</param>
		/// <returns>Field parameter, if found, null otherwise.</returns>
		public Field this[string Var]
		{
			get
			{
				if (this.fieldsByVar.TryGetValue(Var, out Field Result))
					return Result;
				else
					return null;
			}
		}

		/// <summary>
		/// XMPP Client.
		/// </summary>
		public XmppClient Client => this.client;

		/// <summary>
		/// Form type
		/// </summary>
		public FormType Type => this.type;

		/// <summary>
		/// Fields in the form.
		/// </summary>
		public Field[] Fields
		{
			get => this.fields;
			set
			{
				this.fieldsByVar.Clear();
				this.fields = value;

				foreach (Field F in value)
					this.fieldsByVar[F.Var] = F;
			}
		}

		/// <summary>
		/// Form Instructions
		/// </summary>
		public string[] Instructions
		{
			get => this.instructions;
			set => this.instructions = value;
		}

		/// <summary>
		/// Title of the form.
		/// </summary>
		public string Title
		{
			get => this.title;
			set => this.title = value;
		}

		/// <summary>
		/// Header fields in a report result form.
		/// </summary>
		public Field[] Header => this.header;

		/// <summary>
		/// Records in a report result form.
		/// </summary>
		public Field[][] Records => this.records;

		/// <summary>
		/// If the form has pages.
		/// </summary>
		public bool HasPages => this.hasPages;

		/// <summary>
		/// Pages in form.
		/// </summary>
		public Page[] Pages
		{
			get => this.pages;
			set
			{
				this.pages = value;
				this.hasPages = !(this.pages is null) && this.pages.Length > 0;
			}
		}

		/// <summary>
		/// From where the form was sent.
		/// </summary>
		public string From => this.from;

		/// <summary>
		/// To where the form was sent.
		/// </summary>
		public string To => this.to;

		/// <summary>
		/// If the form contains post-back fields.
		/// </summary>
		public bool ContainsPostBackFields => this.containsPostBackFields;

		/// <summary>
		/// If the form contains media.
		/// </summary>
		public bool HasMedia => this.hasMedia;

		/// <summary>
		/// Submits the form.
		/// </summary>
		/// <exception cref="XmppException">If the form cannot be submitted.</exception>
		public async Task Submit()
		{
			if (this.CanSubmit)
			{
				try
				{
					await this.onSubmit(this, this);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}
			else
				throw new XmppException("Form cannot be submitted.");
		}

		/// <summary>
		/// If the form can be submitted.
		/// </summary>
		public bool CanSubmit
		{
			get { return !(this.onSubmit is null); }
		}

		/// <summary>
		/// Cancels the form.
		/// </summary>
		/// <exception cref="XmppException">If the form cannot be cancelled.</exception>
		public async Task Cancel()
		{
			if (this.containsPostBackFields)
			{
				StringBuilder Xml = new StringBuilder();

				Xml.Append("<cancel xmlns='");
				Xml.Append(XmppClient.NamespaceDynamicForms);
				Xml.Append("'>");
				this.ExportXml(Xml, "submit", true, false);
				Xml.Append("</cancel>");

				await this.client.SendIqSet(this.from, Xml.ToString(), null, null);
			}
			else if (this.CanCancel)
			{
				try
				{
					await this.onCancel(this, this);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}
			else
				throw new XmppException("Form cannot be cancelled.");
		}

		/// <summary>
		/// If the form can be cancelled.
		/// </summary>
		public bool CanCancel
		{
			get { return !(this.onCancel is null); }
		}

		/// <summary>
		/// State object user can use to attach information to the form.
		/// </summary>
		public object State
		{
			get => this.state;
			set => this.state = value;
		}

		/// <summary>
		/// Sets the form method handlers for the Submit and Cancel methods.
		/// </summary>
		/// <param name="OnSubmit">Callback method for the Submit method.</param>
		/// <param name="OnCancel">Callback method for the Cancel method.</param>
		public void SetMethodHandlers(EventHandlerAsync<DataForm> OnSubmit, EventHandlerAsync<DataForm> OnCancel)
		{
			this.onSubmit = OnSubmit;
			this.onCancel = OnCancel;
		}

		/// <summary>
		/// Serializes the form, using the current form type.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public void Serialize(StringBuilder Output)
		{
			switch (this.type)
			{
				case FormType.Form:
				default:
					this.SerializeForm(Output);
					break;

				case FormType.Cancel:
					this.SerializeCancel(Output);
					break;

				case FormType.Result:
					this.SerializeResult(Output);
					break;

				case FormType.Submit:
					this.SerializeSubmit(Output);
					break;
			}
		}

		/// <summary>
		/// Serializes the form as a form submission.
		/// </summary>
		/// <param name="Output">Output to serialize the form to.</param>
		/// <returns>Number of fields exported.</returns>
		public int SerializeSubmit(StringBuilder Output)
		{
			return this.ExportXml(Output, "submit", true, false);
		}

		/// <summary>
		/// Serializes the form as a form submission.
		/// </summary>
		/// <param name="Output">Output to serialize the form to.</param>
		/// <param name="ValuesOnly">If only values are to be serialized (default=true)</param>
		/// <returns>Number of fields exported.</returns>
		public int SerializeSubmit(StringBuilder Output, bool ValuesOnly)
		{
			return this.ExportXml(Output, "submit", ValuesOnly, !ValuesOnly);
		}

		/// <summary>
		/// Serializes the form as a result submission.
		/// </summary>
		/// <param name="Output">Output to serialize the form to.</param>
		/// <returns>Number of fields exported.</returns>
		public int SerializeResult(StringBuilder Output)
		{
			return this.ExportXml(Output, "result", true, true);
		}

		/// <summary>
		/// Serializes the form as a form cancellation.
		/// </summary>
		/// <param name="Output">Output to serialize the form to.</param>
		/// <returns>Number of fields exported.</returns>
		public int SerializeCancel(StringBuilder Output)
		{
			return this.ExportXml(Output, "cancel", true, false);
		}

		/// <summary>
		/// Serializes the form as a form cancellation.
		/// </summary>
		/// <param name="Output">Output to serialize the form to.</param>
		/// <param name="ValuesOnly">If only values are to be serialized (default=true)</param>
		/// <returns>Number of fields exported.</returns>
		public int SerializeCancel(StringBuilder Output, bool ValuesOnly)
		{
			return this.ExportXml(Output, "cancel", ValuesOnly, !ValuesOnly);
		}

		/// <summary>
		/// Serializes the form as an editable form.
		/// </summary>
		/// <param name="Output">Output to serialize the form to.</param>
		/// <returns>Number of fields exported.</returns>
		public int SerializeForm(StringBuilder Output)
		{
			return this.ExportXml(Output, "form", false, true);
		}

		internal int ExportXml(StringBuilder Output, string Type, bool ValuesOnly, bool IncludeLabels)
		{
			int NrFieldsExported = 0;

			Output.Append("<x xmlns='");
			Output.Append(XmppClient.NamespaceData);

			if (!ValuesOnly)
			{
				Output.Append("' xmlns:xdv='");
				Output.Append(XmppClient.NamespaceDataValidate);

				Output.Append("' xmlns:xdl='");
				Output.Append(XmppClient.NamespaceDataLayout);

				Output.Append("' xmlns:xdd='");
				Output.Append(XmppClient.NamespaceDynamicForms);
			}

			Output.Append("' type='");
			Output.Append(Type);
			Output.Append("'>");

			if (!ValuesOnly)
			{
				foreach (string s in this.instructions)
				{
					Output.Append("<instructions>");
					Output.Append(XML.Encode(s));
					Output.Append("</instructions>");
				}

				if (!string.IsNullOrEmpty(this.title))
				{
					Output.Append("<title>");
					Output.Append(XML.Encode(this.title));
					Output.Append("</title>");
				}

				if (!(this.pages is null))
				{
					foreach (Page Page in this.pages)
						Page.Serialize(Output);
				}
			}

			foreach (Field Field in this.fields)
			{
				if (Field.Exclude)
					continue;

				if (Field.Serialize(Output, ValuesOnly, IncludeLabels))
					NrFieldsExported++;
			}

			if (this.header.Length > 0)
			{
				Output.Append("<reported>");

				foreach (Field Field in this.header)
					Field.Serialize(Output, false, true);

				Output.Append("</reported>");
			}

			foreach (Field[] Record in this.records)
			{
				Output.Append("<item>");

				foreach (Field Field in Record)
					Field.Serialize(Output, true, false);

				Output.Append("</item>");
			}

			Output.Append("</x>");

			return NrFieldsExported;
		}

		/// <summary>
		/// Signs the form (if necessary), according to XEP-0348.
		/// </summary>
		/// <param name="FormSignatureKey">Form signature key.</param>
		/// <param name="FormSignatureSecret">Form signature secret.</param>
		public async Task Sign(string FormSignatureKey, string FormSignatureSecret)
		{
			Field oauth_version = this["oauth_version"];
			Field oauth_signature_method = this["oauth_signature_method"];
			Field oauth_token = this["oauth_token"];
			Field oauth_token_secret = this["oauth_token_secret"];
			Field oauth_nonce = this["oauth_nonce"];
			Field oauth_timestamp = this["oauth_timestamp"];
			Field oauth_consumer_key = this["oauth_consumer_key"];
			Field oauth_signature = this["oauth_signature"];

			if (!(oauth_version is null ||
				oauth_signature_method is null ||
				oauth_token is null ||
				oauth_token_secret is null ||
				oauth_nonce is null ||
				oauth_timestamp is null ||
				oauth_consumer_key is null ||
				oauth_signature is null))
			{
				SortedDictionary<string, string> Sorted = new SortedDictionary<string, string>();
				DateTime Now = DateTime.Now.ToUniversalTime();
				TimeSpan Span = Now - OAuthFirstDay;
				long TotalSeconds = (long)Span.TotalSeconds;
				string Nonce = Hashes.BinaryToString(XmppClient.GetRandomBytes(16));
				string TokenSecret = oauth_token_secret.ValueString;

				await oauth_consumer_key.SetValue(FormSignatureKey);
				await oauth_timestamp.SetValue(TotalSeconds.ToString());
				await oauth_nonce.SetValue(Nonce);
				await oauth_version.SetValue("1.0");
				await oauth_signature_method.SetValue("HMAC-SHA1");

				foreach (Field F in this.fields)
					Sorted[F.Var] = F.ValueString;

				Sorted.Remove("oauth_token_secret");
				Sorted.Remove("oauth_signature");

				StringBuilder PStr = new StringBuilder();
				bool First = true;

				foreach (KeyValuePair<string, string> Pair in Sorted)
				{
					if (First)
						First = false;
					else
						PStr.Append("&");

					PStr.Append(OAuthEncode(Pair.Key));
					PStr.Append("=");
					PStr.Append(OAuthEncode(Pair.Value));
				}

				StringBuilder BStr = new StringBuilder();

				BStr.Append("submit&&");    // No to-field.
				BStr.Append(OAuthEncode(PStr.ToString()));

				byte[] Key = Encoding.ASCII.GetBytes(OAuthEncode(FormSignatureSecret) + "&" + OAuthEncode(TokenSecret));
				byte[] Hash = Hashes.ComputeHMACSHA1Hash(Key, Encoding.ASCII.GetBytes(BStr.ToString()));

				await oauth_signature.SetValue(OAuthEncode(Convert.ToBase64String(Hash)));
			}
		}

		private static string OAuthEncode(string s)
		{
			StringBuilder Result = new StringBuilder();

			foreach (char ch in s)
			{
				if (OAuthReserved.IndexOf(ch) < 0)
				{
					Result.Append("%");
					Result.Append(((int)ch).ToString("X2"));
				}
				else
					Result.Append(ch);
			}

			return Result.ToString();
		}

		private static readonly DateTime OAuthFirstDay = new DateTime(1970, 1, 1, 0, 0, 0);
		private const string OAuthReserved = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._~";

		/// <summary>
		/// This method joins two forms. The current form represents the old form (which is probably being viewed by a user), and <paramref name="NewForm"/> 
		/// represents the new updated form. The method makes sure values that are currently being edited (and which the server is unaware off) are kept, while
		/// all other changes are transferred to the current form.
		/// </summary>
		/// <param name="NewForm">New version of form.</param>
		public async Task Join(DataForm NewForm)
		{
			Field[] OldFields = this.fields;

			this.fieldsByVar = NewForm.fieldsByVar;
			this.type = NewForm.type;
			this.fields = NewForm.fields;
			this.header = NewForm.header;
			this.records = NewForm.records;
			this.pages = NewForm.pages;
			this.instructions = NewForm.instructions;
			this.title = NewForm.title;

			foreach (Field OldField in OldFields)
			{
				if (!this.fieldsByVar.TryGetValue(OldField.Var, out Field NewField))
					continue;

				if (!OldField.Edited)
					continue;

				await NewField.SetValue(OldField.ValueStrings);
			}

			await this.OnRemoteUpdate.Raise(this.client, this);
		}

		/// <summary>
		/// Event raised when the form has been remotely updated and might need to be redrawn. Currently edited values are kept,
		/// but other properties might have changed, new fields have been added, others removed, layout changed, etc.
		/// </summary>
		public event EventHandlerAsync<DataForm> OnRemoteUpdate = null;

		/// <summary>
		/// Method to call when submitting form.
		/// </summary>
		public EventHandlerAsync<DataForm> OnSubmit => this.onSubmit;

		/// <summary>
		/// Method to call when cancelling form.
		/// </summary>
		public EventHandlerAsync<DataForm> OnCancel => this.onCancel;

		/// <summary>
		/// Removes excluded fields.
		/// </summary>
		public void RemoveExcluded()
		{
			bool HasExcluded = false;

			foreach (Field F in this.fields)
			{
				if (F.Exclude)
				{
					HasExcluded = true;
					break;
				}
			}

			if (HasExcluded)
			{
				List<Field> NewFields = new List<DataForms.Field>();

				foreach (Field F in this.fields)
				{
					if (!F.Exclude)
						NewFields.Add(F);
				}

				this.Fields = NewFields.ToArray();

				if (!(this.pages is null))
				{
					List<Page> NewPages = new List<Page>();

					foreach (Page P in this.pages)
					{
						if (!P.RemoveExcluded())
							NewPages.Add(P);
					}

					this.pages = NewPages.ToArray();
				}
			}
		}

		/// <summary>
		/// Tries to get a page from the form, given its title.
		/// </summary>
		/// <param name="Title">Page title (label).</param>
		/// <param name="Page">Page, if found.</param>
		/// <returns>If a page was found with the given title.</returns>
		public bool TryGetPage(string Title, out Page Page)
		{
			Page = null;

			if (this.pages is null)
				return false;

			foreach (Page P in this.pages)
			{
				if (P.Label == Title)
				{
					Page = P;
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Gets a page on the form. If one does not exist with the current title, a new page is created with the corresponding title.
		/// </summary>
		/// <param name="Title">Title of page.</param>
		/// <returns>Page object.</returns>
		public Page GetPage(string Title)
		{
			if (this.TryGetPage(Title, out Page Page))
				return Page;

			Page = new Page(this, Title);
			this.Add(Page);

			return Page;
		}

		/// <summary>
		/// Adds a page to the form.
		/// </summary>
		/// <param name="Page">Page object.</param>
		public void Add(Page Page)
		{
			if (this.pages is null)
				this.Pages = new Page[] { Page };
			else
			{
				int c = this.pages.Length;

				if (c == 1 && string.IsNullOrEmpty(this.pages[0].Label) &&
					(this.pages[0].Elements is null || this.pages[0].Elements.Length == 0))
				{
					this.pages[0] = Page;
				}
				else
				{
					Array.Resize(ref this.pages, c + 1);
					this.pages[c] = Page;
				}
			}

			this.hasPages = true;
		}

		/// <summary>
		/// Adds a field to the form.
		/// </summary>
		/// <param name="Field">Field object.</param>
		public void Add(Field Field)
		{
			if (this.fields is null)
				this.fields = new Field[] { Field };
			else
			{
				int c = this.fields.Length;
				Array.Resize(ref this.fields, c + 1);
				this.fields[c] = Field;
			}

			if (Field is MediaField)
				this.hasMedia = true;

			this.fieldsByVar[Field.Var] = Field;
		}

	}
}
