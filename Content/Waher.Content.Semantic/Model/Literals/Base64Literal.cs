using System;
using Waher.Content.Semantic.Ontologies;
using Waher.Runtime.Inventory;

namespace Waher.Content.Semantic.Model.Literals
{
	/// <summary>
	/// Represents a byte[] literal.
	/// </summary>
	public class Base64Literal : SemanticLiteral
	{
		/// <summary>
		/// Represents a byte[] literal.
		/// </summary>
		public Base64Literal()
			: base()
		{
		}

		/// <summary>
		/// Represents a byte[] literal.
		/// </summary>
		/// <param name="Value">Parsed value</param>
		public Base64Literal(byte[] Value)
			: base(Value, Convert.ToBase64String(Value))
		{
		}

		/// <summary>
		/// Represents a byte[] literal.
		/// </summary>
		/// <param name="Value">Parsed value</param>
		/// <param name="StringValue">String value.</param>
		public Base64Literal(byte[] Value, string StringValue)
			: base(Value, StringValue)
		{
		}

		/// <summary>
		/// http://www.w3.org/2001/XMLSchema#base64Binary
		/// </summary>
		public static readonly string TypeUri = XmlSchema.base64Binary.OriginalString;

		/// <summary>
		/// Type name
		/// </summary>
		public override string StringType => TypeUri;

		/// <summary>
		/// How well the type supports a given value type.
		/// </summary>
		/// <param name="ValueType">Value Type.</param>
		/// <returns>Support grade.</returns>
		public override Grade Supports(Type ValueType)
		{
			return ValueType == typeof(byte[]) ? Grade.Ok : Grade.NotAtAll;
		}

		/// <summary>
		/// Encapsulates an object value as a semantic literal value.
		/// </summary>
		/// <param name="Value">Object value the literal type supports.</param>
		/// <returns>Encapsulated semantic literal value.</returns>
		public override ISemanticLiteral Encapsulate(object Value)
		{
			if (Value is byte[] Typed)
				return new Base64Literal(Typed);
			else
				return new StringLiteral(Value?.ToString() ?? string.Empty);
		}

		/// <summary>
		/// Tries to parse a string value of the type supported by the class..
		/// </summary>
		/// <param name="Value">String value.</param>
		/// <param name="DataType">Data type.</param>
		/// <param name="Language">Language code if available.</param>
		/// <returns>Parsed literal.</returns>
		public override ISemanticLiteral Parse(string Value, string DataType, string Language)
		{
			try
			{
				return new Base64Literal(Convert.FromBase64String(Value), Value);
			}
			catch (Exception)
			{
				return new CustomLiteral(Value, DataType, Language);
			}
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is Base64Literal Typed &&
				Typed.StringValue == this.StringValue;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return this.StringValue.GetHashCode();
		}
	}
}
