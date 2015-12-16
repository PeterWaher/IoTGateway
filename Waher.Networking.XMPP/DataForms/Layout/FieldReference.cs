using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.DataForms.Layout
{
	/// <summary>
	/// Class managing a field reference.
	/// </summary>
	public class FieldReference : LayoutElement
	{
		private string var;

		internal FieldReference(XmlElement E)
		{
			this.var = XmppClient.XmlAttribute(E, "var");
		}

		internal FieldReference(string Var)
		{
			this.var = Var;
		}

		/// <summary>
		/// Variable name
		/// </summary>
		public string Var { get { return this.var; } }

	}
}
