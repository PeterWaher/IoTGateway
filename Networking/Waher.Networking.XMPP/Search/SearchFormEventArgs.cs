using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.DataForms.DataTypes;
using Waher.Networking.XMPP.DataForms.FieldTypes;
using Waher.Networking.XMPP.DataForms.ValidationMethods;

namespace Waher.Networking.XMPP.Search
{
	/// <summary>
	/// Delegate for search form events or callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void SearchFormEventHandler(object Sender, SearchFormEventArgs e);

	/// <summary>
	/// Event arguments for search form responses.
	/// </summary>
	public class SearchFormEventArgs : IqResultEventArgs
	{
		private XmppClient client;
		private DataForm searchForm;
		private string instructions;
		private string first;
		private string last;
		private string nick;
		private string email;
		private bool supportsForms;

		internal SearchFormEventArgs(XmppClient Client, IqResultEventArgs e, string Instructions, string First, string Last, string Nick, string EMail,
			DataForm SearchForm, bool SupportsForms)
			: base(e)
		{
			this.client = Client;
			this.instructions = Instructions;
			this.first = First;
			this.last = Last;
			this.nick = Nick;
			this.email = EMail;
			this.searchForm = SearchForm;
			this.supportsForms = SupportsForms;
		}

		/// <summary>
		/// Search Form, if available.
		/// </summary>
		public DataForm SearchForm
		{
			get { return this.searchForm; }
		}

		/// <summary>
		/// Form instructions
		/// </summary>
		public string Instructions
		{
			get
			{
				if (!string.IsNullOrEmpty(this.instructions))
					return this.instructions;

				if (this.searchForm != null)
					return XmppClient.Concat(this.searchForm.Instructions);

				return string.Empty;
			}
		}

		/// <summary>
		/// First name
		/// </summary>
		public string FirstName
		{
			get
			{
				return this.GetField(this.first, "first");
			}

			set
			{
				this.SetField(ref this.first, "first", value);
			}
		}

		/// <summary>
		/// Last name
		/// </summary>
		public string LastName
		{
			get
			{
				return this.GetField(this.last, "last");
			}

			set
			{
				this.SetField(ref this.last, "last", value);
			}
		}

		/// <summary>
		/// Nick name
		/// </summary>
		public string NickName
		{
			get
			{
				return this.GetField(this.nick, "nick");
			}

			set
			{
				this.SetField(ref this.nick, "nick", value);
			}
		}

		/// <summary>
		/// EMail 
		/// </summary>
		public string EMail
		{
			get
			{
				return this.GetField(this.email, "email");
			}

			set
			{
				this.SetField(ref this.email, "email", value);
			}
		}

		/// <summary>
		/// If the remote end supports search forms, or if the form was constructed on the client side.
		/// </summary>
		public bool SupportsForms
		{
			get { return this.supportsForms; }
		}

		private string GetField(string FixedValue, string Var)
		{
			if (!string.IsNullOrEmpty(FixedValue))
				return FixedValue;

			if (this.searchForm != null)
			{
				Field Field = this.searchForm[Var];
				if (Field != null)
					return Field.ValueString;
			}

			return string.Empty;
		}

		private void SetField(ref string FixedValue, string Var, string Value)
		{
			FixedValue = Value;

			if (this.searchForm != null)
			{
				Field Field = this.searchForm[Var];
				if (Field != null)
					Field.SetValue(Value);
			}

			switch (Var)
			{
				case "first":
					this.first = Value;
					break;

				case "last":
					this.last = Value;
					break;

				case "nick":
					this.nick = Value;
					break;

				case "email":
					this.email = Value;
					break;
			}
		}

		/// <summary>
		/// Sends a search request.
		/// </summary>
		/// <param name="Callback">Callback method called when response of search request is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void SendSearchRequest(SearchResultEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<query xmlns='");
			Xml.Append(XmppClient.NamespaceSearch);
			Xml.Append("'>");

			if (!this.supportsForms)
			{
				if (!string.IsNullOrEmpty(this.first))
				{
					Xml.Append("<first>");
					Xml.Append(XML.Encode(this.first));
					Xml.Append("</first>");
				}

				if (!string.IsNullOrEmpty(this.last))
				{
					Xml.Append("<last>");
					Xml.Append(XML.Encode(this.last));
					Xml.Append("</last>");
				}

				if (!string.IsNullOrEmpty(this.nick))
				{
					Xml.Append("<nick>");
					Xml.Append(XML.Encode(this.nick));
					Xml.Append("</nick>");
				}

				if (!string.IsNullOrEmpty(this.email))
				{
					Xml.Append("<email>");
					Xml.Append(XML.Encode(this.email));
					Xml.Append("</email>");
				}

				Xml.Append("</query>");

				this.client.SendIqSet(this.From, Xml.ToString(), this.OldSearchResult, new object[] { Callback, State });
			}
			else
			{
				this.searchForm.SerializeSubmit(Xml);

				Xml.Append("</query>");

				this.client.SendIqSet(this.From, Xml.ToString(), this.FormSearchResult, new object[] { Callback, State });
			}
		}

