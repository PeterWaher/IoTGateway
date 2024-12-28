namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// Contains information about a dynamic record generated when serializing
	/// HTTP/2 headers.
	/// </summary>
	public class DynamicRecord
	{
		/// <summary>
		/// HTTP Header reference
		/// </summary>
		public DynamicHeader Header { get; }

		/// <summary>
		/// HTTP Header Value
		/// </summary>
		public string Value { get; }

		/// <summary>
		/// Length of record in dynamic header table.
		/// </summary>
		public int Length { get; internal set; }

		/// <summary>
		/// Creation ordinal number.
		/// </summary>
		public ulong Ordinal { get; }

		/// <summary>
		/// Contains information about a dynamic record generated when serializing
		/// HTTP/2 headers.
		/// </summary>
		/// <param name="Header">HTTP Header reference</param>
		/// <param name="Value">HTTP Header Value</param>
		/// <param name="Length">Length of record in dynamic header table.</param>
		/// <param name="Ordinal">Creation ordinal number.</param>
		public DynamicRecord(DynamicHeader Header, string Value, int Length, ulong Ordinal)
		{
			this.Header = Header;
			this.Value = Value;
			this.Length = Length;
			this.Ordinal = Ordinal;
		}
	}
}
