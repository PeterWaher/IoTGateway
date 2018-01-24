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
		private DataForm form;

		internal LayoutElement(DataForm Form)
		{
			this.form = Form;
		}

		/// <summary>
		/// Data Form.
		/// </summary>
		public DataForm Form
		{
			get { return this.form; }
		}

		internal abstract bool RemoveExcluded();

		internal abstract void Serialize(StringBuilder Output);
	}
}
