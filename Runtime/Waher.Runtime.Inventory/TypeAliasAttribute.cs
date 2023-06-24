using System;

namespace Waher.Runtime.Inventory
{
	/// <summary>
	/// Lets the inventory know the type is also known by another other name. This attribute can be used when changing
	/// name of type or namespace, and allow for backwards compatibility. The attribute can be used multiple times
	/// on each type.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
	public class TypeAliasAttribute : Attribute
	{
		private readonly string typeName;

		/// <summary>
		/// Lets the inventory know the type is also known by another other name. This attribute can be used when changing
		/// name of type or namespace, and allow for backwards compatibility. The attribute can be used multiple times
		/// on each type.
		/// </summary>
		/// <param name="TypeName">Fully qualified alias type name</param>
		public TypeAliasAttribute(string TypeName)
		{
			this.typeName = TypeName;
		}

		/// <summary>
		/// Fully qualified alias type name
		/// </summary>
		public string TypeName => this.typeName;
	}
}
