using System;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content.Binary;
using Waher.Content.Xml;
using Waher.Runtime.IO;

namespace Waher.Content.Test
{
	[TestClass]
	public class XmlTests
	{
		private static Task<string> Load(string FileName)
		{
			return Files.ReadAllTextAsync("Data\\" + FileName);
		}

		[TestMethod]
		public async Task Test_01_OfflineMessage()
		{
			string Xml = await Load("Offline.xml");
			Assert.IsTrue(XML.IsValidXml(Xml, true, true, true, true, false, false));
		}

		[TestMethod]
		public void Test_02_DateTimeOffset()
		{
			Assert.IsTrue(XML.TryParse("2023-03-26T16:21:06.462-03:00", out DateTimeOffset DTO));
			Assert.AreEqual(2023, DTO.Year);
			Assert.AreEqual(3, DTO.Month);
			Assert.AreEqual(26, DTO.Day);
			Assert.AreEqual(16, DTO.Hour);
			Assert.AreEqual(21, DTO.Minute);
			Assert.AreEqual(6, DTO.Second);
			Assert.AreEqual(462, DTO.Millisecond);
			Assert.AreEqual(-3, DTO.Offset.Hours);
			Assert.AreEqual(0, DTO.Offset.Minutes);
		}

		[TestMethod]
		[DataRow("This is a &lt;Test&gt;.", "This is a <Test>.")]
		[DataRow("This is a &apos;Test&apos;.", "This is a 'Test'.")]
		[DataRow("This is a &quot;Test&quot;.", "This is a \"Test\".")]
		[DataRow("This is a &amp;Test&amp;.", "This is a &Test&.")]
		[DataRow("It&apos;s a &quot;Test&quot; of &lt;multiple&gt; entities &amp; such.", "It's a \"Test\" of <multiple> entities & such.")]
		public void Test_03_DecodeString(string Encoded, string Decoded)
		{
			string s = XML.DecodeString(Encoded);
			Assert.AreEqual(Decoded, s);
		}

		[TestMethod]
		[DataRow("IllegalCharacters.xml")]
		[DataRow("IllegalCharacters2.xml")]
		public async Task Test_04_RepairXml(string FileName)
		{
			string Xml = await Load(FileName);
			Assert.Throws<XmlException>(() => XML.ParseXml(Xml));

			string RepairedXml = XML.RepairXml(Xml);
			XmlDocument Doc = XML.ParseXml(RepairedXml);
			Console.Out.WriteLine(Doc.OuterXml);
		}
	}
}
