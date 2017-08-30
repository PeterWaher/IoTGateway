using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content;
using Waher.Events;
using Waher.Events.Console;
using Waher.Networking.Sniffers;
using Waher.Networking.CoAP.CoRE;
using Waher.Networking.CoAP.Options;
using Waher.Runtime.Inventory;
using Waher.Networking.LWM2M;
using Waher.Networking.LWM2M.ContentFormats;
using Waher.Security.DTLS;

namespace Waher.Networking.CoAP.Test
{
	[TestClass]
	public class CoapLwm2mTlvTests
	{
		private TlvWriter writer;

		[TestInitialize]
		public void TestInitialize()
		{
			this.writer = new TlvWriter();
		}

		[TestCleanup]
		public void TestCleanup()
		{
		}

		[TestMethod]
		public void LWM2M_TLV_Test_01_Single_Object_Instance_Request_Example()
		{
			// Example from §6.4.3.1, http://www.openmobilealliance.org/release/LightweightM2M/V1_0-20170208-A/OMA-TS-LightweightM2M-V1_0-20170208-A.pdf

			this.writer.Write(IdentifierType.Resource, 0, "Open Mobile Alliance");  // Manufacturer Resource 
			this.writer.Write(IdentifierType.Resource, 1, "Lightweight M2M Client");  // Model Number 
			this.writer.Write(IdentifierType.Resource, 2, "345000123");  // Serial Number
			this.writer.Write(IdentifierType.Resource, 3, "1.0");  // Firmware Version

			this.writer.Begin(IdentifierType.MultipleResource, 6);  // Available Power Sources 
			this.writer.Write(IdentifierType.ResourceInstance, 0, (sbyte)1);    // Available Power Sources[0] 
			this.writer.Write(IdentifierType.ResourceInstance, 1, (sbyte)5);    // Available Power Sources[1] 
			this.writer.End();

			this.writer.Begin(IdentifierType.MultipleResource, 7);  // Power Source Voltage 
			this.writer.Write(IdentifierType.ResourceInstance, 0, (short)0x0ed8);    // Power Source Voltage[0] 
			this.writer.Write(IdentifierType.ResourceInstance, 1, (short)0x1388);    // Power Source Voltage[1] 
			this.writer.End();

			this.writer.Begin(IdentifierType.MultipleResource, 8);  // Power Source Current
			this.writer.Write(IdentifierType.ResourceInstance, 0, (sbyte)0x7d);    // Power Source Current[0] 
			this.writer.Write(IdentifierType.ResourceInstance, 1, (short)0x0384);    // Power Source Current[1] 
			this.writer.End();

			this.writer.Write(IdentifierType.Resource, 9, (sbyte)0x64);  // Battery Level 
			this.writer.Write(IdentifierType.Resource, 10, (sbyte)0x0f);  // Memory Free

			this.writer.Begin(IdentifierType.MultipleResource, 11);  // Error Code
			this.writer.Write(IdentifierType.ResourceInstance, 0, (sbyte)0x00);    // Error Code[0] 
			this.writer.End();

			this.writer.Write(IdentifierType.Resource, 13, 0x5182428F);  // Current Time
			this.writer.Write(IdentifierType.Resource, 14, "+02:00");  // UTC Offset
			this.writer.Write(IdentifierType.Resource, 16, "U");  // Supported Binding and Modes 

			byte[] Encoded = this.writer.ToArray();
			byte[] Expected = new byte[]
			{
				0xC8, 0x00, 0x14, 0x4F, 0x70, 0x65, 0x6E, 0x20, 0x4D, 0x6F, 0x62, 0x69, 0x6C, 0x65, 0x20, 0x41, 0x6C, 0x6C, 0x69, 0x61, 0x6E, 0x63, 0x65,
				0xC8, 0x01, 0x16, 0x4C, 0x69, 0x67, 0x68, 0x74, 0x77, 0x65, 0x69, 0x67, 0x68, 0x74, 0x20, 0x4D, 0x32, 0x4D, 0x20, 0x43, 0x6C, 0x69, 0x65, 0x6E, 0x74,
				0xC8, 0x02, 0x09, 0x33, 0x34, 0x35, 0x30, 0x30, 0x30, 0x31, 0x32, 0x33,
				0xC3, 0x03, 0x31, 0x2E, 0x30,
				0x86, 0x06,
					0x41, 0x00, 0x01,
					0x41, 0x01, 0x05,
				0x88, 0x07, 0x08,
					0x42, 0x00, 0x0E, 0xD8,
					0x42, 0x01, 0x13, 0x88,
				0x87, 0x08,
					0x41, 0x00, 0x7D,
					0x42, 0x01, 0x03, 0x84,
				0xC1, 0x09, 0x64,
				0xC1, 0x0A, 0x0F,
				0x83, 0x0B,
					0x41, 0x00, 0x00,
				0xC4, 0x0D, 0x51, 0x82, 0x42, 0x8F,
				0xC6, 0x0E, 0x2B, 0x30, 0x32, 0x3A, 0x30, 0x30,
				0xC1, 0x10, 0x55
			};

			AssertEqual(Expected, Encoded);
		}

