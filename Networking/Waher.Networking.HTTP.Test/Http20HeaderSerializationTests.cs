using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Waher.Networking.HTTP.HTTP2;
using Waher.Security;

namespace Waher.Networking.HTTP.Test
{
	/// <summary>
	/// Test cases from RFC 7541:
	/// https://datatracker.ietf.org/doc/html/rfc7541
	/// </summary>
	[TestClass]
	public class Http20HeaderSerializationTests
	{
		/// <summary>
		/// C.1.1.  Example 1: Encoding 10 Using a 5-Bit Prefix
		/// </summary>
		[TestMethod]
		public void Test_01_C_1_1()
		{
			HeaderWriter w = new(256, 256);
			Assert.IsTrue(w.WriteByteBits(0, 3));
			Assert.IsTrue(w.WriteInteger(10));

			byte[] Output = w.ToArray();
			Assert.AreEqual("0a", Hashes.BinaryToString(Output));

			HeaderReader r = new(Output, 256);
			Assert.IsTrue(r.ReadByteBits(out byte b, 2));
			Assert.AreEqual(0, b);

			Assert.IsTrue(r.ReadInteger(out ulong Value));
			Assert.AreEqual(10UL, Value);
		}

		/// <summary>
		/// C.1.2.  Example 2: Encoding 1337 Using a 5-Bit Prefix
		/// </summary>
		[TestMethod]
		public void Test_02_C_1_2()
		{
			HeaderWriter w = new(256, 256);
			Assert.IsTrue(w.WriteByteBits(0, 3));
			Assert.IsTrue(w.WriteInteger(1337));

			byte[] Output = w.ToArray();
			Assert.AreEqual("1f 9a 0a", Hashes.BinaryToString(Output, true));

			HeaderReader r = new(Output, 256);
			Assert.IsTrue(r.ReadByteBits(out byte b, 3));
			Assert.AreEqual(0, b);

			Assert.IsTrue(r.ReadInteger(out ulong Value));
			Assert.AreEqual(1337UL, Value);
		}

		/// <summary>
		/// C.1.3.  Example 3: Encoding 42 Starting at an Octet Boundary
		/// </summary>
		[TestMethod]
		public void Test_03_C_1_3()
		{
			HeaderWriter w = new(256, 256);
			Assert.IsTrue(w.WriteInteger(42));

			byte[] Output = w.ToArray();
			Assert.AreEqual("2a", Hashes.BinaryToString(Output, true));

			HeaderReader r = new(Output, 256);
			Assert.IsTrue(r.ReadInteger(out ulong Value));
			Assert.AreEqual(42UL, Value);
		}

		/// <summary>
		/// C.2.1.  Literal Header Field with Indexing
		/// </summary>
		[TestMethod]
		public void Test_04_C_2_1()
		{
			HeaderWriter w = new(256, 256);
			Assert.IsTrue(w.WriteHeader("custom-key", "custom-header", IndexMode.Indexed, false));

			byte[] Output = w.ToArray();
			Assert.AreEqual("40 0a 63 75 73 74 6f 6d 2d 6b 65 79 0d 63 75 73 74 6f 6d 2d 68 65 61 64 65 72",
				Hashes.BinaryToString(Output, true));

			HeaderReader r = new(Output, 256);
			Assert.IsTrue(r.ReadFields(out IEnumerable<HttpField> Fields));
			List<HttpField> Fields2 = [.. Fields];

			Assert.AreEqual(1, Fields2.Count);
			Assert.AreEqual("custom-key", Fields2[0].Key);
			Assert.AreEqual("custom-header", Fields2[0].Value);

			foreach (DynamicHeaders d in new DynamicHeaders[] { w, r })
			{
				Assert.AreEqual(1U, d.NrDynamicHeaderRecords);
				Assert.AreEqual(55, d.DynamicHeaderSize);

				DynamicRecord[] Records = d.GetDynamicRecords();
				Assert.AreEqual(1, Records.Length);

				DynamicRecord Rec = Records[0];
				Assert.AreEqual(55, Rec.Length);
				Assert.AreEqual("custom-key", Rec.Header.Header);
				Assert.AreEqual("custom-header", Rec.Value);
			}
		}

		/// <summary>
		/// C.2.2.  Literal Header Field without Indexing
		/// </summary>
		[TestMethod]
		public void Test_05_C_2_2()
		{
			HeaderWriter w = new(256, 256);
			Assert.IsTrue(w.WriteHeader(":path", "/sample/path", IndexMode.NotIndexed, false));

			byte[] Output = w.ToArray();
			Assert.AreEqual("04 0c 2f 73 61 6d 70 6c 65 2f 70 61 74 68",
				Hashes.BinaryToString(Output, true));

			HeaderReader r = new(Output, 256);
			Assert.IsTrue(r.ReadFields(out IEnumerable<HttpField> Fields));
			List<HttpField> Fields2 = [.. Fields];

			Assert.AreEqual(1, Fields2.Count);
			Assert.AreEqual(":path", Fields2[0].Key);
			Assert.AreEqual("/sample/path", Fields2[0].Value);

			foreach (DynamicHeaders d in new DynamicHeaders[] { w, r })
			{
				Assert.AreEqual(0U, d.NrDynamicHeaderRecords);
				Assert.AreEqual(0, d.DynamicHeaderSize);

				DynamicRecord[] Records = d.GetDynamicRecords();
				Assert.AreEqual(0, Records.Length);
			}
		}

