using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Waher.Networking.XMPP.DataForms.DataTypes;
using Waher.Networking.XMPP.DataForms.FieldTypes;
using Waher.Networking.XMPP.DataForms.ValidationMethods;
using Waher.Networking.XMPP.DataForms.Layout;

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
	/// Data Form callback method delegate.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="Form">Data Form.</param>
	public delegate void DataFormCallbackMethod(object Sender, DataForm Form);

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
	/// XEP-0331: Data Forms - Color Field Types
	/// http://xmpp.org/extensions/xep-0331.html
	/// 
	/// XEP-0348: Signing Forms: 
	/// http://xmpp.org/extensions/xep-0348.html
	/// </summary>
	public class DataForm
	{
		private Dictionary<string, Field> fieldsByVar = new Dictionary<string, Field>();
		private DataFormCallbackMethod onSubmit;
		private DataFormCallbackMethod onCancel;
		private FormType type;
		private Field[] fields;
		private Field[] header;
		private Field[][] records;
		private Page[] pages;
		private string[] instructions;
		private string title = string.Empty;
		private object state = null;

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
		/// XEP-0331: Data Forms - Color Field Types
		/// http://xmpp.org/extensions/xep-0331.html
		/// 
		/// XEP-0348: Signing Forms: 
		/// http://xmpp.org/extensions/xep-0348.html
		/// </summary>
		/// <param name="X">Data Form definition.</param>
		public DataForm(XmlElement X, DataFormCallbackMethod OnSubmit, DataFormCallbackMethod OnCancel)
		{
			List<string> Instructions = new List<string>();
			List<Field> Fields = new List<Field>();
			List<Field[]> Records = new List<Field[]>();
			List<Page> Pages = null;

			this.onSubmit = OnSubmit;
			this.onCancel = OnCancel;

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
						Field Field = this.ParseField((XmlElement)N);
						Fields.Add(Field);

						if (!string.IsNullOrEmpty(Field.Var))
							this.fieldsByVar[Field.Var] = Field;
						break;

					case "reported":
						List<Field> Header = new List<Field>();
						
						foreach (XmlNode N2 in N.ChildNodes)
						{
							if (N2.LocalName == "field")
							{
								Field = this.ParseField((XmlElement)N2);
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
								Field = this.ParseField((XmlElement)N2);
								Record.Add(Field);
							}
						}

						Records.Add(Record.ToArray());
						break;

					case "page":
						if (Pages == null)
							Pages = new List<Page>();

						Pages.Add(new Page((XmlElement)N));
						break;
				}
			}

			this.instructions = Instructions.ToArray();
			this.fields = Fields.ToArray();
			this.records = Records.ToArray();

			if (this.header == null)
				this.header = new Field[0];

			if (Pages == null)
				this.pages = new Page[] { new Page(this.title, this.fields) };
			else
				this.pages = Pages.ToArray();
		}

		private Field ParseField(XmlElement E)
		{
			string Label = XmppClient.XmlAttribute(E, "label");
			string Type = XmppClient.XmlAttribute(E, "type");
			string Var = XmppClient.XmlAttribute(E, "var");
			List<string> ValueStrings = null;
			List<KeyValuePair<string, string>> OptionStrings = null;
			string Description = string.Empty;
			string DataTypeName = null;
			DataType DataType = null;
			ValidationMethod ValidationMethod = null;
			Field Field;
			bool Required = false;

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
					Field = new BooleanField(Var, Label, Required,
						ValueStrings == null ? null : ValueStrings.ToArray(),
						OptionStrings == null ? null : OptionStrings.ToArray(),
						Description, DataType, ValidationMethod);
					break;

				case "fixed":
					Field = new FixedField(Var, Label, Required,
						ValueStrings == null ? null : ValueStrings.ToArray(),
						OptionStrings == null ? null : OptionStrings.ToArray(),
						Description, DataType, ValidationMethod);
					break;

				case "hidden":
					Field = new HiddenField(Var, Label, Required,
						ValueStrings == null ? null : ValueStrings.ToArray(),
						OptionStrings == null ? null : OptionStrings.ToArray(),
						Description, DataType, ValidationMethod);
					break;

				case "jid-multi":
					Field = new JidMultiField(Var, Label, Required,
						ValueStrings == null ? null : ValueStrings.ToArray(),
						OptionStrings == null ? null : OptionStrings.ToArray(),
						Description, DataType, ValidationMethod);
					break;

				case "jid-single":
					Field = new JidSingleField(Var, Label, Required,
						ValueStrings == null ? null : ValueStrings.ToArray(),
						OptionStrings == null ? null : OptionStrings.ToArray(),
						Description, DataType, ValidationMethod);
					break;

				case "list-multi":
					Field = new ListMultiField(Var, Label, Required,
						ValueStrings == null ? null : ValueStrings.ToArray(),
						OptionStrings == null ? null : OptionStrings.ToArray(),
						Description, DataType, ValidationMethod);
					break;

				case "list-single":
					Field = new ListSingleField(Var, Label, Required,
						ValueStrings == null ? null : ValueStrings.ToArray(),
						OptionStrings == null ? null : OptionStrings.ToArray(),
						Description, DataType, ValidationMethod);
					break;

				case "text-multi":
					Field = new TextMultiField(Var, Label, Required,
						ValueStrings == null ? null : ValueStrings.ToArray(),
						OptionStrings == null ? null : OptionStrings.ToArray(),
						Description, DataType, ValidationMethod);
					break;

				case "text-private":
					Field = new TextPrivateField(Var, Label, Required,
						ValueStrings == null ? null : ValueStrings.ToArray(),
						OptionStrings == null ? null : OptionStrings.ToArray(),
						Description, DataType, ValidationMethod);
					break;

				case "text-single":
				default:
					Field = new TextSingleField(Var, Label, Required,
						ValueStrings == null ? null : ValueStrings.ToArray(),
						OptionStrings == null ? null : OptionStrings.ToArray(),
						Description, DataType, ValidationMethod);
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
				Field Result;

				if (this.fieldsByVar.TryGetValue(Var, out Result))
					return Result;
				else
					return null;
			}
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

		/// <summary>
		/// Header fields in a report result form.
		/// </summary>
		public Field[] Header { get { return this.header; } }

		/// <summary>
		/// Records in a report result form.
		/// </summary>
		public Field[][] Records { get { return this.records; } }

		/// <summary>
		/// Pages in form.
		/// </summary>
		public Page[] Pages { get { return this.pages; } }

		/// <summary>
		/// Submits the form.
		/// </summary>
		/// <exception cref="XmppException">If the form cannot be submitted.</exception>
		public void Submit()
		{
			if (this.CanSubmit)
			{
				try
				{
					this.onSubmit(this, this);
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.Message);
					Debug.WriteLine(ex.StackTrace);
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
			get { return this.onSubmit != null; } 
		}

		/// <summary>
		/// Cancels the form.
		/// </summary>
		/// <exception cref="XmppException">If the form cannot be cancelled.</exception>
		public void Cancel()
		{
			if (this.CanCancel)
			{
				try
				{
					this.onCancel(this, this);
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.Message);
					Debug.WriteLine(ex.StackTrace);
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
			get { return this.onCancel != null; }
		}

		/// <summary>
		/// State object user can use to attach information to the form.
		/// </summary>
		public object State
		{
			get { return this.state; }
			set { this.state = value; }
		}

		/// <summary>
		/// Serializes the form as a form submission.
		/// </summary>
		/// <param name="Output">Output to serialize the form to.</param>
		public void SerializeSubmit(StringBuilder Output)
		{
			this.Serialize(Output, "submit", true);
		}

		public void SerializeCancel(StringBuilder Output)
		{
			this.Serialize(Output, "cancel", true);
		}

		internal void Serialize(StringBuilder Output, string Type, bool ValuesOnly)
		{
			Output.Append("<x xmlns='");
			Output.Append(XmppClient.NamespaceData);
			Output.Append("' type='");
			Output.Append(Type);
			Output.Append("'>");

			if (!ValuesOnly)
			{
				foreach (string s in this.instructions)
				{
					Output.Append("<instructions>");
					Output.Append(XmppClient.XmlEncode(s));
					Output.Append("</instructions>");
				}

				if (!string.IsNullOrEmpty(this.title))
				{
					Output.Append("<title>");
					Output.Append(XmppClient.XmlEncode(this.title));
					Output.Append("</title>");
				}
			}

			foreach (Field Field in this.fields)
				Field.Serialize(Output, ValuesOnly);

			if (this.header.Length > 0)
			{
				Output.Append("<reported>");

				foreach (Field Field in this.header)
					Field.Serialize(Output, false);

				Output.Append("</reported>");
			}

			foreach (Field[] Record in this.records)
			{
				Output.Append("<item>");

				foreach (Field Field in Record)
					Field.Serialize(Output, true);

				Output.Append("</item>");
			}

			Output.Append("</x>");
		}

		/// <summary>
		/// Signs the form (if necessary), according to XEP-0348.
		/// </summary>
		/// <param name="FormSignatureKey">Form signature key.</param>
		/// <param name="FormSignatureSecret">Form signature secret.</param>
		public void Sign(string FormSignatureKey, string FormSignatureSecret)
		{
			Field oauth_version = this["oauth_version"];
			Field oauth_signature_method = this["oauth_signature_method"];
			Field oauth_token = this["oauth_token"];
			Field oauth_token_secret = this["oauth_token_secret"];
			Field oauth_nonce = this["oauth_nonce"];
			Field oauth_timestamp = this["oauth_timestamp"];
			Field oauth_consumer_key = this["oauth_consumer_key"];
			Field oauth_signature = this["oauth_signature"];

			if (oauth_version != null &&
				oauth_signature_method != null &&
				oauth_token != null &&
				oauth_token_secret != null &&
				oauth_nonce != null &&
				oauth_timestamp != null &&
				oauth_consumer_key != null &&
				oauth_signature != null)
			{
				SortedDictionary<string, string> Sorted = new SortedDictionary<string, string>();
				DateTime Now = DateTime.Now.ToUniversalTime();
				TimeSpan Span = Now - OAuthFirstDay;
				long TotalSeconds = (long)Span.TotalSeconds;
				string Nonce = Guid.NewGuid().ToString().Replace("-", string.Empty);
				string TokenSecret = oauth_token_secret.ValueString;

				oauth_consumer_key.SetValue(false, FormSignatureKey);
				oauth_timestamp.SetValue(false, TotalSeconds.ToString());
				oauth_nonce.SetValue(false, Nonce);
				oauth_version.SetValue(false, "1.0");
				oauth_signature_method.SetValue(false, "HMAC-SHA1");

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

				BStr.Append("submit&&");	// No to-field.
				BStr.Append(OAuthEncode(PStr.ToString()));

				byte[] Key = System.Text.Encoding.ASCII.GetBytes(OAuthEncode(FormSignatureSecret) + "&" + OAuthEncode(TokenSecret));

				HMACSHA1 HMACSHA1 = new HMACSHA1(Key, true);
				byte[] Hash = HMACSHA1.ComputeHash(System.Text.Encoding.ASCII.GetBytes(BStr.ToString()));

				oauth_signature.SetValue(false, OAuthEncode(Convert.ToBase64String(Hash)));
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

	}
}
