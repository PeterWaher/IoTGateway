using System;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// Interface for semantic nodes.
	/// </summary>
	public interface ISemanticElement : IComparable
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
