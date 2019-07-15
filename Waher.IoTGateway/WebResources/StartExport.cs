using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.HTTP;
using Waher.Networking.XMPP;
using Waher.IoTGateway;
using Waher.IoTGateway.WebResources.ExportFormats;

namespace Waher.IoTGateway.WebResources
{
	/// <summary>
	/// Starts data export
	/// </summary>
	public class StartExport : HttpSynchronousResource, IHttpPostMethod
	{
		internal static RNGCryptoServiceProvider rnd = new RNGCryptoServiceProvider();
		internal static AesCryptoServiceProvider aes = GetCryptoProvider();

		/// <summary>
		/// Starts data export
		/// </summary>
		public StartExport()
			: base("/StartExport")
		{
		}

		private static AesCryptoServiceProvider GetCryptoProvider()
		{
			AesCryptoServiceProvider Result = new AesCryptoServiceProvider()
			{
				BlockSize = 128,
				KeySize = 256,
				Mode = CipherMode.CBC,
				Padding = PaddingMode.None
			};

			return Result;
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

			if (!Request.HasData ||
				!(Request.DecodeData() is Dictionary<string, object> RequestObj) ||
				!RequestObj.TryGetValue("TypeOfFile", out object Obj) ||
				!(Obj is string TypeOfFile) ||
				!RequestObj.TryGetValue("Database", out Obj) ||
				!(Obj is bool Database) ||
				!RequestObj.TryGetValue("WebContent", out Obj) ||
				!(Obj is bool WebContent))
			{
				throw new BadRequestException();
			}

			KeyValuePair<string, IExportFormat> Exporter = GetExporter(TypeOfFile);

			lock (synchObject)
			{
				if (exporting)
				{
					Response.StatusCode = 409;
					Response.StatusMessage = "Conflict";
					Response.ContentType = "text/plain";
					Response.Write("Export is underway.");
					return;
				}
				else
					exporting = true;
			}

			Export.ExportType = TypeOfFile;
			Export.ExportDatabase = Database;
			Export.ExportWebContent = WebContent;

			List<string> Folders = new List<string>();

			foreach (Export.FolderCategory FolderCategory in Export.GetRegisteredFolders())
			{
				if (RequestObj.TryGetValue(FolderCategory.CategoryId, out Obj) && Obj is bool b)
				{
					Export.SetExportFolderAsync(FolderCategory.CategoryId, b).Wait();

					if (b)
						Folders.AddRange(FolderCategory.Folders);
				}
			}

			Task _ = DoExport(Exporter.Value, Database, WebContent, Folders.ToArray());

			Response.StatusCode = 200;
			Response.ContentType = "text/plain";
			Response.Write(Exporter.Key);
		}

		private static bool exporting = false;
		private static readonly object synchObject = new object();

		internal static KeyValuePair<string, IExportFormat> GetExporter(string TypeOfFile)
		{
			IExportFormat Output;
			string BasePath = Export.FullExportFolder;

			if (!Directory.Exists(BasePath))
				Directory.CreateDirectory(BasePath);

			BasePath += Path.DirectorySeparatorChar;

			string FullFileName = BasePath + DateTime.Now.ToString("yyyy-MM-dd HH_mm_ss");

			switch (TypeOfFile)
			{
				case "XML":
					FullFileName = GetUniqueFileName(FullFileName, ".xml");
					FileStream fs = new FileStream(FullFileName, FileMode.Create, FileAccess.Write);
					DateTime Created = File.GetCreationTime(FullFileName);
					XmlWriterSettings Settings = XML.WriterSettings(true, false);
					Settings.Async = true;
					XmlWriter XmlOutput = XmlWriter.Create(fs, Settings);
					string FileName = FullFileName.Substring(BasePath.Length);
					Output = new XmlExportFormat(FileName, Created, XmlOutput, fs);
					break;

				case "Binary":
					FullFileName = GetUniqueFileName(FullFileName, ".bin");
					fs = new FileStream(FullFileName, FileMode.Create, FileAccess.Write);
					Created = File.GetCreationTime(FullFileName);
					FileName = FullFileName.Substring(BasePath.Length);
					Output = new BinaryExportFormat(FileName, Created, fs, fs);
					break;

				case "Compressed":
					FullFileName = GetUniqueFileName(FullFileName, ".gz");
					fs = new FileStream(FullFileName, FileMode.Create, FileAccess.Write);
					Created = File.GetCreationTime(FullFileName);
					FileName = FullFileName.Substring(BasePath.Length);
					GZipStream gz = new GZipStream(fs, CompressionLevel.Optimal, false);
					Output = new BinaryExportFormat(FileName, Created, gz, fs);
					break;

				case "Encrypted":
					FullFileName = GetUniqueFileName(FullFileName, ".bak");
					fs = new FileStream(FullFileName, FileMode.Create, FileAccess.Write);
					Created = File.GetCreationTime(FullFileName);
					FileName = FullFileName.Substring(BasePath.Length);

					byte[] Key = new byte[32];
					byte[] IV = new byte[16];

					lock (rnd)
					{
						rnd.GetBytes(Key);
						rnd.GetBytes(IV);
					}

					ICryptoTransform AesTransform = aes.CreateEncryptor(Key, IV);
					CryptoStream cs = new CryptoStream(fs, AesTransform, CryptoStreamMode.Write);

					gz = new GZipStream(cs, CompressionLevel.Optimal, false);
					Output = new BinaryExportFormat(FileName, Created, gz, fs, cs, 32);

					string BasePath2 = Export.FullKeyExportFolder;

					if (!Directory.Exists(BasePath2))
						Directory.CreateDirectory(BasePath2);

					BasePath2 += Path.DirectorySeparatorChar;
					string FullFileName2 = BasePath2 + FullFileName.Substring(BasePath.Length).Replace(".bak", ".key");

					using (XmlOutput = XmlWriter.Create(FullFileName2, XML.WriterSettings(true, false)))
					{
						XmlOutput.WriteStartDocument();
						XmlOutput.WriteStartElement("KeyAes256", Export.ExportNamepace);
						XmlOutput.WriteAttributeString("key", System.Convert.ToBase64String(Key));
						XmlOutput.WriteAttributeString("iv", System.Convert.ToBase64String(IV));
						XmlOutput.WriteEndElement();
						XmlOutput.WriteEndDocument();
					}

					long Size;

					try
					{
						using (fs = File.OpenRead(FullFileName2))
						{
							Size = fs.Length;
						}
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
						Size = 0;
					}

					Created = File.GetCreationTime(FullFileName2);

					ExportFormat.UpdateClientsFileUpdated(FullFileName2.Substring(BasePath2.Length), Size, Created);
					break;

				default:
					throw new NotSupportedException("Unsupported file type.");
			}

			if (FullFileName.StartsWith(BasePath))
				FullFileName = FullFileName.Substring(BasePath.Length);

			return new KeyValuePair<string, IExportFormat>(FullFileName, Output);
		}