		/// <summary>
		/// C.2.3.  Literal Header Field Never Indexed
		/// </summary>
		[TestMethod]
		public void Test_06_C_2_3()
		{
			HeaderWriter w = new(256, 256);
			Assert.IsTrue(w.WriteHeader("password", "secret", IndexMode.NeverIndexed, false));

			byte[] Output = w.ToArray();
			Assert.AreEqual("10 08 70 61 73 73 77 6f 72 64 06 73 65 63 72 65 74",
				Hashes.BinaryToString(Output, true));

			HeaderReader r = new(Output, 256);
			Assert.IsTrue(r.ReadFields(out IEnumerable<HttpField> Fields));
			List<HttpField> Fields2 = [.. Fields];

			Assert.AreEqual(1, Fields2.Count);
			Assert.AreEqual("password", Fields2[0].Key);
			Assert.AreEqual("secret", Fields2[0].Value);

			foreach (DynamicHeaders d in new DynamicHeaders[] { w, r })
			{
				Assert.AreEqual(0U, d.NrDynamicHeaderRecords);
				Assert.AreEqual(0, d.DynamicHeaderSize);

				DynamicRecord[] Records = d.GetDynamicRecords();
				Assert.AreEqual(0, Records.Length);
			}
		}

		/// <summary>
		/// C.2.4.  Indexed Header Field
		/// </summary>
		[TestMethod]
		public void Test_07_C_2_4()
		{
			HeaderWriter w = new(256, 256);
			Assert.IsTrue(w.WriteHeader(":method", "GET", IndexMode.Indexed, false));

			byte[] Output = w.ToArray();
			Assert.AreEqual("82",
				Hashes.BinaryToString(Output, true));

			HeaderReader r = new(Output, 256);
			Assert.IsTrue(r.ReadFields(out IEnumerable<HttpField> Fields));
			List<HttpField> Fields2 = [.. Fields];

			Assert.AreEqual(1, Fields2.Count);
			Assert.AreEqual(":method", Fields2[0].Key);
			Assert.AreEqual("GET", Fields2[0].Value);

			foreach (DynamicHeaders d in new DynamicHeaders[] { w, r })
			{
				Assert.AreEqual(0U, d.NrDynamicHeaderRecords);
				Assert.AreEqual(0, d.DynamicHeaderSize);

				DynamicRecord[] Records = d.GetDynamicRecords();
				Assert.AreEqual(0, Records.Length);
			}
		}

		/// <summary>
		/// C.3.1.  First Request (without Huffman encoding)
		/// </summary>
		[TestMethod]
		public void Test_08_C_3_1()
		{
			HeaderWriter w = new(256, 256);
			FirstRequest(w, false);

			byte[] Output = w.ToArray();
			Assert.AreEqual("82 86 84 41 0f 77 77 77 2e 65 78 61 6d 70 6c 65 2e 63 6f 6d",
				Hashes.BinaryToString(Output, true));

			HeaderReader r = new(Output, 256);
			Assert.IsTrue(r.ReadFields(out IEnumerable<HttpField> Fields));
			List<HttpField> Fields2 = [.. Fields];

			Assert.AreEqual(4, Fields2.Count);
			Assert.AreEqual(":method", Fields2[0].Key);
			Assert.AreEqual("GET", Fields2[0].Value);
			Assert.AreEqual(":scheme", Fields2[1].Key);
			Assert.AreEqual("http", Fields2[1].Value);
			Assert.AreEqual(":path", Fields2[2].Key);
			Assert.AreEqual("/", Fields2[2].Value);
			Assert.AreEqual(":authority", Fields2[3].Key);
			Assert.AreEqual("www.example.com", Fields2[3].Value);

			foreach (DynamicHeaders d in new DynamicHeaders[] { w, r })
			{
				Assert.AreEqual(1U, d.NrDynamicHeaderRecords);
				Assert.AreEqual(57, d.DynamicHeaderSize);

				DynamicRecord[] Records = d.GetDynamicRecords();
				Assert.AreEqual(1, Records.Length);

				DynamicRecord Rec = Records[0];
				Assert.AreEqual(57, Rec.Length);
				Assert.AreEqual(":authority", Rec.Header.Header);
				Assert.AreEqual("www.example.com", Rec.Value);
			}
		}

		private static void FirstRequest(HeaderWriter w, bool Huffman)
		{
			Assert.IsTrue(w.WriteHeader(":method", "GET", IndexMode.Indexed, Huffman));
			Assert.IsTrue(w.WriteHeader(":scheme", "http", IndexMode.Indexed, Huffman));
			Assert.IsTrue(w.WriteHeader(":path", "/", IndexMode.Indexed, Huffman));
			Assert.IsTrue(w.WriteHeader(":authority", "www.example.com", IndexMode.Indexed, Huffman));
		}

