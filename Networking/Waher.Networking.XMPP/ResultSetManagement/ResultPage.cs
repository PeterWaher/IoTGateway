using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.ResultSetManagement
{
	/// <summary>
	/// Contains information about a result page, as deinfed in XEP-0059: Result Set Management
	/// </summary>
	public class ResultPage
	{
		private string first = null;
		private string last = null;
		private int? count = null;
		private int? firstIndex = null;

		/// <summary>
		/// Contains information about a restricted query, as deinfed in XEP-0059: Result Set Management
		/// </summary>
		/// <param name="Rsm">XML definition</param>
		public ResultPage(XmlElement Rsm)
		{
			foreach (XmlNode N in Rsm.ChildNodes)
			{
				if (N is XmlElement E)
				{
					switch (E.LocalName)
					{
						case "count":
							if (int.TryParse(E.InnerText, out int i))
								this.count = i;
							break;

						case "first":
							this.first = E.InnerText;
							if (E.HasAttribute("index") && int.TryParse(E["index"].Value, out i))
								this.firstIndex = i;
							break;

						case "last":
							this.last = E.InnerText;
							break;
					}
				}
			}
		}

		/// <summary>
		/// First item/key in response.
		/// </summary>
		public string First
		{
			get { return this.first; }
		}

		/// <summary>
		/// Last item/key in response.
		/// </summary>
		public string Last
		{
			get { return this.last; }
		}

		/// <summary>
		/// Total number of items in result set.
		/// </summary>
		public int? Count
		{
			get { return this.count; }
		}

		/// <summary>
		/// Index of first response.
		/// </summary>
		public int? FirstIndex
		{
			get { return this.firstIndex; }
		}

		/// <summary>
		/// Checks if the result is paginated.
		/// </summary>
		/// <param name="FirstSibling">First sibling.</param>
		/// <returns>Pagination, if found, null otherwise.</returns>
		public static ResultPage IsPaginated(XmlNode FirstSibling)
		{
			while (FirstSibling != null && (FirstSibling.LocalName != "set" || FirstSibling.NamespaceURI != RestrictedQuery.NamespaceResultSetManagement))
				FirstSibling = FirstSibling.NextSibling;

			if (FirstSibling != null && FirstSibling is XmlElement E)
				return new ResultPage(E);
			else
				return null;
		}

		/// <summary>
		/// Appends pagination information to an XML request.
		/// </summary>
		/// <param name="Xml">XML output.</param>
		public void Append(StringBuilder Xml)
		{
			Append(Xml, this.first, this.firstIndex, this.last, this.count);
		}

		/// <summary>
		/// Appends pagination information to an XML result response.
		/// </summary>
		/// <param name="Xml">XML output.</param>
		/// <param name="First">First element.</param>
		/// <param name="FirstIndex">Index of first element.</param>
		/// <param name="Last">Last element.</param>
		/// <param name="Count">Number of items in result set.</param>
		public static void Append(StringBuilder Xml, string First, int? FirstIndex, string Last, int? Count)
		{
			Xml.Append("<set xmlns='");
			Xml.Append(RestrictedQuery.NamespaceResultSetManagement);
			Xml.Append("'>");

			if (First != null)
			{
				Xml.Append("<first");

				if (FirstIndex.HasValue)
				{
					Xml.Append(" index='");
					Xml.Append(FirstIndex.Value.ToString());
					Xml.Append('\'');
				}

				Xml.Append('>');
				Xml.Append(XML.Encode(First));
				Xml.Append("</first>");
			}

			if (Last != null)
			{
				Xml.Append("<last>");
				Xml.Append(XML.Encode(Last));
				Xml.Append("</last>");
			}

			if (Count.HasValue)
			{
				Xml.Append("<count>");
				Xml.Append(Count.Value.ToString());
				Xml.Append("</count>");
			}

			Xml.Append("</set>");
		}
	}
}
