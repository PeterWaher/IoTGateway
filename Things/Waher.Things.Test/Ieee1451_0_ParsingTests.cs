using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using Waher.Networking.MQTT;
using Waher.Runtime.Inventory;
using Waher.Security;
using Waher.Things.Ieee1451;
using Waher.Things.Ieee1451.Ieee1451_0;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;
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
		[DataRow("AgECAEUAAAAwOQAwOQAwOQfoBUk2WZAAMDkAMDkAMDkH6AVJOC5QhiWKC3L2EtaHB+gFSRHc8AABMjk4LjE1AAAAZwFGxwD/sRM=")]   // Source: ubi.pt
		public void Test_01_ParseMessage(string Base64Encoded)
		{
			byte[] Bin = Convert.FromBase64String(Base64Encoded);
			Console.Out.WriteLine("Length: " + Bin.Length.ToString());

			Assert.IsTrue(Ieee1451Parser.TryParseMessage(Bin, out Message Message));
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
		[DataRow(false, false, "030201003F00303900303900303907E8054936599000303900303900303907E80549382E5086258A0B72F612D68707E8054911DCF00001010000000000012A05F2000000")]    // Source: ubi.pt
		public void Test_02_ParseMetaTEDSRequest(bool Base64, bool IncludesMqttPackage, string Encoded)
		{
			byte[] Bin = Base64 ? Convert.FromBase64String(Encoded) : Hashes.StringToBinary(Encoded);
			if (IncludesMqttPackage)
				ProcessMqttPackage(ref Bin);

			Console.Out.WriteLine("Length: " + Bin.Length.ToString());

			Assert.IsTrue(Ieee1451Parser.TryParseMessage(Bin, out Message Message));

			TedsAccessMessage TedsAccessMessage = Message as TedsAccessMessage;
			Assert.IsNotNull(TedsAccessMessage);
			Assert.AreEqual(NetworkServiceType.TedsAccessServices, Message.NetworkServiceType);
			Assert.AreEqual(TedsAccessService.Read, TedsAccessMessage.TedsAccessService);
			Assert.AreEqual(MessageType.Command, Message.MessageType);

			Assert.IsTrue(TedsAccessMessage.TryParseRequest(out ChannelAddress Channel,
				out TedsAccessCode TedsAccesCode, out uint TedsOffset, out double TimeoutSeconds));

			Assert.IsNotNull(Channel);
			Assert.IsNotNull(Channel.ApplicationId);
			Assert.IsNotNull(Channel.NcapId);
			Assert.IsNotNull(Channel.TimId);

			Console.Out.WriteLine("Application: " + Hashes.BinaryToString(Channel.ApplicationId));
			Console.Out.WriteLine("NCAP: " + Hashes.BinaryToString(Channel.NcapId));
			Console.Out.WriteLine("TIM: " + Hashes.BinaryToString(Channel.TimId));
			Console.Out.WriteLine("Channel: " + Channel.ChannelId.ToString());
			Console.Out.WriteLine("TEDS: " + TedsAccesCode.ToString());
			Console.Out.WriteLine("Offset: " + TedsOffset.ToString());
			Console.Out.WriteLine("Timeout: " + TimeoutSeconds.ToString());
		}

		[DataTestMethod]
		[DataRow(false, "030202006D000000303900303900303907E8054936599000303900303900303907E80549382E5086258A0B72F612D68707E8054911DCF000010000000000000031030500FF010201041086258A0B72F612D68707E8054911DCF00A043F99999A0B043FB333330C0440A666660D020001F22C")]    // Source: ubi.pt
		public void Test_03_ParseMetaTEDSResponse(bool Base64, string Encoded)
		{
			byte[] Bin = Base64 ? Convert.FromBase64String(Encoded) : Hashes.StringToBinary(Encoded);
			Console.Out.WriteLine("Length: " + Bin.Length.ToString());

			Assert.IsTrue(Ieee1451Parser.TryParseMessage(Bin, out Message Message));

			TedsAccessMessage TedsAccessMessage = Message as TedsAccessMessage;
			Assert.IsNotNull(TedsAccessMessage);
			Assert.AreEqual(NetworkServiceType.TedsAccessServices, Message.NetworkServiceType);
			Assert.AreEqual(TedsAccessService.Read, TedsAccessMessage.TedsAccessService);
			Assert.AreEqual(MessageType.Reply, Message.MessageType);

			Assert.IsTrue(TedsAccessMessage.TryParseTeds(true, out ushort ErrorCode, out Teds Teds));
			Assert.AreEqual(0, ErrorCode);

			Assert.IsNotNull(Teds.Records);
			Assert.IsTrue(Teds.Records.Length > 0);

			foreach (Field Field in Teds.GetFields(ThingReference.Empty, DateTime.Now))
				Console.Out.WriteLine(Field.ToString());
		}

		[DataTestMethod]
		[DataRow(false, false, "030201003F00303900303900303907E8054936599000303900303900303907E80549382E5086258A0B72F612D68707E8054911DCF00001030000000000012A05F2000000")]    // Source: ubi.pt
		[DataRow(true, true, "MnAAKF8xNDUxLjEuNi9EMC9JTlRFUk9QLUlFQ09OMjAyNC9GUk9NLVhNUFAABgMCAQA/v7fnJXNmvLbUCZ8q9nseCc79hBCoBEvosu8QKDgrTaC14PU2zl5LBadyjsPbZW9HAAEDAAAAAAACVAvkAAAA")]
		public void Test_04_ParseTransducerChannelTEDSRequest(bool Base64, bool IncludesMqttPackage, string Encoded)
		{
			byte[] Bin = Base64 ? Convert.FromBase64String(Encoded) : Hashes.StringToBinary(Encoded);
			if (IncludesMqttPackage)
				ProcessMqttPackage(ref Bin);
			
			Console.Out.WriteLine("Length: " + Bin.Length.ToString());

			Assert.IsTrue(Ieee1451Parser.TryParseMessage(Bin, out Message Message));

			TedsAccessMessage TedsAccessMessage = Message as TedsAccessMessage;
			Assert.IsNotNull(TedsAccessMessage);
			Assert.AreEqual(NetworkServiceType.TedsAccessServices, Message.NetworkServiceType);
			Assert.AreEqual(TedsAccessService.Read, TedsAccessMessage.TedsAccessService);
			Assert.AreEqual(MessageType.Command, Message.MessageType);

			Assert.IsTrue(TedsAccessMessage.TryParseRequest(out ChannelAddress Channel,
				out TedsAccessCode TedsAccesCode, out uint TedsOffset, out double TimeoutSeconds));

			Assert.IsNotNull(Channel);
			Assert.IsNotNull(Channel.ApplicationId);
			Assert.IsNotNull(Channel.NcapId);
			Assert.IsNotNull(Channel.TimId);

			Console.Out.WriteLine("Application: " + Hashes.BinaryToString(Channel.ApplicationId));
			Console.Out.WriteLine("NCAP: " + Hashes.BinaryToString(Channel.NcapId));
			Console.Out.WriteLine("TIM: " + Hashes.BinaryToString(Channel.TimId));
			Console.Out.WriteLine("Channel: " + Channel.ChannelId.ToString());
			Console.Out.WriteLine("TEDS: " + TedsAccesCode.ToString());
			Console.Out.WriteLine("Offset: " + TedsOffset.ToString());
			Console.Out.WriteLine("Timeout: " + TimeoutSeconds.ToString());
		}

		[DataTestMethod]
		[DataRow(false, false, "030202009A000000303900303900303907E8054936599000303900303900303907E80549382E5086258A0B72F612D68707E8054911DCF00001000000000000005E030500FF0302010A01000B01000C0B00808080808080828080800D04436926660E0443C713330F0440000000100100110101120A2801012901042A02000E140440A0000015043F80000017044396000018043F800000190440A00000F0F8")]    // Source: ubi.pt
		[DataRow(true, true, "MnYAKF8xNDUxLjEuNi9EMC9JTlRFUk9QLUlFQ09OMjAyNC9GUk9NLVhNUFAArgMCAgBFAAB0Nq0uGRNaCCBH91H3sF3bzv2EEKgES+iy7xAoOCtNoLXg9TbOXksFp3KOw9tlb0cAAQAAAAAAAAAJAwVj/wECAf6I")]
		public void Test_05_ParseTransducerChannelTEDSResponse(bool Base64, bool IncludesMqttPackage, string Encoded)
		{
			byte[] Bin = Base64 ? Convert.FromBase64String(Encoded) : Hashes.StringToBinary(Encoded);
			if (IncludesMqttPackage)
				ProcessMqttPackage(ref Bin);

			Console.Out.WriteLine("Length: " + Bin.Length.ToString());

			Assert.IsTrue(Ieee1451Parser.TryParseMessage(Bin, out Message Message));

			TedsAccessMessage TedsAccessMessage = Message as TedsAccessMessage;
			Assert.IsNotNull(TedsAccessMessage);
			Assert.AreEqual(NetworkServiceType.TedsAccessServices, Message.NetworkServiceType);
			Assert.AreEqual(TedsAccessService.Read, TedsAccessMessage.TedsAccessService);
			Assert.AreEqual(MessageType.Reply, Message.MessageType);

			Assert.IsTrue(TedsAccessMessage.TryParseTeds(true, out ushort ErrorCode, out Teds Teds));
			Assert.AreEqual(0, ErrorCode);

			Assert.IsNotNull(Teds.Records);
			Assert.IsTrue(Teds.Records.Length > 0);

			foreach (Field Field in Teds.GetFields(ThingReference.Empty, DateTime.Now))
				Console.Out.WriteLine(Field.ToString());
		}

		[DataTestMethod]
		[DataRow(false, "030201003F00303900303900303907E8054936599000303900303900303907E80549382E5086258A0B72F612D68707E8054911DCF000010C0000000000012A05F2000000")]    // Source: ubi.pt
		public void Test_06_ParseTransducerNameTEDSRequest(bool Base64, string Encoded)
		{
			byte[] Bin = Base64 ? Convert.FromBase64String(Encoded) : Hashes.StringToBinary(Encoded);
			Console.Out.WriteLine("Length: " + Bin.Length.ToString());

			Assert.IsTrue(Ieee1451Parser.TryParseMessage(Bin, out Message Message));

			TedsAccessMessage TedsAccessMessage = Message as TedsAccessMessage;
			Assert.IsNotNull(TedsAccessMessage);
			Assert.AreEqual(NetworkServiceType.TedsAccessServices, Message.NetworkServiceType);
			Assert.AreEqual(TedsAccessService.Read, TedsAccessMessage.TedsAccessService);
			Assert.AreEqual(MessageType.Command, Message.MessageType);

			Assert.IsTrue(TedsAccessMessage.TryParseRequest(out ChannelAddress Channel,
				out TedsAccessCode TedsAccesCode, out uint TedsOffset, out double TimeoutSeconds));

			Assert.IsNotNull(Channel);
			Assert.IsNotNull(Channel.ApplicationId);
			Assert.IsNotNull(Channel.NcapId);
			Assert.IsNotNull(Channel.TimId);

			Console.Out.WriteLine("Application: " + Hashes.BinaryToString(Channel.ApplicationId));
			Console.Out.WriteLine("NCAP: " + Hashes.BinaryToString(Channel.NcapId));
			Console.Out.WriteLine("TIM: " + Hashes.BinaryToString(Channel.TimId));
			Console.Out.WriteLine("Channel: " + Channel.ChannelId.ToString());
			Console.Out.WriteLine("TEDS: " + TedsAccesCode.ToString());
			Console.Out.WriteLine("Offset: " + TedsOffset.ToString());
			Console.Out.WriteLine("Timeout: " + TimeoutSeconds.ToString());
		}

		[DataTestMethod]
		[DataRow(false, "0302020054000000303900303900303907E8054936599000303900303900303907E80549382E5086258A0B72F612D68707E8054911DCF000010000000000000018030500FF0C0201040100050A54504D20333620554249FC43")]    // Source: ubi.pt
		public void Test_07_ParseTransducerNameTEDSResponse(bool Base64, string Encoded)
		{
			byte[] Bin = Base64 ? Convert.FromBase64String(Encoded) : Hashes.StringToBinary(Encoded);
			Console.Out.WriteLine("Length: " + Bin.Length.ToString());

			Assert.IsTrue(Ieee1451Parser.TryParseMessage(Bin, out Message Message));

			TedsAccessMessage TedsAccessMessage = Message as TedsAccessMessage;
			Assert.IsNotNull(TedsAccessMessage);
			Assert.AreEqual(NetworkServiceType.TedsAccessServices, Message.NetworkServiceType);
			Assert.AreEqual(TedsAccessService.Read, TedsAccessMessage.TedsAccessService);
			Assert.AreEqual(MessageType.Reply, Message.MessageType);

			Assert.IsTrue(TedsAccessMessage.TryParseTeds(true, out ushort ErrorCode, out Teds Teds));
			Assert.AreEqual(0, ErrorCode);

			Assert.IsNotNull(Teds.Records);
			Assert.IsTrue(Teds.Records.Length > 0);

			foreach (Field Field in Teds.GetFields(ThingReference.Empty, DateTime.Now))
				Console.Out.WriteLine(Field.ToString());
		}

		[DataTestMethod]
		[DataRow(false, false, "020101003B00303900303900303907E8054936599000303900303900303907E80549382E5086258A0B72F612D68707E8054911DCF000010300012A05F2000000")]    // Source: ubi.pt
		[DataRow(true, false, "AgEBADs+poEuJakJwciMRiACMHvHADA5ADA5ADA5B+gFSTguUIYligty9hLWhwfoBUkR3PAAAQUAASoF8gAAAA==")]
		[DataRow(true, true, "MmwAKF8xNDUxLjEuNi9EMC9JTlRFUk9QLUlFQ09OMjAyNC9VQkktTkNBUDEABQIBAQA7PqaBLiWpCcHIjEYgAjB7xwAwOQAwOQAwOQfoBUk4LlCGJYoLcvYS1ocH6AVJEdzwAAEFAAEqBfIAAAA=")]
		[DataRow(true, true, "MmwAKF8xNDUxLjEuNi9EMC9JTlRFUk9QLUlFQ09OMjAyNC9VQkktTkNBUDEAAwIBAQA7PqaBLiWpCcHIjEYgAjB7xwAwOQAwOQAwOQfoBUk4LlCGJYoLcvYS1ocH6AVJEdzwAAEFAAJUC+QAAAA=")]
		[DataRow(true, true, "MmwAKF8xNDUxLjEuNi9EMC9JTlRFUk9QLUlFQ09OMjAyNC9GUk9NLVhNUFAAZQIBAQA7v7fnJXNmvLbUCZ8q9nseCc79hBCoBEvosu8QKDgrTaC14PU2zl5LBadyjsPbZW9HAAEFAAJUC+QAAAA=")]
		public void Test_08_ParseTransducerSampleDataRequest(bool Base64, bool IncludesMqttPackage, string Encoded)
		{
			byte[] Bin = Base64 ? Convert.FromBase64String(Encoded) : Hashes.StringToBinary(Encoded);
			if (IncludesMqttPackage)
				ProcessMqttPackage(ref Bin);

			Console.Out.WriteLine("Length: " + Bin.Length.ToString());

			Assert.IsTrue(Ieee1451Parser.TryParseMessage(Bin, out Message Message));

			TransducerAccessMessage TransducerAccessMessage = Message as TransducerAccessMessage;
			Assert.IsNotNull(TransducerAccessMessage);
			Assert.AreEqual(NetworkServiceType.TransducerAccessServices, Message.NetworkServiceType);
			Assert.AreEqual(TransducerAccessService.SyncReadTransducerSampleDataFromAChannelOfATIM, TransducerAccessMessage.TransducerAccessService);
			Assert.AreEqual(MessageType.Command, Message.MessageType);

			Assert.IsTrue(TransducerAccessMessage.TryParseRequest(out ChannelAddress Channel,
				out SamplingMode SamplingMode, out double TimeoutSeconds));

			Assert.IsNotNull(Channel);
			Assert.IsNotNull(Channel.ApplicationId);
			Assert.IsNotNull(Channel.NcapId);
			Assert.IsNotNull(Channel.TimId);

			Console.Out.WriteLine("Application: " + Hashes.BinaryToString(Channel.ApplicationId));
			Console.Out.WriteLine("NCAP: " + Hashes.BinaryToString(Channel.NcapId));
			Console.Out.WriteLine("TIM: " + Hashes.BinaryToString(Channel.TimId));
			Console.Out.WriteLine("Channel: " + Channel.ChannelId.ToString());
			Console.Out.WriteLine("Sampling: " + SamplingMode.ToString());
			Console.Out.WriteLine("Timeout: " + TimeoutSeconds.ToString());
		}

		[DataTestMethod]
		[DataRow(false, false, "0201020045000000303900303900303907E8054936599000303900303900303907E80549382E5086258A0B72F612D68707E8054911DCF000013330302E3135000000670010B72575FEF3")]    // Source: ubi.pt
		[DataRow(true, true, "MnYAKF8xNDUxLjEuNi9EMC9JTlRFUk9QLUlFQ09OMjAyNC9VQkktTkNBUDEADQIBAgBFAAA+poEuJakJwciMRiACMHvHADA5ADA5ADA5B+gFSTguUIYligty9hLWhwfoBUkR3PAAATI5OC4xNQAAAGcOfHcTz8Do")]
		[DataRow(true, true, "MnYAKF8xNDUxLjEuNi9EMC9JTlRFUk9QLUlFQ09OMjAyNC9VQkktTkNBUDEABwIBAgBFAACBLiWpCcHIjEYgAjB7xwAwOQAwOQAwOQfoBUk4LlCGJYoLcvYS1ocH6AVJEdzwAAEAATI5Ni4xNQAAAGcVQ4Aet1Wy")]
		[DataRow(true, true, "MnYAKF8xNDUxLjEuNi9EMC9JTlRFUk9QLUlFQ09OMjAyNC9GUk9NLVhNUFAAwAIBAgBFAAB0Nq0uGRNaCCBH91H3sF3bzv2EEKgES+iy7xAoOCtNoLXg9TbOXksFp3KOw9tlb0cAATI4MS41MwAAAGceumoAAAAA")]
		public void Test_09_ParseTransducerSampleDataResponse(bool Base64, bool IncludesMqttPackage, string Encoded)
		{
			byte[] Bin = Base64 ? Convert.FromBase64String(Encoded) : Hashes.StringToBinary(Encoded);
			if (IncludesMqttPackage)
				ProcessMqttPackage(ref Bin);

			Console.Out.WriteLine("Length: " + Bin.Length.ToString());

			Assert.IsTrue(Ieee1451Parser.TryParseMessage(Bin, out Message Message));

			TransducerAccessMessage TransducerAccessMessage = Message as TransducerAccessMessage;
			Assert.IsNotNull(TransducerAccessMessage);
			Assert.AreEqual(NetworkServiceType.TransducerAccessServices, Message.NetworkServiceType);
			Assert.AreEqual(TransducerAccessService.SyncReadTransducerSampleDataFromAChannelOfATIM, TransducerAccessMessage.TransducerAccessService);
			Assert.AreEqual(MessageType.Reply, Message.MessageType);

			Assert.IsTrue(TransducerAccessMessage.TryParseTransducerData(ThingReference.Empty, null, out ushort ErrorCode, out TransducerData Data));
			Assert.AreEqual(0, ErrorCode);

			ChannelAddress Channel = Data.ChannelInfo;

			Assert.IsNotNull(Channel);
			Assert.IsNotNull(Channel.ApplicationId);
			Assert.IsNotNull(Channel.NcapId);
			Assert.IsNotNull(Channel.TimId);

			Console.Out.WriteLine("Application: " + Hashes.BinaryToString(Channel.ApplicationId));
			Console.Out.WriteLine("NCAP: " + Hashes.BinaryToString(Channel.NcapId));
			Console.Out.WriteLine("TIM: " + Hashes.BinaryToString(Channel.TimId));
			Console.Out.WriteLine("Channel: " + Channel.ChannelId.ToString());

			Assert.IsNotNull(Data.Fields);
			Assert.IsTrue(Data.Fields.Length > 0);

			foreach (Field Field in Data.Fields)
				Console.Out.WriteLine(Field.ToString());
		}

		private static void ProcessMqttPackage(ref byte[] Bin)
		{
			BinaryInput Packet = new(Bin);
			MqttHeader Header;

			do
			{
				Header = MqttHeader.Parse(Packet);
				Assert.IsNotNull(Header);
			}
			while (Header.ControlPacketType == MqttControlPacketType.PUBACK);

			Assert.AreEqual(MqttControlPacketType.PUBLISH, Header.ControlPacketType);

			string Topic = Packet.ReadString();
			Console.Out.WriteLine("Topic: " + Topic);

			if (Header.QualityOfService > MqttQualityOfService.AtMostOnce)
				Packet.ReadUInt16();

			Bin = Packet.ReadBytes(Packet.BytesLeft);
		}

		[DataTestMethod]
		[DataRow(false, false, "010801001000303900303900303907E80549382E50")]    // Source: ubi.pt
		[DataRow(true, true, "MkEAKF8xNDUxLjEuNi9EMC9JTlRFUk9QLUlFQ09OMjAyNC9VQkktTkNBUDEABQEIAQAQPqaBLiWpCcHIjEYgAjB7xw==")]
		[DataRow(true, true, "MkEAKF8xNDUxLjEuNi9EMC9JTlRFUk9QLUlFQ09OMjAyNC9GUk9NLVhNUFAACAEIAQAQv7fnJXNmvLbUCZ8q9nseCQ==")]
		public void Test_10_ParseDiscoverNcapRequest(bool Base64, bool IncludesMqttPackage, string Encoded)
		{
			byte[] Bin = Base64 ? Convert.FromBase64String(Encoded) : Hashes.StringToBinary(Encoded);
			if (IncludesMqttPackage)
				ProcessMqttPackage(ref Bin);

			Console.Out.WriteLine("Length: " + Bin.Length.ToString());

			Assert.IsTrue(Ieee1451Parser.TryParseMessage(Bin, out Message Message));

			DiscoveryMessage DiscoveryMessage = Message as DiscoveryMessage;
			Assert.IsNotNull(DiscoveryMessage);
			Assert.AreEqual(NetworkServiceType.DiscoveryServices, Message.NetworkServiceType);
			Assert.AreEqual(DiscoveryService.NCAPDiscovery, DiscoveryMessage.DiscoveryService);
			Assert.AreEqual(MessageType.Command, Message.MessageType);

			Assert.IsTrue(DiscoveryMessage.TryParseMessage(out ushort ErrorCode, out DiscoveryData Data));
			Assert.AreEqual(0, ErrorCode);
			Assert.IsNotNull(Data);

			Assert.IsNotNull(Data.Channel);
			Assert.IsNotNull(Data.Channel.ApplicationId);
			Assert.IsNull(Data.Channel.NcapId);
			Assert.IsNull(Data.Channel.TimId);
			Assert.AreEqual(0, Data.Channel.ChannelId);

			Console.Out.WriteLine("Application: " + Hashes.BinaryToString(Data.Channel.ApplicationId));
		}

		[DataTestMethod]
		[DataRow(false, false, "0108020031000000303900303900303907E80549382E5000303900303900303907E80549382E505542492D4E434150310001728A86D2")]    // Source: ubi.pt
		[DataRow(true, true, "QAIABTJiAChfMTQ1MS4xLjYvRDAvSU5URVJPUC1JRUNPTjIwMjQvVUJJLU5DQVAxAAoBCAIAMQAAPqaBLiWpCcHIjEYgAjB7xwAwOQAwOQAwOQfoBUk4LlBVQkktTkNBUDEAAXKKhtI=")]
		[DataRow(true, true, "MmMAKF8xNDUxLjEuNi9EMC9JTlRFUk9QLUlFQ09OMjAyNC9GUk9NLVhNUFAABwEIAgAyAAB0Nq0uGRNaCCBH91H3sF3bzv2EEKgES+iy7xAoOCtNoFBST1hZLU5DQVAAAX8AAAE=")]
		public void Test_11_ParseDiscoverNcapResponse(bool Base64, bool IncludesMqttPackage, string Encoded)
		{
			byte[] Bin = Base64 ? Convert.FromBase64String(Encoded) : Hashes.StringToBinary(Encoded);
			if (IncludesMqttPackage)
				ProcessMqttPackage(ref Bin);

			Console.Out.WriteLine("Length: " + Bin.Length.ToString());

			Assert.IsTrue(Ieee1451Parser.TryParseMessage(Bin, out Message Message));

			DiscoveryMessage DiscoveryMessage = Message as DiscoveryMessage;
			Assert.IsNotNull(DiscoveryMessage);
			Assert.AreEqual(NetworkServiceType.DiscoveryServices, Message.NetworkServiceType);
			Assert.AreEqual(DiscoveryService.NCAPDiscovery, DiscoveryMessage.DiscoveryService);
			Assert.AreEqual(MessageType.Reply, Message.MessageType);

			Assert.IsTrue(DiscoveryMessage.TryParseMessage(out ushort ErrorCode, out DiscoveryData Data));
			Assert.AreEqual(0, ErrorCode);
			Assert.IsNotNull(Data);

			Assert.IsNotNull(Data.Channel);
			Assert.IsNotNull(Data.Channel.ApplicationId);
			Assert.IsNotNull(Data.Channel.NcapId);
			Assert.IsNull(Data.Channel.TimId);
			Assert.AreEqual(0, Data.Channel.ChannelId);

			DiscoveryDataEntity Entity = Data as DiscoveryDataEntity;
			Assert.IsNotNull(Entity);
			Assert.IsFalse(string.IsNullOrEmpty(Entity.Name));

			Console.Out.WriteLine("Application: " + Hashes.BinaryToString(Data.Channel.ApplicationId));
			Console.Out.WriteLine("NCAP: " + Hashes.BinaryToString(Data.Channel.NcapId));
			Console.Out.WriteLine("Name: " + Entity.Name);
		}

		[DataTestMethod]
		[DataRow(false, false, "010901002000303900303900303907E80549382E5000303900303900303907E80549382E50")]    // Source: ubi.pt
		[DataRow(true, true, "MlEAKF8xNDUxLjEuNi9EMC9JTlRFUk9QLUlFQ09OMjAyNC9GUk9NLVhNUFAADAEJAQAgv7fnJXNmvLbUCZ8q9nseCc79hBCoBEvosu8QKDgrTaA=")]
		[DataRow(true, true, "QAIABzJRAChfMTQ1MS4xLjYvRDAvSU5URVJPUC1JRUNPTjIwMjQvRlJPTS1YTVBQAA4BCQEAIL+35yVzZry21AmfKvZ7HgnO/YQQqARL6LLvECg4K02g")]
		public void Test_12_ParseDiscoverTimRequest(bool Base64, bool IncludesMqttPackage, string Encoded)
		{
			byte[] Bin = Base64 ? Convert.FromBase64String(Encoded) : Hashes.StringToBinary(Encoded);
			if (IncludesMqttPackage)
				ProcessMqttPackage(ref Bin);

			Console.Out.WriteLine("Length: " + Bin.Length.ToString());

			Assert.IsTrue(Ieee1451Parser.TryParseMessage(Bin, out Message Message));

			DiscoveryMessage DiscoveryMessage = Message as DiscoveryMessage;
			Assert.IsNotNull(DiscoveryMessage);
			Assert.AreEqual(NetworkServiceType.DiscoveryServices, Message.NetworkServiceType);
			Assert.AreEqual(DiscoveryService.NCAPTIMDiscovery, DiscoveryMessage.DiscoveryService);
			Assert.AreEqual(MessageType.Command, Message.MessageType);

			Assert.IsTrue(DiscoveryMessage.TryParseMessage(out ushort ErrorCode, out DiscoveryData Data));
			Assert.AreEqual(0, ErrorCode);
			Assert.IsNotNull(Data);

			Assert.IsNotNull(Data.Channel);
			Assert.IsNotNull(Data.Channel.ApplicationId);
			Assert.IsNotNull(Data.Channel.NcapId);
			Assert.IsNull(Data.Channel.TimId);
			Assert.AreEqual(0, Data.Channel.ChannelId);

			Console.Out.WriteLine("Application: " + Hashes.BinaryToString(Data.Channel.ApplicationId));
			Console.Out.WriteLine("NCAP: " + Hashes.BinaryToString(Data.Channel.NcapId));
		}

		[DataTestMethod]
		[DataRow(false, false, "010902003D000000303900303900303907E80549382E5000303900303900303907E80549382E50000186258A0B72F612D68707E8054911DCF05542492D54494D3100")]    // Source: ubi.pt
		[DataRow(true, true, "MmYAKF8xNDUxLjEuNi9EMC9JTlRFUk9QLUlFQ09OMjAyNC9GUk9NLVhNUFAACAEJAgA1AAB0Nq0uGRNaCCBH91H3sF3bzv2EEKgES+iy7xAoOCtNoAABteD1Ns5eSwWnco7D22VvRwA=")]
		public void Test_13_ParseDiscoverTimResponse(bool Base64, bool IncludesMqttPackage, string Encoded)
		{
			byte[] Bin = Base64 ? Convert.FromBase64String(Encoded) : Hashes.StringToBinary(Encoded);
			if (IncludesMqttPackage)
				ProcessMqttPackage(ref Bin);

			Console.Out.WriteLine("Length: " + Bin.Length.ToString());

			Assert.IsTrue(Ieee1451Parser.TryParseMessage(Bin, out Message Message));

			DiscoveryMessage DiscoveryMessage = Message as DiscoveryMessage;
			Assert.IsNotNull(DiscoveryMessage);
			Assert.AreEqual(NetworkServiceType.DiscoveryServices, Message.NetworkServiceType);
			Assert.AreEqual(DiscoveryService.NCAPTIMDiscovery, DiscoveryMessage.DiscoveryService);
			Assert.AreEqual(MessageType.Reply, Message.MessageType);

			Assert.IsTrue(DiscoveryMessage.TryParseMessage(out ushort ErrorCode, out DiscoveryData Data));
			Assert.AreEqual(0, ErrorCode);
			Assert.IsNotNull(Data);

			Assert.IsNotNull(Data.Channel);
			Assert.IsNotNull(Data.Channel.ApplicationId);
			Assert.IsNotNull(Data.Channel.NcapId);
			Assert.IsNull(Data.Channel.TimId);
			Assert.AreEqual(0, Data.Channel.ChannelId);

			DiscoveryDataEntities Entities = Data as DiscoveryDataEntities;
			Assert.IsNotNull(Entities);

			Console.Out.WriteLine("Application: " + Hashes.BinaryToString(Data.Channel.ApplicationId));
			Console.Out.WriteLine("NCAP: " + Hashes.BinaryToString(Data.Channel.NcapId));

			Assert.AreEqual(Entities.Identities.Length, Entities.Names.Length);

			int i, c = Entities.Names.Length;

			for (i = 0; i < c; i++)
			{
				Console.Out.WriteLine("TIM " + (i + 1).ToString() + ": " +
					Hashes.BinaryToString(Entities.Identities[i]) + " (" +
					Entities.Names[i] + ")");
			}
		}

		[DataTestMethod]
		[DataRow(false, false, "010A01003000303900303900303907E80549382E5000303900303900303907E80549382E5000303900303900303907E80549382E50")]    // Source: ubi.pt
		[DataRow(true, true, "QAIACDJhAChfMTQ1MS4xLjYvRDAvSU5URVJPUC1JRUNPTjIwMjQvRlJPTS1YTVBQABABCgEAML+35yVzZry21AmfKvZ7HgnO/YQQqARL6LLvECg4K02gteD1Ns5eSwWnco7D22VvRw==")]
		[DataRow(true, true, "MmEAKF8xNDUxLjEuNi9EMC9JTlRFUk9QLUlFQ09OMjAyNC9GUk9NLVhNUFAAJQEKAQAwv7fnJXNmvLbUCZ8q9nseCc79hBCoBEvosu8QKDgrTaC14PU2zl5LBadyjsPbZW9H")]
		public void Test_14_ParseDiscoverChannelRequest(bool Base64, bool IncludesMqttPackage, string Encoded)
		{
			byte[] Bin = Base64 ? Convert.FromBase64String(Encoded) : Hashes.StringToBinary(Encoded);
			if (IncludesMqttPackage)
				ProcessMqttPackage(ref Bin);

			Console.Out.WriteLine("Length: " + Bin.Length.ToString());

			Assert.IsTrue(Ieee1451Parser.TryParseMessage(Bin, out Message Message));

			DiscoveryMessage DiscoveryMessage = Message as DiscoveryMessage;
			Assert.IsNotNull(DiscoveryMessage);
			Assert.AreEqual(NetworkServiceType.DiscoveryServices, Message.NetworkServiceType);
			Assert.AreEqual(DiscoveryService.NCAPTIMTransducerDiscovery, DiscoveryMessage.DiscoveryService);
			Assert.AreEqual(MessageType.Command, Message.MessageType);

			Assert.IsTrue(DiscoveryMessage.TryParseMessage(out ushort ErrorCode, out DiscoveryData Data));
			Assert.AreEqual(0, ErrorCode);
			Assert.IsNotNull(Data);

			Assert.IsNotNull(Data.Channel);
			Assert.IsNotNull(Data.Channel.ApplicationId);
			Assert.IsNotNull(Data.Channel.NcapId);
			Assert.IsNotNull(Data.Channel.TimId);
			Assert.AreEqual(0, Data.Channel.ChannelId);

			Console.Out.WriteLine("Application: " + Hashes.BinaryToString(Data.Channel.ApplicationId));
			Console.Out.WriteLine("NCAP: " + Hashes.BinaryToString(Data.Channel.NcapId));
			Console.Out.WriteLine("TIM: " + Hashes.BinaryToString(Data.Channel.TimId));
		}

		[DataTestMethod]
		[DataRow(false, false, "010A02004D000000303900303900303907E80549382E5000303900303900303907E80549382E5086258A0B72F612D68707E8054911DCF0000100015542492D5452414E5344554345524348414E4E454C3100")]    // Source: ubi.pt
		public void Test_15_ParseDiscoverChannelResponse(bool Base64, bool IncludesMqttPackage, string Encoded)
		{
			byte[] Bin = Base64 ? Convert.FromBase64String(Encoded) : Hashes.StringToBinary(Encoded);
			if (IncludesMqttPackage)
				ProcessMqttPackage(ref Bin);

			Console.Out.WriteLine("Length: " + Bin.Length.ToString());

			Assert.IsTrue(Ieee1451Parser.TryParseMessage(Bin, out Message Message));

			DiscoveryMessage DiscoveryMessage = Message as DiscoveryMessage;
			Assert.IsNotNull(DiscoveryMessage);
			Assert.AreEqual(NetworkServiceType.DiscoveryServices, Message.NetworkServiceType);
			Assert.AreEqual(DiscoveryService.NCAPTIMTransducerDiscovery, DiscoveryMessage.DiscoveryService);
			Assert.AreEqual(MessageType.Reply, Message.MessageType);

			Assert.IsTrue(DiscoveryMessage.TryParseMessage(out ushort ErrorCode, out DiscoveryData Data));
			Assert.AreEqual(0, ErrorCode);
			Assert.IsNotNull(Data);

			Assert.IsNotNull(Data.Channel);
			Assert.IsNotNull(Data.Channel.ApplicationId);
			Assert.IsNotNull(Data.Channel.NcapId);
			Assert.IsNotNull(Data.Channel.TimId);
			Assert.AreEqual(0, Data.Channel.ChannelId);

			DiscoveryDataChannels Channels = Data as DiscoveryDataChannels;
			Assert.IsNotNull(Channels);

			Console.Out.WriteLine("Application: " + Hashes.BinaryToString(Data.Channel.ApplicationId));
			Console.Out.WriteLine("NCAP: " + Hashes.BinaryToString(Data.Channel.NcapId));
			Console.Out.WriteLine("TIM: " + Hashes.BinaryToString(Data.Channel.TimId));

			Assert.AreEqual(Channels.Channels.Length, Channels.Names.Length);

			int i, c = Channels.Names.Length;

			for (i = 0; i < c; i++)
			{
				Console.Out.WriteLine("Channel " + (i + 1).ToString() + ": " +
					Channels.Channels[i].ToString() + " (" + Channels.Names[i] + ")");
			}
		}

		[DataTestMethod]
		[DataRow(false, false, 
			"030202009A000000303900303900303907E8054936599000303900303900303907E80549382E5086258A0B72F612D68707E8054911DCF00001000000000000005E030500FF0302010A01000B01000C0B00808080808080828080800D04436926660E0443C713330F0440000000100100110101120A2801012901042A02000E140440A0000015043F80000017044396000018043F800000190440A00000F0F8",
			"0201020045000000303900303900303907E8054936599000303900303900303907E80549382E5086258A0B72F612D68707E8054911DCF000013330302E3135000000670010B72575FEF3")]    // Source: ubi.pt
		public void Test_16_ParseTransducerSampleDataResponseWithTeds(bool Base64, bool IncludesMqttPackage, 
			string TedsEncoded, string DataEncoded)
		{
			byte[] Bin = Base64 ? Convert.FromBase64String(TedsEncoded) : Hashes.StringToBinary(TedsEncoded);
			if (IncludesMqttPackage)
				ProcessMqttPackage(ref Bin);

			Assert.IsTrue(Ieee1451Parser.TryParseMessage(Bin, out Message Message));
			TedsAccessMessage TedsAccessMessage = Message as TedsAccessMessage;
			Assert.IsNotNull(TedsAccessMessage);

			Assert.IsTrue(TedsAccessMessage.TryParseTeds(out ushort ErrorCode, out Teds Teds));
			Assert.AreEqual(0, ErrorCode);

			Bin = Base64 ? Convert.FromBase64String(DataEncoded) : Hashes.StringToBinary(DataEncoded);
			if (IncludesMqttPackage)
				ProcessMqttPackage(ref Bin);

			Assert.IsTrue(Ieee1451Parser.TryParseMessage(Bin, out Message));

			TransducerAccessMessage TransducerAccessMessage = Message as TransducerAccessMessage;
			Assert.IsNotNull(TransducerAccessMessage);

			Assert.IsTrue(TransducerAccessMessage.TryParseTransducerData(ThingReference.Empty, Teds, out ErrorCode, out TransducerData Data));
			Assert.AreEqual(0, ErrorCode);

			Assert.IsNotNull(Data.Fields);
			Assert.IsTrue(Data.Fields.Length > 0);

			foreach (Field Field in Data.Fields)
				Console.Out.WriteLine(Field.ToString());
		}

	}
}
