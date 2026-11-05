using Waher.Runtime.Collections;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Named ASN.1 node.
	/// </summary>
	public interface INamedNode
	{
		/// <summary>
		/// Name of node.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// If the named element must be qualified.
		/// </summary>
		bool MustBeQualified { get; }

		/// <summary>
		/// If the named node represents a type.
		/// </summary>
		bool IsType { get; }

		/// <summary>
		/// Gets the fully qualified name, in reverse order.
		/// </summary>
		void GetFullyQualifiedName(ChunkedList<string> Fqn);
	}
}
