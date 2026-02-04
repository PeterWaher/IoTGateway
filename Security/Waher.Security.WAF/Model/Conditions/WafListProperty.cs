using Waher.Persistence.Attributes;

namespace Waher.Security.WAF.Model.Conditions
{
	/// <summary>
	/// Contains information about a property in a database list.
	/// </summary>
	[CollectionName("WafLists")]
	[TypeName(TypeNameSerialization.None)]
	[Index("List", "Property")]
	public class WafListProperty
	{
		/// <summary>
		/// Contains information about a property in a database list.
		/// </summary>
		public WafListProperty()
		{
		}

		/// <summary>
		/// Object ID
		/// </summary>
		[ObjectId]
		public string ObjectId { get; set; }

		/// <summary>
		/// List Name
		/// </summary>
		public string List { get; set; }

		/// <summary>
		/// Property Name
		/// </summary>
		public string Property { get; set; }
	}
}
