using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Text;
using Waher.Networking.HTTP;
using Waher.Runtime.IO;

namespace Waher.IoTGateway.WebResources
{
	/// <summary>
	/// Web Resource for updating where backup files are storeed.
	/// </summary>
	public class UpdateBackupFolderSettings : HttpSynchronousResource, IHttpPostMethod
	{
		/// <summary>
		/// Web Resource for updating where backup files are storeed.
		/// </summary>
		public UpdateBackupFolderSettings()
			: base("/UpdateBackupFolderSettings")
		{
		}

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths => false;

		/// <summary>
		/// If the resource uses user sessions.
		/// </summary>
		public override bool UserSessions => true;

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
		public async Task POST(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request, "Admin.Data.Backup");

			if (!Request.HasData)
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			ContentResponse Content = await Request.DecodeDataAsync();

			if (Content.HasError ||
				!(Content.Decoded is Dictionary<string, object> Form) ||
				!Form.TryGetValue("ExportFolder", out object Obj) || !(Obj is string ExportFolder) ||
				!Form.TryGetValue("KeyFolder", out Obj) || !(Obj is string KeyFolder) ||
				!Form.TryGetValue("BackupHosts", out Obj) || !(Obj is string BackupHostsStr) ||
				!Form.TryGetValue("KeyHosts", out Obj) || !(Obj is string KeyHostsStr))
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}


			ExportFolder = ExportFolder.Trim();
			KeyFolder = KeyFolder.Trim();

			string[] BackupHosts = BackupHostsStr.Split(',');
			string[] KeyHosts = KeyHostsStr.Split(',');
			int Len;

			Trim(BackupHosts);
			Trim(KeyHosts);

			Len = ExportFolder.Length;
			if (Len > 0 && (ExportFolder[Len - 1] == '/' || ExportFolder[Len - 1] == '\\'))
				ExportFolder = ExportFolder[..(Len - 1)];

			Len = KeyFolder.Length;
			if (Len > 0 && (KeyFolder[Len - 1] == '/' || KeyFolder[Len - 1] == '\\'))
				KeyFolder = KeyFolder[..(Len - 1)];

			if (!string.IsNullOrEmpty(ExportFolder))
			{
				if (!Path.IsPathRooted(ExportFolder))
				{
					Response.StatusCode = 400;
					Response.StatusMessage = "Bad Request";
					Response.ContentType = PlainTextCodec.DefaultContentType;
					await Response.Write("Export folder must be rooted. Relative paths are not accepted.");
					await Response.SendResponse();
					return;
				}

				if (!Directory.Exists(ExportFolder))
				{
					Response.StatusCode = 400;
					Response.StatusMessage = "Bad Request";
					Response.ContentType = PlainTextCodec.DefaultContentType;
					await Response.Write("Export folder does not exist, or cannot be accessed or reached.");
					await Response.SendResponse();
					return;
				}

				try
				{
					string s = ExportFolder + Path.DirectorySeparatorChar + "test.txt";
					await Files.WriteAllTextAsync(s, "test");
					File.Delete(s);
				}
				catch (Exception)
				{
					Response.StatusCode = 400;
					Response.StatusMessage = "Bad Request";
					Response.ContentType = PlainTextCodec.DefaultContentType;
					await Response.Write("Not allowed to write data to the export folder.");
					await Response.SendResponse();
					return;
				}
			}

			if (!string.IsNullOrEmpty(KeyFolder))
			{
				if (!Path.IsPathRooted(KeyFolder))
				{
					Response.StatusCode = 400;
					Response.StatusMessage = "Bad Request";
					Response.ContentType = PlainTextCodec.DefaultContentType;
					await Response.Write("Key folder must be rooted. Relative paths are not accepted.");
					await Response.SendResponse();
					return;
				}

				if (!Directory.Exists(KeyFolder))
				{
					Response.StatusCode = 400;
					Response.StatusMessage = "Bad Request";
					Response.ContentType = PlainTextCodec.DefaultContentType;
					await Response.Write("Key folder does not exist, or cannot be accessed or reached.");
					await Response.SendResponse();
					return;
				}

				try
				{
					string s = KeyFolder + Path.DirectorySeparatorChar + "test.txt";
					await Files.WriteAllTextAsync(s, "test");
					File.Delete(s);
				}
				catch (Exception)
				{
					Response.StatusCode = 400;
					Response.StatusMessage = "Bad Request";
					Response.ContentType = PlainTextCodec.DefaultContentType;
					await Response.Write("Not allowed to write data to the key folder.");
					await Response.SendResponse();
					return;
				}
			}

			await Export.SetExportFolderAsync(ExportFolder);
			await Export.SetExportKeyFolderAsync(KeyFolder);
			await Export.SetBackupHostsAsync(BackupHosts);
			await Export.SetKeyHostsAsync(KeyHosts);

			Response.StatusCode = 200;
			Response.StatusMessage = "OK";
			await Response.SendResponse();
		}

		private static void Trim(string[] Array)
		{
			int i, c = Array.Length;

			for (i = 0; i < c; i++)
				Array[i] = Array[i].Trim();
		}
	}
}
