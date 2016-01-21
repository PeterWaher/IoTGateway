using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Networking.XMPP.DataForms.Layout
{
	/// <summary>
	/// Base class for all layout elements in a data form layout.
	/// </summary>
	public abstract class LayoutElement
	{
		internal LayoutElement()
		{
		}

		/// <summary>
		/// Exports the element to XAML.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Form">Data form containing element.</param>
		public abstract void ExportXAML(XmlWriter Output, DataForm Form);
	}
}
