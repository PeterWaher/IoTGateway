using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content.Xml;

namespace Waher.Content.Test
{
	[TestClass]
    public class XmlTests
    {
		private static string Load(string FileName)
		{
			return File.ReadAllText("Data\\" + FileName);
		}

		[TestMethod]
		public void Test_01_OfflineMessage()
		{
			string Xml = Load("Offline.xml");
			Assert.IsTrue(XML.IsValidXml(Xml, true, true, true, true, false, false));
		}

	}
}
