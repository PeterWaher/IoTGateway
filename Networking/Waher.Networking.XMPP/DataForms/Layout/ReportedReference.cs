using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP.DataForms.Layout
{
	/// <summary>
	/// Class managing a reported section reference.
	/// </summary>
	public class ReportedReference : LayoutElement
	{
		internal ReportedReference()
		{
		}

		/// <summary>
		/// Exports the form to XAML.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Form">Data form containing element.</param>
		public override void ExportXAML(XmlWriter Output, DataForm Form)
		{
			// TODO: Include table of results.
		}
	}
}
