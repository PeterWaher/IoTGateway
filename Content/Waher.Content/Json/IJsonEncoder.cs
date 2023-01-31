using System;
using System.Text;
using Waher.Runtime.Inventory;

namespace Waher.Content.Json
{
	/// <summary>
	/// Interface for encoding objects of certain types to JSON.
	/// </summary>
	public interface IJsonEncoder : IProcessingSupport<Type>
	{
		/// <summary>
		/// Encodes the <paramref name="Object"/> to JSON.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Indent">Any indentation to apply.</param>
		/// <param name="Json">JSON output.</param>
		void Encode(object Object, int? Indent, StringBuilder Json);
	}
}
