namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// Information about how to decode an item
	/// </summary>
	public class HuffmanDecoding
	{
		/// <summary>
		/// Huffman decoding node, if 0 is decoded.
		/// </summary>
		public HuffmanDecoding Zero;

		/// <summary>
		/// Huffman decoding node, if 1 is decoded.
		/// </summary>
		public HuffmanDecoding One;

		/// <summary>
		/// If node is part of the End of String (EOS) encoding.
		/// </summary>
		public bool PartOfEoS;

		/// <summary>
		/// If node is a leaf node.
		/// </summary>
		public bool LeafNode;

		/// <summary>
		/// Value, if node is a leaf node.
		/// </summary>
		public byte? Value;

		/// <summary>
		/// Information about how to decode an item
		/// </summary>
		public HuffmanDecoding()
		{
			this.Zero = null;
			this.One = null;
			this.PartOfEoS = false;
			this.Value = null;
			this.LeafNode = false;
		}

		/// <summary>
		/// Information about how to decode an item
		/// </summary>
		/// <param name="Value">Binary value.</param>
		/// <param name="PartOfEoS">If node is part of the EoS encoding.</param>
		public HuffmanDecoding(byte Value, bool PartOfEoS)
		{
			this.Zero = null;
			this.One = null;
			this.PartOfEoS = PartOfEoS;
			this.Value = Value;
			this.LeafNode = true;
		}
	}
}
