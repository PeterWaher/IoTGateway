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
		/// <param name="Form">Data Form.</param>
		/// <param name="Label">Label</param>
		/// <param name="ChildElements">Child elements.</param>
		public Page(DataForm Form, string Label, params LayoutElement[] ChildElements)
			: base(Form, Label, ChildElements)
		{
		}

		/// <summary>
		/// Class managing a page in a data form layout.
		/// </summary>
		/// <param name="Form">Data Form.</param>
		/// <param name="Label">Label</param>
		/// <param name="Fields">Fields to include in section. These will be converted to <see cref="FieldReference"/> objects.</param>
		public Page(DataForm Form, string Label, Field[] Fields)
			: base(Form, Label, Fields)
		{
		}

		internal Page(DataForm Form, XmlElement E)
			: base(Form, E)
		{
		}

		internal Page(DataForm Form, string Title, ReportedReference ReportedReference)
			: base(Form, Title, ReportedReference)
		{
		}
	}
}
