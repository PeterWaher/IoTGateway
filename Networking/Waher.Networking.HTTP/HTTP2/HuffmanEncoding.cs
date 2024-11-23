namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// Information about how to encode an item
	/// </summary>
	public class HuffmanEncoding
	{
		/// <summary>
		/// Binary value.
		/// </summary>
		public readonly uint Value;

		/// <summary>
		/// Number of bits of <see cref="Value"/>.
		/// </summary>
		public readonly byte NrBits;

		/// <summary>
		/// Information about how to encode an item
		/// </summary>
		/// <param name="Value">Binary value.</param>
		/// <param name="NrBits">Number of bits of <paramref name="Value"/>.</param>
		public HuffmanEncoding(uint Value, byte NrBits)
		{
			this.Value = Value;
			this.NrBits = NrBits;
		}
	}
}