		/// <summary>
		/// C.3.2.  Second Request (without Huffman encoding)
		/// </summary>
		[TestMethod]
		public void Test_09_C_3_2()
		{
			HeaderWriter w = new(256, 256);

			FirstRequest(w, false);
			byte[] Output = w.ToArray();
			HeaderReader r = new(Output, 256);
			Assert.IsTrue(r.ReadFields(out _));

			w.Reset();
			SecondRequest(w, false);

			Output = w.ToArray();
			Assert.AreEqual("82 86 84 be 58 08 6e 6f 2d 63 61 63 68 65",
				Hashes.BinaryToString(Output, true));

			r.Reset(Output);
			Assert.IsTrue(r.ReadFields(out IEnumerable<HttpField> Fields));
			List<HttpField> Fields2 = [.. Fields];

			Assert.AreEqual(5, Fields2.Count);
			Assert.AreEqual(":method", Fields2[0].Key);
			Assert.AreEqual("GET", Fields2[0].Value);
			Assert.AreEqual(":scheme", Fields2[1].Key);
			Assert.AreEqual("http", Fields2[1].Value);
			Assert.AreEqual(":path", Fields2[2].Key);
			Assert.AreEqual("/", Fields2[2].Value);
			Assert.AreEqual(":authority", Fields2[3].Key);
			Assert.AreEqual("www.example.com", Fields2[3].Value);
			Assert.AreEqual("cache-control", Fields2[4].Key);
			Assert.AreEqual("no-cache", Fields2[4].Value);

			foreach (DynamicHeaders d in new DynamicHeaders[] { w, r })
			{
				Assert.AreEqual(2U, d.NrDynamicHeaderRecords);
				Assert.AreEqual(110, d.DynamicHeaderSize);

				DynamicRecord[] Records = d.GetDynamicRecords();
				Assert.AreEqual(2, Records.Length);

				DynamicRecord Rec = Records[0];
				Assert.AreEqual(53, Rec.Length);
				Assert.AreEqual("cache-control", Rec.Header.Header);
				Assert.AreEqual("no-cache", Rec.Value);

				Rec = Records[1];
				Assert.AreEqual(57, Rec.Length);
				Assert.AreEqual(":authority", Rec.Header.Header);
				Assert.AreEqual("www.example.com", Rec.Value);
			}
		}

		private static void SecondRequest(HeaderWriter w, bool Huffman)
		{
			Assert.IsTrue(w.WriteHeader(":method", "GET", IndexMode.Indexed, Huffman));
			Assert.IsTrue(w.WriteHeader(":scheme", "http", IndexMode.Indexed, Huffman));
			Assert.IsTrue(w.WriteHeader(":path", "/", IndexMode.Indexed, Huffman));
			Assert.IsTrue(w.WriteHeader(":authority", "www.example.com", IndexMode.Indexed, Huffman));
			Assert.IsTrue(w.WriteHeader("cache-control", "no-cache", IndexMode.Indexed, Huffman));
		}

		/// <summary>
		/// C.3.3.  Third Request (without Huffman encoding)
		/// </summary>
		[TestMethod]
		public void Test_10_C_3_3()
		{
			HeaderWriter w = new(256, 256);

			FirstRequest(w, false);
			byte[] Output = w.ToArray();
			HeaderReader r = new(Output, 256);
			Assert.IsTrue(r.ReadFields(out _));

			w.Reset();
			SecondRequest(w, false);

			Output = w.ToArray();
			r.Reset(Output);
			Assert.IsTrue(r.ReadFields(out _));

			w.Reset();
			ThirdRequest(w, false);
			Output = w.ToArray();

			Assert.AreEqual("82 87 85 bf 40 0a 63 75 73 74 6f 6d 2d 6b 65 79 0c 63 75 73 74 6f 6d 2d 76 61 6c 75 65",
				Hashes.BinaryToString(Output, true));

			r.Reset(Output);
			Assert.IsTrue(r.ReadFields(out IEnumerable<HttpField> Fields));
			List<HttpField> Fields2 = [.. Fields];

			Assert.AreEqual(5, Fields2.Count);
			Assert.AreEqual(":method", Fields2[0].Key);
			Assert.AreEqual("GET", Fields2[0].Value);
			Assert.AreEqual(":scheme", Fields2[1].Key);
			Assert.AreEqual("https", Fields2[1].Value);
			Assert.AreEqual(":path", Fields2[2].Key);
			Assert.AreEqual("/index.html", Fields2[2].Value);
			Assert.AreEqual(":authority", Fields2[3].Key);
			Assert.AreEqual("www.example.com", Fields2[3].Value);
			Assert.AreEqual("custom-key", Fields2[4].Key);
			Assert.AreEqual("custom-value", Fields2[4].Value);

			foreach (DynamicHeaders d in new DynamicHeaders[] { w, r })
			{
				Assert.AreEqual(3U, d.NrDynamicHeaderRecords);
				Assert.AreEqual(164, d.DynamicHeaderSize);

				DynamicRecord[] Records = d.GetDynamicRecords();
				Assert.AreEqual(3, Records.Length);

				DynamicRecord Rec = Records[0];
				Assert.AreEqual(54, Rec.Length);
				Assert.AreEqual("custom-key", Rec.Header.Header);
				Assert.AreEqual("custom-value", Rec.Value);

				Rec = Records[1];
				Assert.AreEqual(53, Rec.Length);
				Assert.AreEqual("cache-control", Rec.Header.Header);
				Assert.AreEqual("no-cache", Rec.Value);

				Rec = Records[2];
				Assert.AreEqual(57, Rec.Length);
				Assert.AreEqual(":authority", Rec.Header.Header);
				Assert.AreEqual("www.example.com", Rec.Value);
			}
		}

		private static void ThirdRequest(HeaderWriter w, bool Huffman)
		{
			Assert.IsTrue(w.WriteHeader(":method", "GET", IndexMode.Indexed, Huffman));
			Assert.IsTrue(w.WriteHeader(":scheme", "https", IndexMode.Indexed, Huffman));
			Assert.IsTrue(w.WriteHeader(":path", "/index.html", IndexMode.Indexed, Huffman));
			Assert.IsTrue(w.WriteHeader(":authority", "www.example.com", IndexMode.Indexed, Huffman));
			Assert.IsTrue(w.WriteHeader("custom-key", "custom-value", IndexMode.Indexed, Huffman));
		}

