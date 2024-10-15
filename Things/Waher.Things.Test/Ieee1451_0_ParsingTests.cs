using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;
using Waher.Networking.MQTT;
using Waher.Networking;
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
		[DataRow(false, "030201003F00303900303900303907E8054936599000303900303900303907E80549382E5086258A0B72F612D68707E8054911DCF00001010000000000012A05F2000000")]    // Source: ubi.pt
		public void Test_02_ParseMetaTEDSRequest(bool Base64, string Encoded)
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
		[DataRow(false, "030201003F00303900303900303907E8054936599000303900303900303907E80549382E5086258A0B72F612D68707E8054911DCF00001030000000000012A05F2000000")]    // Source: ubi.pt
		public void Test_04_ParseTransducerChannelTEDSRequest(bool Base64, string Encoded)
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
		[DataRow(false, "030202009A000000303900303900303907E8054936599000303900303900303907E80549382E5086258A0B72F612D68707E8054911DCF00001000000000000005E030500FF0302010A01000B01000C0B00808080808080828080800D04436926660E0443C713330F0440000000100100110101120A2801012901042A02000E140440A0000015043F80000017044396000018043F800000190440A00000F0F8")]    // Source: ubi.pt
		public void Test_05_ParseTransducerChannelTEDSResponse(bool Base64, string Encoded)
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

			Assert.IsTrue(TransducerAccessMessage.TryParseTransducerData(ThingReference.Empty, out ushort ErrorCode, out TransducerData Data));
			Assert.AreEqual(0, ErrorCode);

			Assert.IsNotNull(Data.Fields);
			Assert.IsTrue(Data.Fields.Length > 0);

			foreach (Field Field in Data.Fields)
				Console.Out.WriteLine(Field.ToString());
		}

		private static void ProcessMqttPackage(ref byte[] Bin)
		{
			BinaryInput Packet = new(Bin);
			MqttHeader Header = MqttHeader.Parse(Packet);
			Assert.IsNotNull(Header);
			Assert.AreEqual(MqttControlPacketType.PUBLISH, Header.ControlPacketType);

			string Topic = Packet.ReadString();
			Console.Out.WriteLine("Topic: " + Topic);

			if (Header.QualityOfService > MqttQualityOfService.AtMostOnce)
				Packet.ReadUInt16();

			Bin = Packet.ReadBytes(Packet.BytesLeft);
		}
	}
}
