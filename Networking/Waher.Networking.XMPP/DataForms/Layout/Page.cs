using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.DataForms.Layout
{
	/// <summary>
	/// Class managing a page in a data form layout.
	/// </summary>
	public class Page : Section
	{
		internal Page(XmlElement E)
			: base(E)
		{
		}

		internal Page(string Title, Field[] Fields)
			: base(Title, Fields)
		{
		}

		internal Page(string Title, ReportedReference ReportedReference)
			: base(Title, ReportedReference)
		{
		}
	}
}
