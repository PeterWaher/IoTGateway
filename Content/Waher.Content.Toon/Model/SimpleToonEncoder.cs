namespace Waher.Content.Toon.Model
{
	/// <summary>
	/// Abstract base class for simple TOON encoders.
	/// </summary>
	public abstract class SimpleToonEncoder : ToonEncoder 
	{
		/// <summary>
		/// If the encoder encodes values into multiple rows.
		/// </summary>
		public override bool EncodesMultipleRows => false;

		/// <summary>
		/// If the encoder encodes a value as a vector.
		/// </summary>
		public override bool EncodesAsVector(object Value) => false;

		/// <summary>
		/// Encodes the <paramref name="Object"/> to TOON.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Indent">Any indentation to apply.</param>
		/// <param name="Toon">TOON output.</param>
		/// <param name="Brackets">How to manage brackets when encoding vectors.</param>
		public override void Encode(object Object, int? Indent, ToonOutput Toon, BracketsMode Brackets)
		{
			this.Encode(Object, Indent, Toon);
		}
	}
}
