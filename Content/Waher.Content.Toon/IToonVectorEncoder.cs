using System.Text;

namespace Waher.Content.Toon
{
	/// <summary>
	/// Interface for encoding objects of certain types to TOON.
	/// </summary>
	public interface IToonVectorEncoder : IToonEncoder 
	{
		/// <summary>
		/// Gets the number of elements in the vector.
		/// </summary>
		/// <param name="Vector">Vector object.</param>
		/// <returns>Number of elements in the vector. If null is returned, the
		/// <paramref name="Vector"/> item should not be considered a vector.</returns>
		int? GetCount(object Vector);

		/// <summary>
		/// Encodes the <paramref name="Object"/> to TOON.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Indent">Any indentation to apply.</param>
		/// <param name="Toon">TOON output.</param>
		/// <param name="UseBrackets">If brackets should be used around the vector.</param>
		void Encode(object Object, int? Indent, StringBuilder Toon, bool UseBrackets);
	}
}
