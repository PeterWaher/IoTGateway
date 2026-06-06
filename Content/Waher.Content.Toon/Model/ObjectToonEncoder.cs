namespace Waher.Content.Toon.Model
{
	/// <summary>
	/// Abstract base class for object TOON encoders.
	/// </summary>
	public abstract class ObjectToonEncoder : MultiRowToonEncoder 
	{
		/// <summary>
		/// If the encoder encodes a value as an object.
		/// </summary>
		public override bool EncodesAsObject(object Value) => true;

		/// <summary>
		/// Checks if an object can be folded to a shorter representation, and if so, 
		/// returns the folded name and value.
		/// </summary>
		/// <param name="Object">Object to encode</param>
		/// <param name="FoldedName">Folded name</param>
		/// <param name="FoldedValue">Folded value.</param>
		/// <returns>True if the object can be folded, otherwise false.</returns>
		public abstract override bool CanFold(object Object, out string FoldedName, out object FoldedValue);
	}
}
