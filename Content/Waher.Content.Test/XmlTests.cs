using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content.Xml;

namespace Waher.Content.Test
{
	[TestClass]
    public class XmlTests
    {
		private static Task<string> Load(string FileName)
		{
			return Resources.ReadAllTextAsync("Data\\" + FileName);
		}

		[TestMethod]
		public async Task Test_01_OfflineMessage()
		{
			string Xml = await Load("Offline.xml");
			Assert.IsTrue(XML.IsValidXml(Xml, true, true, true, true, false, false));
		}

	}
}
