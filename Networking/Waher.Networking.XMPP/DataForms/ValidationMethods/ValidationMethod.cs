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

		internal abstract void Validate(Field Field, DataType DataType, object[] Parsed, string[] Strings);

	}
}
