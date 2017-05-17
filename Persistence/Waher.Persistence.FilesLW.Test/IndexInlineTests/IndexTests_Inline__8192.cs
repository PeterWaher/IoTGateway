using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content;
using Waher.Persistence.Files.Test.Classes;
using Waher.Persistence.Files.Serialization;
using Waher.Persistence.Files.Statistics;
using Waher.Script;

namespace Waher.Persistence.Files.Test.IndexInlineTests
{
	[TestClass]
	public class IndexTests_Inline__8192 : IndexTests
	{
		public override int BlockSize
		{
			get
			{
				return 8192;
			}
		}
	}
}
