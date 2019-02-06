using System;
using System.IO;
using Waher.Content;
using Waher.IoTGateway;
using Waher.Networking.HTTP;

namespace Waher.IoTGateway.WebResources
{
	public class UpdateBackupFolderSettings : HttpSynchronousResource, IHttpPostMethod
	{
		public const int MaxAvatarSize = 64;

		public UpdateBackupFolderSettings()
			: base("/UpdateBackupFolderSettings")
		{
		}

		public override bool HandlesSubPaths
		{
			get
			{
				return false;
			}
		}

		public override bool UserSessions
		{
			get
			{
				return true;
			}
		}

		public bool AllowsPOST => true;

		public void POST(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request);

			if (!Request.HasData)
				throw new BadRequestException();

			object Obj = Request.DecodeData();
			string[] P;

			if (!(Obj is string s))
				throw new BadRequestException();

			P = s.Split('\n');
			if (P.Length != 2)
				throw new BadRequestException();

			string ExportFolder = P[0].Trim();
			string KeyFolder = P[1].Trim();
			int Len;

			Len = ExportFolder.Length;
			if (Len > 0 && (ExportFolder[Len - 1] == '/' || ExportFolder[Len - 1] == '\\'))
				ExportFolder = ExportFolder.Substring(0, Len - 1);

			Len = KeyFolder.Length;
			if (Len > 0 && (KeyFolder[Len - 1] == '/' || KeyFolder[Len - 1] == '\\'))
				KeyFolder = KeyFolder.Substring(0, Len - 1);

			if (!string.IsNullOrEmpty(ExportFolder))
			{
				if (!Path.IsPathRooted(ExportFolder))
				{
					Response.StatusCode = 400;
					Response.StatusMessage = "Bad Request";
					Response.ContentType = "text/plain";
					Response.Write("Export folder must be rooted. Relative paths are not accepted.");
					Response.SendResponse();
					return;
				}

				if (!Directory.Exists(ExportFolder))
				{
					Response.StatusCode = 400;
					Response.StatusMessage = "Bad Request";
					Response.ContentType = "text/plain";
					Response.Write("Export folder does not exist, or cannot be accessed or reached.");
					Response.SendResponse();
					return;
				}

				try
				{
					s = ExportFolder + Path.DirectorySeparatorChar + "test.txt";
					File.WriteAllText(s, "test");
					File.Delete(s);
				}
				catch (Exception)
				{
					Response.StatusCode = 400;
					Response.StatusMessage = "Bad Request";
					Response.ContentType = "text/plain";
					Response.Write("Not allowed to write data to the export folder.");
					Response.SendResponse();
					return;
				}
			}

			if (!string.IsNullOrEmpty(KeyFolder))
			{
				if (!Path.IsPathRooted(KeyFolder))
				{
					Response.StatusCode = 400;
					Response.StatusMessage = "Bad Request";
					Response.ContentType = "text/plain";
					Response.Write("Key folder must be rooted. Relative paths are not accepted.");
					Response.SendResponse();
					return;
				}

				if (!Directory.Exists(KeyFolder))
				{
					Response.StatusCode = 400;
					Response.StatusMessage = "Bad Request";
					Response.ContentType = "text/plain";
					Response.Write("Key folder does not exist, or cannot be accessed or reached.");
					Response.SendResponse();
					return;
				}

				try
				{
					s = KeyFolder + Path.DirectorySeparatorChar + "test.txt";
					File.WriteAllText(s, "test");
					File.Delete(s);
				}
				catch (Exception)
				{
					Response.StatusCode = 400;
					Response.StatusMessage = "Bad Request";
					Response.ContentType = "text/plain";
					Response.Write("Not allowed to write data to the key folder.");
					Response.SendResponse();
					return;
				}
			}

			Export.ExportFolder = ExportFolder;
			Export.ExportKeyFolder = KeyFolder;

			Response.StatusCode = 200;
			Response.StatusMessage = "OK";
			Response.SendResponse();
		}
	}
}