		internal static string GetUniqueFileName(string Base, string Extension)
		{
			string Suffix = string.Empty;
			string s;
			int i = 1;

			while (true)
			{
				s = Base + Suffix + Extension;
				if (!File.Exists(s))
					return s;

				i++;
				Suffix = " (" + i.ToString() + ")";
			}
		}

		/// <summary>
		/// Exports gateway data
		/// </summary>
		/// <param name="Output">Export Output</param>
		/// <param name="Database">If the contents of the database is to be exported.</param>
		/// <param name="WebContent">If web content is to be exported.</param>
		/// <param name="Folders">Root subfolders to export.</param>
		public static async Task DoExport(IExportFormat Output, bool Database, bool WebContent, string[] Folders)
		{
			try
			{
				List<KeyValuePair<string, object>> Tags = new List<KeyValuePair<string, object>>()
				{
					new KeyValuePair<string, object>("Database", Database)
				};

				foreach (string Folder in Folders)
					Tags.Add(new KeyValuePair<string, object>(Folder, true));

				Log.Informational("Starting export.", Output.FileName, Tags.ToArray());

				await Output.Start();

				if (Database)
				{
					StringBuilder Temp = new StringBuilder();
					using (XmlWriter w = XmlWriter.Create(Temp, XML.WriterSettings(false, true)))
					{
						await Persistence.Database.Analyze(w, string.Empty, Path.Combine(Gateway.RootFolder, "Data"), false, true);
					}

					await Persistence.Database.Export(Output);
				}

				if (WebContent || Folders.Length > 0)
				{
					await Output.StartFiles();
					try
					{
						string[] FileNames;
						string Folder2;

						if (WebContent)
						{
							await ExportFile(Path.Combine(Gateway.AppDataFolder, "Gateway.config"), Output);

							FileNames = Directory.GetFiles(Gateway.RootFolder, "*.*", SearchOption.TopDirectoryOnly);

							foreach (string FileName in FileNames)
								await ExportFile(FileName, Output);

							Export.FolderCategory[] ExportFolders = Export.GetRegisteredFolders();

							foreach (string Folder in Directory.GetDirectories(Gateway.RootFolder, "*.*", SearchOption.TopDirectoryOnly))
							{
								bool IsWebContent = true;

								foreach (Export.FolderCategory FolderCategory in ExportFolders)
								{
									foreach (string Folder3 in FolderCategory.Folders)
									{
										if (string.Compare(Folder3, Folder, true) == 0)
										{
											IsWebContent = false;
											break;
										}
									}

									if (!IsWebContent)
										break;
								}

								if (IsWebContent)
								{
									FileNames = Directory.GetFiles(Folder, "*.*", SearchOption.AllDirectories);

									foreach (string FileName in FileNames)
										await ExportFile(FileName, Output);
								}
							}
						}

						foreach (string Folder in Folders)
						{
							if (Directory.Exists(Folder2 = Path.Combine(Gateway.RootFolder, Folder)))
							{
								FileNames = Directory.GetFiles(Folder2, "*.*", SearchOption.AllDirectories);

								foreach (string FileName in FileNames)
									await ExportFile(FileName, Output);
							}
						}
					}
					finally
					{
						await Output.EndFiles();
					}
				}

				Log.Informational("Export successfully completed.", Output.FileName);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);

				string[] Tabs = ClientEvents.GetTabIDsForLocation("/Settings/Backup.md");
				ClientEvents.PushEvent(Tabs, "BackupFailed", "{\"fileName\":\"" + CommonTypes.JsonStringEncode(Output.FileName) +
					"\", \"message\": \"" + CommonTypes.JsonStringEncode(ex.Message) + "\"}", true, "User");
			}
			finally
			{
				try
				{
					await Output.End();
					Output.Dispose();
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}

				lock (synchObject)
				{
					exporting = false;
				}
			}
		}

		private static async Task ExportFile(string FileName, IExportFormat Output)
		{
			using (FileStream fs = File.OpenRead(FileName))
			{
				if (FileName.StartsWith(Gateway.AppDataFolder))
					FileName = FileName.Substring(Gateway.AppDataFolder.Length);

				await Output.ExportFile(FileName, fs);
			}

			await Output.UpdateClient(false);
		}

	}
}
