namespace Waher.Security.DTLS
{
	/// <summary>
	/// Interface for user credentials.
	/// </summary>
    public interface IDtlsCredentials
    {
		/// <summary>
		/// UTF-8 encoded Identity.
		/// </summary>
		byte[] Identity { get; }

		/// <summary>
		/// Identity string.
		/// </summary>
		string IdentityString { get; }
    }
}
