using System;
using System.Reflection;

namespace Waher.Persistence.Filters
{
	/// <summary>
	/// Abstract base class for all field filters operating on a constant value.
	/// </summary>
	public abstract class FilterFieldValue : FilterField
	{
		private readonly object value;

		/// <summary>
		/// Abstract base class for all field filters operating on a constant value.
		/// </summary>
		/// <param name="FieldName">Field Name.</param>
		/// <param name="Value">Value.</param>
		public FilterFieldValue(string FieldName, object Value)
			: base(FieldName)
		{
			this.value = Value;
		}

		/// <summary>
		/// Value.
		/// </summary>
		public object Value
		{
			get { return this.value; }
		}

		/// <summary>
		/// Returns a normalized filter.
		/// </summary>
		/// <returns>Normalized filter.</returns>
		public override Filter Normalize()
		{
			return this.Copy();
		}

		/// <summary>
		/// Performs a comparison on the object with the field value <paramref name="Value"/>.
		/// </summary>
		/// <param name="Value">Field value for comparison.</param>
		/// <returns>Result of comparison.</returns>
		public override bool Compare(object Value)
		{
			Type T1 = Value.GetType();
			Type T2 = this.value.GetType();
			TypeCode Code1 = Convert.GetTypeCode(Value);
			TypeClass Class1 = GetTypeClass(Code1);

			if (T1 != T2 || Class1 == TypeClass.Numeric || Class1 == TypeClass.String)
			{
				TypeCode Code2 = Convert.GetTypeCode(this.value);
				TypeClass Class2 = GetTypeClass(Code2);

				if (Class1 != Class2)
					return false;

				switch (Class1)
				{
					case TypeClass.Boolean:     // Cannot be different types
					case TypeClass.DateTime:    // Cannot be different types
					case TypeClass.Unknown:     // Unknown type
					default:
						return false;

					case TypeClass.Numeric:
						if (!(Value is double d1))
							d1 = Convert.ToDouble(Value);

						if (!(this.value is double d2))
							d2 = Convert.ToDouble(this.value);

						return this.Compare(d1, d2);

					case TypeClass.String:
						if (!(Value is string s1))
							s1 = Value.ToString();

						if (!(this.value is string s2))
							s2 = this.value.ToString();

						return this.Compare(s1, s2);
				}
			}
			else
				return this.Compare(Value, this.value);
		}

		/// <summary>
		/// Compares two object values.
		/// </summary>
		/// <param name="Value1">Value 1</param>
		/// <param name="Value2">Value 2</param>
		/// <returns>Result</returns>
		protected abstract bool Compare(object Value1, object Value2);

		/// <summary>
		/// Compares two numerical values.
		/// </summary>
		/// <param name="Value1">Value 1</param>
		/// <param name="Value2">Value 2</param>
		/// <returns>Result</returns>
		protected abstract bool Compare(double Value1, double Value2);

		/// <summary>
		/// Compares two string values.
		/// </summary>
		/// <param name="Value1">Value 1</param>
		/// <param name="Value2">Value 2</param>
		/// <returns>Result</returns>
		protected abstract bool Compare(string Value1, string Value2);

		private enum TypeClass
		{
			Boolean,
			Numeric,
			String,
			DateTime,
			Unknown
		}

		private static TypeClass GetTypeClass(TypeCode TC)
		{
			switch (TC)
			{
				case TypeCode.Boolean:
					return TypeClass.Boolean;

				case TypeCode.Char:
				case TypeCode.String:
					return TypeClass.String;

				case TypeCode.DateTime:
					return TypeClass.DateTime;
			
				case TypeCode.Single:
				case TypeCode.Double:
				case TypeCode.Decimal:
				case TypeCode.Byte:
				case TypeCode.SByte:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					return TypeClass.Numeric;

				case TypeCode.Empty:
				case TypeCode.Object:
				default:
					return TypeClass.Unknown;
			}
		}

	}
}
