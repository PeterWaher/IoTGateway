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

		/// <summary>
		/// Exports the form to XAML.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Form">Data form containing element.</param>
		public override void ExportXAML(XmlWriter Output, DataForm Form)
		{
			Field Field = Form[this.var];
			if (Field != null)
				Field.ExportXAML(Output, Form);
		}

	}
}
