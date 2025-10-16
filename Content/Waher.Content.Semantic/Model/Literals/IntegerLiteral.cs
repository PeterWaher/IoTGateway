using System;
using System.Numerics;
using Waher.Content.Semantic.Ontologies;
using Waher.Runtime.Inventory;

namespace Waher.Content.Semantic.Model.Literals
{
    /// <summary>
    /// Represents an integer literal of undefined size.
    /// </summary>
    public class IntegerLiteral : SemanticNumericLiteral
	{
        private readonly string dataType = null;

        /// <summary>
        /// Represents an integer literal of undefined size.
        /// </summary>
        public IntegerLiteral()
            : base()
        {
        }

        /// <summary>
        /// Represents an integer literal of undefined size.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        public IntegerLiteral(BigInteger Value)
            : base(Value, Value.ToString())
        {
		}

		/// <summary>
		/// Represents an integer literal of undefined size.
		/// </summary>
		/// <param name="Value">Parsed value</param>
		/// <param name="StringValue">String value.</param>
		public IntegerLiteral(BigInteger Value, string StringValue)
            : base(Value, StringValue)
        {
		}

		/// <summary>
		/// Represents an integer literal of undefined size.
		/// </summary>
		/// <param name="Value">Parsed value</param>
		/// <param name="StringValue">String value.</param>
		/// <param name="DataType">Data Type.</param>
		public IntegerLiteral(BigInteger Value, string StringValue, string DataType)
            : base(Value, StringValue)
        {
			this.dataType = DataType;
        }

		/// <summary>
		/// http://www.w3.org/2001/XMLSchema#byte
		/// </summary>
		public static readonly string TypeUri = XmlSchema.integer.OriginalString;

		/// <summary>
		/// Type name
		/// </summary>
		public override string StringType => this.dataType ?? TypeUri;

		/// <summary>
		/// How well the type supports a given value type.
		/// </summary>
		/// <param name="ValueType">Value Type.</param>
		/// <returns>Support grade.</returns>
		public override Grade Supports(Type ValueType)
		{
			return ValueType == typeof(BigInteger) ? Grade.Ok : Grade.NotAtAll;
		}

		/// <summary>
		/// Comparable numeric value.
		/// </summary>
		public override double ComparableValue => (double)((BigInteger)this.Value);

		/// <summary>
		/// How well the type supports a given data type.
		/// </summary>
		/// <param name="DataType">Data type.</param>
		/// <returns>Support grade.</returns>
		public override Grade Supports(string DataType)
        {
            switch (DataType)
            {
				case IntegerUri:
				case NegativeIntegerUri:
				case NonNegativeIntegerUri:
				case NonPositiveIntegerUri:
				case PositiveIntegerUri:
                    return Grade.Ok;

                default:
                    return Grade.NotAtAll;
            }
        }

		/// <summary>
		/// http://www.w3.org/2001/XMLSchema#integer
		/// </summary>
		public const string IntegerUri = XmlSchema.Namespace + "integer";

		/// <summary>
		/// http://www.w3.org/2001/XMLSchema#negativeInteger
		/// </summary>
		public const string NegativeIntegerUri = XmlSchema.Namespace + "negativeInteger";

		/// <summary>
		/// http://www.w3.org/2001/XMLSchema#nonNegativeInteger
		/// </summary>
		public const string NonNegativeIntegerUri = XmlSchema.Namespace + "nonNegativeInteger";

		/// <summary>
		/// http://www.w3.org/2001/XMLSchema#nonPositiveInteger
		/// </summary>
		public const string NonPositiveIntegerUri = XmlSchema.Namespace + "nonPositiveInteger";

		/// <summary>
		/// http://www.w3.org/2001/XMLSchema#positiveInteger
		/// </summary>
		public const string PositiveIntegerUri = XmlSchema.Namespace + "positiveInteger";


		/// <summary>
		/// Tries to parse a string value of the type supported by the class..
		/// </summary>
		/// <param name="Value">String value.</param>
		/// <param name="DataType">Data type.</param>
		/// <param name="Language">Language code if available.</param>
		/// <returns>Parsed literal.</returns>
		public override ISemanticLiteral Parse(string Value, string DataType, string Language)
        {
            if (BigInteger.TryParse(Value, out BigInteger i))
                return new IntegerLiteral(i, Value, DataType);
            else
                return new CustomLiteral(Value, DataType, Language);
        }

		/// <summary>
		/// Encapsulates an object value as a semantic literal value.
		/// </summary>
		/// <param name="Value">Object value the literal type supports.</param>
		/// <returns>Encapsulated semantic literal value.</returns>
		public override ISemanticLiteral Encapsulate(object Value)
		{
			if (Value is BigInteger Typed)
				return new IntegerLiteral(Typed);
			else
				return new StringLiteral(Value?.ToString() ?? string.Empty);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is IntegerLiteral Typed &&
				Typed.StringValue == this.StringValue;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return this.StringValue.GetHashCode();
		}

		/// <summary>
		/// Compares the current instance with another object of the same type and returns
		/// an integer that indicates whether the current instance precedes, follows, or
		/// occurs in the same position in the sort order as the other object.
		/// </summary>
		/// <param name="obj">An object to compare with this instance.</param>
		/// <returns>A value that indicates the relative order of the objects being compared. The
		/// return value has these meanings: Value Meaning Less than zero This instance precedes
		/// obj in the sort order. Zero This instance occurs in the same position in the
		/// sort order as obj. Greater than zero This instance follows obj in the sort order.</returns>
		/// <exception cref="ArgumentException">obj is not the same type as this instance.</exception>
		public override int CompareTo(object obj)
		{
			if (obj is IntegerLiteral Typed)
				return ((BigInteger)this.Value).CompareTo((BigInteger)Typed.Value);
			else
				return base.CompareTo(obj);
		}
	}
}
