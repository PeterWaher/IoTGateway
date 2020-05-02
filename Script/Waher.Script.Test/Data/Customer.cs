using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Attributes;

namespace Waher.Script.Test.Data
{
	[CollectionName("Customers")]
	[TypeName(TypeNameSerialization.FullName)]
	public class Customer
	{
		[ObjectId]
		public Guid ObjectID;
		public int CustomerID;
		public string CustomerName;
		public string ContactName;
		public string Country;
	}
}
