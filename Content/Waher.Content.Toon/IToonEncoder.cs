using System;
using System.Collections;
using System.Collections.Generic;
using Waher.Runtime.Inventory;

namespace Waher.Content.Toon
{
	/// <summary>
	/// How brackets should be handled when encoding vectors.
	/// </summary>
	public enum BracketsMode
	{
		/// <summary>
		/// Embed vector inside brackets
		/// </summary>
		Embed,

		/// <summary>
		/// Ignore brackets
		/// </summary>
		Ignore,

		/// <summary>
		/// Prefix vector with number of elements of vector within brackets.
		/// </summary>
		Count
	}

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
		/// If the encoder encodes a value as a vector.
		/// </summary>
		bool EncodesAsVector(object Value);

		/// <summary>
		/// Encodes the <paramref name="Object"/> to TOON.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Indent">Any indentation to apply.</param>
		/// <param name="Toon">TOON output.</param>
		/// <param name="Brackets">How to manage brackets when encoding vectors.</param>
		void Encode(object Object, int? Indent, ToonOutput Toon, BracketsMode Brackets);

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
