using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Content.Xml;
using Waher.Networking.XMPP.DataForms;

namespace Waher.Networking.XMPP
{
	/// <summary>
	/// Event arguments for message events.
	/// </summary>
	public class MessageFormEventArgs : MessageEventArgs
	{
		private readonly DataForm form;
		private readonly string formType;

		/// <summary>
		/// Event arguments for message events.
		/// </summary>
		/// <param name="Form">Form embedded in message.</param>
		/// <param name="FormType">Type of form, as defined by the FORM_TYPE field (if available in the form).</param>
		/// <param name="e">Values are taken from this object.</param>
		public MessageFormEventArgs(DataForm Form, string FormType, MessageEventArgs e)
			: base(e)
		{
			this.form = Form;
			this.formType = FormType;
		}

		/// <summary>
		/// Form embedded in message.
		/// </summary>
		public DataForm Form
		{
			get { return this.form; }
		}

		/// <summary>
		/// Type of form, as defined by the FORM_TYPE field (if available in the form).
		/// </summary>
		public string FormType
		{
			get { return this.formType; }
		}
	}
}
