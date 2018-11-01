using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Emoji;
using Waher.Content.Emoji.Emoji1;
using Waher.Content.Markdown;
using Waher.Events;
using Waher.Networking.HTTP;
using Waher.Script;
using Waher.Security;
using Waher.Persistence;
using Waher.Networking.XMPP;
using Waher.IoTGateway;
using Waher.IoTGateway.WebResources.ExportFormats;

namespace Waher.IoTGateway.WebResources
{
	/// <summary>
	/// Deletes an exported file.
	/// </summary>
	public class DeleteExport : HttpSynchronousResource, IHttpPostMethod
	{
		/// <summary>
		/// Deletes an exported file.
		/// </summary>
		public DeleteExport()
			: base("/DeleteExport")
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

			if (!(Request.DecodeData() is string FileName))
				throw new BadRequestException();

			string Dir;

			if (FileName.EndsWith(".key", StringComparison.CurrentCultureIgnoreCase))
				Dir = Export.FullKeyExportFolder;
			else
				Dir = Export.FullExportFolder;

			if (!Directory.Exists(Dir))
				throw new NotFoundException();

			string FullFileName = Dir + Path.DirectorySeparatorChar + FileName;
			if (!File.Exists(FullFileName))
				throw new NotFoundException();

			File.Delete(FullFileName);

			Log.Informational("Export deleted.", FileName);

			Response.StatusCode = 200;
			Response.ContentType = "text/plain";
			Response.Write("1");

			ExportFormat.UpdateClientsFileDeleted(FileName);
		}

	}
}
