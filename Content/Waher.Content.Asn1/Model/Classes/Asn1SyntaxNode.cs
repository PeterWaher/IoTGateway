using System.Collections.Generic;
using Waher.Content.Asn1.Exceptions;

namespace Waher.Content.Asn1.Model.Classes
{
	/// <summary>
	/// Abstract base class for syntax nodes.
	/// </summary>
	public abstract class Asn1SyntaxNode : Asn1Node
	{
		/// <summary>
		/// Abstract base class for syntax nodes.
		/// </summary>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1SyntaxNode(Asn1Document Document)
			: base(Document)
		{
		}

		/// <summary>
		/// If the syntax node matches the current position.
		/// </summary>
		/// <param name="Properties">Properties parsed.</param>
		/// <returns>If the node matches the current position.</returns>
		public abstract bool MatchesNext(Dictionary<string, Asn1Node> Properties);

		/// <summary>
		/// Gets a syntax error exception object.
		/// </summary>
		/// <returns>Exception object.</returns>
		public abstract Asn1SyntaxException GetException();
	}
}
