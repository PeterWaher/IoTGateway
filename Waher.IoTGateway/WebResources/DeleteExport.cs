using System;
using System.IO;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Text;
using Waher.Events;
using Waher.IoTGateway.WebResources.ExportFormats;
using Waher.Networking.HTTP;

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
			if (Content.HasError || !(Content.Decoded is string FileName))
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			string Dir;

			if (FileName.EndsWith(".key", StringComparison.CurrentCultureIgnoreCase))
				Dir = await Export.GetFullKeyExportFolderAsync();
			else
				Dir = await Export.GetFullExportFolderAsync();

			if (!Directory.Exists(Dir))
			{
				await Response.SendResponse(new NotFoundException("Folder not found: " + Dir));
				return;
			}

			string FullFileName = Dir + Path.DirectorySeparatorChar + FileName;
			if (!File.Exists(FullFileName))
			{
				await Response.SendResponse(new NotFoundException("File not found: " + FullFileName));
				return;
			}

			File.Delete(FullFileName);

			Log.Informational("Export deleted.", FileName);

			Response.StatusCode = 200;
			Response.ContentType = PlainTextCodec.DefaultContentType;
			await Response.Write("1");

			ExportFormat.UpdateClientsFileDeleted(FileName);
		}

	}
}
