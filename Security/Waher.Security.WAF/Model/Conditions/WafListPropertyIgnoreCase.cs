using Waher.Persistence;
using Waher.Persistence.Attributes;

namespace Waher.Security.WAF.Model.Conditions
{
	/// <summary>
	/// Contains information about a property in a database list.
	/// </summary>
	[CollectionName("WafListsIgnoreCase")]
	[TypeName(TypeNameSerialization.None)]
	[Index("List", "Property")]
	public class WafListPropertyIgnoreCase
	{
		/// <summary>
		/// Contains information about a property in a database list.
		/// </summary>
		public WafListPropertyIgnoreCase()
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
		public CaseInsensitiveString Property { get; set; }
	}
}
