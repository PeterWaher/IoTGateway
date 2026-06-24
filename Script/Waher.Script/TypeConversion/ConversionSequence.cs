using System;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.TypeConversion
{
	/// <summary>
	/// Performs a sequence of type conversions to convert an object from one type to another.
	/// </summary>
	public class ConversionSequence : ITypeConverter
	{
		private readonly ITypeConverter[] converters;
		private readonly Type from;
		private readonly Type to;
		private readonly double weight;
		private readonly int c;

		/// <summary>
		/// Performs a sequence of type conversions to convert an object from one type to another.
		/// </summary>
		/// <param name="Converters">Sequence of converters.</param>
		public ConversionSequence(params ITypeConverter[] Converters)
		{
			this.converters = Converters;
			this.c = this.converters.Length;
			this.from = this.converters[0].From;
			this.to = this.converters[this.c - 1].To;

			this.weight = 1;
			for (int i = 0; i < this.c; i++)
				this.weight *= this.converters[i].Weight;
		}

		/// <summary>
		/// Sequence of converters.
		/// </summary>
		public ITypeConverter[] Converters => this.converters;

		/// <summary>
		/// Converter converts objects of this type.
		/// </summary>
		public Type From => this.from;

		/// <summary>
		/// Converter converts objects to this type.
		/// </summary>
		public Type To => this.to;

		/// <summary>
		/// Weight of the converter. An estimate of how well the converter performs, or
		/// how much information is retained in the conversion. 1 = lossless conversion,
		/// 0 = information lost.
		/// </summary>
		public double Weight => this.weight;

		/// <summary>
		/// Converts the object in <paramref name="Value"/> to an object of type <see cref="To"/>.
		/// </summary>
		/// <param name="Value">Object to be converted.</param>
		/// <param name="Result">Converted object value.</param>
		/// <returns>If conversion was possible.</returns>
		public bool TryConvert(object Value, out object Result)
		{
			int i;

			for (i = 0; i < this.c; i++)
			{
				if (!this.converters[i].TryConvert(Value, out object Value2))
				{
					Result = null;
					return false;
				}

				Value = Value2;
			}

			Result = Value;
			return true;
		}

		/// <summary>
		/// Converts the object in <paramref name="Value"/> to an object of type <see cref="To"/>, encapsulated in an
		/// <see cref="IElement"/>.
		/// </summary>
		/// <param name="Value">Object to be converted.</param>
		/// <param name="Result">Converted object value.</param>
		/// <returns>If conversion was possible.</returns>
		public bool TryConvertToElement(object Value, out IElement Result)
		{
			int i;
			int c1 = this.c - 1;

			for (i = 0; i < c1; i++)
			{
				if (!this.converters[i].TryConvert(Value, out object Value2))
				{
					Result = null;
					return false;
				}

				Value = Value2;
			}

			return this.converters[c1].TryConvertToElement(Value, out Result);
		}
	}
}
