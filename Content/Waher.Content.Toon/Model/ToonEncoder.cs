using System;
using System.Collections;
using System.Collections.Generic;
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
		/// If the encoder encodes a value as a vector.
		/// </summary>
		public abstract bool EncodesAsVector(object Value);

		/// <summary>
		/// If the encoder encodes a value as an object.
		/// </summary>
		public abstract bool EncodesAsObject(object Value);

		/// <summary>
		/// Checks if an object can be folded to a shorter representation, and if so, 
		/// returns the folded name and value.
		/// </summary>
		/// <param name="Object">Object to encode</param>
		/// <param name="FoldedName">Folded name</param>
		/// <param name="FoldedValue">Folded value.</param>
		/// <returns>True if the object can be folded, otherwise false.</returns>
		public virtual bool CanFold(object Object, out string FoldedName, out object FoldedValue)
		{
			FoldedName = null;
			FoldedValue = null;
			return false;
		}

		/// <summary>
		/// Encodes the <paramref name="Object"/> to TOON.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Indent">Any indentation to apply.</param>
		/// <param name="Toon">TOON output.</param>
		/// <param name="Brackets">How to manage brackets when encoding vectors.</param>
		public abstract void Encode(object Object, int? Indent, ToonOutput Toon, BracketsMode Brackets);

		/// <summary>
		/// Gets an enumerator for the child-elements of an object.
		/// </summary>
		/// <param name="Object">Object to get child-elements from.</param>
		/// <returns>Enumerator for the child-elements, or null if not a vector.</returns>
		public virtual IEnumerator GetElements(object Object) => null;

		/// <summary>
		/// Gets available parameters to encode from an object.
		/// </summary>
		/// <param name="Object">Object to get parameters from.</param>
		/// <returns>Enumerator for the parameters, or null if not applicable.</returns>
		public virtual IEnumerator<KeyValuePair<string, object>> GetParameters(object Object) => null;
	}
}
