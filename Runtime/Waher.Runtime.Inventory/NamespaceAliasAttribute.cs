using System;

namespace Waher.Runtime.Inventory
{
	/// <summary>
	/// Lets the inventory know a namespace is also known by an other name. This attribute can be used when changing
	/// the name of a namespace, and allow for backwards compatibility. The attribute can be used multiple times
	/// on an assembly.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
	public class NamespaceAliasAttribute : Attribute
	{
		private readonly string @namespace;

		/// <summary>
		/// Lets the inventory know the type is also known by other names. This attribute can be used when changing
		/// name of type or namespace, and allow for backwards compatibility. The attribute can be used multiple times
		/// on each type.
		/// </summary>
		/// <param name="Namespace">Fully qualified alias namespace</param>
		public NamespaceAliasAttribute(string Namespace)
		{
			this.@namespace = Namespace;
		}

		/// <summary>
		/// Fully qualified alias namespace
		/// </summary>
		public string Namespace => this.@namespace;
	}
}
