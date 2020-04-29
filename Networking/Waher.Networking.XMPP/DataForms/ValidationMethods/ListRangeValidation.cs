using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.XMPP.DataForms.DataTypes;

namespace Waher.Networking.XMPP.DataForms.ValidationMethods
{
	/// <summary>
	/// Performs list-range validation.
	/// 
	/// Defined in:
	/// http://xmpp.org/extensions/xep-0122.html#usecases-ranges
	/// </summary>
	public class ListRangeValidation : ValidationMethod
	{
		private readonly ValidationMethod additional;
		private int min;
		private int max;

		/// <summary>
		/// Performs list-range validation.
		/// 
		/// Defined in:
		/// http://xmpp.org/extensions/xep-0122.html#usecases-ranges
		/// </summary>
		/// <param name="Additional">Additional validation method.</param>
		/// <param name="Min">Minimum number of options to select.</param>
		/// <param name="Max">Maximum number of options to select.</param>
		public ListRangeValidation(ValidationMethod Additional, int Min, int Max)
			: base()
		{
			this.additional = Additional;
			this.min = Min;
			this.max = Max;
		}

		/// <summary>
		/// Additional validation method.
		/// </summary>
		public ValidationMethod Additional { get { return this.additional; } }

		/// <summary>
		/// Minimum number of options to select.
		/// </summary>
		public int Min { get { return this.min; } }

		/// <summary>
		/// Maximum number of options to select.
		/// </summary>
		public int Max { get { return this.max; } }

		internal override void Serialize(StringBuilder Output)
		{
			if (this.additional != null)
				this.additional.Serialize(Output);

			Output.Append("<list-range");

			if (this.min > 0)
			{
				Output.Append(" min='");
				Output.Append(this.min.ToString());
				Output.Append("'");
			}

			if (this.max < int.MaxValue)
			{
				Output.Append(" max='");
				Output.Append(this.max.ToString());
				Output.Append("'");
			}

			Output.Append("/>");
		}

		/// <summary>
		/// Validates the contents of a field. If an error is found, the <see cref="Field.Error"/> property is set accordingly.
		/// The <see cref="Field.Error"/> property is not cleared if no error is found.
		/// </summary>
		/// <param name="Field">Field</param>
		/// <param name="DataType">Data type of field.</param>
		/// <param name="Parsed">Parsed values.</param>
		/// <param name="Strings">String values.</param>
		public override void Validate(Field Field, DataType DataType, object[] Parsed, string[] Strings)
		{
			if (this.additional != null)
				this.additional.Validate(Field, DataType, Parsed, Strings);

			if (Strings.Length < this.min)
			{
				if (this.min == 1)
					Field.Error = "At least 1 value needs to be provided.";
				else
					Field.Error = "At least " + this.min.ToString() + " values need to be provided.";
			}
			else if (Strings.Length > this.max)
			{
				if (this.max == 1)
					Field.Error = "At most 1 value can be provided.";
				else
					Field.Error = "At most " + this.max.ToString() + " values can be provided.";
			}
		}

		/// <summary>
		/// Merges the validation method with a secondary validation method, if possible.
		/// </summary>
		/// <param name="SecondaryValidationMethod">Secondary validation method to merge with.</param>
		/// <param name="DataType">Underlying data type.</param>
		/// <returns>If merger was possible.</returns>
		public override bool Merge(ValidationMethod SecondaryValidationMethod, DataType DataType)
		{
			if (!(SecondaryValidationMethod is ListRangeValidation V2))
				return false;

			if ((this.additional is null) ^ (V2.additional is null))
				return false;

			if (!this.additional.Merge(V2.additional, DataType))
				return false;

			this.min = Math.Max(this.min, V2.min);
			this.max = Math.Min(this.max, V2.max);

			return this.min <= this.max;
		}
	}
}
