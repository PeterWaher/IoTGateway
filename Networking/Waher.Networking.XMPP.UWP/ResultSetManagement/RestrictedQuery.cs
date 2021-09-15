using System;
using System.Text;
using System.Xml;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.ResultSetManagement
{
	/// <summary>
	/// Contains information about a restricted query, as deinfed in XEP-0059: Result Set Management
	/// </summary>
    public class RestrictedQuery
    {
		/// <summary>
		/// http://jabber.org/protocol/rsm
		/// </summary>
		public const string NamespaceResultSetManagement = "http://jabber.org/protocol/rsm";

		private string after = null;
		private string before = null;
		private int? index = null;
		private int? max = null;

		/// <summary>
		/// Contains information about a restricted query, as deinfed in XEP-0059: Result Set Management
		/// </summary>
		public RestrictedQuery()
		{
		}

		/// <summary>
		/// Contains information about a restricted query, as deinfed in XEP-0059: Result Set Management
		/// </summary>
		/// <param name="After">Return items after this key.</param>
		/// <param name="Before">Return items before this key.</param>
		/// <param name="Index">Request results from this index.</param>
		/// <param name="Max">If result set should be limited in size.</param>
		public RestrictedQuery(string After, string Before, int? Index, int? Max)
		{
			this.after = After;
			this.before = Before;
			this.index = Index;
			this.max = Max;
		}

		/// <summary>
		/// Contains information about a restricted query, as deinfed in XEP-0059: Result Set Management
		/// </summary>
		/// <param name="Rsm">XML definition</param>
		public RestrictedQuery(XmlElement Rsm)
		{
			foreach (XmlNode N in Rsm.ChildNodes)
			{
				if (N is XmlElement E)
				{
					switch (E.LocalName)
					{
						case "after":
							this.after = E.InnerText;
							break;

						case "before":
							this.before = E.InnerText;
							break;

						case "index":
							if (int.TryParse(E.InnerText, out int i))
								this.index = i;
							break;

						case "max":
							if (int.TryParse(E.InnerText, out i))
								this.max = i;
							break;
					}
				}
			}
		}

		/// <summary>
		/// Return items after this key.
		/// </summary>
		public string After
		{
			get { return this.after; }
			set { this.after = value; }
		}

		/// <summary>
		/// Return items before this key.
		/// </summary>
		public string Before
		{
			get { return this.before; }
			set { this.before = value; }
		}

		/// <summary>
		/// Request results from this index.
		/// </summary>
		public int? Index
		{
			get { return this.index; }
			set { this.index = value; }
		}

		/// <summary>
		/// If result set should be limited in size.
		/// </summary>
		public int? Max
		{
			get { return this.max; }
			set { this.max = value; }
		}

		/// <summary>
		/// Searches for a restricted query, by traversing siblings.
		/// </summary>
		/// <param name="FirstSibling">First sibling.</param>
		/// <returns>Restricted query, if found, null otherwise.</returns>
		public static RestrictedQuery IsRestricted(XmlNode FirstSibling)
		{
			while (FirstSibling != null && (FirstSibling.LocalName != "set" || FirstSibling.NamespaceURI != NamespaceResultSetManagement))
				FirstSibling = FirstSibling.NextSibling;

			if (FirstSibling != null && FirstSibling is XmlElement E)
				return new RestrictedQuery(E);
			else
				return null;
		}

		/// <summary>
		/// Appends pagination information to an XML request.
		/// </summary>
		/// <param name="Xml">XML output.</param>
		public void Append(StringBuilder Xml)
		{
			Append(Xml, this.after, this.before, this.index, this.max);
		}

		/// <summary>
		/// Appends pagination information to an XML request.
		/// </summary>
		/// <param name="Xml">XML output.</param>
		/// <param name="After">Fetch records after this record.</param>
		/// <param name="Before">Fetch records before this record.</param>
		/// <param name="Index">Fetch records starting at this index.</param>
		/// <param name="MaxCount">Return at most this number of items.</param>
		public static void Append(StringBuilder Xml, string After, string Before, int? Index, int? MaxCount)
		{
			Xml.Append("<set xmlns='");
			Xml.Append(NamespaceResultSetManagement);
			Xml.Append("'>");

			if (!(After is null))
			{
				Xml.Append("<after>");
				Xml.Append(XML.Encode(After));
				Xml.Append("</after>");
			}

			if (!(Before is null))
			{
				Xml.Append("<before>");
				Xml.Append(XML.Encode(Before));
				Xml.Append("</before>");
			}

			if (Index.HasValue)
			{
				Xml.Append("<index>");
				Xml.Append(Index.Value.ToString());
				Xml.Append("</index>");
			}

			if (MaxCount.HasValue)
			{
				Xml.Append("<max>");
				Xml.Append(MaxCount.Value.ToString());
				Xml.Append("</max>");
			}

			Xml.Append("</set>");
		}
	}
}
