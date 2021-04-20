using System;

namespace Waher.Persistence.Attributes
{
	/// <summary>
	/// This attribute defines the name of the collection that will house objects of this type.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
	public class CollectionNameAttribute : Attribute
	{
		private readonly string name;

		/// <summary>
		/// This attribute defines the name of the collection that will house objects of this type.
		/// </summary>
		/// <param name="Name">Collection name.</param>
		public CollectionNameAttribute(string Name)
			: base()
		{
			this.name = Name;
		}

		/// <summary>
		/// Collection name.
		/// </summary>
		public string Name
		{
			get { return this.name; }
		}
	}
}