		/// <summary>
		/// C.4.1.  First Request (with Huffman encoding)
		/// </summary>
		[TestMethod]
		public void Test_11_C_4_1()
		{
			HeaderWriter w = new(256, 256);
			FirstRequest(w, true);

			byte[] Output = w.ToArray();
			Assert.AreEqual("82 86 84 41 8c f1 e3 c2 e5 f2 3a 6b a0 ab 90 f4 ff",
				Hashes.BinaryToString(Output, true));

			HeaderReader r = new(Output, 256);
			Assert.IsTrue(r.ReadFields(out IEnumerable<HttpField> Fields));
			List<HttpField> Fields2 = [.. Fields];

			Assert.AreEqual(4, Fields2.Count);
			Assert.AreEqual(":method", Fields2[0].Key);
			Assert.AreEqual("GET", Fields2[0].Value);
			Assert.AreEqual(":scheme", Fields2[1].Key);
			Assert.AreEqual("http", Fields2[1].Value);
			Assert.AreEqual(":path", Fields2[2].Key);
			Assert.AreEqual("/", Fields2[2].Value);
			Assert.AreEqual(":authority", Fields2[3].Key);
			Assert.AreEqual("www.example.com", Fields2[3].Value);

			foreach (DynamicHeaders d in new DynamicHeaders[] { w, r })
			{
				Assert.AreEqual(1U, d.NrDynamicHeaderRecords);
				Assert.AreEqual(57, d.DynamicHeaderSize);

				DynamicRecord[] Records = d.GetDynamicRecords();
				Assert.AreEqual(1, Records.Length);

				DynamicRecord Rec = Records[0];
				Assert.AreEqual(57, Rec.Length);
				Assert.AreEqual(":authority", Rec.Header.Header);
				Assert.AreEqual("www.example.com", Rec.Value);
			}
		}

		/// <summary>
		/// C.4.2.  Second Request (with Huffman encoding)
		/// </summary>
		[TestMethod]
		public void Test_12_C_4_2()
		{
			HeaderWriter w = new(256, 256);

			FirstRequest(w, true);
			byte[] Output = w.ToArray();
			HeaderReader r = new(Output, 256);
			Assert.IsTrue(r.ReadFields(out _));

			w.Reset();
			SecondRequest(w, true);

			Output = w.ToArray();
			Assert.AreEqual("82 86 84 be 58 86 a8 eb 10 64 9c bf",
				Hashes.BinaryToString(Output, true));

			r.Reset(Output);
			Assert.IsTrue(r.ReadFields(out IEnumerable<HttpField> Fields));
			List<HttpField> Fields2 = [.. Fields];

			Assert.AreEqual(5, Fields2.Count);
			Assert.AreEqual(":method", Fields2[0].Key);
			Assert.AreEqual("GET", Fields2[0].Value);
			Assert.AreEqual(":scheme", Fields2[1].Key);
			Assert.AreEqual("http", Fields2[1].Value);
			Assert.AreEqual(":path", Fields2[2].Key);
			Assert.AreEqual("/", Fields2[2].Value);
			Assert.AreEqual(":authority", Fields2[3].Key);
			Assert.AreEqual("www.example.com", Fields2[3].Value);
			Assert.AreEqual("cache-control", Fields2[4].Key);
			Assert.AreEqual("no-cache", Fields2[4].Value);

			foreach (DynamicHeaders d in new DynamicHeaders[] { w, r })
			{
				Assert.AreEqual(2U, d.NrDynamicHeaderRecords);
				Assert.AreEqual(110, d.DynamicHeaderSize);

				DynamicRecord[] Records = d.GetDynamicRecords();
				Assert.AreEqual(2, Records.Length);

				DynamicRecord Rec = Records[0];
				Assert.AreEqual(53, Rec.Length);
				Assert.AreEqual("cache-control", Rec.Header.Header);
				Assert.AreEqual("no-cache", Rec.Value);

				Rec = Records[1];
				Assert.AreEqual(57, Rec.Length);
				Assert.AreEqual(":authority", Rec.Header.Header);
				Assert.AreEqual("www.example.com", Rec.Value);
			}
		}

