using System.Collections.Generic;
using Waher.Content.Asn1.Exceptions;
using Waher.Content.Asn1.Model.Values;
using Waher.Script.Units.DerivedQuantities;

namespace Waher.Content.Asn1.Model.Classes
{
	/// <summary>
	/// Defines an reference to a class property.
	/// </summary>
	public class Asn1PropertyReference : Asn1SyntaxNode
	{
		private readonly string property;
		private readonly bool isTypeReference;

		/// <summary>
		/// Defines an reference to a class property.
		/// </summary>
		/// <param name="Property">Property</param>
		/// <param name="Document">ASN.1 document hosting the node.</param>
		public Asn1PropertyReference(string Property, Asn1Document Document)
			: base(Document)
		{
			this.property = Property;
			this.isTypeReference = char.IsUpper(Property[0]);
		}

		/// <summary>
		/// Expected token.
		/// </summary>
		public string Property => this.property;

		/// <summary>
		/// If the reference is a type reference.
		/// </summary>
		public bool IsTypeReference => this.isTypeReference;

		/// <inheritdoc/>
		public override string ToString() => this.property;

		/// <summary>
		/// If the syntax node matches the current position.
		/// </summary>
		/// <param name="Properties">Properties parsed.</param>
		/// <returns>If the node matches the current position.</returns>
		public override bool MatchesNext(Dictionary<string, Asn1Node> Properties)
		{
			if (this.isTypeReference)
			{
				Asn1Type Type = this.Document.ParseType(this.property, false, null, false);
				Properties[this.property] = Type;
			}
			else
			{
				Asn1Value Value = this.Document.ParseValue();

				if (Value is Asn1ValueReference ValueRef &&
					this.Document.PeekNextToken() == ".")
				{
					this.Document.pos++;
					this.Document.AssertNextToken("&");

					string PropertyName = this.Document.ParseIdentifier();

					Properties[this.property] = new Asn1PropertyValueReference(this.property,
						ValueRef.Identifier, PropertyName, this.Document);
				}
				else
					Properties[this.property] = new Asn1PropertyValue(this.property, Value, this.Document);
			}

			return true;
		}

		/// <summary>
		/// Gets a syntax error exception object.
		/// </summary>
		/// <returns>Exception object.</returns>
		public override Asn1SyntaxException GetException()
		{
			return this.Document.SyntaxError("Expected value for property: " + this.property);
		}

	}
}
