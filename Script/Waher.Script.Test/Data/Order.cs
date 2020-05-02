using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Attributes;

namespace Waher.Script.Test.Data
{
	[CollectionName("Orders")]
	[TypeName(TypeNameSerialization.FullName)]
	public class Order
	{
		[ObjectId]
		public Guid ObjectID;
		public int OrderID;
		public int CustomerID;
		public DateTime OrderDate;
	}
}
