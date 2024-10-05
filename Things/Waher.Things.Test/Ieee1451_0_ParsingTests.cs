using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;
using Waher.Things.Ieee1451;
using Waher.Things.Ieee1451.Ieee1451_0;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;
using Waher.Things.Ieee1451.Ieee1451_0.TEDS;
using Waher.Things.SensorData;

namespace Waher.Things.Test
{
	[TestClass]
	public class Ieee1451_0_ParsingTests
	{
		[AssemblyInitialize]
		public static Task AssemblyInitialize(TestContext _)
		{
			Types.Initialize(typeof(Ieee1451Parser).Assembly);
			return Task.CompletedTask;
		}

		[DataTestMethod]
		[DataRow("AgECAEUAAAAwOQAwOQAwOQfoBUk2WZAAMDkAMDkAMDkH6AVJOC5QhiWKC3L2EtaHB+gFSRHc8AABMjk4LjE1AAAAZwFGxwD/sRM=")]
		public void Test_01_ParseMessage(string Base64Encoded)
		{
			byte[] Bin = Convert.FromBase64String(Base64Encoded);
			Assert.IsTrue(Parser.TryParseMessage(Bin, out Ieee1451_0Message Message));
			Assert.IsNotNull(Message);
			Assert.IsNotNull(Message.Body);
			Assert.IsNotNull(Message.Tail);
			Assert.AreEqual(0, Message.Tail.Length);

			Console.Out.WriteLine("NetworkServiceType: " + Message.NetworkServiceType.ToString());
			Console.Out.WriteLine("NetworkServiceId: " + Message.NetworkServiceIdName + " (" + Message.NetworkServiceId.ToString() + ")");
			Console.Out.WriteLine("MessageType: " + Message.MessageType.ToString());
			Console.Out.WriteLine("Body Length: " + Message.Body.Length.ToString());
			Console.Out.WriteLine("Tail Length: " + Message.Tail.Length.ToString());
		}

		[DataTestMethod]
		[DataRow("AwICAG0AAAAwOQAwOQAwOQfoBUk2WZAAMDkAMDkAMDkH6AVJOC5QhiWKC3L2EtaHB+gFSRHc8AABAAAAAAAAADEDBQD/AQIBBBCGJYoLcvYS1ocH6AVJEdzwCgQ/mZmaCwQ/szMzDARApmZmDQIAAQ3T")]
		public void Test_02_ParseMetaTEDS(string Base64Encoded)
		{
			byte[] Bin = Convert.FromBase64String(Base64Encoded);
			Assert.IsTrue(Parser.TryParseMessage(Bin, out Ieee1451_0Message Message));

			TedsAccessMessage TedsAccessMessage = Message as TedsAccessMessage;
			Assert.IsNotNull(TedsAccessMessage);
			Assert.AreEqual(NetworkServiceType.TedsAccessServices, Message.NetworkServiceType);
			Assert.AreEqual(TedsAccessService.Read, TedsAccessMessage.TedsAccessService);
			Assert.AreEqual(MessageType.Reply, Message.MessageType);

			Assert.IsTrue(TedsAccessMessage.TryParseTeds(false, out Ieee1451_0Teds Teds));
			// TODO: Check CheckSum

			Assert.IsNotNull(Teds.Records);
			Assert.IsTrue(Teds.Records.Length > 0);

			foreach (Field Field in Teds.GetFields(ThingReference.Empty, DateTime.Now))
				Console.Out.WriteLine(Field.ToString());
		}

		[DataTestMethod]
		[DataRow("AwICAJoAAAAwOQAwOQAwOQfoBUk2WZAAMDkAMDkAMDkH6AVJOC5QhiWKC3L2EtaHB+gFSRHc8AABAAAAAAAAAF4DBQD/AwIBCgEACwEADAsAgICAgICAgoCAgA0EQ2kmZg4EQ8cTMw8EQ4jTMxABABEBARIKKAEBKQEEKgIADhQEQKAAABUEP4AAABcEQ5YAABgEP4AAABkEQKAAABCY")]
		public void Test_03_ParseTransducerChannelTEDS(string Base64Encoded)
		{
			byte[] Bin = Convert.FromBase64String(Base64Encoded);
			Assert.IsTrue(Parser.TryParseMessage(Bin, out Ieee1451_0Message Message));

			TedsAccessMessage TedsAccessMessage = Message as TedsAccessMessage;
			Assert.IsNotNull(TedsAccessMessage);
			Assert.AreEqual(NetworkServiceType.TedsAccessServices, Message.NetworkServiceType);
			Assert.AreEqual(TedsAccessService.Read, TedsAccessMessage.TedsAccessService);
			Assert.AreEqual(MessageType.Reply, Message.MessageType);

			Assert.IsTrue(TedsAccessMessage.TryParseTeds(false, out Ieee1451_0Teds Teds));
			// TODO: Check CheckSum

			Assert.IsNotNull(Teds.Records);
			Assert.IsTrue(Teds.Records.Length > 0);

			foreach (Field Field in Teds.GetFields(ThingReference.Empty, DateTime.Now))
				Console.Out.WriteLine(Field.ToString());
		}

	}
}
