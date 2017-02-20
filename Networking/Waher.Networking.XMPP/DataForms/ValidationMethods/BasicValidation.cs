using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.XMPP.DataForms.DataTypes;

namespace Waher.Networking.XMPP.DataForms.ValidationMethods
{
	/// <summary>
	/// Performs basic validation.
	/// 
	/// Defined in:
	/// http://xmpp.org/extensions/xep-0122.html#usercases-validation.basic
	/// </summary>
	public class BasicValidation : ValidationMethod
	{
		/// <summary>
		/// Performs basic validation.
		/// 
		/// Defined in:
		/// http://xmpp.org/extensions/xep-0122.html#usercases-validation.basic
		/// </summary>
		public BasicValidation()
			: base()
		{
		}

		internal override void Serialize(StringBuilder Output)
		{
			Output.Append("<basic/>");
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
			KeyValuePair<string, string>[] Options = Field.Options;

			if (Options != null && Options.Length > 0)
			{
				foreach (string s in Strings)
				{
					if (Array.FindIndex<KeyValuePair<string, string>>(Options, P => P.Value == s) < 0)
					{
						Field.Error = "Value not in allowed set of options.";
						return;
					}
				}
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
			return SecondaryValidationMethod is BasicValidation;
		}
	}
}
