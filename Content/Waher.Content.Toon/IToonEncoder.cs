using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Waher.Runtime.Inventory;

namespace Waher.Content.Toon
{
	/// <summary>
	/// Interface for encoding objects of certain types to TOON.
	/// </summary>
	public interface IToonEncoder : IProcessingSupport<Type>
	{
		/// <summary>
		/// Encodes the <paramref name="Object"/> to TOON.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Indent">Any indentation to apply.</param>
		/// <param name="Toon">TOON output.</param>
		void Encode(object Object, int? Indent, ToonOutput Toon);

		/// <summary>
		/// If the encoder encodes values into multiple rows.
		/// </summary>
		bool EncodesMultipleRows { get; }

		/// <summary>
		/// If the encoder encodes vectors.
		/// </summary>
		bool EncodesVectors { get; }
		
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
		void Encode(object Object, int? Indent, ToonOutput Toon, bool UseBrackets);

		/// <summary>
		/// Gets an enumerator for the child-elements of an object.
		/// </summary>
		/// <param name="Object">Object to get child-elements from.</param>
		/// <returns>Enumerator for the child-elements, or null if not a vector.</returns>
		IEnumerator GetElements(object Object);

		/// <summary>
		/// Gets available parameters to encode from an object.
		/// </summary>
		/// <param name="Object">Object to get parameters from.</param>
		/// <returns>Enumerator for the parameters, or null if not applicable.</returns>
		IEnumerator<KeyValuePair<string, object>> GetParameters(object Object);
	}
}
