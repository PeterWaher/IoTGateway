using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Networking.HTTP.HTTP2;
using Waher.Security;

namespace Waher.Networking.HTTP.Test
{
	/// <summary>
	/// Test cases from RFC 7541:
	/// https://datatracker.ietf.org/doc/html/rfc7541
	/// </summary>
	[TestClass]
	public class Http2HeaderSerializationTests
	{
		/// <summary>
		/// C.1.1.  Example 1: Encoding 10 Using a 5-Bit Prefix
		/// </summary>
		[TestMethod]
		public void Test_01_C_1_1()
		{
			HeaderWriter w = new(1000, 4096);
			Assert.IsTrue(w.WriteBits(0, 3));
			Assert.IsTrue(w.WriteInteger(10));

			byte[] Output = w.ToArray();
			Assert.AreEqual("0a", Hashes.BinaryToString(Output));
		}

		/// <summary>
		/// C.1.2.  Example 2: Encoding 1337 Using a 5-Bit Prefix
		/// </summary>
		[TestMethod]
		public void Test_02_C_1_2()
		{
			HeaderWriter w = new(1000, 4096);
			Assert.IsTrue(w.WriteBits(0, 3));
			Assert.IsTrue(w.WriteInteger(1337));

			byte[] Output = w.ToArray();
			Assert.AreEqual("1f 9a 0a", Hashes.BinaryToString(Output, true));
		}

		/// <summary>
		/// C.1.3.  Example 3: Encoding 42 Starting at an Octet Boundary
		/// </summary>
		[TestMethod]
		public void Test_03_C_1_3()
		{
			HeaderWriter w = new(1000, 4096);
			Assert.IsTrue(w.WriteInteger(42));

			byte[] Output = w.ToArray();
			Assert.AreEqual("2a", Hashes.BinaryToString(Output, true));
		}

		/// <summary>
		/// C.2.1.  Literal Header Field with Indexing
		/// </summary>
		[TestMethod]
		public void Test_04_C_2_1()
		{
			HeaderWriter w = new(1000, 4096);
			Assert.IsTrue(w.WriteHeader("custom-key", "custom-header", IndexMode.Indexed, false));

			byte[] Output = w.ToArray();
			Assert.AreEqual("40 0a 63 75 73 74 6f 6d 2d 6b 65 79 0d 63 75 73 74 6f 6d 2d 68 65 61 64 65 72",
				Hashes.BinaryToString(Output, true));

			Assert.AreEqual(1U, w.NrDynamicHeaderRecords);
			Assert.AreEqual(55U, w.DynamicHeaderSize);

			DynamicRecord[] Records = w.GetDynamicRecords();
			Assert.AreEqual(1, Records.Length);

			DynamicRecord Rec = Records[0];
			Assert.AreEqual(55U, Rec.Length);
			Assert.AreEqual("custom-key", Rec.Header.Header);
			Assert.AreEqual("custom-header", Rec.Value);
		}

		/// <summary>
		/// C.2.2.  Literal Header Field without Indexing
		/// </summary>
		[TestMethod]
		public void Test_05_C_2_2()
		{
			HeaderWriter w = new(1000, 4096);
			Assert.IsTrue(w.WriteHeader(":path", "/sample/path", IndexMode.NotIndexed, false));

			byte[] Output = w.ToArray();
			Assert.AreEqual("04 0c 2f 73 61 6d 70 6c 65 2f 70 61 74 68",
				Hashes.BinaryToString(Output, true));

			Assert.AreEqual(0U, w.NrDynamicHeaderRecords);
			Assert.AreEqual(0U, w.DynamicHeaderSize);

			DynamicRecord[] Records = w.GetDynamicRecords();
			Assert.AreEqual(0, Records.Length);
		}

		/// <summary>
		/// C.2.3.  Literal Header Field Never Indexed
		/// </summary>
		[TestMethod]
		public void Test_06_C_2_3()
		{
			HeaderWriter w = new(1000, 4096);
			Assert.IsTrue(w.WriteHeader("password", "secret", IndexMode.NeverIndexed, false));

			byte[] Output = w.ToArray();
			Assert.AreEqual("10 08 70 61 73 73 77 6f 72 64 06 73 65 63 72 65 74",
				Hashes.BinaryToString(Output, true));

			Assert.AreEqual(0U, w.NrDynamicHeaderRecords);
			Assert.AreEqual(0U, w.DynamicHeaderSize);

			DynamicRecord[] Records = w.GetDynamicRecords();
			Assert.AreEqual(0, Records.Length);
		}

		/// <summary>
		/// C.2.4.  Indexed Header Field
		/// </summary>
		[TestMethod]
		public void Test_07_C_2_4()
		{
			HeaderWriter w = new(1000, 4096);
			Assert.IsTrue(w.WriteHeader(":method", "GET", IndexMode.Indexed, false));

			byte[] Output = w.ToArray();
			Assert.AreEqual("82",
				Hashes.BinaryToString(Output, true));

			Assert.AreEqual(0U, w.NrDynamicHeaderRecords);
			Assert.AreEqual(0U, w.DynamicHeaderSize);

			DynamicRecord[] Records = w.GetDynamicRecords();
			Assert.AreEqual(0, Records.Length);
		}

