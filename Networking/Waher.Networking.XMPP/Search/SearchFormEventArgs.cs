using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.DataForms.DataTypes;
using Waher.Networking.XMPP.DataForms.FieldTypes;
using Waher.Networking.XMPP.DataForms.ValidationMethods;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Search
{
	/// <summary>
	/// Event arguments for search form responses.
	/// </summary>
	public class SearchFormEventArgs : IqResultEventArgs
	{
		private readonly XmppClient client;
		private readonly DataForm searchForm;
		private readonly string instructions;
		private string first;
		private string last;
		private string nick;
		private string email;
		private readonly bool supportsForms;

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
		public DataForm SearchForm => this.searchForm;

		/// <summary>
		/// Form instructions
		/// </summary>
		public string Instructions
		{
			get
			{
				if (!string.IsNullOrEmpty(this.instructions))
					return this.instructions;

				if (!(this.searchForm is null))
					return XmppClient.Concat(this.searchForm.Instructions);

				return string.Empty;
			}
		}

		/// <summary>
		/// First name
		/// </summary>
		public string FirstName => this.GetField(this.first, "first");

		/// <summary>
		/// Last name
		/// </summary>
		public string LastName => this.GetField(this.last, "last");

		/// <summary>
		/// Nick name
		/// </summary>
		public string NickName => this.GetField(this.nick, "nick");

		/// <summary>
		/// EMail 
		/// </summary>
		public string EMail => this.GetField(this.email, "email");

		/// <summary>
		/// Sets the first name
		/// </summary>
		public Task SetFirstName(string Value)
		{
			this.first = Value;
			return this.SetField("first", Value);
		}

		/// <summary>
		/// Sets the last name
		/// </summary>
		public Task SetLastName(string Value)
		{
			this.last = Value;
			return this.SetField("last", Value);
		}

		/// <summary>
		/// Sets the nick name
		/// </summary>
		public Task SetNickName(string Value)
		{
			this.nick = Value;
			return this.SetField("nick", Value);
		}

		/// <summary>
		/// Sets the email address
		/// </summary>
		public Task SetEMail(string Value)
		{
			this.email = Value;
			return this.SetField("email", Value);
		}

		/// <summary>
		/// If the remote end supports search forms, or if the form was constructed on the client side.
		/// </summary>
		public bool SupportsForms => this.supportsForms;

		private string GetField(string FixedValue, string Var)
		{
			if (!string.IsNullOrEmpty(FixedValue))
				return FixedValue;

			if (!(this.searchForm is null))
			{
				Field Field = this.searchForm[Var];
				if (!(Field is null))
					return Field.ValueString;
			}

			return string.Empty;
		}

		private async Task SetField(string Var, string Value)
		{
			if (!(this.searchForm is null))
			{
				Field Field = this.searchForm[Var];
				if (!(Field is null))
					await Field.SetValue(Value);
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
		public Task SendSearchRequest(EventHandlerAsync<SearchResultEventArgs> Callback, object State)
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

				return this.client.SendIqSet(this.From, Xml.ToString(), this.OldSearchResult, new object[] { Callback, State });
			}
			else
			{
				this.searchForm.SerializeSubmit(Xml);

				Xml.Append("</query>");

				return this.client.SendIqSet(this.From, Xml.ToString(), this.FormSearchResult, new object[] { Callback, State });
			}
		}

		private Task OldSearchResult(object Sender, IqResultEventArgs e)
		{
			object[] P = (object[])e.State;
			EventHandlerAsync<SearchResultEventArgs> Callback = (EventHandlerAsync<SearchResultEventArgs>)P[0];
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

			return this.CallResponseMethod(Callback, State, Records, Headers.ToArray(), e);
		}

		private async Task CallResponseMethod(EventHandlerAsync<SearchResultEventArgs> Callback, object State, List<Dictionary<string, string>> Records,
			Field[] Headers, IqResultEventArgs e)
		{
			SearchResultEventArgs e2 = new SearchResultEventArgs(Records.ToArray(), Headers, e)
			{
				State = State
			};

			if (!(Callback is null))
			{
				try
				{
					await Callback(this.client, e2);
				}
				catch (Exception ex)
				{
					await this.client.Exception(ex);
				}
			}
		}

		private Task FormSearchResult(object Sender, IqResultEventArgs e)
		{
			object[] P = (object[])e.State;
			EventHandlerAsync<SearchResultEventArgs> Callback = (EventHandlerAsync<SearchResultEventArgs>)P[0];
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

								if (!(Form.Header is null))
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

			return this.CallResponseMethod(Callback, State, Records, Headers.ToArray(), e);
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
		public async Task<SearchResultEventArgs> SearchAsync()
		{
			TaskCompletionSource<SearchResultEventArgs> Result = new TaskCompletionSource<SearchResultEventArgs>();

			await this.SendSearchRequest((sender, e) =>
			{
				if (e.Ok)
					Result.SetResult(e);
				else
					Result.SetException(e.StanzaError ?? new XmppException("Unable to perform search operation."));

				return Task.CompletedTask;
			}, null);

			return await Result.Task;
		}

	}
}
