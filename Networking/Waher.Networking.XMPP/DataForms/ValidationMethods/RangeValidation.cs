using System;
using System.Text;
using Waher.Content.Xml;
using Waher.Networking.XMPP.DataForms.DataTypes;

namespace Waher.Networking.XMPP.DataForms.ValidationMethods
{
	/// <summary>
	/// Performs range validation.
	/// 
	/// Defined in:
	/// http://xmpp.org/extensions/xep-0122.html#usercases-validation.range
	/// </summary>
	public class RangeValidation : BasicValidation
	{
		private string min;
		private string max;

		/// <summary>
		/// Performs range validation.
		/// 
		/// Defined in:
		/// http://xmpp.org/extensions/xep-0122.html#usercases-validation.range
		/// </summary>
		/// <param name="Min">Minimum value (string representation).</param>
		/// <param name="Max">Maximum value (string representation).</param>
		public RangeValidation(string Min, string Max)
			: base()
		{
			this.min = Min;
			this.max = Max;
		}

		/// <summary>
		/// Minimum value (string representation).
		/// </summary>
		public string Min { get { return this.min; } }

		/// <summary>
		/// Maximum value (string representation).
		/// </summary>
		public string Max { get { return this.max; } }

		internal override void Serialize(StringBuilder Output)
		{
			Output.Append("<range");

			if (!string.IsNullOrEmpty(this.min))
			{
				Output.Append(" min='");
				Output.Append(XML.Encode(this.min));
				Output.Append("'");
			}

			if (!string.IsNullOrEmpty(this.max))
			{
				Output.Append(" max='");
				Output.Append(XML.Encode(this.max));
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
			base.Validate(Field, DataType, Parsed, Strings);

			IComparable Min;
			IComparable Max;

			if (string.IsNullOrEmpty(this.min))
				Min = null;
			else
				Min = DataType.Parse(this.min) as IComparable;

			if (string.IsNullOrEmpty(this.max))
				Max = null;
			else
				Max = DataType.Parse(this.max) as IComparable;

			foreach (object Obj in Parsed)
			{
				if (Min != null && Min.CompareTo(Obj) > 0)
					Field.Error = "Value out of range.";
				else if (Max != null && Max.CompareTo(Obj) < 0)
					Field.Error = "Value out of range.";
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
			if (!(SecondaryValidationMethod is RangeValidation V2))
				return false;

			if (string.IsNullOrEmpty(this.min) ^ string.IsNullOrEmpty(V2.min))
				return false;

			if (string.IsNullOrEmpty(this.max) ^ string.IsNullOrEmpty(V2.max))
				return false;

			if (this.min == V2.min && this.max == V2.max)
				return true;

			if (DataType is null)
				return false;

			object Min, Max;
			object Min2, Max2;
			IComparable Comparable;
			int i;

			if (!string.IsNullOrEmpty(this.min) && this.min != V2.min)
			{
				if (string.IsNullOrEmpty(this.min))
					Min = null;
				else
				{
					Min = DataType.Parse(this.min);
					if (Min is null)
						return false;
				}

				if (string.IsNullOrEmpty(V2.min))
					Min2 = null;
				else
				{
					Min2 = DataType.Parse(V2.min);
					if (Min2 is null)
						return false;
				}

				Comparable = Min as IComparable;
				if (Comparable is null)
					return false;

				try
				{
					i = Comparable.CompareTo(Min2);
				}
				catch (Exception)
				{
					return false;
				}

				if (i < 0)
					this.min = V2.min;
			}

			if (!string.IsNullOrEmpty(this.max) && this.max != V2.max)
			{
				if (string.IsNullOrEmpty(this.max))
					Max = null;
				else
				{
					Max = DataType.Parse(this.max);
					if (Max is null)
						return false;
				}

				if (string.IsNullOrEmpty(V2.max))
					Max2 = null;
				else
				{
					Max2 = DataType.Parse(V2.max);
					if (Max2 is null)
						return false;
				}

				Comparable = Max as IComparable;
				if (Comparable is null)
					return false;

				try
				{
					i = Comparable.CompareTo(Max2);
				}
				catch (Exception)
				{
					return false;
				}

				if (i > 0)
					this.max = V2.max;
			}

			return true;
		}

	}
}
