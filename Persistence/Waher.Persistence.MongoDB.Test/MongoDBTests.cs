using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Waher.Persistence;
using Waher.Persistence.MongoDB;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Persistence.MongoDB.Test
{
	[TestFixture]
    public class MongoDBTests
    {
		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			try
			{
				Database.Register(new MongoDBProvider("MongoDBTest", "Default"));
			}
			catch (Exception)
			{
				// Ignore: The register method can only be executed once.
			}
		}

		[Test]
		public void Test_01_Insert_Default()
		{
			ThingReference Node = new ThingReference("Node1");
			Database.Insert(Node);
		}

	}
}
