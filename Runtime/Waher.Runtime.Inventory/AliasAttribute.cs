using System;

namespace Waher.Runtime.Inventory
{
	/// <summary>
	/// Lets the inventory know the type is also known by other names. This attribute can be used when changing
	/// name of type or namespace, and allow for backwards compatibility.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
	public class AliasAttribute : Attribute
	{
		private readonly string[] typeNames;

		/// <summary>
		/// Lets the inventory know the type is also known by other names. This attribute can be used when changing
		/// name of type or namespace, and allow for backwards compatibility.
		/// </summary>
		public AliasAttribute(params string[] TypeNames)
		{
			this.typeNames = TypeNames;
		}

		/// <summary>
		/// Fully qualified alias type names
		/// </summary>
		public string[] TypeNames => this.typeNames;
	}
}