		[TestMethod]
		public void LWM2M_TLV_Test_02_Multiple_Object_Instance_Request_Example_A()
		{
			// Example from §6.4.3.2, http://www.openmobilealliance.org/release/LightweightM2M/V1_0-20170208-A/OMA-TS-LightweightM2M-V1_0-20170208-A.pdf

			this.writer.Begin(IdentifierType.ObjectInstance, 0);

			this.writer.Write(IdentifierType.Resource, 0, "Open Mobile Alliance");  // Manufacturer Resource 
			this.writer.Write(IdentifierType.Resource, 1, "Lightweight M2M Client");  // Model Number 
			this.writer.Write(IdentifierType.Resource, 2, "345000123");  // Serial Number
			this.writer.Write(IdentifierType.Resource, 3, "1.0");  // Firmware Version

			this.writer.Begin(IdentifierType.MultipleResource, 6);  // Available Power Sources 
			this.writer.Write(IdentifierType.ResourceInstance, 0, (sbyte)1);    // Available Power Sources[0] 
			this.writer.Write(IdentifierType.ResourceInstance, 1, (sbyte)5);    // Available Power Sources[1] 
			this.writer.End();

			this.writer.Begin(IdentifierType.MultipleResource, 7);  // Power Source Voltage 
			this.writer.Write(IdentifierType.ResourceInstance, 0, (short)0x0ed8);    // Power Source Voltage[0] 
			this.writer.Write(IdentifierType.ResourceInstance, 1, (short)0x1388);    // Power Source Voltage[1] 
			this.writer.End();

			this.writer.Begin(IdentifierType.MultipleResource, 8);  // Power Source Current
			this.writer.Write(IdentifierType.ResourceInstance, 0, (sbyte)0x7d);    // Power Source Current[0] 
			this.writer.Write(IdentifierType.ResourceInstance, 1, (short)0x0384);    // Power Source Current[1] 
			this.writer.End();

			this.writer.Write(IdentifierType.Resource, 9, (sbyte)0x64);  // Battery Level 
			this.writer.Write(IdentifierType.Resource, 10, (sbyte)0x0f);  // Memory Free

			this.writer.Begin(IdentifierType.MultipleResource, 11);  // Error Code
			this.writer.Write(IdentifierType.ResourceInstance, 0, (sbyte)0x00);    // Error Code[0] 
			this.writer.End();

			this.writer.Write(IdentifierType.Resource, 13, 0x5182428F);  // Current Time
			this.writer.Write(IdentifierType.Resource, 14, "+02:00");  // UTC Offset
			this.writer.Write(IdentifierType.Resource, 16, "U");  // Supported Binding and Modes 

			this.writer.End();

			byte[] Encoded = this.writer.ToArray();
			byte[] Expected = new byte[]
			{
				0x08, 0x00, 0x79,
					0xC8, 0x00, 0x14, 0x4F, 0x70, 0x65, 0x6E, 0x20, 0x4D, 0x6F, 0x62, 0x69, 0x6C, 0x65, 0x20, 0x41, 0x6C, 0x6C, 0x69, 0x61, 0x6E, 0x63, 0x65,
					0xC8, 0x01, 0x16, 0x4C, 0x69, 0x67, 0x68, 0x74, 0x77, 0x65, 0x69, 0x67, 0x68, 0x74, 0x20, 0x4D, 0x32, 0x4D, 0x20, 0x43, 0x6C, 0x69, 0x65, 0x6E, 0x74,
					0xC8, 0x02, 0x09, 0x33, 0x34, 0x35, 0x30, 0x30, 0x30, 0x31, 0x32, 0x33,
					0xC3, 0x03, 0x31, 0x2E, 0x30,
					0x86, 0x06,
						0x41, 0x00, 0x01,
						0x41, 0x01, 0x05,
					0x88, 0x07, 0x08,
						0x42, 0x00, 0x0E, 0xD8,
						0x42, 0x01, 0x13, 0x88,
					0x87, 0x08,
						0x41, 0x00, 0x7D,
						0x42, 0x01, 0x03, 0x84,
					0xC1, 0x09, 0x64,
					0xC1, 0x0A, 0x0F,
					0x83, 0x0B,
						0x41, 0x00, 0x00,
					0xC4, 0x0D, 0x51, 0x82, 0x42, 0x8F,
					0xC6, 0x0E, 0x2B, 0x30, 0x32, 0x3A, 0x30, 0x30,
					0xC1, 0x10, 0x55
			};

			AssertEqual(Expected, Encoded);
		}

