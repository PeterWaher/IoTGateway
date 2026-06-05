using System;
using System.Text;
using Waher.Runtime.Inventory;

namespace Waher.Content.Toon.Model
{
	/// <summary>
	/// Abstract base class for TOON encoders.
	/// </summary>
	public abstract class ToonEncoder : IToonEncoder 
	{
		/// <summary>
		/// How well the TOON encoder encodes objects of type <paramref name="ObjectType"/>.
		/// </summary>
		/// <param name="ObjectType">Type of object to encode.</param>
		/// <returns>How well objects of the given type are encoded.</returns>
		public abstract Grade Supports(Type ObjectType);

		/// <summary>
		/// Encodes the <paramref name="Object"/> to TOON.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Indent">Any indentation to apply.</param>
		/// <param name="Toon">TOON output.</param>
		public abstract void Encode(object Object, int? Indent, ToonOutput Toon);

		/// <summary>
		/// If the encoder encodes values into multiple rows.
		/// </summary>
		public abstract bool EncodesMultipleRows { get; }

		/// <summary>
		/// If the encoder encodes vectors.
		/// </summary>
		public abstract bool EncodesVectors { get; }

		/// <summary>
		/// Gets the number of elements in the vector.
		/// </summary>
		/// <param name="Vector">Vector object.</param>
		/// <returns>Number of elements in the vector. If null is returned, the
		/// <paramref name="Vector"/> item should not be considered a vector.</returns>
		public abstract int? GetCount(object Vector);

		/// <summary>
		/// Encodes the <paramref name="Object"/> to TOON.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Indent">Any indentation to apply.</param>
		/// <param name="Toon">TOON output.</param>
		/// <param name="UseBrackets">If brackets should be used around the vector.</param>
		public abstract void Encode(object Object, int? Indent, ToonOutput Toon, bool UseBrackets);
	}
}
