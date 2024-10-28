using Waher.Persistence.Attributes;

namespace Waher.Persistence.XmlLedger.Test.Classes
{
	[CollectionName("Test")]
	public class SimpleObject
	{
		[ObjectId]
		public Guid ObjectId { get; set; }
		public string? Text { get; set; }
		public int Number1 { get; set; }
		public double Number2 { get; set; }
		public bool Flag { get; set; }
	}
}
