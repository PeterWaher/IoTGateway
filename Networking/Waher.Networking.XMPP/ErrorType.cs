namespace Waher.Networking.XMPP
{
	/// <summary>
	/// Error Type
	/// </summary>
	public enum ErrorType
	{
		/// <summary>
		/// No error
		/// </summary>
		None,

		/// <summary>
		/// Retry after providing credentials
		/// </summary>
		Auth,

		/// <summary>
		/// Do not retry (the error cannot be remedied)
		/// </summary>
		Cancel,

		/// <summary>
		/// Proceed (the condition was only a warning)
		/// </summary>
		Continue,

		/// <summary>
		/// Retry after changing the data sent
		/// </summary>
		Modify,

		/// <summary>
		/// Retry after waiting (the error is temporary)
		/// </summary>
		Wait,

		/// <summary>
		/// Undefined error type
		/// </summary>
		Undefined
	}
}
