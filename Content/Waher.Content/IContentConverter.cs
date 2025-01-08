using Waher.Runtime.Inventory;
using System.Threading.Tasks;

namespace Waher.Content
{
	/// <summary>
	/// Basic interface for Internet Content encoders. A class implementing this interface and having a default constructor, will be able
	/// to partake in object encodings through the static <see cref="InternetContent"/> class. No registration is required.
	/// </summary>
	public interface IContentConverter
	{
		/// <summary>
		/// Converts content from these content types.
		/// </summary>
		string[] FromContentTypes
		{
			get;
		}

		/// <summary>
		/// Converts content to these content types. Return an array of "*" to signify content can be converted to any
		/// (or at the time, an unknown) content type.
		/// </summary>
		string[] ToContentTypes
		{
			get;
		}

		/// <summary>
		/// How well the content is converted.
		/// </summary>
		Grade ConversionGrade
		{
			get;
		}

		/// <summary>
		/// Performs the actual conversion.
		/// </summary>
		/// <param name="State">State of the current conversion.</param>
		/// <param name="Progress">Optional progress reporting of encoding/decoding. Can be null.</param>
		/// <returns>If the result is dynamic (true), or only depends on the source (false).</returns>
		Task<bool> ConvertAsync(ConversionState State, ICodecProgress Progress);
	}
}
