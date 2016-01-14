using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Content;

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
			this.var = XML.Attribute(E, "var");
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
