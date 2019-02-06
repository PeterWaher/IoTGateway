using System;
using Waher.Content;
using Waher.IoTGateway;
using Waher.Networking.HTTP;

namespace Waher.IoTGateway.WebResources
{
	public class UpdateBackupSettings : HttpSynchronousResource, IHttpPostMethod
	{
		public UpdateBackupSettings()
			: base("/UpdateBackupSettings")
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
			if (P.Length != 5)
				throw new BadRequestException();

			if (!CommonTypes.TryParse(P[0].Trim(), out bool AutomaticBackups))
				throw new BadRequestException();

			if (!TimeSpan.TryParse(P[1].Trim(), out TimeSpan BackupTime))
			{
				Response.StatusCode = 400;
				Response.StatusMessage = "Bad Request";
				Response.ContentType = "text/plain";
				Response.Write("Backup time invalid.");
				Response.SendResponse();
				return;
			}

			if (!int.TryParse(P[2].Trim(), out int KeepDays) || KeepDays < 0)
			{
				Response.StatusCode = 400;
				Response.StatusMessage = "Bad Request";
				Response.ContentType = "text/plain";
				Response.Write("Invalid number of days specified.");
				Response.SendResponse();
				return;
			}

			if (!int.TryParse(P[3].Trim(), out int KeepMonths) || KeepMonths < 0)
			{
				Response.StatusCode = 400;
				Response.StatusMessage = "Bad Request";
				Response.ContentType = "text/plain";
				Response.Write("Invalid number of months specified.");
				Response.SendResponse();
				return;
			}

			if (!int.TryParse(P[4].Trim(), out int KeepYears) || KeepYears < 0)
			{
				Response.StatusCode = 400;
				Response.StatusMessage = "Bad Request";
				Response.ContentType = "text/plain";
				Response.Write("Invalid number of years specified.");
				Response.SendResponse();
				return;
			}

			Export.AutomaticBackups = AutomaticBackups;
			Export.BackupTime = BackupTime;
			Export.BackupKeepDays = KeepDays;
			Export.BackupKeepMonths = KeepMonths;
			Export.BackupKeepYears = KeepYears;

			Response.StatusCode = 200;
			Response.StatusMessage = "OK";
			Response.SendResponse();
		}
	}
}
