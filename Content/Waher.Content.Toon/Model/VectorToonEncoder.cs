using System.Collections;

namespace Waher.Content.Toon.Model
{
	/// <summary>
	/// Abstract base class for vector TOON encoders.
	/// </summary>
	public abstract class VectorToonEncoder : ToonEncoder 
	{
		/// <summary>
		/// If the encoder encodes values into multiple rows.
		/// </summary>
		public override bool EncodesMultipleRows => false;

		/// <summary>
		/// If the encoder encodes a value as a vector.
		/// </summary>
		public override bool EncodesAsVector(object Value) => true;

		/// <summary>
		/// If the encoder encodes a value as an object.
		/// </summary>
		public override bool EncodesAsObject(object Value) => false;

		/// <summary>
		/// Encodes the <paramref name="Object"/> to TOON.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Indent">Any indentation to apply.</param>
		/// <param name="Toon">TOON output.</param>
		public override void Encode(object Object, int? Indent, ToonOutput Toon)
		{
			this.Encode(Object, Indent, Toon, BracketsMode.Count);
		}

		/// <summary>
		/// Gets an enumerator for the child-elements of an object.
		/// </summary>
		/// <param name="Object">Object to get child-elements from.</param>
		/// <returns>Enumerator for the child-elements, or null if not a vector.</returns>
		public abstract override IEnumerator GetElements(object Object);
	}
}
