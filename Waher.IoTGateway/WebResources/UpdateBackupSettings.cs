using System;
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
		public async Task POST(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request, "Admin.Data.Backup");

			if (!Request.HasData)
				throw new BadRequestException();

			object Obj = Request.DecodeData();
			string[] P;

			if (!(Obj is string s))
				throw new BadRequestException();

			P = s.Split('\n');
			if (P.Length != 5)
				throw new BadRequestException();

			if (!CommonTypes.TryParse(P[0].Trim(), out bool AutomaticBackups))
				throw new BadRequestException();

			if (!TimeSpan.TryParse(P[1].Trim(), out TimeSpan BackupTime))
			{
				Response.StatusCode = 400;
				Response.StatusMessage = "Bad Request";
				Response.ContentType = "text/plain";
				await Response.Write("Backup time invalid.");
				await Response.SendResponse();
				return;
			}

			if (!int.TryParse(P[2].Trim(), out int KeepDays) || KeepDays < 0)
			{
				Response.StatusCode = 400;
				Response.StatusMessage = "Bad Request";
				Response.ContentType = "text/plain";
				await Response.Write("Invalid number of days specified.");
				await Response.SendResponse();
				return;
			}

			if (!int.TryParse(P[3].Trim(), out int KeepMonths) || KeepMonths < 0)
			{
				Response.StatusCode = 400;
				Response.StatusMessage = "Bad Request";
				Response.ContentType = "text/plain";
				await Response.Write("Invalid number of months specified.");
				await Response.SendResponse();
				return;
			}

			if (!int.TryParse(P[4].Trim(), out int KeepYears) || KeepYears < 0)
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
