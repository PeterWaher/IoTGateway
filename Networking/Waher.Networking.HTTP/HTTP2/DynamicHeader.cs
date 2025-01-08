using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// Contains information about a header containing dynamic records generated 
	/// when serializing HTTP/2 headers.
	/// </summary>
	public class DynamicHeader
	{
		internal readonly Dictionary<string, DynamicRecord> Values = new Dictionary<string, DynamicRecord>();

		/// <summary>
		/// HTTP Header (lower-case)
		/// </summary>
		public string Header { get; }

		/// <summary>
		/// Creation ordinal number of first record.
		/// </summary>
		public ulong Ordinal { get; }

		/// <summary>
		/// Length of header string.
		/// </summary>
		public uint HeaderLength { get; }

		/// <summary>
		/// Contains information about a header containing dynamic records generated 
		/// when serializing HTTP/2 headers.
		/// </summary>
		/// <param name="Header">HTTP Header (lower-case)</param>
		/// <param name="Ordinal">Creation ordinal number of first record.</param>
		public DynamicHeader(string Header, ulong Ordinal)
		{
			this.Header = Header;
			this.Ordinal = Ordinal;
			this.HeaderLength = (uint)Encoding.UTF8.GetBytes(Header).Length;
		}
	}
}
