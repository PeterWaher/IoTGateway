namespace Waher.Networking.XMPP.DataForms
{
	/// <summary>
	/// Type of data form.
	/// </summary>
	public enum FormType
	{
		/// <summary>
		/// Data Form
		/// </summary>
		Form,

		/// <summary>
		/// Form cancellation
		/// </summary>
		Cancel,

		/// <summary>
		/// Form Result
		/// </summary>
		Result,

		/// <summary>
		/// Form submission
		/// </summary>
		Submit,

		/// <summary>
		/// Undefined form type.
		/// </summary>
		Undefined
	}
}
