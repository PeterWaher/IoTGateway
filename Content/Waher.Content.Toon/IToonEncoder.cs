using System;
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
		void Encode(object Object, int? Indent, StringBuilder Toon);
	}
}
