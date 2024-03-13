using Waher.Runtime.Inventory;

namespace Waher.Networking.HTTP.ContentEncodings
{
	/// <summary>
	/// Interface for content encodings in HTTP transfers.
	/// </summary>
	public interface IContentEncoding : IProcessingSupport<string>
	{
		/// <summary>
		/// Label identifying the Content-Encoding
		/// </summary>
		string Label { get; }

		/// <summary>
		/// Gets an encoder.
		/// </summary>
		/// <param name="Output">Output stream.</param>
		/// <returns>Encoder</returns>
		TransferEncoding GetEncoder(TransferEncoding Output);
	}
}
