using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Xml;
using System.Xml.Xsl;
using Waher.Content;
using Waher.Events;
using Waher.Events.Console;
using Waher.Networking;

namespace Waher.Utility.GetEmojiCatalog
{
	class Program
	{
		static void Main(string[] args)
		{
			string Html;

			Log.Register(new ConsoleEventSink());

			try
			{
				if (!File.Exists("table.htm") || (DateTime.Now - File.GetLastWriteTime("table.htm")).TotalHours >= 1.0)
				{
					Log.Informational("Downloading table.");

					WebClient Client = new WebClient();
					Client.DownloadFile("http://unicodey.com/emoji-data/table.htm", "table.htm");

					Log.Informational("Loading table");
					Html = File.ReadAllText("table.htm");

					Log.Informational("Fixing encoding errors.");
					Html = Html.
						Replace("<td><3</td>", "<td>&lt;3</td>").
						Replace("<td></3</td>", "<td>&lt;/3</td>").
						Replace("</body>\n<html>", "</body>\n</html>");

					File.WriteAllText("table.htm", Html);
				}
				else
				{
					Log.Informational("Loading table");
					Html = File.ReadAllText("table.htm");
				}

				Log.Informational("Transforming to C#.");

				XslCompiledTransform Transform = XML.LoadTransform("Waher.Utility.GetEmojiCatalog.Transforms.HtmlToCSharp.xslt", typeof(Program).Assembly);
				string CSharp = XML.Transform(Html, Transform);

				Log.Informational("Saving C#.");
				File.WriteAllText("EmojiUtilities.cs", CSharp);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}
	}
}
