using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Content.Xsl;
using Waher.Events;
using Waher.Networking.HTTP;
using Waher.Persistence;

namespace Waher.IoTGateway.WebResources
{
	/// <summary>
	/// Starts analyzing the database
	/// </summary>
	public class StartAnalyze : HttpSynchronousResource, IHttpPostMethod
	{
		/// <summary>
		/// Starts analyzing the database
		/// </summary>
		public StartAnalyze()
			: base("/StartAnalyze")
		{
		}

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// If the resource uses user sessions.
		/// </summary>
		public override bool UserSessions
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// If the POST method is allowed.
		/// </summary>
		public bool AllowsPOST => true;

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public void POST(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request);

			if (!Request.HasData)
				throw new BadRequestException();

			object Obj = Request.DecodeData();
			if (!(Obj is Dictionary<string, object> Parameters))
				throw new BadRequestException();

			if (!Parameters.TryGetValue("repair", out Obj) || !(Obj is bool Repair))
				throw new BadRequestException();

			string BasePath = Export.FullExportFolder;

			if (!Directory.Exists(BasePath))
				Directory.CreateDirectory(BasePath);

			BasePath += Path.DirectorySeparatorChar;

			string FullFileName = Path.Combine(BasePath, "DBReport " + DateTime.Now.ToString("yyyy-MM-dd HH_mm_ss"));
			FullFileName = StartExport.GetUniqueFileName(FullFileName, ".xml");

			FileStream fs = new FileStream(FullFileName, FileMode.Create, FileAccess.Write);
			DateTime Created = File.GetCreationTime(FullFileName);
			XmlWriter XmlOutput = XmlWriter.Create(fs, XML.WriterSettings(true, false));
			string FileName = FullFileName.Substring(BasePath.Length);

			lock (synchObject)
			{
				if (analyzing)
				{
					Response.StatusCode = 409;
					Response.StatusMessage = "Conflict";
					Response.ContentType = "text/plain";
					Response.Write("Analysis is underway.");
					return;
				}
				else
					analyzing = true;
			}

			Task.Run(() => DoAnalyze(FullFileName, FileName, Created, XmlOutput, fs, Repair));

			Response.StatusCode = 200;
			Response.ContentType = "text/plain";
			Response.Write(FileName);
		}

		private static bool analyzing = false;
		private static readonly object synchObject = new object();
		private static XslCompiledTransform xslt = null;
		internal static Encoding utf8Bom = new UTF8Encoding(true);

		/// <summary>
		/// Analyzes the object database
		/// </summary>
		/// <param name="FullPath">Full path of report file</param>
		/// <param name="FileName">Filename of report</param>
		/// <param name="Created">Time when report file was created</param>
		/// <param name="XmlOutput">XML Output</param>
		/// <param name="fs">File stream</param>
		/// <param name="Repair">If database should be repaired, if errors are found</param>
		public static async Task DoAnalyze(string FullPath, string FileName, DateTime Created, XmlWriter XmlOutput, FileStream fs, bool Repair)
		{
			try
			{
				Log.Informational("Starting analyzing database.", FileName);

				ExportFormats.ExportFormat.UpdateClientsFileUpdated(FileName, 0, Created);

				await Database.Analyze(XmlOutput, null, Gateway.AppDataFolder, false, Repair);

				XmlOutput.Flush();
				fs.Flush();

				ExportFormats.ExportFormat.UpdateClientsFileUpdated(FileName, fs.Length, Created);

				XmlOutput.Dispose();
				XmlOutput = null;

				fs.Dispose();
				fs = null;

				if (xslt == null)
					xslt = XSL.LoadTransform(typeof(Gateway).Namespace + ".Transforms.DbStatXmlToHtml.xslt");
				
				string s = File.ReadAllText(FullPath);
				s = XSL.Transform(s, xslt);
				byte[] Bin = utf8Bom.GetBytes(s);

				string FullPath2 = FullPath.Substring(0, FullPath.Length - 4) + ".html";
				File.WriteAllBytes(FullPath2, Bin);

				ExportFormats.ExportFormat.UpdateClientsFileUpdated(FileName.Substring(0, FileName.Length - 4) + ".html", Bin.Length, Created);

				Log.Informational("Database analysis successfully completed.", FileName);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);

				string[] Tabs = ClientEvents.GetTabIDsForLocation("/Backup.md");
				ClientEvents.PushEvent(Tabs, "BackupFailed", "{\"fileName\":\"" + CommonTypes.JsonStringEncode(FileName) +
					"\", \"message\": \"" + CommonTypes.JsonStringEncode(ex.Message) + "\"}", true, "User");
			}
			finally
			{
				try
				{
					XmlOutput?.Dispose();
					fs?.Dispose();
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}

				lock (synchObject)
				{
					analyzing = false;
				}
			}
		}

	}
}
