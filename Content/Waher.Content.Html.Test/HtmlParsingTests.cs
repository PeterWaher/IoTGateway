using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content.Xml;
using Waher.Runtime.Inventory;

namespace Waher.Content.Html.Test
{
	[TestClass]
	public class HtmlParsingTests
	{
		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext Context)
		{
			Types.Initialize(typeof(InternetContent).Assembly, 
				typeof(HtmlDocument).Assembly);
		}

		private async Task LoadAndParse(string Url)
		{
			using (HttpClient Client = new HttpClient())
			{
				Client.Timeout = TimeSpan.FromMilliseconds(30000);
				Client.DefaultRequestHeaders.ExpectContinue = false;

				HttpResponseMessage Response = await Client.GetAsync(Url);
				Response.EnsureSuccessStatusCode();

				byte[] Data = await Response.Content.ReadAsByteArrayAsync();
				string ContentType = Response.Content.Headers.ContentType.ToString();

				HtmlDocument Doc = InternetContent.Decode(ContentType, Data, new Uri(Url)) as HtmlDocument;
				Assert.IsNotNull(Doc);

				HtmlElement Root = Doc.Root;    // Makes sure document is parsed.
				Assert.IsNotNull(Root);

				XmlWriterSettings Settings = XML.WriterSettings(true, true);
				using (XmlWriter Output = XmlWriter.Create(Console.Out, Settings))
				{
					Doc.Export(Output);
					Output.Flush();
				}
			}
		}

		[TestMethod]
		public async Task HtmlParseTest_01_Google()
		{
			await this.LoadAndParse("http://google.com/");
		}

		[TestMethod]
		public async Task HtmlParseTest_02_Trocadero()
		{
			await this.LoadAndParse("http://www.kristianstadsbladet.se/tt-ekonomi/folkstorm-nar-trocadero-forsvinner/");
		}
	}
}
