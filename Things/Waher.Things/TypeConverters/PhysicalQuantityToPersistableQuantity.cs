using System;
using Waher.Content;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;
using Waher.Script.TypeConversion;
using Waher.Things.SensorData;

namespace Waher.Things.TypeConverters
{
	/// <summary>
	/// Converts a <see cref="PhysicalQuantity"/> to a <see cref="PersistableQuantity"/>.
	/// </summary>
	public class PhysicalQuantityToPersistableQuantity : ITypeConverter
	{
		/// <summary>
		/// Converts a <see cref="PhysicalQuantity"/> to a <see cref="PersistableQuantity"/>.
		/// </summary>
		public PhysicalQuantityToPersistableQuantity()
		{
		}

		/// <summary>
		/// Converter converts objects of this type.
		/// </summary>
		public Type From => typeof(PhysicalQuantity);

		/// <summary>
		/// Converter converts objects to this type.
		/// </summary>
		public Type To => typeof(PersistableQuantity);

		/// <summary>
		/// Converts the object in <paramref name="Value"/> to an object of type <see cref="To"/>.
		/// </summary>
		/// <param name="Value">Object to be converted.</param>
		/// <returns>Object of type <see cref="To"/>.</returns>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of type <see cref="From"/>.</exception>
		public object Convert(object Value)
		{
			if (Value is IPhysicalQuantity PQ)
			{
				PhysicalQuantity Q = PQ.ToPhysicalQuantity();
				return new PersistableQuantity(Q.Magnitude, Q.Unit.ToString(), CommonTypes.GetNrDecimals(Q.Magnitude));
			}
			else
				throw new ArgumentException("Not a PhysicalQuantity.", nameof(Value));
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
			if (Value is IPhysicalQuantity PQ)
			{
				PhysicalQuantity Q = PQ.ToPhysicalQuantity();
				return new ObjectValue(new PersistableQuantity(Q.Magnitude, Q.Unit.ToString(), CommonTypes.GetNrDecimals(Q.Magnitude)));
			}
			else
				throw new ArgumentException("Not a PhysicalQuantity.", nameof(Value));
		}
	}
}
