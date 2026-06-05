using System.Text;

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
		/// If the encoder encodes vectors.
		/// </summary>
		public override bool EncodesVectors => true;

		/// <summary>
		/// Encodes the <paramref name="Object"/> to TOON.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Indent">Any indentation to apply.</param>
		/// <param name="Toon">TOON output.</param>
		public override void Encode(object Object, int? Indent, ToonOutput Toon)
		{
			this.Encode(Object, Indent, Toon, true);	
		}
	}
}
