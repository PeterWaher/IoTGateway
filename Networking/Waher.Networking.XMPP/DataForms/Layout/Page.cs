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
		/// <summary>
		/// Class managing a page in a data form layout.
		/// </summary>
		/// <param name="Label">Label</param>
		/// <param name="ChildElements">Child elements.</param>
		public Page(string Label, params LayoutElement[] ChildElements)
			: base(Label, ChildElements)
		{
		}

		/// <summary>
		/// Class managing a page in a data form layout.
		/// </summary>
		/// <param name="Label">Label</param>
		/// <param name="Fields">Fields to include in section. These will be converted to <see cref="FieldReference"/> objects.</param>
		public Page(string Label, Field[] Fields)
			: base(Label, Fields)
		{
		}

		internal Page(XmlElement E)
			: base(E)
		{
		}

		internal Page(string Title, ReportedReference ReportedReference)
			: base(Title, ReportedReference)
		{
		}
	}
}
