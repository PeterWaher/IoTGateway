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
		[DataRow("AwIBAEMAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAQAAABAAAAAAAAAAAAAAAAAAAAA=")]   // 
		[DataRow("AwICAJlgAAAwOQAwOQAwOQfoBUk2WZAAMDkAMDkAMDkH6AVJOC5QhiWKC3L2EtaHB+gFSRHc8AAAAGADBQD/AwIBCgEACwEADAsAgICAgICAgoCAgA0EQ2kmZg4EQ8cTMw8EQ4jTMxABABEBARIKKAEBKQEEKgIADhQEQKAAABUEP4AAABcEQ5YAABgEP4AAABkEQKAAAACM")]
		[DataRow("AgECAEUAAAAwOQAwOQAwOQfoBUk2WZAAMDkAMDkAMDkH6AVJOC5QhiWKC3L2EtaHB+gFSRHc8AABASoAAGb5UzwDpn6x")]
		public void Test_01_ParseMessage(string Base64Encoded)
		{
			byte[] Bin = Convert.FromBase64String(Base64Encoded);
			Assert.IsTrue(Parser.TryParseMessage(Bin, out Ieee14510Message Message));
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
		[DataRow("AgECAEUAAAAwOQAwOQAwOQfoBUk2WZAAMDkAMDkAMDkH6AVJOC5QhiWKC3L2EtaHB+gFSRHc8AABASoAAGb5UzwDpn6x")]
		public void Test_02_ParseTEDS(string Base64Encoded)
		{
			byte[] Bin = Convert.FromBase64String(Base64Encoded);
			Assert.IsTrue(Parser.TryParseMessage(Bin, out Ieee14510Message Message));
			Assert.IsTrue(Message.TryParseTeds(false, out Ieee14510Teds Teds));
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
