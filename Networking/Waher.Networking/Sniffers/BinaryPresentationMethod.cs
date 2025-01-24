namespace Waher.Networking.Sniffers
{
	/// <summary>
	/// How binary data is to be presented.
	/// </summary>
	public enum BinaryPresentationMethod
	{
		/// <summary>
		/// Has hexadecimal strings.
		/// </summary>
		Hexadecimal,

		/// <summary>
		/// Has base64 strings.
		/// </summary>
		Base64,

		/// <summary>
		/// Has simple byte counts.
		/// </summary>
		ByteCount
	}
}