		/// <summary>
		/// C.4.3.  Third Request (with Huffman encoding)
		/// </summary>
		[TestMethod]
		public void Test_13_C_4_3()
		{
			HeaderWriter w = new(256, 256);

			FirstRequest(w, true);
			byte[] Output = w.ToArray();
			HeaderReader r = new(Output, 256);
			Assert.IsTrue(r.ReadFields(out _));

			w.Reset();
			SecondRequest(w, true);

			Output = w.ToArray();
			r.Reset(Output);
			Assert.IsTrue(r.ReadFields(out _));

			w.Reset();
			ThirdRequest(w, true);
			Output = w.ToArray();

			Assert.AreEqual("82 87 85 bf 40 88 25 a8 49 e9 5b a9 7d 7f 89 25 a8 49 e9 5b b8 e8 b4 bf",
				Hashes.BinaryToString(Output, true));

			r.Reset(Output);
			Assert.IsTrue(r.ReadFields(out IEnumerable<HttpField> Fields));
			List<HttpField> Fields2 = [.. Fields];

			Assert.AreEqual(5, Fields2.Count);
			Assert.AreEqual(":method", Fields2[0].Key);
			Assert.AreEqual("GET", Fields2[0].Value);
			Assert.AreEqual(":scheme", Fields2[1].Key);
			Assert.AreEqual("https", Fields2[1].Value);
			Assert.AreEqual(":path", Fields2[2].Key);
			Assert.AreEqual("/index.html", Fields2[2].Value);
			Assert.AreEqual(":authority", Fields2[3].Key);
			Assert.AreEqual("www.example.com", Fields2[3].Value);
			Assert.AreEqual("custom-key", Fields2[4].Key);
			Assert.AreEqual("custom-value", Fields2[4].Value);

			foreach (DynamicHeaders d in new DynamicHeaders[] { w, r })
			{
				Assert.AreEqual(3U, d.NrDynamicHeaderRecords);
				Assert.AreEqual(164, d.DynamicHeaderSize);

				DynamicRecord[] Records = d.GetDynamicRecords();
				Assert.AreEqual(3, Records.Length);

				DynamicRecord Rec = Records[0];
				Assert.AreEqual(54, Rec.Length);
				Assert.AreEqual("custom-key", Rec.Header.Header);
				Assert.AreEqual("custom-value", Rec.Value);

				Rec = Records[1];
				Assert.AreEqual(53, Rec.Length);
				Assert.AreEqual("cache-control", Rec.Header.Header);
				Assert.AreEqual("no-cache", Rec.Value);

				Rec = Records[2];
				Assert.AreEqual(57, Rec.Length);
				Assert.AreEqual(":authority", Rec.Header.Header);
				Assert.AreEqual("www.example.com", Rec.Value);
			}
		}

		/// <summary>
		/// C.5.1.  First Response (without Huffman encoding)
		/// </summary>
		[TestMethod]
		public void Test_14_C_5_1()
		{
			HeaderWriter w = new(256, 256);
			FirstResponse(w, false);

			byte[] Output = w.ToArray();
			Assert.AreEqual("48 03 33 30 32 58 07 70 72 69 76 61 74 65 61 1d 4d 6f 6e 2c 20 32 31 20 4f 63 74 20 32 30 31 33 20 32 30 3a 31 33 3a 32 31 20 47 4d 54 6e 17 68 74 74 70 73 3a 2f 2f 77 77 77 2e 65 78 61 6d 70 6c 65 2e 63 6f 6d",
				Hashes.BinaryToString(Output, true));

			HeaderReader r = new(Output, 256);
			Assert.IsTrue(r.ReadFields(out IEnumerable<HttpField> Fields));
			List<HttpField> Fields2 = [.. Fields];

			Assert.AreEqual(4, Fields2.Count);
			Assert.AreEqual(":status", Fields2[0].Key);
			Assert.AreEqual("302", Fields2[0].Value);
			Assert.AreEqual("cache-control", Fields2[1].Key);
			Assert.AreEqual("private", Fields2[1].Value);
			Assert.AreEqual("date", Fields2[2].Key);
			Assert.AreEqual("Mon, 21 Oct 2013 20:13:21 GMT", Fields2[2].Value);
			Assert.AreEqual("location", Fields2[3].Key);
			Assert.AreEqual("https://www.example.com", Fields2[3].Value);

			foreach (DynamicHeaders d in new DynamicHeaders[] { w, r })
			{
				Assert.AreEqual(4U, d.NrDynamicHeaderRecords);
				Assert.AreEqual(222, d.DynamicHeaderSize);

				DynamicRecord[] Records = d.GetDynamicRecords();
				Assert.AreEqual(4, Records.Length);

				DynamicRecord Rec = Records[0];
				Assert.AreEqual(63, Rec.Length);
				Assert.AreEqual("location", Rec.Header.Header);
				Assert.AreEqual("https://www.example.com", Rec.Value);

				Rec = Records[1];
				Assert.AreEqual(65, Rec.Length);
				Assert.AreEqual("date", Rec.Header.Header);
				Assert.AreEqual("Mon, 21 Oct 2013 20:13:21 GMT", Rec.Value);

				Rec = Records[2];
				Assert.AreEqual(52, Rec.Length);
				Assert.AreEqual("cache-control", Rec.Header.Header);
				Assert.AreEqual("private", Rec.Value);

				Rec = Records[3];
				Assert.AreEqual(42, Rec.Length);
				Assert.AreEqual(":status", Rec.Header.Header);
				Assert.AreEqual("302", Rec.Value);
			}
		}

		private static void FirstResponse(HeaderWriter w, bool Huffman)
		{
			Assert.IsTrue(w.WriteHeader(":status", "302", IndexMode.Indexed, Huffman));
			Assert.IsTrue(w.WriteHeader("cache-control", "private", IndexMode.Indexed, Huffman));
			Assert.IsTrue(w.WriteHeader("date", "Mon, 21 Oct 2013 20:13:21 GMT", IndexMode.Indexed, Huffman));
			Assert.IsTrue(w.WriteHeader("location", "https://www.example.com", IndexMode.Indexed, Huffman));
		}

