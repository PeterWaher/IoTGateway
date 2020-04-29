using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.DataForms.Layout
{
	/// <summary>
	/// Class managing a field reference.
	/// </summary>
	public class FieldReference : LayoutElement
	{
		private readonly string var;

		/// <summary>
		/// Class managing a field reference.
		/// </summary>
		/// <param name="Form">Data form.</param>
		/// <param name="Var">Variable name.</param>
		public FieldReference(DataForm Form, string Var)
			: base(Form)
		{
			this.var = Var;
		}

		internal FieldReference(DataForm Form, XmlElement E)
			: base(Form)
		{
			this.var = XML.Attribute(E, "var");
		}

		/// <summary>
		/// Variable name
		/// </summary>
		public string Var { get { return this.var; } }

		internal override bool RemoveExcluded()
		{
			Field F = this.Form[this.var];
			return F is null || F.Exclude;
		}

		internal override void Serialize(StringBuilder Output)
		{
			Output.Append("<xdl:fieldref var='");
			Output.Append(XML.Encode(this.var));
			Output.Append("'/>");
		}
	}
}
