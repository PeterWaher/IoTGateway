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
	}
}
