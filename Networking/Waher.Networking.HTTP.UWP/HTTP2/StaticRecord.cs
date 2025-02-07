using System.Text;

namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// Static record.
	/// </summary>
	public class StaticRecord
	{
		/// <summary>
		/// Index in static table.
		/// </summary>
		public byte Index { get; }

		/// <summary>
		/// Header
		/// </summary>
		public string Header { get; }

		/// <summary>
		/// Value
		/// </summary>
		public string Value { get; }

		/// <summary>
		/// Length of header.
		/// </summary>
		public int HeaderLength { get; }

		/// <summary>
		/// Static record.
		/// </summary>
		/// <param name="Index">Index in static table.</param>
		/// <param name="Header">Header</param>
		public StaticRecord(byte Index, string Header)
			: this(Index, Header, null)
		{
		}

		/// <summary>
		/// Static record.
		/// </summary>
		/// <param name="Index">Index in static table.</param>
		/// <param name="Header">Header</param>
		/// <param name="Value">Value</param>
		public StaticRecord(byte Index, string Header, string Value)
		{
			this.Index = Index;
			this.Header = Header;
			this.HeaderLength = Encoding.UTF8.GetBytes(Header).Length;
			this.Value = Value;
		}
	}
}
