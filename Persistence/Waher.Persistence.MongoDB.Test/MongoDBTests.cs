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
		public void Test_03_InsertManyDifferent()
		{
			ThingReference Ref = new ThingReference("Node1");
			DateTime TP = DateTime.Now;

			Database.Insert(
				new QuantityField(Ref, TP, "Temperature", 12.3, 1, "°C", FieldType.Momentary, FieldQoS.AutomaticReadout, "TestModule", 1, 2, 3),
				new BooleanField(Ref, TP, "Error", false, FieldType.Status, FieldQoS.AutomaticReadout, "TestModule", 2),
				new DateTimeField(Ref, TP, "Last Battery Change", new DateTime(2016, 2, 13, 10, 50, 25), FieldType.Status, FieldQoS.AutomaticReadout, "TestModule", 3),
				new Int32Field(Ref, TP, "Nr Restarts", 123, FieldType.Status, FieldQoS.AutomaticReadout, "TestModule", 4),
				new TimeField(Ref, TP, "Time of day", TP.TimeOfDay, FieldType.Status, FieldQoS.AutomaticReadout, "TestModule", 5),
				new StringField(Ref, TP, "Serial Number", "1234567890", FieldType.Identity, FieldQoS.AutomaticReadout, "TestModule", 6));
		}

		[Test]
		public void Test_04_Find()
		{
			Task<IEnumerable<ThingReference>> Task = Database.Find<ThingReference>();
			Assert.IsTrue(Task.Wait(5000));
			IEnumerable<ThingReference> ThingReferences = Task.Result;

			foreach (ThingReference ThingReference in ThingReferences)
				Console.Out.WriteLine(ThingReference.ToString());
		}

		[Test]
		public void Test_05_FindFilter()
		{
			Task<IEnumerable<ThingReference>> Task = Database.Find<ThingReference>(new FilterFieldEqualTo("NodeId", "Node2"));
			Assert.IsTrue(Task.Wait(5000));
			IEnumerable<ThingReference> ThingReferences = Task.Result;

			foreach (ThingReference ThingReference in ThingReferences)
				Console.Out.WriteLine(ThingReference.ToString());
		}

		[Test]
		public void Test_06_FindFilterSort()
		{
			Task<IEnumerable<ThingReference>> Task = Database.Find<ThingReference>(new FilterFieldLikeRegEx("NodeId", "Node(2|3)"), "-NodeId");
			Assert.IsTrue(Task.Wait(5000));
			IEnumerable<ThingReference> ThingReferences = Task.Result;

			foreach (ThingReference ThingReference in ThingReferences)
				Console.Out.WriteLine(ThingReference.ToString());
		}

		[Test]
		public void Test_07_FindInherited()
		{
			Task<IEnumerable<Field>> Task = Database.Find<Field>();
			Assert.IsTrue(Task.Wait(5000));
			IEnumerable<Field> Fields = Task.Result;

			foreach (Field Field in Fields)
				Console.Out.WriteLine(Field.ToString());
		}

		[Test]
		public void Test_08_Update()
		{
			Task<IEnumerable<ThingReference>> Task = Database.Find<ThingReference>();
			Assert.IsTrue(Task.Wait(5000));
			IEnumerable<ThingReference> ThingReferences = Task.Result;

			foreach (ThingReference ThingReference in ThingReferences)
				ThingReference.SourceId = "Source 1";

			Database.Update(ThingReferences);

			Task = Database.Find<ThingReference>();
			Assert.IsTrue(Task.Wait(5000));
			ThingReferences = Task.Result;

			foreach (ThingReference ThingReference in ThingReferences)
				Console.Out.WriteLine(ThingReference.ToString());
		}

		[Test]
		public void Test_09_Delete()
		{
			Task<IEnumerable<Field>> Task = Database.Find<Field>();
			Assert.IsTrue(Task.Wait(5000));
			IEnumerable<Field> Fields = Task.Result;

			Database.Delete(Fields);
		}

	}
}
