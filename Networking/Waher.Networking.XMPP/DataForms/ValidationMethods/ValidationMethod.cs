using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.XMPP.DataForms.DataTypes;

namespace Waher.Networking.XMPP.DataForms.ValidationMethods
{
	/// <summary>
	/// Base class of all validation methods.
	/// </summary>
	public abstract class ValidationMethod
	{
		/// <summary>
		/// Base class of all validation methods.
		/// </summary>
		public ValidationMethod()
		{
		}

		internal abstract void Serialize(StringBuilder Output);

		/// <summary>
		/// Validates the contents of a field. If an error is found, the <see cref="Field.Error"/> property is set accordingly.
		/// The <see cref="Field.Error"/> property is not cleared if no error is found.
		/// </summary>
		/// <param name="Field">Field</param>
		/// <param name="DataType">Data type of field.</param>
		/// <param name="Parsed">Parsed values.</param>
		/// <param name="Strings">String values.</param>
		public abstract void Validate(Field Field, DataType DataType, object[] Parsed, string[] Strings);

	}
}
