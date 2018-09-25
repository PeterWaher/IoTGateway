using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Persistence.Files.Test.Classes;

namespace Waher.Persistence.Files.Test
{
	[TestClass]
	public class PersistenceTests
	{
		[ClassInitialize]
		public static void ClassInitialize(TestContext Context)
		{
			try
			{
				IDatabaseProvider p = Database.Provider;
			}
			catch
			{
				Database.Register(new FilesProvider("Data", "Default", 8192, 8192, 8192,
					Encoding.UTF8, 10000, true));
			}
		}

		[TestMethod]
		public async Task PersistenceTests_01_SaveNew()
		{
			int i;

			for (i = 1; i <= 5; i++)
			{
				await Database.Insert(new StringFields()
				{
					A = string.Empty,
					B = i.ToString(),
					C = null
				});
			}
		}

		[TestMethod]
		public async Task PersistenceTests_02_FindAll()
		{
			foreach (StringFields Obj in await Database.Find<StringFields>())
				Console.Out.WriteLine(Obj.ToString());
		}


	}
}