		[TestMethod]
		public void LWM2M_TLV_Test_03_Multiple_Object_Instance_Request_Example_B()
		{
			// Example from §6.4.3.2, http://www.openmobilealliance.org/release/LightweightM2M/V1_0-20170208-A/OMA-TS-LightweightM2M-V1_0-20170208-A.pdf

			this.writer.Begin(IdentifierType.ObjectInstance, 0);    // Access Control Object Instance 0
			this.writer.Write(IdentifierType.Resource, 0, (sbyte)1);    // Object ID 
			this.writer.Write(IdentifierType.Resource, 1, (sbyte)0);    // Object Instance ID 

			this.writer.Begin(IdentifierType.MultipleResource, 2);  // ACL
			this.writer.Write(IdentifierType.ResourceInstance, 127, (sbyte)7);  // ACL [127]
			this.writer.End();

			this.writer.Write(IdentifierType.Resource, 3, (sbyte)0x7f); // Access Control Owner 
			this.writer.End();

			this.writer.Begin(IdentifierType.ObjectInstance, 2);    // Access Control Object Instance 2
			this.writer.Write(IdentifierType.Resource, 0, (sbyte)3);    // Object ID 
			this.writer.Write(IdentifierType.Resource, 1, (sbyte)0);    // Object Instance ID 

			this.writer.Begin(IdentifierType.MultipleResource, 2);  // ACL
			this.writer.Write(IdentifierType.ResourceInstance, 127, (sbyte)7);  // ACL [127]
			this.writer.Write(IdentifierType.ResourceInstance, 310, (sbyte)1);  // ACL [127]
			this.writer.End();

			this.writer.Write(IdentifierType.Resource, 3, (sbyte)0x7f); // Access Control Owner 
			this.writer.End();


			byte[] Encoded = this.writer.ToArray();
			byte[] Expected = new byte[]
			{
				0x08, 0x00, 0x0E,
					0xC1, 0x00, 0x01,
					0xC1, 0x01, 0x00,
					0x83, 0x02,
						0x41, 0x7F, 0x07,
					0xC1, 0x03, 0x7F,
				0x08, 0x02, 0x12,
					0xC1, 0x00, 0x03,
					0xC1, 0x01, 0x00,
					0x87, 0x02,
						0x41, 0x7F, 0x07,
						0x61, 0x01, 0x36, 0x01,
					0xC1, 0x03, 0x7F
			};

			AssertEqual(Expected, Encoded);
		}

		[TestMethod]
		public void LWM2M_TLV_Test_04_Multiple_Object_Instance_Request_Example_D()
		{
			// Example from §6.4.3.2, http://www.openmobilealliance.org/release/LightweightM2M/V1_0-20170208-A/OMA-TS-LightweightM2M-V1_0-20170208-A.pdf

			this.writer.Begin(IdentifierType.ObjectInstance, 0);    // Server Object Instance 0 
			this.writer.Write(IdentifierType.Resource, 0, (sbyte)1);    // Short Server ID 
			this.writer.Write(IdentifierType.Resource, 1, 86400);    // Lifetime
			this.writer.Write(IdentifierType.Resource, 6, true);    // Notification Storing When Disabled or Offline 
			this.writer.Write(IdentifierType.Resource, 7, "U"); // Binding 
			this.writer.End();


			byte[] Encoded = this.writer.ToArray();
			byte[] Expected = new byte[]
			{
				0x08, 0x00, 0x0F,
					0xC1, 0x00, 0x01,
					0xC4, 0x01, 0x00, 0x01, 0x51, 0x80,
					0xC1, 0x06, 0x01,
					0xC1, 0x07, 0x55
			};

			AssertEqual(Expected, Encoded);
		}

		[TestMethod]
		public void LWM2M_TLV_Test_05_Object_Link_Example_1()
		{
			// Example from §6.4.3.3, http://www.openmobilealliance.org/release/LightweightM2M/V1_0-20170208-A/OMA-TS-LightweightM2M-V1_0-20170208-A.pdf

			this.writer.Begin(IdentifierType.MultipleResource, 0);    // Res 0 lnk 
			this.writer.Write(IdentifierType.ResourceInstance, 0, 0x0042, 0x0000);    // Res 0 lnk [0] 
			this.writer.Write(IdentifierType.ResourceInstance, 1, 0x0042, 0x0001);    // Res 0 lnk [1] 
			this.writer.End();
			this.writer.Write(IdentifierType.Resource, 1, "8613800755500");    // Res 1
			this.writer.Write(IdentifierType.Resource, 2, 0x12345678);    // Res 2

			byte[] Encoded = this.writer.ToArray();
			byte[] Expected = new byte[]
			{
				0x88, 0x00, 0x0C,
					0x44, 0x00, 0x00, 0x42, 0x00, 0x00,
					0x44, 0x01, 0x00, 0x42, 0x00, 0x01,
				0xC8, 0x01, 0x0D, 0x38, 0x36, 0x31, 0x33, 0x38, 0x30, 0x30, 0x37, 0x35, 0x35, 0x35, 0x30, 0x30,
				0xC4, 0x02, 0x12, 0x34, 0x56, 0x78
			};

			AssertEqual(Expected, Encoded);
		}

