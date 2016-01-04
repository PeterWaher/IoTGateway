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
		private ValidationMethod additional;
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

		internal override void Validate(Field Field, DataType DataType, object[] Parsed, string[] Strings)
		{
			if (this.additional != null)
				this.additional.Validate(Field, DataType, Parsed, Strings);

			if (Strings.Length < this.min)
				Field.Error = "At least " + this.min.ToString() + " values need to be provided.";
			else if (Strings.Length > this.max)
				Field.Error = "At most " + this.max.ToString() + " values can be provided.";
		}
	}
}
