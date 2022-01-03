using System;

namespace Waher.Networking.XMPP.DataForms.DataTypes
{
	/// <summary>
	/// Abstract base class for all data types used in forms.
	/// </summary>
	public abstract class DataType
	{
		private readonly string typeName;

		/// <summary>
		/// Abstract base class for all data types used in forms.
		/// </summary>
		/// <param name="TypeName">Type Name</param>
		public DataType(string TypeName)
		{
			this.typeName = TypeName;
		}

		/// <summary>
		/// Type Name
		/// </summary>
		public string TypeName { get { return this.typeName; } }

		/// <summary>
		/// Parses a string.
		/// </summary>
		/// <param name="Value">String value.</param>
		/// <returns>Parsed value, if possible, null otherwise.</returns>
		public abstract object Parse(string Value);

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return this.GetType() == obj.GetType();
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return this.GetType().GetHashCode();
		}
	}
}