		[TestMethod]
		public void LWM2M_TLV_Test_06_Object_Link_Example_2()
		{
			// Example from §6.4.3.3, http://www.openmobilealliance.org/release/LightweightM2M/V1_0-20170208-A/OMA-TS-LightweightM2M-V1_0-20170208-A.pdf

			this.writer.Begin(IdentifierType.ObjectInstance, 0);    // Object 66 Instance 0  
			this.writer.Write(IdentifierType.Resource, 0, "myService 1");    // Res 0
			this.writer.Write(IdentifierType.Resource, 1, "Internet.15.234");    // Res 1
			this.writer.Write(IdentifierType.Resource, 2, 0x0043, 0x0000);    // Res 2 lnk
			this.writer.End();

			this.writer.Begin(IdentifierType.ObjectInstance, 1);    // Object 66 Instance 1  
			this.writer.Write(IdentifierType.Resource, 0, "myService 2");    // Res 0
			this.writer.Write(IdentifierType.Resource, 1, "Internet.15.235");    // Res 1
			this.writer.Write(IdentifierType.Resource, 2, 0xffff, 0xffff);    // Res 2 lnk
			this.writer.End();

			byte[] Encoded = this.writer.ToArray();
			byte[] Expected = new byte[]
			{
				0x08, 0x00, 0x26,
					0xC8, 0x00, 0x0B, 0x6D, 0x79, 0x53, 0x65, 0x72, 0x76, 0x69, 0x63, 0x65, 0x20, 0x31,
					0xC8, 0x01, 0x0F, 0x49, 0x6E, 0x74, 0x65, 0x72, 0x6E, 0x65, 0x74, 0x2E, 0x31, 0x35, 0x2E, 0x32, 0x33, 0x34,
					0xC4, 0x02, 0x00, 0x43, 0x00, 0x00,
				0x08, 0x01, 0x26,
					0xC8, 0x00, 0x0B, 0x6D, 0x79, 0x53, 0x65, 0x72, 0x76, 0x69, 0x63, 0x65, 0x20, 0x32,
					0xC8, 0x01, 0x0F, 0x49, 0x6E, 0x74, 0x65, 0x72, 0x6E, 0x65, 0x74, 0x2E, 0x31, 0x35, 0x2E, 0x32, 0x33, 0x35,
					0xC4, 0x02, 0xFF, 0xFF, 0xFF, 0xFF
			};

			AssertEqual(Expected, Encoded);
		}

		public static void AssertEqual(byte[] Expected, byte[] A)
		{
			if (A == null)
				throw new ArgumentException("Array is null.", nameof(A));

			int i, c = Expected.Length;
			if (c != A.Length)
				throw new Exception("Arrays of different length.");

			for (i = 0; i < c; i++)
			{
				if (Expected[i] != A[i])
					throw new Exception("Arrays differ at index " + i.ToString() +
						": Expected " + Expected[i].ToString() + ", but got " + A[i].ToString());
			}
		}
		[TestMethod]
		public void LWM2M_TLV_Test_07_Decode_BootstrapInfo()
		{
			byte[] Tlv = new byte[]
			{
				0xc8, 0x00, 0x1e, 0x63, 0x6f, 0x61, 0x70, 0x3a,
				0x2f, 0x2f, 0x6c, 0x65, 0x73, 0x68, 0x61, 0x6e,
				0x2e, 0x65, 0x63, 0x6c, 0x69, 0x70, 0x73, 0x65,
				0x2e, 0x6f, 0x72, 0x67, 0x3a, 0x35, 0x36, 0x38,
				0x33, 0xc1, 0x01, 0x01, 0xc1, 0x02, 0x03, 0xc0,
				0x03, 0xc0, 0x04, 0xc0, 0x05, 0xc1, 0x06, 0x03,
				0xc0, 0x07, 0xc0, 0x08, 0xc0, 0x09, 0xc1, 0x0a,
				0x6f, 0xc1, 0x0b, 0x01, 0xc1, 0x0c, 0x00
			};

			object Decoded = InternetContent.Decode(TlvDecoder.ContentType, Tlv, null);
			Assert.IsNotNull(Decoded);

			TlvRecord[] Records = Decoded as TlvRecord[];
			Assert.IsNotNull(Records);

			foreach (TlvRecord Record in Records)
				Console.Out.WriteLine(Record.ToString());
		}

	}
}
