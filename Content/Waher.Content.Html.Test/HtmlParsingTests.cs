using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content.Xml;
using Waher.Runtime.Inventory;
using Waher.Content.Html.Elements;

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

				Assert.IsNotNull(Doc.Root);
				Assert.IsNotNull(Doc.Html);
				Assert.IsNotNull(Doc.Head);
				Assert.IsNotNull(Doc.Body);
				Assert.IsNotNull(Doc.Title);

				List<HtmlNode> Todo = new List<HtmlNode>()
				{
					Doc.Root
				};

				string s;
				HtmlNode N;
				int i = 0;
				int Last = -1;

				while (i < Todo.Count)
				{
					N = Todo[i++];

					if (Last >= 0)
						s = "\r\n\r\n" + Doc.HtmlText.Substring(Last + 1);
					else
						s = string.Empty;

					Assert.IsTrue(N.StartPosition > Last, "Start position not set properly. Start=" + N.StartPosition.ToString() + ", Last=" + Last.ToString() + s);
					Assert.IsTrue(N.EndPosition >= N.StartPosition, "End position not set.\r\n\r\n" + Doc.HtmlText.Substring(N.StartPosition));
					Assert.IsTrue(!string.IsNullOrEmpty(N.OuterHtml), "OuterHTML not set properly.\r\n\r\n" + Doc.HtmlText.Substring(N.StartPosition));

					if (N is HtmlElement E)
					{
						Last = E.EndPositionOfStartTag;

						if (E.HasChildren)
							Todo.InsertRange(i, E.Children);

						Assert.IsTrue(E.InnerHtml != null, "InnerHTML not set properly.\r\n\r\n" + Doc.HtmlText.Substring(N.StartPosition));
					}
					else
						Last = N.EndPosition;
				}

				PageMetaData MetaData = Doc.GetMetaData();

				if (Doc.Meta != null)
				{
					foreach (Meta Meta in Doc.Meta)
						Console.Out.WriteLine(Meta.OuterHtml);
				}

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

		[TestMethod]
		public async Task HtmlParseTest_03_TheGuardian()
		{
			await this.LoadAndParse("https://www.theguardian.com/technology/2018/mar/04/has-dopamine-got-us-hooked-on-tech-facebook-apps-addiction");
		}
	}
}
