using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

	}
}
