using Waher.Persistence;
using Waher.Persistence.Attributes;

namespace Waher.Runtime.Counters.CounterObjects
{
	/// <summary>
	/// Contains a runtime counter.
	/// </summary>
	[CollectionName("RuntimeCounters")]
	[TypeName(TypeNameSerialization.None)]
	[Index("Key")]
	public class RuntimeCounter
	{
		/// <summary>
		/// Contains a runtime counter.
		/// </summary>
		public RuntimeCounter()
		{
			/// <summary>
			/// Contains a runtime counter.
			/// </summary>
		}

		/// <summary>
		/// Object ID
		/// </summary>
		[ObjectId]
		public string ObjectId { get; set; }

		/// <summary>
		/// Counter Key
		/// </summary>
		public CaseInsensitiveString Key { get; set; }

		/// <summary>
		/// Counter
		/// </summary>
		public long Counter { get; set; }
	}
}
