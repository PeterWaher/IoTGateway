using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Events;
using Waher.Events.Console;
using Waher.Persistence;
using Waher.Persistence.Files;
using Waher.Persistence.Serialization;
using Waher.Runtime.Inventory;
using Waher.Script;

namespace Waher.Runtime.Language.Test
{
	[TestClass]
	public class LanguageTests
	{
		private static ConsoleEventSink consoleEventSink = null;
		private static FilesProvider filesProvider = null;

		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext _)
		{
			Types.Initialize(
				typeof(FilesProvider).Assembly, 
				typeof(ObjectSerializer).Assembly, 
				typeof(LanguageTests).Assembly, 
				typeof(Expression).Assembly);

			Log.Register(consoleEventSink = new ConsoleEventSink());

			filesProvider = new FilesProvider("Data", "Default", 8192, 10000, 8192, Encoding.UTF8, 10000, true);
			Database.Register(filesProvider);
		}

		[AssemblyCleanup]
		public static void AssemblyCleanup()
		{
			if (filesProvider != null)
			{
				filesProvider.Dispose();
				filesProvider = null;
			}

			if (consoleEventSink != null)
			{
				Log.Unregister(consoleEventSink);
				consoleEventSink = null;
			}
		}

		[TestMethod]
		public async Task Language_Test_01_GetLanguage()
		{
			Language Language = await Translator.GetLanguageAsync("en");
			if (Language is null)
				Language = await Translator.CreateLanguageAsync("en", "English", null, 0, 0);
			Assert.IsNotNull(Language);
		}

		[TestMethod]
		public async Task Language_Test_02_GetDefaultLanguage()
		{
			Language Language = await Translator.GetDefaultLanguageAsync();
			Assert.IsNotNull(Language);
		}

		[TestMethod]
		public async Task Language_Test_03_GetLanguages()
		{
			Language[] Languages = await Translator.GetLanguagesAsync();
			Assert.IsNotNull(Languages);
			Assert.IsTrue(Languages.Length > 0);
		}

		[TestMethod]
		public async Task Language_Test_04_GetNamespace()
		{
			Language Language = await Translator.GetLanguageAsync("en");
			Namespace Namespace = await Language.GetNamespaceAsync("Test");
			if (Namespace is null)
				await Language.CreateNamespaceAsync("Test");
		}

		[TestMethod]
		public async Task Language_Test_05_GetNamespaces()
		{
			Language Language = await Translator.GetLanguageAsync("en");
			Namespace[] Namespaces = await Language.GetNamespacesAsync();
			Assert.IsNotNull(Namespaces);
			Assert.IsTrue(Namespaces.Length > 0);
		}

		[TestMethod]
		public async Task Language_Test_06_GetString()
		{
			Language Language = await Translator.GetLanguageAsync("en");
			Namespace Namespace = await Language.GetNamespaceAsync("Test");
			string s = await Namespace.GetStringAsync(1, "Hello world.");
			Assert.AreEqual("Hello world.", s);
		}

		[TestMethod]
		public async Task Language_Test_07_GetStrings()
		{
			Language Language = await Translator.GetLanguageAsync("en");
			Namespace Namespace = await Language.GetNamespaceAsync("Test");
			LanguageString[] Strings = await Namespace.GetStringsAsync();
			Assert.IsNotNull(Strings);
			Assert.IsTrue(Strings.Length > 0);
		}

		[TestMethod]
		public async Task Language_Test_08_GetString2()
		{
			Language Language = await Translator.GetLanguageAsync("en");
			string s = await Language.GetStringAsync(typeof(LanguageTests), 1, "Hello world.");
			Assert.AreEqual("Hello world.", s);
		}

	}
}
