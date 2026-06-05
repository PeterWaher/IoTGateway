using System.Collections.Generic;

namespace Waher.Content.Toon.Model
{
	/// <summary>
	/// Abstract base class for object TOON encoders.
	/// </summary>
	public abstract class MultiRowToonEncoder : ToonEncoder 
	{
		/// <summary>
		/// If the encoder encodes values into multiple rows.
		/// </summary>
		public override bool EncodesMultipleRows => true;

		/// <summary>
		/// If the encoder encodes vectors.
		/// </summary>
		public override bool EncodesVectors => false;

		/// <summary>
		/// Gets the number of elements in the vector.
		/// </summary>
		/// <param name="Vector">Vector object.</param>
		/// <returns>Number of elements in the vector. If null is returned, the
		/// <paramref name="Vector"/> item should not be considered a vector.</returns>
		public override int? GetCount(object Vector) => null;

		/// <summary>
		/// Encodes the <paramref name="Object"/> to TOON.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Indent">Any indentation to apply.</param>
		/// <param name="Toon">TOON output.</param>
		/// <param name="UseBrackets">If brackets should be used around the vector.</param>
		public override void Encode(object Object, int? Indent, ToonOutput Toon, bool UseBrackets)
		{
			this.Encode(Object, Indent, Toon);
		}

		/// <summary>
		/// Gets available parameters to encode from an object.
		/// </summary>
		/// <param name="Object">Object to get parameters from.</param>
		/// <returns>Enumerator for the parameters, or null if not applicable.</returns>
		public abstract override IEnumerator<KeyValuePair<string, object>> GetParameters(object Object);
	}
}
