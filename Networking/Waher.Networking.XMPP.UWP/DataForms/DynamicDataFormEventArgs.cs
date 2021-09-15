using System;

namespace Waher.Networking.XMPP.DataForms
{
	/// <summary>
	/// Event arguments for dynamic data form events.
	/// </summary>
	public class DynamicDataFormEventArgs : MessageEventArgs
	{
		private readonly DataForm form;
		private readonly string sessionVariable;
		private readonly string language;

		internal DynamicDataFormEventArgs(DataForm Form, string SessionVariable, string Language, MessageEventArgs e)
			: base(e)
		{
			this.form = Form;
			this.sessionVariable = SessionVariable;
			this.language = Language;
		}

		/// <summary>
		/// New updated form. This form has to be joined to the previous form using the <see cref="DataForm.Join"/> method.
		/// </summary>
		public DataForm Form { get { return this.form; } }

		/// <summary>
		/// Session Variable used to identify the form that has been updated.
		/// </summary>
		public string SessionVariable { get { return this.sessionVariable; } }

		/// <summary>
		/// Optional language of form.
		/// </summary>
		public string Language { get { return this.language; } }
	}
}
