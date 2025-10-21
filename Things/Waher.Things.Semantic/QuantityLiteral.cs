using System;
using System.Web;
using Waher.Content;
using Waher.Content.Semantic;
using Waher.Content.Semantic.Model;
using Waher.Content.Semantic.Model.Literals;
using Waher.Runtime.Inventory;
using Waher.Script.Objects;
using Waher.Script.Units;
using Waher.Things.Semantic.Ontologies;

namespace Waher.Things.Semantic
{
	/// <summary>
	/// Semantic literal for physical quantities.
	/// </summary>
	public class QuantityLiteral : SemanticLiteral
	{
		private readonly PhysicalQuantity value;
		private readonly string stringType;

		/// <summary>
		/// Semantic literal for physical quantities.
		/// </summary>
		public QuantityLiteral()
			: base(string.Empty, string.Empty)
		{
		}

		/// <summary>
		/// Semantic literal for physical quantities.
		/// </summary>
		/// <param name="Value">Physical Quantity value.</param>
		public QuantityLiteral(PhysicalQuantity Value)
			: base(Value.Magnitude, CommonTypes.Encode(Value.Magnitude))
		{
			this.value = Value;
			this.stringType = IoTSensorData.UnitNamespace + HttpUtility.UrlEncode(this.value?.Unit.ToString() ?? string.Empty);
		}

		/// <summary>
		/// Type name (or null if literal value is a string)
		/// </summary>
		public override string StringType => this.stringType;

		/// <summary>
		/// Encapsulates an object value as a semantic literal value.
		/// </summary>
		/// <param name="Value">Object value the literal type supports.</param>
		/// <returns>Encapsulated semantic literal value.</returns>
		public override ISemanticLiteral Encapsulate(object Value)
		{
			if (Value is IPhysicalQuantity Typed)
				return new QuantityLiteral(Typed.ToPhysicalQuantity());
			else
				return new StringLiteral(Value?.ToString() ?? string.Empty);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is QuantityLiteral Typed &&
				(this.value?.Equals(Typed.value) ?? Typed.value is null);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return this.value.GetHashCode();
		}

		/// <summary>
		/// Associated object value.
		/// </summary>
		public override object AssociatedObjectValue => this.value;

		/// <summary>
		/// Tries to parse a string value of the type supported by the class..
		/// </summary>
		/// <param name="Value">String value.</param>
		/// <param name="DataType">Data type.</param>
		/// <param name="Language">Language code if available.</param>
		/// <returns>Parsed literal.</returns>
		public override ISemanticLiteral Parse(string Value, string DataType, string Language)
		{
			if (DataType.StartsWith(IoTSensorData.UnitNamespace, StringComparison.CurrentCultureIgnoreCase) &&
				CommonTypes.TryParse(Value, out double Magnitude) &&
				Unit.TryParse(HttpUtility.UrlDecode(DataType[IoTSensorData.UnitNamespace.Length..]), out Unit Unit2))
			{
				return new QuantityLiteral(new PhysicalQuantity(Magnitude, Unit2));
			}
			else
				return new CustomLiteral(Value, DataType, Language);
		}

		/// <summary>
		/// How well the type supports a given data type.
		/// </summary>
		/// <param name="DataType">Data type.</param>
		/// <returns>Support grade.</returns>
		public override Grade Supports(string DataType)
		{
			if (DataType.StartsWith(IoTSensorData.UnitNamespace, StringComparison.CurrentCultureIgnoreCase) &&
				Unit.TryParse(HttpUtility.UrlDecode(DataType[IoTSensorData.UnitNamespace.Length..]), out _))
			{
				return Grade.Ok;
			}
			else
				return Grade.NotAtAll;
		}

		/// <summary>
		/// How well the type supports a given value type.
		/// </summary>
		/// <param name="ValueType">Value Type.</param>
		/// <returns>Support grade.</returns>
		public override Grade Supports(Type ValueType)
		{
			return ValueType == typeof(PhysicalQuantity) ? Grade.Ok : Grade.NotAtAll;
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
			if (obj is QuantityLiteral Typed)
				return this.value.CompareTo(Typed.value);

			return base.CompareTo(obj);
		}
	}
}