		/// <summary>
		/// C.3.1.  First Request
		/// </summary>
		[TestMethod]
		public void Test_08_C_3_1()
		{
			HeaderWriter w = new(1000, 4096);
			FirstRequest(w);

			byte[] Output = w.ToArray();
			Assert.AreEqual("82 86 84 41 0f 77 77 77 2e 65 78 61 6d 70 6c 65 2e 63 6f 6d",
				Hashes.BinaryToString(Output, true));

			Assert.AreEqual(1U, w.NrDynamicHeaderRecords);
			Assert.AreEqual(57U, w.DynamicHeaderSize);

			DynamicRecord[] Records = w.GetDynamicRecords();
			Assert.AreEqual(1, Records.Length);

			DynamicRecord Rec = Records[0];
			Assert.AreEqual(57U, Rec.Length);
			Assert.AreEqual(":authority", Rec.Header.Header);
			Assert.AreEqual("www.example.com", Rec.Value);
		}

		private static void FirstRequest(HeaderWriter w)
		{
			Assert.IsTrue(w.WriteHeader(":method", "GET", IndexMode.Indexed, false));
			Assert.IsTrue(w.WriteHeader(":scheme", "http", IndexMode.Indexed, false));
			Assert.IsTrue(w.WriteHeader(":path", "/", IndexMode.Indexed, false));
			Assert.IsTrue(w.WriteHeader(":authority", "www.example.com", IndexMode.Indexed, false));
		}

		/// <summary>
		/// C.3.2.  Second Request
		/// </summary>
		[TestMethod]
		public void Test_09_C_3_2()
		{
			HeaderWriter w = new(1000, 4096);

			FirstRequest(w);
			w.Reset();
			SecondRequest(w);

			byte[] Output = w.ToArray();
			Assert.AreEqual("82 86 84 be 58 08 6e 6f 2d 63 61 63 68 65",
				Hashes.BinaryToString(Output, true));

			Assert.AreEqual(2U, w.NrDynamicHeaderRecords);
			Assert.AreEqual(110U, w.DynamicHeaderSize);

			DynamicRecord[] Records = w.GetDynamicRecords();
			Assert.AreEqual(2, Records.Length);

			DynamicRecord Rec = Records[0];
			Assert.AreEqual(53U, Rec.Length);
			Assert.AreEqual("cache-control", Rec.Header.Header);
			Assert.AreEqual("no-cache", Rec.Value);

			Rec = Records[1];
			Assert.AreEqual(57U, Rec.Length);
			Assert.AreEqual(":authority", Rec.Header.Header);
			Assert.AreEqual("www.example.com", Rec.Value);
		}

		private static void SecondRequest(HeaderWriter w)
		{
			Assert.IsTrue(w.WriteHeader(":method", "GET", IndexMode.Indexed, false));
			Assert.IsTrue(w.WriteHeader(":scheme", "http", IndexMode.Indexed, false));
			Assert.IsTrue(w.WriteHeader(":path", "/", IndexMode.Indexed, false));
			Assert.IsTrue(w.WriteHeader(":authority", "www.example.com", IndexMode.Indexed, false));
			Assert.IsTrue(w.WriteHeader("cache-control", "no-cache", IndexMode.Indexed, false));
		}

		/// <summary>
		/// C.3.3.  Third Request
		/// </summary>
		[TestMethod]
		public void Test_10_C_3_3()
		{
			HeaderWriter w = new(1000, 4096);

			FirstRequest(w);
			w.Reset();
			SecondRequest(w);
			w.Reset();
			ThirdRequest(w);

			byte[] Output = w.ToArray();
			Assert.AreEqual("82 87 85 bf 40 0a 63 75 73 74 6f 6d 2d 6b 65 79 0c 63 75 73 74 6f 6d 2d 76 61 6c 75 65",
				Hashes.BinaryToString(Output, true));

			Assert.AreEqual(3U, w.NrDynamicHeaderRecords);
			Assert.AreEqual(164U, w.DynamicHeaderSize);

			DynamicRecord[] Records = w.GetDynamicRecords();
			Assert.AreEqual(3, Records.Length);

			DynamicRecord Rec = Records[0];
			Assert.AreEqual(54U, Rec.Length);
			Assert.AreEqual("custom-key", Rec.Header.Header);
			Assert.AreEqual("custom-value", Rec.Value);

			Rec = Records[1];
			Assert.AreEqual(53U, Rec.Length);
			Assert.AreEqual("cache-control", Rec.Header.Header);
			Assert.AreEqual("no-cache", Rec.Value);

			Rec = Records[2];
			Assert.AreEqual(57U, Rec.Length);
			Assert.AreEqual(":authority", Rec.Header.Header);
			Assert.AreEqual("www.example.com", Rec.Value);
		}

		private static void ThirdRequest(HeaderWriter w)
		{
			Assert.IsTrue(w.WriteHeader(":method", "GET", IndexMode.Indexed, false));
			Assert.IsTrue(w.WriteHeader(":scheme", "https", IndexMode.Indexed, false));
			Assert.IsTrue(w.WriteHeader(":path", "/index.html", IndexMode.Indexed, false));
			Assert.IsTrue(w.WriteHeader(":authority", "www.example.com", IndexMode.Indexed, false));
			Assert.IsTrue(w.WriteHeader("custom-key", "custom-value", IndexMode.Indexed, false));
		}
	}
}
