using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.DataForms
{
	/// <summary>
	/// Event arguments for data form results.
	/// </summary>
	public class DataFormEventArgs : IqResultEventArgs
	{
		private readonly DataForm form;

		/// <summary>
		/// Event arguments for data form results.
		/// </summary>
		/// <param name="Form">Data form.</param>
		/// <param name="e">IQ result event arguments.</param>
		public DataFormEventArgs(DataForm Form, IqResultEventArgs e)
			: base(e)
		{
			this.form = Form;
		}

		/// <summary>
		/// Form result, if available.
		/// </summary>
		public DataForm Form { get { return this.form; } }
	}
}
