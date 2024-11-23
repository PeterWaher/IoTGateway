namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// How labels are serialized with regards to indexing.
	/// </summary>
	public enum IndexMode : byte
	{
		/// <summary>
		/// Indexed (§6.2.1 in RFC 7541)
		/// </summary>
		Indexed,

		/// <summary>
		/// Not indexed (§6.2.2 in RFC 7541)
		/// </summary>
		NotIndexed,

		/// <summary>
		/// Never indexed (§6.2.3 in RFC 7541)
		/// </summary>
		NeverIndexed
	}
}
