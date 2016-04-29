using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Waher.Persistence;
using Waher.Persistence.Filters;
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
		public void Test_01_Insert()
		{
			ThingReference Node = new ThingReference("Node1");
			Database.Insert(Node);
		}

		[Test]
		public void Test_02_InsertMany()
		{
			Database.Insert(new ThingReference("Node2"), new ThingReference("Node3"), new ThingReference("Node4"));
		}

		[Test]
		public void Test_03_Find()
		{
			Task<IEnumerable<ThingReference>> Task = Database.Find<ThingReference>();
			Assert.IsTrue(Task.Wait(5000));
			IEnumerable<ThingReference> ThingReferences = Task.Result;

			foreach (ThingReference ThingReference in ThingReferences)
				Console.Out.WriteLine(ThingReference.ToString());
		}

		[Test]
		public void Test_04_FindFilter()
		{
			Task<IEnumerable<ThingReference>> Task = Database.Find<ThingReference>(new FilterFieldEqualTo("NodeId", "Node2"));
			Assert.IsTrue(Task.Wait(5000));
			IEnumerable<ThingReference> ThingReferences = Task.Result;

			foreach (ThingReference ThingReference in ThingReferences)
				Console.Out.WriteLine(ThingReference.ToString());
		}

	}
}
