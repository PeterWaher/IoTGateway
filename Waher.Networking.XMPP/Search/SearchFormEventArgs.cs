using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Networking.XMPP.DataForms;

namespace Waher.Networking.XMPP.Search
{
	/// <summary>
	/// Delegate for search form events or callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void SearchFormEventHandler(XmppClient Sender, SearchFormEventArgs e);

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

		internal SearchFormEventArgs(XmppClient Client, IqResultEventArgs e, string Instructions, string First, string Last, string Nick, string EMail,
			DataForm SearchForm)
			: base(e)
		{
			this.client = Client;
			this.instructions = Instructions;
			this.first = First;
			this.last = Last;
			this.nick = Nick;
			this.email = EMail;
			this.searchForm = SearchForm;
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

			if (this.searchForm == null)
			{
				if (!string.IsNullOrEmpty(this.first))
				{
					Xml.Append("<first>");
					Xml.Append(XmppClient.XmlEncode(this.first));
					Xml.Append("</first>");
				}

				if (!string.IsNullOrEmpty(this.last))
				{
					Xml.Append("<last>");
					Xml.Append(XmppClient.XmlEncode(this.last));
					Xml.Append("</last>");
				}

				if (!string.IsNullOrEmpty(this.nick))
				{
					Xml.Append("<nick>");
					Xml.Append(XmppClient.XmlEncode(this.nick));
					Xml.Append("</nick>");
				}

				if (!string.IsNullOrEmpty(this.email))
				{
					Xml.Append("<email>");
					Xml.Append(XmppClient.XmlEncode(this.email));
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

		private void OldSearchResult(XmppClient Sender, IqResultEventArgs e)
		{
			object[] P = (object[])e.State;
			SearchResultEventHandler Callback = (SearchResultEventHandler)P[0];
			object State = (object)P[1];
			List<Dictionary<string, string>> Records = new List<Dictionary<string, string>>();

			if (e.Ok)
			{
				foreach (XmlNode N in e.Response.ChildNodes)
				{
					if (N.LocalName == "query")
					{
						foreach (XmlNode N2 in N.ChildNodes)
						{
							if (N2.LocalName == "item")
							{
								Dictionary<string, string> Record = new Dictionary<string, string>();
								Record["jid"] = XmppClient.XmlAttribute((XmlElement)N2, "jid");

								foreach (XmlNode N3 in N2.ChildNodes)
								{
									XmlElement E = N3 as XmlElement;
									if (E != null)
										Record[E.LocalName] = E.InnerText;
								}

								Records.Add(Record);
							}
						}
					}
				}
			}

			this.CallResponseMethod(Callback, State, Records, e);
		}

		private void CallResponseMethod(SearchResultEventHandler Callback, object State, List<Dictionary<string, string>> Records, IqResultEventArgs e)
		{
			SearchResultEventArgs e2 = new SearchResultEventArgs(Records.ToArray(), e);
			e2.State = State;

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

		private void FormSearchResult(XmppClient Sender, IqResultEventArgs e)
		{
			object[] P = (object[])e.State;
			SearchResultEventHandler Callback = (SearchResultEventHandler)P[0];
			object State = (object)P[1];
			List<Dictionary<string, string>> Records = new List<Dictionary<string, string>>();
			
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
								Dictionary<string, string> Record = new Dictionary<string, string>();

								foreach (Field[] FormRecord in Form.Records)
								{
									foreach (Field FormField in FormRecord)
										Record[FormField.Var] = FormField.ValueString;
								}

								Records.Add(Record);
							}
						}
					}
				}
			}

			this.CallResponseMethod(Callback, State, Records, e);
		}

		/// <summary>
		/// Performs a search request
		/// </summary>
		/// <param name="Timeout">Timeout in milliseconds.</param>
		/// <exception cref="TimeoutException">If timeout occurs.</exception>
		public SearchResultEventArgs Search(int Timeout)
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			SearchResultEventArgs e = null;

			try
			{
				this.SendSearchRequest((sender, e2) =>
				{
					e = e2;
					Done.Set();
				}, null);

				if (!Done.WaitOne(Timeout))
					throw new TimeoutException();
			}
			finally
			{
				Done.Close();
			}

			if (!e.Ok)
				throw e.StanzaError;

			return e;
		}

	}
}
