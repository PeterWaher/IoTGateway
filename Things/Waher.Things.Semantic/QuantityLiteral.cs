using System;
using Waher.Content;
using Waher.Content.Semantic;
using Waher.Content.Semantic.Model;
using Waher.Content.Semantic.Model.Literals;
using Waher.Runtime.Inventory;
using Waher.Script;
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

		/// <summary>
		/// Semantic literal for physical quantities.
		/// </summary>
		public QuantityLiteral()
			: base()
		{
		}

		/// <summary>
		/// Semantic literal for physical quantities.
		/// </summary>
		/// <param name="Value">Physical Quantity value.</param>
		public QuantityLiteral(PhysicalQuantity Value)
			: base(Value, Expression.ToString(Value.Magnitude))
		{
			this.value = Value;
		}

		/// <summary>
		/// Type name (or null if literal value is a string)
		/// </summary>
		public override string StringType => IoTSensorData.UnitNamespace + this.value?.Unit;

		/// <summary>
		/// Encapsulates an object value as a semantic literal value.
		/// </summary>
		/// <param name="Value">Object value the literal type supports.</param>
		/// <returns>Encapsulated semantic literal value.</returns>
		public override ISemanticLiteral Encapsulate(object Value)
		{
			if (Value is PhysicalQuantity Typed)
				return new QuantityLiteral(Typed);
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
				Unit.TryParse(DataType.Substring(IoTSensorData.UnitNamespace.Length), out Unit Unit2))
			{
				return new QuantityLiteral(new PhysicalQuantity(Magnitude, Unit2));
			}
			else
				return new CustomLiteral(Value, DataType, Language);
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
	}
}
