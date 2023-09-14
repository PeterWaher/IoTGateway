using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;
using Waher.Script.TypeConversion;
using Waher.Script.Units;
using Waher.Things.SensorData;

namespace Waher.Things.TypeConverters
{
	/// <summary>
	/// Converts a <see cref="PersistableQuantity"/> to a <see cref="PhysicalQuantity"/>.
	/// </summary>
	public class PersistableQuantityToPhysicalQuantity : ITypeConverter
	{
		/// <summary>
		/// Converts a <see cref="PersistableQuantity"/> to a <see cref="PhysicalQuantity"/>.
		/// </summary>
		public PersistableQuantityToPhysicalQuantity()
		{
		}

		/// <summary>
		/// Converter converts objects of this type.
		/// </summary>
		public Type From => typeof(PersistableQuantity);

		/// <summary>
		/// Converter converts objects to this type.
		/// </summary>
		public Type To => typeof(PhysicalQuantity);

		/// <summary>
		/// Converts the object in <paramref name="Value"/> to an object of type <see cref="To"/>.
		/// </summary>
		/// <param name="Value">Object to be converted.</param>
		/// <returns>Object of type <see cref="To"/>.</returns>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of type <see cref="From"/>.</exception>
		public object Convert(object Value)
		{
			if (Value is PersistableQuantity Q)
				return new PhysicalQuantity(Q.Value, new Unit(Q.Unit));
			else
				throw new ArgumentException("Not a PersistableQuantity.", nameof(Value));
		}

		/// <summary>
		/// Converts the object in <paramref name="Value"/> to an object of type <see cref="To"/>, encapsulated in an
		/// <see cref="IElement"/>.
		/// </summary>
		/// <param name="Value">Object to be converted.</param>
		/// <returns>Object of type <see cref="To"/>, encapsulated in an <see cref="IElement"/>.</returns>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of type <see cref="From"/>.</exception>
		public IElement ConvertToElement(object Value)
		{
			if (Value is PersistableQuantity Q)
				return new PhysicalQuantity(Q.Value, new Unit(Q.Unit));
			else
				throw new ArgumentException("Not a PersistableQuantity.", nameof(Value));
		}
	}
}
