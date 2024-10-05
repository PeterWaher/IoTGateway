using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Waher.Things.Ieee1451.Ieee1451_0;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;

namespace Waher.Things.Test
{
	[TestClass]
	public class Ieee1451_0_ParsingTests
	{
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
		public void Test_02_ParseTEDS(string Base64Encoded)
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

			foreach (TedsRecord Record in Teds.Records)
			{
				Console.Out.WriteLine(Record.Type.ToString() + "\t" +
					Convert.ToBase64String(Record.RawValue));
			}
		}

	}
}