		/// <summary>
		/// C.5.2.  Second Response (without Huffman encoding)
		/// </summary>
		[TestMethod]
		public void Test_15_C_5_2()
		{
			HeaderWriter w = new(256, 256);

			FirstResponse(w, false);
			byte[] Output = w.ToArray();
			HeaderReader r = new(Output, 256);
			Assert.IsTrue(r.ReadFields(out _));

			w.Reset();
			SecondResponse(w, false);

			Output = w.ToArray();
			Assert.AreEqual("48 03 33 30 37 c1 c0 bf",
				Hashes.BinaryToString(Output, true));

			r.Reset(Output);
			Assert.IsTrue(r.ReadFields(out IEnumerable<HttpField> Fields));
			List<HttpField> Fields2 = [.. Fields];

			Assert.AreEqual(4, Fields2.Count);
			Assert.AreEqual(":status", Fields2[0].Key);
			Assert.AreEqual("307", Fields2[0].Value);
			Assert.AreEqual("cache-control", Fields2[1].Key);
			Assert.AreEqual("private", Fields2[1].Value);
			Assert.AreEqual("date", Fields2[2].Key);
			Assert.AreEqual("Mon, 21 Oct 2013 20:13:21 GMT", Fields2[2].Value);
			Assert.AreEqual("location", Fields2[3].Key);
			Assert.AreEqual("https://www.example.com", Fields2[3].Value);

			foreach (DynamicHeaders d in new DynamicHeaders[] { w, r })
			{
				Assert.AreEqual(4U, d.NrDynamicHeaderRecords);
				Assert.AreEqual(222, d.DynamicHeaderSize);

				DynamicRecord[] Records = d.GetDynamicRecords();
				Assert.AreEqual(4, Records.Length);

				DynamicRecord Rec = Records[0];
				Assert.AreEqual(42, Rec.Length);
				Assert.AreEqual(":status", Rec.Header.Header);
				Assert.AreEqual("307", Rec.Value);

				Rec = Records[1];
				Assert.AreEqual(63, Rec.Length);
				Assert.AreEqual("location", Rec.Header.Header);
				Assert.AreEqual("https://www.example.com", Rec.Value);

				Rec = Records[2];
				Assert.AreEqual(65, Rec.Length);
				Assert.AreEqual("date", Rec.Header.Header);
				Assert.AreEqual("Mon, 21 Oct 2013 20:13:21 GMT", Rec.Value);

				Rec = Records[3];
				Assert.AreEqual(52, Rec.Length);
				Assert.AreEqual("cache-control", Rec.Header.Header);
				Assert.AreEqual("private", Rec.Value);
			}
		}

		private static void SecondResponse(HeaderWriter w, bool Huffman)
		{
			Assert.IsTrue(w.WriteHeader(":status", "307", IndexMode.Indexed, Huffman));
			Assert.IsTrue(w.WriteHeader("cache-control", "private", IndexMode.Indexed, Huffman));
			Assert.IsTrue(w.WriteHeader("date", "Mon, 21 Oct 2013 20:13:21 GMT", IndexMode.Indexed, Huffman));
			Assert.IsTrue(w.WriteHeader("location", "https://www.example.com", IndexMode.Indexed, Huffman));
		}

		/// <summary>
		/// C.5.3.  Third Response (without Huffman encoding)
		/// </summary>
		[TestMethod]
		public void Test_16_C_5_3()
		{
			HeaderWriter w = new(256, 256);

			FirstResponse(w, false);
			byte[] Output = w.ToArray();
			HeaderReader r = new(Output, 256);
			Assert.IsTrue(r.ReadFields(out _));

			w.Reset();
			SecondResponse(w, false);
			Output = w.ToArray();
			r.Reset(Output);
			Assert.IsTrue(r.ReadFields(out _));

			w.Reset();
			ThirdResponse(w, false);

			Output = w.ToArray();
			Assert.AreEqual("88 c1 61 1d 4d 6f 6e 2c 20 32 31 20 4f 63 74 20 32 30 31 33 20 32 30 3a 31 33 3a 32 32 20 47 4d 54 c0 5a 04 67 7a 69 70 77 38 66 6f 6f 3d 41 53 44 4a 4b 48 51 4b 42 5a 58 4f 51 57 45 4f 50 49 55 41 58 51 57 45 4f 49 55 3b 20 6d 61 78 2d 61 67 65 3d 33 36 30 30 3b 20 76 65 72 73 69 6f 6e 3d 31",
				Hashes.BinaryToString(Output, true));

			r.Reset(Output);
			Assert.IsTrue(r.ReadFields(out IEnumerable<HttpField> Fields));
			List<HttpField> Fields2 = [.. Fields];

			Assert.AreEqual(6, Fields2.Count);
			Assert.AreEqual(":status", Fields2[0].Key);
			Assert.AreEqual("200", Fields2[0].Value);
			Assert.AreEqual("cache-control", Fields2[1].Key);
			Assert.AreEqual("private", Fields2[1].Value);
			Assert.AreEqual("date", Fields2[2].Key);
			Assert.AreEqual("Mon, 21 Oct 2013 20:13:22 GMT", Fields2[2].Value);
			Assert.AreEqual("location", Fields2[3].Key);
			Assert.AreEqual("https://www.example.com", Fields2[3].Value);
			Assert.AreEqual("content-encoding", Fields2[4].Key);
			Assert.AreEqual("gzip", Fields2[4].Value);
			Assert.AreEqual("set-cookie", Fields2[5].Key);
			Assert.AreEqual("foo=ASDJKHQKBZXOQWEOPIUAXQWEOIU; max-age=3600; version=1", Fields2[5].Value);

			foreach (DynamicHeaders d in new DynamicHeaders[] { w, r })
			{
				Assert.AreEqual(3U, d.NrDynamicHeaderRecords);
				Assert.AreEqual(215, d.DynamicHeaderSize);

				DynamicRecord[] Records = d.GetDynamicRecords();
				Assert.AreEqual(3, Records.Length);

				DynamicRecord Rec = Records[0];
				Assert.AreEqual(98, Rec.Length);
				Assert.AreEqual("set-cookie", Rec.Header.Header);
				Assert.AreEqual("foo=ASDJKHQKBZXOQWEOPIUAXQWEOIU; max-age=3600; version=1", Rec.Value);

				Rec = Records[1];
				Assert.AreEqual(52, Rec.Length);
				Assert.AreEqual("content-encoding", Rec.Header.Header);
				Assert.AreEqual("gzip", Rec.Value);

				Rec = Records[2];
				Assert.AreEqual(65, Rec.Length);
				Assert.AreEqual("date", Rec.Header.Header);
				Assert.AreEqual("Mon, 21 Oct 2013 20:13:22 GMT", Rec.Value);
			}
		}

