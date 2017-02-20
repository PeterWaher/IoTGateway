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
		internal ReportedReference(DataForm Form)
			: base(Form)
		{
		}

		internal override bool RemoveExcluded()
		{
			return false;
		}
	}
}