		private void OldSearchResult(object Sender, IqResultEventArgs e)
		{
			object[] P = (object[])e.State;
			SearchResultEventHandler Callback = (SearchResultEventHandler)P[0];
			object State = P[1];
			List<Dictionary<string, string>> Records = new List<Dictionary<string, string>>();
			List<Field> Headers = new List<Field>();

			if (e.Ok)
			{
				foreach (XmlNode N in e.Response.ChildNodes)
				{
					if (N.LocalName == "query")
					{
						Dictionary<string, bool> HeadersSorted = new Dictionary<string, bool>();
						string Header;

						foreach (XmlNode N2 in N.ChildNodes)
						{
							if (N2.LocalName == "item")
							{
								Dictionary<string, string> Record = new Dictionary<string, string>()
								{
									{ "jid", XML.Attribute((XmlElement)N2, "jid") }
								};

								foreach (XmlNode N3 in N2.ChildNodes)
								{
									if (N3 is XmlElement E)
									{
										Header = E.LocalName;
										Record[Header] = E.InnerText;

										if (!HeadersSorted.ContainsKey(Header))
										{
											HeadersSorted[Header] = true;

											switch (Header)
											{
												case "first":
													Headers.Add(new TextSingleField(null, Header, "First Name", false, null, null, string.Empty,
														new StringDataType(), new BasicValidation(), string.Empty, false, false, false));
													break;

												case "last":
													Headers.Add(new TextSingleField(null, Header, "Last Name", false, null, null, string.Empty,
														new StringDataType(), new BasicValidation(), string.Empty, false, false, false));
													break;

												case "nick":
													Headers.Add(new TextSingleField(null, Header, "Nick Name", false, null, null, string.Empty,
														new StringDataType(), new BasicValidation(), string.Empty, false, false, false));
													break;

												case "email":
													Headers.Add(new TextSingleField(null, Header, "e-Mail", false, null, null, string.Empty,
														new StringDataType(), new BasicValidation(), string.Empty, false, false, false));
													break;

												default:
													Headers.Add(new TextSingleField(null, Header, Header, false, null, null, string.Empty,
														new StringDataType(), new BasicValidation(), string.Empty, false, false, false));
													break;
											}
										}
									}
								}

								Records.Add(Record);
							}
						}
					}
				}
			}

			this.CallResponseMethod(Callback, State, Records, Headers.ToArray(), e);
		}

		private void CallResponseMethod(SearchResultEventHandler Callback, object State, List<Dictionary<string, string>> Records,
			Field[] Headers, IqResultEventArgs e)
		{
			SearchResultEventArgs e2 = new SearchResultEventArgs(Records.ToArray(), Headers, e)
			{
				State = State
			};

			if (Callback != null)
			{
				try
				{
					Callback(this.client, e2);
				}
				catch (Exception ex)
				{
					this.client.Exception(ex);
				}
			}
		}

		private void FormSearchResult(object Sender, IqResultEventArgs e)
		{
			object[] P = (object[])e.State;
			SearchResultEventHandler Callback = (SearchResultEventHandler)P[0];
			object State = P[1];
			List<Dictionary<string, string>> Records = new List<Dictionary<string, string>>();
			List<Field> Headers = new List<Field>();

			if (e.Ok)
			{
				foreach (XmlNode N in e.Response.ChildNodes)
				{
					if (N.LocalName == "query")
					{
						foreach (XmlNode N2 in N.ChildNodes)
						{
							if (N2.LocalName == "x")
							{
								DataForm Form = new DataForm(this.client, (XmlElement)N2, null, null, e.From, e.To);
								Dictionary<string, bool> HeadersSorted = new Dictionary<string, bool>();
								string Header;

								if (Form.Header != null)
								{
									foreach (Field F in Form.Header)
									{
										Header = F.Var;

										if (!HeadersSorted.ContainsKey(Header))
										{
											HeadersSorted[Header] = true;
											Headers.Add(F);
										}
									}
								}

								foreach (Field[] FormRecord in Form.Records)
								{
									Dictionary<string, string> Record = new Dictionary<string, string>();

									foreach (Field FormField in FormRecord)
									{
										Header = FormField.Var;
										Record[Header] = FormField.ValueString;

										if (!HeadersSorted.ContainsKey(Header))
										{
											HeadersSorted[Header] = true;

											Headers.Add(new TextSingleField(null, Header, string.IsNullOrEmpty(FormField.Label) ? Header : FormField.Label,
												false, null, null, string.Empty, new StringDataType(), new BasicValidation(), string.Empty, false, false, false));
										}
									}

									Records.Add(Record);
								}
							}
						}
					}
				}
			}

			this.CallResponseMethod(Callback, State, Records, Headers.ToArray(), e);
		}

		/// <summary>
		/// Performs a synchronous search request
		/// </summary>
		/// <param name="Timeout">Timeout in milliseconds.</param>
		/// <exception cref="TimeoutException">If timeout occurs.</exception>
		public SearchResultEventArgs Search(int Timeout)
		{
			Task<SearchResultEventArgs> Result = this.SearchAsync();

			if (!Result.Wait(Timeout))
				throw new TimeoutException();

			return Result.Result;
		}

		/// <summary>
		/// Performs a synchronous search request
		/// </summary>
		/// <exception cref="TimeoutException">If timeout occurs.</exception>
		public Task<SearchResultEventArgs> SearchAsync()
		{
			TaskCompletionSource<SearchResultEventArgs> Result = new TaskCompletionSource<SearchResultEventArgs>();

			this.SendSearchRequest((sender, e) =>
			{
				if (e.Ok)
					Result.SetResult(e);
				else
					Result.SetException(e.StanzaError ?? new XmppException("Unable to perform search operation."));
			}, null);

			return Result.Task;
		}

	}
}