		private static void ThirdResponse(HeaderWriter w, bool Huffman)
		{
			Assert.IsTrue(w.WriteHeader(":status", "200", IndexMode.Indexed, Huffman));
			Assert.IsTrue(w.WriteHeader("cache-control", "private", IndexMode.Indexed, Huffman));
			Assert.IsTrue(w.WriteHeader("date", "Mon, 21 Oct 2013 20:13:22 GMT", IndexMode.Indexed, Huffman));
			Assert.IsTrue(w.WriteHeader("location", "https://www.example.com", IndexMode.Indexed, Huffman));
			Assert.IsTrue(w.WriteHeader("content-encoding", "gzip", IndexMode.Indexed, Huffman));
			Assert.IsTrue(w.WriteHeader("set-cookie", "foo=ASDJKHQKBZXOQWEOPIUAXQWEOIU; max-age=3600; version=1", IndexMode.Indexed, Huffman));
		}

		/// <summary>
		/// C.6.1.  First Response (with Huffman encoding)
		/// </summary>
		[TestMethod]
		public void Test_17_C_6_1()
		{
			HeaderWriter w = new(256, 256);
			FirstResponse(w, true);

			byte[] Output = w.ToArray();
			Assert.AreEqual("48 82 64 02 58 85 ae c3 77 1a 4b 61 96 d0 7a be 94 10 54 d4 44 a8 20 05 95 04 0b 81 66 e0 82 a6 2d 1b ff 6e 91 9d 29 ad 17 18 63 c7 8f 0b 97 c8 e9 ae 82 ae 43 d3",
				Hashes.BinaryToString(Output, true));

			HeaderReader r = new(Output, 256);
			Assert.IsTrue(r.ReadFields(out IEnumerable<HttpField> Fields));
			List<HttpField> Fields2 = [.. Fields];

			Assert.AreEqual(4, Fields2.Count);
			Assert.AreEqual(":status", Fields2[0].Key);
			Assert.AreEqual("302", Fields2[0].Value);
			Assert.AreEqual("cache-control", Fields2[1].Key);
			Assert.AreEqual("private", Fields2[1].Value);
			Assert.AreEqual("date", Fields2[2].Key);
			Assert.AreEqual("Mon, 21 Oct 2013 20:13:21 GMT", Fields2[2].Value);
			Assert.AreEqual("location", Fields2[3].Key);
			Assert.AreEqual("https://www.example.com", Fields2[3].Value);

			foreach (DynamicHeaders d in new DynamicHeaders[] { w, r })
			{
				Assert.AreEqual(4U, d.NrDynamicHeaderRecords);
				Assert.AreEqual(222, d.DynamicHeaderSize);

				DynamicRecord[] Records = d.GetDynamicRecords();
				Assert.AreEqual(4, Records.Length);

				DynamicRecord Rec = Records[0];
				Assert.AreEqual(63, Rec.Length);
				Assert.AreEqual("location", Rec.Header.Header);
				Assert.AreEqual("https://www.example.com", Rec.Value);

				Rec = Records[1];
				Assert.AreEqual(65, Rec.Length);
				Assert.AreEqual("date", Rec.Header.Header);
				Assert.AreEqual("Mon, 21 Oct 2013 20:13:21 GMT", Rec.Value);

				Rec = Records[2];
				Assert.AreEqual(52, Rec.Length);
				Assert.AreEqual("cache-control", Rec.Header.Header);
				Assert.AreEqual("private", Rec.Value);

				Rec = Records[3];
				Assert.AreEqual(42, Rec.Length);
				Assert.AreEqual(":status", Rec.Header.Header);
				Assert.AreEqual("302", Rec.Value);
			}
		}

