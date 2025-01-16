namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// Priority settings waiting to be applied.
	/// </summary>
	internal class PrioritySettings
	{
		/// <summary>
		/// RFC 9218 priority settings.
		/// </summary>
		public int? Rfc9218Priority;

		/// <summary>
		/// RFC 9218 incremental setting
		/// </summary>
		public bool? Rfc9218Incremental;

	}
}
