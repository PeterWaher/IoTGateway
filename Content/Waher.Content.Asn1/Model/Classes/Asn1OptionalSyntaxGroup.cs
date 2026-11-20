using System.Collections.Generic;
using Waher.Content.Asn1.Exceptions;

namespace Waher.Content.Asn1.Model.Classes
{
	/// <summary>
	/// Optional syntax group
	/// </summary>
	public class Asn1OptionalSyntaxGroup : Asn1SyntaxNode 
	{
		private readonly Asn1SyntaxNode[] syntax;

		/// <summary>
		/// Optional syntax group
		/// </summary>
		/// <param name="Syntax">Optional syntax.</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1OptionalSyntaxGroup(Asn1SyntaxNode[] Syntax, Asn1Document Document)
			: base(Document)
		{
			this.syntax = Syntax;
		}

		/// <summary>
		/// If the syntax node matches the current position.
		/// </summary>
		/// <param name="Properties">Properties parsed.</param>
		/// <returns>If the node matches the current position.</returns>
		public override bool MatchesNext(Dictionary<string, Asn1PropertyValue> Properties)
		{
			Dictionary<string, Asn1PropertyValue> Properties2 = new Dictionary<string, Asn1PropertyValue>();
			int Bak = this.Document.pos;

			foreach (Asn1SyntaxNode SyntaxNode in this.syntax)
			{
				if (!SyntaxNode.MatchesNext(Properties2))
				{
					this.Document.pos = Bak;
					return true;
				}
			}

			foreach (KeyValuePair<string, Asn1PropertyValue> P in Properties2)
				Properties[P.Key] = P.Value;

			return true;
		}

		/// <summary>
		/// Gets a syntax error exception object.
		/// </summary>
		/// <returns>Exception object.</returns>
		public override Asn1SyntaxException GetException()
		{
			foreach (Asn1SyntaxNode SyntaxNode in this.syntax)
				return SyntaxNode.GetException();

			return this.Document.SyntaxError("Optional syntax empty.");
		}
	}
}
