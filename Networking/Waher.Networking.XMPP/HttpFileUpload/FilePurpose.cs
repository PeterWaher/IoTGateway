namespace Waher.Networking.XMPP.HttpFileUpload
{
	/// <summary>
	/// Purpose of file uploaded
	/// </summary>
	/// <remarks>
	/// Not part of XEP-0363: HTTP File Upload
	/// </remarks>
	public enum FilePurpose
	{
		/// <summary>
		/// Temporary file.
		/// </summary>
		Temporary,

		/// <summary>
		/// Backup file
		/// </summary>
		Backup,

		/// <summary>
		/// Encrypted Storage
		/// </summary>
		Encrypted,

		/// <summary>
		/// PubSub Storage
		/// </summary>
		PubSub,

		/// <summary>
		/// Used for internal transfer of files between client and server.
		/// </summary>
		InternalTransfer
	}
}
