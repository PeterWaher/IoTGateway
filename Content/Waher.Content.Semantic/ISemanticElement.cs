using System;
using Waher.Script.Abstraction.Elements;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// Interface for semantic nodes.
	/// </summary>
	public interface ISemanticElement : IComparable, IElement
	{
		/// <summary>
		/// Property used by processor, to tag information to an element.
		/// </summary>
		object Tag { get; set; }

		/// <summary>
		/// If element is a literal.
		/// </summary>
		bool IsLiteral { get; }
	}
}
