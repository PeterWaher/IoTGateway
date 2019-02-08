using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Persistence.MongoDB;
using Waher.Runtime.Inventory;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Persistence.MongoDB.Test
{
	[TestClass]
	public class MongoDBTests
	{
		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext Context)
		{
			Types.Initialize(typeof(MongoDBProvider).Assembly, 
				typeof(MongoDBTests).Assembly, 
				typeof(Types).Assembly,
				typeof(ThingReference).Assembly);

			Database.Register(new MongoDBProvider("MongoDBTest", "Default"));
		}

		[TestMethod]
		public async Task Test_01_Insert()
		{
			ThingReference Node = new ThingReference("Node1");
			await Database.Insert(Node);
		}

		[TestMethod]
		public async Task Test_02_InsertMany()
		{
			await Database.Insert(new ThingReference("Node2"), new ThingReference("Node3"), new ThingReference("Node4"));
		}

		[TestMethod]
		public async Task Test_03_InsertManyDifferent()
		{
			ThingReference Ref = new ThingReference("Node1");
			DateTime TP = DateTime.Now;

			await Database.Insert(
				new QuantityField(Ref, TP, "Temperature", 12.3, 1, "°C", FieldType.Momentary, FieldQoS.AutomaticReadout, "TestModule", 1, 2, 3),
				new BooleanField(Ref, TP, "Error", false, FieldType.Status, FieldQoS.AutomaticReadout, "TestModule", 2),
				new DateTimeField(Ref, TP, "Last Battery Change", new DateTime(2016, 2, 13, 10, 50, 25), FieldType.Status, FieldQoS.AutomaticReadout, "TestModule", 3),
				new Int32Field(Ref, TP, "Nr Restarts", 123, FieldType.Status, FieldQoS.AutomaticReadout, "TestModule", 4),
				new TimeField(Ref, TP, "Time of day", TP.TimeOfDay, FieldType.Status, FieldQoS.AutomaticReadout, "TestModule", 5),
				new StringField(Ref, TP, "Serial Number", "1234567890", FieldType.Identity, FieldQoS.AutomaticReadout, "TestModule", 6));
		}

		[TestMethod]
		public async Task Test_04_Find()
		{
			IEnumerable<ThingReference> ThingReferences = await Database.Find<ThingReference>();

			foreach (ThingReference ThingReference in ThingReferences)
				Console.Out.WriteLine(ThingReference.ToString());
		}

		[TestMethod]
		public async Task Test_05_FindFilter()
		{
			IEnumerable<ThingReference> ThingReferences = await Database.Find<ThingReference>(new FilterFieldEqualTo("NodeId", "Node2"));

			foreach (ThingReference ThingReference in ThingReferences)
				Console.Out.WriteLine(ThingReference.ToString());
		}

		[TestMethod]
		public async Task Test_06_FindFilterSort()
		{
			IEnumerable<ThingReference> ThingReferences = await Database.Find<ThingReference>(new FilterFieldLikeRegEx("NodeId", "Node(2|3)"), "-NodeId");

			foreach (ThingReference ThingReference in ThingReferences)
				Console.Out.WriteLine(ThingReference.ToString());
		}

		[TestMethod]
		public async Task Test_07_FindInherited()
		{
			IEnumerable<Field> Fields = await Database.Find<Field>();

			foreach (Field Field in Fields)
				Console.Out.WriteLine(Field.ToString());
		}

		[TestMethod]
		public async Task Test_08_Update()
		{
			IEnumerable<ThingReference> ThingReferences = await Database.Find<ThingReference>();

			foreach (ThingReference ThingReference in ThingReferences)
				ThingReference.SourceId = "Source 1";

			await Database.Update(ThingReferences);

			ThingReferences = await Database.Find<ThingReference>();

			foreach (ThingReference ThingReference in ThingReferences)
				Console.Out.WriteLine(ThingReference.ToString());
		}

		[TestMethod]
		public async Task Test_09_Delete()
		{
			IEnumerable<Field> Fields = await Database.Find<Field>();
			await Database.Delete(Fields);
		}

	}
}