		/// <summary>
		/// C.6.2.  Second Response (with Huffman encoding)
		/// </summary>
		[TestMethod]
		public void Test_18_C_6_2()
		{
			HeaderWriter w = new(256, 256);

			FirstResponse(w, true);
			byte[] Output = w.ToArray();
			HeaderReader r = new(Output, 256);
			Assert.IsTrue(r.ReadFields(out _));

			w.Reset();
			SecondResponse(w, true);

			Output = w.ToArray();
			Assert.AreEqual("48 83 64 0e ff c1 c0 bf",
				Hashes.BinaryToString(Output, true));

			r.Reset(Output);
			Assert.IsTrue(r.ReadFields(out IEnumerable<HttpField> Fields));
			List<HttpField> Fields2 = [.. Fields];

			Assert.AreEqual(4, Fields2.Count);
			Assert.AreEqual(":status", Fields2[0].Key);
			Assert.AreEqual("307", Fields2[0].Value);
			Assert.AreEqual("cache-control", Fields2[1].Key);
			Assert.AreEqual("private", Fields2[1].Value);
			Assert.AreEqual("date", Fields2[2].Key);
			Assert.AreEqual("Mon, 21 Oct 2013 20:13:21 GMT", Fields2[2].Value);
			Assert.AreEqual("location", Fields2[3].Key);
			Assert.AreEqual("https://www.example.com", Fields2[3].Value);

			foreach (DynamicHeaders d in new DynamicHeaders[] { w, r })
			{
				Assert.AreEqual(4U, d.NrDynamicHeaderRecords);
				Assert.AreEqual(222, d.DynamicHeaderSize);

				DynamicRecord[] Records = d.GetDynamicRecords();
				Assert.AreEqual(4, Records.Length);

				DynamicRecord Rec = Records[0];
				Assert.AreEqual(42, Rec.Length);
				Assert.AreEqual(":status", Rec.Header.Header);
				Assert.AreEqual("307", Rec.Value);

				Rec = Records[1];
				Assert.AreEqual(63, Rec.Length);
				Assert.AreEqual("location", Rec.Header.Header);
				Assert.AreEqual("https://www.example.com", Rec.Value);

				Rec = Records[2];
				Assert.AreEqual(65, Rec.Length);
				Assert.AreEqual("date", Rec.Header.Header);
				Assert.AreEqual("Mon, 21 Oct 2013 20:13:21 GMT", Rec.Value);

				Rec = Records[3];
				Assert.AreEqual(52, Rec.Length);
				Assert.AreEqual("cache-control", Rec.Header.Header);
				Assert.AreEqual("private", Rec.Value);
			}
		}

		/// <summary>
		/// C.6.3.  Third Response (with Huffman encoding)
		/// </summary>
		[TestMethod]
		public void Test_19_C_6_3()
		{
			HeaderWriter w = new(256, 256);

			FirstResponse(w, true);
			byte[] Output = w.ToArray();
			HeaderReader r = new(Output, 256);
			Assert.IsTrue(r.ReadFields(out _));

			w.Reset();
			SecondResponse(w, true);
			Output = w.ToArray();
			r.Reset(Output);
			Assert.IsTrue(r.ReadFields(out _));

			w.Reset();
			ThirdResponse(w, true);

			Output = w.ToArray();
			Assert.AreEqual("88 c1 61 96 d0 7a be 94 10 54 d4 44 a8 20 05 95 04 0b 81 66 e0 84 a6 2d 1b ff c0 5a 83 9b d9 ab 77 ad 94 e7 82 1d d7 f2 e6 c7 b3 35 df df cd 5b 39 60 d5 af 27 08 7f 36 72 c1 ab 27 0f b5 29 1f 95 87 31 60 65 c0 03 ed 4e e5 b1 06 3d 50 07",
				Hashes.BinaryToString(Output, true));

			r.Reset(Output);
			Assert.IsTrue(r.ReadFields(out IEnumerable<HttpField> Fields));
			List<HttpField> Fields2 = [.. Fields];

			Assert.AreEqual(6, Fields2.Count);
			Assert.AreEqual(":status", Fields2[0].Key);
			Assert.AreEqual("200", Fields2[0].Value);
			Assert.AreEqual("cache-control", Fields2[1].Key);
			Assert.AreEqual("private", Fields2[1].Value);
			Assert.AreEqual("date", Fields2[2].Key);
			Assert.AreEqual("Mon, 21 Oct 2013 20:13:22 GMT", Fields2[2].Value);
			Assert.AreEqual("location", Fields2[3].Key);
			Assert.AreEqual("https://www.example.com", Fields2[3].Value);
			Assert.AreEqual("content-encoding", Fields2[4].Key);
			Assert.AreEqual("gzip", Fields2[4].Value);
			Assert.AreEqual("set-cookie", Fields2[5].Key);
			Assert.AreEqual("foo=ASDJKHQKBZXOQWEOPIUAXQWEOIU; max-age=3600; version=1", Fields2[5].Value);

			foreach (DynamicHeaders d in new DynamicHeaders[] { w, r })
			{
				Assert.AreEqual(3U, d.NrDynamicHeaderRecords);
				Assert.AreEqual(215, d.DynamicHeaderSize);

				DynamicRecord[] Records = d.GetDynamicRecords();
				Assert.AreEqual(3, Records.Length);

				DynamicRecord Rec = Records[0];
				Assert.AreEqual(98, Rec.Length);
				Assert.AreEqual("set-cookie", Rec.Header.Header);
				Assert.AreEqual("foo=ASDJKHQKBZXOQWEOPIUAXQWEOIU; max-age=3600; version=1", Rec.Value);

				Rec = Records[1];
				Assert.AreEqual(52, Rec.Length);
				Assert.AreEqual("content-encoding", Rec.Header.Header);
				Assert.AreEqual("gzip", Rec.Value);

				Rec = Records[2];
				Assert.AreEqual(65, Rec.Length);
				Assert.AreEqual("date", Rec.Header.Header);
				Assert.AreEqual("Mon, 21 Oct 2013 20:13:22 GMT", Rec.Value);
			}
		}

	}
}
