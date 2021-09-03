using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking.HTTP;

namespace Waher.IoTGateway.WebResources
{
	/// <summary>
	/// Web Resource for updating backup settings.
	/// </summary>
	public class UpdateBackupSettings : HttpSynchronousResource, IHttpPostMethod
	{
		/// <summary>
		/// Web Resource for updating backup settings.
		/// </summary>
		public UpdateBackupSettings()
			: base("/UpdateBackupSettings")
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
				throw new BadRequestException();

			object Obj = Request.DecodeData();

			if (!(Obj is Dictionary<string, object> Form))
				throw new BadRequestException();

			if (!Form.TryGetValue("AutomaticBackups", out Obj) || !CommonTypes.TryParse(Obj.ToString().Trim(), out bool AutomaticBackups))
			{
				Response.StatusCode = 400;
				Response.StatusMessage = "Bad Request";
				Response.ContentType = "text/plain";
				await Response.Write("Automatic backups value invalid.");
				await Response.SendResponse();
				return;
			}

			if (!Form.TryGetValue("BackupTime", out Obj) || !TimeSpan.TryParse(Obj.ToString().Trim(), out TimeSpan BackupTime))
			{
				Response.StatusCode = 400;
				Response.StatusMessage = "Bad Request";
				Response.ContentType = "text/plain";
				await Response.Write("Backup time invalid.");
				await Response.SendResponse();
				return;
			}

			if (!Form.TryGetValue("KeepDays", out Obj) || !int.TryParse(Obj.ToString().Trim(), out int KeepDays) || KeepDays < 0)
			{
				Response.StatusCode = 400;
				Response.StatusMessage = "Bad Request";
				Response.ContentType = "text/plain";
				await Response.Write("Invalid number of days specified.");
				await Response.SendResponse();
				return;
			}

			if (!Form.TryGetValue("KeepMonths", out Obj) || !int.TryParse(Obj.ToString().Trim(), out int KeepMonths) || KeepMonths < 0)
			{
				Response.StatusCode = 400;
				Response.StatusMessage = "Bad Request";
				Response.ContentType = "text/plain";
				await Response.Write("Invalid number of months specified.");
				await Response.SendResponse();
				return;
			}

			if (!Form.TryGetValue("KeepYears", out Obj) || !int.TryParse(Obj.ToString().Trim(), out int KeepYears) || KeepYears < 0)
			{
				Response.StatusCode = 400;
				Response.StatusMessage = "Bad Request";
				Response.ContentType = "text/plain";
				await Response.Write("Invalid number of years specified.");
				await Response.SendResponse();
				return;
			}

			Export.AutomaticBackups = AutomaticBackups;
			Export.BackupTime = BackupTime;
			Export.BackupKeepDays = KeepDays;
			Export.BackupKeepMonths = KeepMonths;
			Export.BackupKeepYears = KeepYears;

			Response.StatusCode = 200;
			Response.StatusMessage = "OK";
			await Response.SendResponse();
		}
	}
}
