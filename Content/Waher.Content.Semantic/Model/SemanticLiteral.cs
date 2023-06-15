using System;
using System.Text;
using Waher.Runtime.Inventory;

namespace Waher.Content.Semantic.Model
{
	/// <summary>
	/// Abstract base class for semantic literal values.
	/// </summary>
	public abstract class SemanticLiteral : SemanticElement, ISemanticLiteral
	{
		private object value;
		private Type valueType;
		private IComparable comparable;

		/// <summary>
		/// Abstract base class for semantic literal values.
		/// </summary>
		public SemanticLiteral()
		{
		}

		/// <summary>
		/// Abstract base class for semantic literal values.
		/// </summary>
		/// <param name="Value">Parsed Value</param>
		/// <param name="StringValue">String Value</param>
		public SemanticLiteral(object Value, string StringValue)
		{
			this.value = Value;
			this.valueType = null;
			this.comparable = null;

			this.StringValue = StringValue;
		}

		/// <summary>
		/// If element is a literal.
		/// </summary>
		public override bool IsLiteral => true;

		/// <summary>
		/// Parsed value.
		/// </summary>
		public object Value
		{
			get => this.value;
			set
			{
				this.value = value;
				this.valueType = null;
				this.comparable = null;
			}
		}

		/// <summary>
		/// Associated object value.
		/// </summary>
		public override object AssociatedObjectValue => this.value;

		/// <summary>
		/// Type name (or null if literal value is a string)
		/// </summary>
		public abstract string StringType { get; }

		/// <summary>
		/// String representation of value.
		/// </summary>
		public string StringValue { get; }

		/// <summary>
		/// How well the type supports a given data type.
		/// </summary>
		/// <param name="DataType">Data type.</param>
		/// <returns>Support grade.</returns>
		public virtual Grade Supports(string DataType)
		{
			return DataType == this.StringType ? Grade.Ok : Grade.NotAtAll;
		}

		/// <summary>
		/// How well the type supports a given value type.
		/// </summary>
		/// <param name="ValueType">Value Type.</param>
		/// <returns>Support grade.</returns>
		public abstract Grade Supports(Type ValueType);

		/// <summary>
		/// Tries to parse a string value of the type supported by the class..
		/// </summary>
		/// <param name="Value">String value.</param>
		/// <param name="DataType">Data type.</param>
		/// <param name="Language">Language code if available.</param>
		/// <returns>Parsed literal.</returns>
		public abstract ISemanticLiteral Parse(string Value, string DataType, string Language);
		
		/// <summary>
		/// Encapsulates an object value as a semantic literal value.
		/// </summary>
		/// <param name="Value">Object value the literal type supports.</param>
		/// <returns>Encapsulated semantic literal value.</returns>
		public abstract ISemanticLiteral Encapsulate(object Value);

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append('"');
			sb.Append(JSON.Encode(this.StringValue));
			sb.Append("\"^^<");
			sb.Append(this.StringType);
			sb.Append('>');

			return sb.ToString();
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
			if (obj is SemanticLiteral Typed)
			{
				if (this.valueType is null)
				{
					this.valueType = this.value?.GetType() ?? typeof(object);
					this.comparable = this.value as IComparable;
				}

				if (Typed.valueType is null)
				{
					Typed.valueType = Typed.value?.GetType() ?? typeof(object);
					Typed.comparable = Typed.value as IComparable;
				}

				if (this.valueType == Typed.valueType && !(this.comparable is null))
					return this.comparable.CompareTo(Typed.value);
			}
				
			return base.CompareTo(obj);
		}
	}
}
