using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Waher.IoTGateway.WebResources.ExportFormats;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Content.Xsl;
using Waher.Events;
using Waher.Networking.HTTP;
using Waher.Networking.HTTP.HeaderFields;
using Waher.Persistence;
using Waher.Persistence.Serialization;
using Waher.Runtime.Cache;
using Waher.Runtime.Language;
using Waher.Script;

namespace Waher.IoTGateway.Setup
{
	/// <summary>
	/// Restore Configuration
	/// </summary>
	public class RestoreConfiguration : SystemConfiguration, IDisposable
	{
		private static RestoreConfiguration instance = null;

		private HttpResource uploadBackup = null;
		private HttpResource uploadKey = null;
		private HttpResource restore = null;

		private readonly Dictionary<string, TemporaryFile> backupFilePerSession = new Dictionary<string, TemporaryFile>();
		private readonly Dictionary<string, TemporaryFile> keyFilePerSession = new Dictionary<string, TemporaryFile>();
		private int expectedBlockBackup = 0;
		private int expectedBlockKey = 0;
		private bool reloadConfiguration = false;

		/// <summary>
		/// Current instance of configuration.
		/// </summary>
		public static RestoreConfiguration Instance => instance;

		/// <summary>
		/// Resource to be redirected to, to perform the configuration.
		/// </summary>
		public override string Resource => "/Settings/Restore.md";

		/// <summary>
		/// Priority of the setting. Configurations are sorted in ascending order.
		/// </summary>
		public override int Priority => 150;

		/// <summary>
		/// Gets a title for the system configuration.
		/// </summary>
		/// <param name="Language">Current language.</param>
		/// <returns>Title string</returns>
		public override Task<string> Title(Language Language)
		{
			return Language.GetStringAsync(typeof(Gateway), 9, "Restore");
		}

		/// <summary>
		/// Is called during startup to configure the system.
		/// </summary>
		public override Task ConfigureSystem()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Sets the static instance of the configuration.
		/// </summary>
		/// <param name="Configuration">Configuration object</param>
		public override void SetStaticInstance(ISystemConfiguration Configuration)
		{
			instance = Configuration as RestoreConfiguration;
		}

		/// <summary>
		/// Initializes the setup object.
		/// </summary>
		/// <param name="WebServer">Current Web Server object.</param>
		public override Task InitSetup(HttpServer WebServer)
		{
			this.uploadBackup = WebServer.Register("/Settings/UploadBackup", null, this.UploadBackup, true, false, true);
			this.uploadKey = WebServer.Register("/Settings/UploadKey", null, this.UploadKey, true, false, true);
			this.restore = WebServer.Register("/Settings/Restore", null, this.Restore, true, false, true);

			WebServer.SessionRemoved += WebServer_SessionRemoved;

			return base.InitSetup(WebServer);
		}

		/// <summary>
		/// Unregisters the setup object.
		/// </summary>
		/// <param name="WebServer">Current Web Server object.</param>
		public override Task UnregisterSetup(HttpServer WebServer)
		{
			WebServer.Unregister(this.uploadBackup);
			WebServer.Unregister(this.uploadKey);
			WebServer.Unregister(this.restore);

			WebServer.SessionRemoved -= WebServer_SessionRemoved;

			return base.UnregisterSetup(WebServer);
		}

		private void WebServer_SessionRemoved(object Sender, Runtime.Cache.CacheItemEventArgs<string, Variables> e)
		{
			RemoveFile(e.Key, this.backupFilePerSession);
			RemoveFile(e.Key, this.keyFilePerSession);
		}

		private static void RemoveFile(string Key, Dictionary<string, TemporaryFile> Files)
		{
			lock (Files)
			{
				if (Files.TryGetValue(Key, out TemporaryFile File))
				{
					Files.Remove(Key);
					File.Dispose();
				}
			}
		}

		private Task UploadBackup(HttpRequest Request, HttpResponse Response)
		{
			Upload(Request, Response, ref this.expectedBlockBackup, this.backupFilePerSession, "backup");
			return Task.CompletedTask;
		}

		private Task UploadKey(HttpRequest Request, HttpResponse Response)
		{
			Upload(Request, Response, ref this.expectedBlockKey, this.keyFilePerSession, "key");
			return Task.CompletedTask;
		}

		private static void Upload(HttpRequest Request, HttpResponse Response, ref int ExpectedBlockNr, Dictionary<string, TemporaryFile> Files, string Name)
		{
			Gateway.AssertUserAuthenticated(Request);

			HttpFieldCookie Cookie;
			TemporaryFile File;
			string TabID;
			string HttpSessionID;

			if (!Request.HasData ||
				!Request.Header.TryGetHeaderField("X-TabID", out HttpField F) ||
				string.IsNullOrEmpty(TabID = F.Value) ||
				!Request.Header.TryGetHeaderField("X-BlockNr", out F) ||
				!int.TryParse(F.Value, out int BlockNr) ||
				!Request.Header.TryGetHeaderField("X-More", out F) ||
				!CommonTypes.TryParse(F.Value, out bool More) ||
				(Cookie = Request.Header.Cookie) is null ||
				string.IsNullOrEmpty(HttpSessionID = Cookie["HttpSessionID"]))
			{
				throw new BadRequestException();
			}

			if (BlockNr == 0)
			{
				ExpectedBlockNr = 0;
				RemoveFile(HttpSessionID, Files);

				if (Request.Header.TryGetHeaderField("X-FileName", out F) && !string.IsNullOrEmpty(F.Value))
					Request.Session[Name + "FileName"] = F.Value;
				else
					throw new BadRequestException();
			}

			if (BlockNr != ExpectedBlockNr)
				throw new BadRequestException();

			ExpectedBlockNr++;

			lock (Files)
			{
				if (!Files.TryGetValue(HttpSessionID, out File))
				{
					File = new TemporaryFile();
					Files[HttpSessionID] = File;
				}
			}

			Request.DataStream.CopyTo(File);

			if (!More)
				File.Flush();

			ShowStatus(TabID, Name + "Bytes", Export.FormatBytes(File.Length) + " received of " + Name + " file.");

			Response.StatusCode = 200;
		}

		private static void CollectionFound(string TabID, string CollectionName)
		{
			ClientEvents.PushEvent(new string[] { TabID }, "CollectionFound", CollectionName, false);
		}

		private static void ShowStatus(string TabID, string Id, string Message)
		{
			ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", JSON.Encode(new Dictionary<string, object>()
			{
				{ "id", Id },
				{ "message", Message },
			}, false), true);
		}

		private static void ShowStatus(string TabID, string Message)
		{
			ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", Message, false);
		}

		private Task Restore(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request);

			HttpFieldCookie Cookie;
			TemporaryFile BackupFile;
			TemporaryFile KeyFile;
			string TabID;
			string HttpSessionID;

			if (!Request.HasData ||
				!Request.Header.TryGetHeaderField("X-TabID", out HttpField F) ||
				string.IsNullOrEmpty(TabID = F.Value) ||
				(Cookie = Request.Header.Cookie) is null ||
				string.IsNullOrEmpty(HttpSessionID = Cookie["HttpSessionID"]))
			{
				throw new BadRequestException();
			}

			object Obj = Request.DecodeData();
			if (!(Obj is Dictionary<string, object> Parameters))
				throw new BadRequestException();

			if (!Parameters.TryGetValue("overwrite", out Obj) || !(Obj is bool Overwrite) ||
				!Parameters.TryGetValue("onlySelectedCollections", out Obj) || !(Obj is bool OnlySelectedCollections) ||
				!Parameters.TryGetValue("selectedCollections", out Obj) || !(Obj is Array SelectedCollections))
			{
				throw new BadRequestException();
			}

			BackupFile = GetAndRemoveFile(HttpSessionID, this.backupFilePerSession);
			KeyFile = GetAndRemoveFile(HttpSessionID, this.keyFilePerSession);

			this.Restore(BackupFile, KeyFile, TabID, Request.Session["backupFileName"]?.ToString(),
				Overwrite, OnlySelectedCollections, SelectedCollections);

			Response.StatusCode = 200;
		
			return Task.CompletedTask;
		}

		private static TemporaryFile GetAndRemoveFile(string SessionID, Dictionary<string, TemporaryFile> Files)
		{
			lock (Files)
			{
				if (Files.TryGetValue(SessionID, out TemporaryFile File))
				{
					Files.Remove(SessionID);
					return File;
				}
				else
					return null;
			}
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			Clear(this.backupFilePerSession);
			Clear(this.keyFilePerSession);
		}

		private static void Clear(Dictionary<string, TemporaryFile> Files)
		{
			if (Files != null)
			{
				lock (Files)
				{
					foreach (TemporaryFile File in Files.Values)
						File.Dispose();

					Files.Clear();
				}
			}
		}

		private async void Restore(TemporaryFile BackupFile, TemporaryFile KeyFile, string TabID, string BackupFileName, bool Overwrite,
			bool OnlySelectedCollections, Array SelectedCollections)
		{
			ICryptoTransform AesTransform1 = null;
			ICryptoTransform AesTransform2 = null;
			CryptoStream cs1 = null;
			CryptoStream cs2 = null;

			try
			{
				if (Overwrite)
					ShowStatus(TabID, "Restoring backup.");
				else
					ShowStatus(TabID, "Validating backup.");

				if (BackupFile is null || string.IsNullOrEmpty(BackupFileName))
					throw new Exception("No backup file selected.");

				string Extension = Path.GetExtension(BackupFileName);
				ValidateBackupFile Import = new ValidateBackupFile(BackupFileName, null);

				(AesTransform1, cs1) = await DoImport(BackupFile, KeyFile, TabID, Extension, Import, false, false, new string[0]);

				if (Overwrite)
				{
					ShowStatus(TabID, "Restoring backup.");
					Import = new RestoreBackupFile(BackupFileName, Import.ObjectIdMap);

					(AesTransform2, cs2) = await DoImport(BackupFile, KeyFile, TabID, Extension, Import, true,
						OnlySelectedCollections, SelectedCollections);

					this.reloadConfiguration = true;
					await DoAnalyze(TabID);

					Caches.ClearAll();
				}

				StringBuilder Result = new StringBuilder();

				if (Overwrite)
					Result.AppendLine("Restoration complete.");
				else
					Result.AppendLine("Verification complete.");

				if (Import.NrCollections > 0 || Import.NrObjects > 0 || Import.NrProperties > 0 || Import.NrFiles > 0)
				{
					Result.AppendLine();
					Result.AppendLine("Contents of file:");
					Result.AppendLine();

					if (Import.NrCollections > 0)
					{
						Result.Append(Import.NrCollections.ToString());
						if (Import.NrCollections > 1)
							Result.AppendLine(" collections.");
						else
							Result.AppendLine(" collection.");
					}

					if (Import.NrObjects > 0)
					{
						Result.Append(Import.NrObjects.ToString());
						if (Import.NrObjects > 1)
							Result.AppendLine(" objects.");
						else
							Result.AppendLine(" object.");
					}

					if (Import.NrProperties > 0)
					{
						Result.Append(Import.NrProperties.ToString());
						if (Import.NrProperties > 1)
							Result.AppendLine(" properties.");
						else
							Result.AppendLine(" property.");
					}

					if (Import.NrFiles > 0)
					{
						Result.Append(Import.NrFiles.ToString());
						if (Import.NrFiles > 1)
							Result.Append(" files");
						else
							Result.Append(" file");

						Result.Append(" (");
						Result.Append(Export.FormatBytes(Import.NrFileBytes));
						Result.AppendLine(").");
					}

					if (Import is RestoreBackupFile Restore && Restore.NrObjectsFailed > 0)
					{
						Result.Append(Restore.NrObjectsFailed.ToString());
						if (Import.NrProperties > 1)
							Result.Append(" objects");
						else
							Result.Append(" object");

						Result.AppendLine(" failed.");
					}
				}

				if (Overwrite)
				{
					Result.AppendLine();
					Result.Append("Click on the Next button to continue.");
				}

				ClientEvents.PushEvent(new string[] { TabID }, "RestoreFinished", JSON.Encode(new Dictionary<string, object>()
				{
					{ "ok", true },
					{ "message", Result.ToString() }
				}, false), true);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				ShowStatus(TabID, "Failure: " + ex.Message);

				ClientEvents.PushEvent(new string[] { TabID }, "RestoreFinished", JSON.Encode(new Dictionary<string, object>()
				{
					{ "ok", false },
					{ "message", ex.Message }
				}, false), true);
			}
			finally
			{
				AesTransform1?.Dispose();
				AesTransform2?.Dispose();
				cs1?.Dispose();
				cs2?.Dispose();
				BackupFile?.Dispose();
				KeyFile?.Dispose();
			}
		}

		private static async Task<(ICryptoTransform, CryptoStream)> DoImport(TemporaryFile BackupFile, TemporaryFile KeyFile, string TabID,
			string Extension, ValidateBackupFile Import, bool Overwrite, bool OnlySelectedCollections, Array SelectedCollections)
		{
			ICryptoTransform AesTransform = null;
			CryptoStream cs = null;

			BackupFile.Position = 0;

			switch (Extension.ToLower())
			{
				case ".xml":
					await RestoreXml(BackupFile, TabID, Import, Overwrite, OnlySelectedCollections, SelectedCollections);
					break;

				case ".bin":
					await RestoreBinary(BackupFile, TabID, Import, Overwrite, OnlySelectedCollections, SelectedCollections);
					break;

				case ".gz":
					await RestoreCompressed(BackupFile, TabID, Import, Overwrite, OnlySelectedCollections, SelectedCollections);
					break;

				case ".bak":
					if (KeyFile is null)
						throw new Exception("No key file provided.");

					KeyFile.Position = 0;

					(AesTransform, cs) = await RestoreEncrypted(BackupFile, KeyFile, TabID, Import, Overwrite, OnlySelectedCollections, SelectedCollections);
					break;

				default:
					throw new Exception("Unrecognized file extension: " + Extension);
			}

			return (AesTransform, cs);
		}

		private static async Task RestoreXml(Stream BackupFile, string TabID, ValidateBackupFile Import, bool Overwrite,
			bool OnlySelectedCollections, Array SelectedCollections)
		{
			XmlReaderSettings Settings = new XmlReaderSettings()
			{
				Async = true,
				CloseInput = true,
				ConformanceLevel = ConformanceLevel.Document,
				CheckCharacters = true,
				DtdProcessing = DtdProcessing.Prohibit,
				IgnoreComments = true,
				IgnoreProcessingInstructions = true,
				IgnoreWhitespace = true
			};
			XmlReader r = XmlReader.Create(BackupFile, Settings);
			DateTime LastReport = DateTime.Now;
			bool ImportCollection = !OnlySelectedCollections;
			bool DatabaseStarted = false;
			bool CollectionStarted = false;
			bool IndexStarted = false;
			bool ObjectStarted = false;
			bool FilesStarted = false;
			bool FirstFile = true;

			if (!r.ReadToFollowing("Export", Export.ExportNamepace))
				throw new Exception("Invalid backup XML file.");

			await Import.Start();

			while (await r.ReadAsync())
			{
				if (r.IsStartElement())
				{
					switch (r.LocalName)
					{
						case "Database":
							if (r.Depth != 1)
								throw new Exception("Database element not expected.");

							if (Overwrite)
								ShowStatus(TabID, "Restoring database section.");
							else
								ShowStatus(TabID, "Validating database section.");

							await Import.StartExport();
							DatabaseStarted = true;
							break;

						case "Collection":
							if (r.Depth != 2 || !DatabaseStarted)
								throw new Exception("Collection element not expected.");

							if (!r.MoveToAttribute("name"))
								throw new Exception("Collection name missing.");

							string CollectionName = r.Value;

							if (ObjectStarted)
							{
								await Import.EndObject();
								ObjectStarted = false;
							}

							if (IndexStarted)
							{
								await Import.EndIndex();
								IndexStarted = false;
							}

							if (CollectionStarted)
								await Import.EndCollection();

							if (OnlySelectedCollections)
								ImportCollection = Array.IndexOf(SelectedCollections, CollectionName) >= 0;

							if (ImportCollection)
							{
								await Import.StartCollection(CollectionName);
								CollectionStarted = true;

								CollectionFound(TabID, CollectionName);
							}
							else
								CollectionStarted = false;
							break;

						case "Index":
							if (r.Depth != 3 || !CollectionStarted)
								throw new Exception("Index element not expected.");

							if (ImportCollection)
							{
								await Import.StartIndex();
								IndexStarted = true;
							}
							break;

						case "Field":
							if (r.Depth != 4 || !IndexStarted)
								throw new Exception("Field element not expected.");

							if (r.MoveToFirstAttribute())
							{
								string FieldName = null;
								bool Ascending = true;

								do
								{
									switch (r.LocalName)
									{
										case "name":
											FieldName = r.Value;
											break;

										case "ascending":
											if (!CommonTypes.TryParse(r.Value, out Ascending))
												throw new Exception("Invalid boolean value.");
											break;

										case "xmlns":
											break;

										default:
											throw new Exception("Unexpected attribute: " + r.LocalName);
									}
								}
								while (r.MoveToNextAttribute());

								if (string.IsNullOrEmpty(FieldName))
									throw new Exception("Invalid field name.");

								if (ImportCollection)
									await Import.ReportIndexField(FieldName, Ascending);
							}
							else
								throw new Exception("Field attributes expected.");

							break;

						case "Obj":
							if (r.Depth != 3 || !CollectionStarted)
								throw new Exception("Obj element not expected.");

							if (IndexStarted)
							{
								await Import.EndIndex();
								IndexStarted = false;
							}

							using (XmlReader r2 = r.ReadSubtree())
							{
								await r2.ReadAsync();

								if (!r2.MoveToFirstAttribute())
									throw new Exception("Object attributes missing.");

								if (ObjectStarted)
								{
									await Import.EndObject();
									ObjectStarted = false;
								}

								string ObjectId = null;
								string TypeName = string.Empty;

								do
								{
									switch (r2.LocalName)
									{
										case "id":
											ObjectId = r2.Value;
											break;

										case "type":
											TypeName = r2.Value;
											break;

										case "xmlns":
											break;

										default:
											throw new Exception("Unexpected attribute: " + r2.LocalName);
									}
								}
								while (r2.MoveToNextAttribute());

								if (ImportCollection)
								{
									await Import.StartObject(ObjectId, TypeName);
									ObjectStarted = true;
								}

								while (await r2.ReadAsync())
								{
									if (r2.IsStartElement())
									{
										KeyValuePair<string, object> P = await ReadValue(r2);

										if (ImportCollection)
											await Import.ReportProperty(P.Key, P.Value);
									}
								}
							}

							if (ImportCollection)
								await Import.EndObject();
							break;

						case "Files":
							if (r.Depth != 1)
								throw new Exception("Files element not expected.");

							if (ObjectStarted)
							{
								await Import.EndObject();
								ObjectStarted = false;
							}

							if (IndexStarted)
							{
								await Import.EndIndex();
								IndexStarted = false;
							}

							if (CollectionStarted)
							{
								await Import.EndCollection();
								CollectionStarted = false;
							}

							if (DatabaseStarted)
							{
								await Import.EndExport();
								DatabaseStarted = false;
							}

							if (Overwrite)
								ShowStatus(TabID, "Restoring files section.");
							else
								ShowStatus(TabID, "Validating files section.");

							await Import.StartFiles();
							FilesStarted = true;
							break;

						case "File":
							if (r.Depth != 2 || !FilesStarted)
								throw new Exception("File element not expected.");

							using (XmlReader r2 = r.ReadSubtree())
							{
								await r2.ReadAsync();

								if (!r2.MoveToAttribute("fileName"))
									throw new Exception("File name missing.");

								string FileName = r.Value;

								if (Path.IsPathRooted(FileName))
								{
									if (FileName.StartsWith(Gateway.AppDataFolder))
										FileName = FileName.Substring(Gateway.AppDataFolder.Length);
									else
										throw new Exception("Absolute path names not allowed: " + FileName);
								}

								FileName = Path.Combine(Gateway.AppDataFolder, FileName);

								using (TemporaryFile fs = new TemporaryFile())
								{
									while (await r2.ReadAsync())
									{
										if (r2.IsStartElement())
										{
											while (r2.LocalName == "Chunk")
											{
												string Base64 = await r2.ReadElementContentAsStringAsync();
												byte[] Data = Convert.FromBase64String(Base64);
												fs.Write(Data, 0, Data.Length);
											}
										}
									}

									fs.Position = 0;
									if (FirstFile && FileName.EndsWith("Gateway.config", StringComparison.CurrentCultureIgnoreCase))
										ImportGatewayConfig(fs);
									else
										await Import.ExportFile(FileName, fs);

									FirstFile = false;
								}
							}
							break;

						default:
							throw new Exception("Unexpected element: " + r.LocalName);
					}
				}

				ShowReport(TabID, Import, ref LastReport, Overwrite);
			}

			if (ObjectStarted)
				await Import.EndObject();

			if (IndexStarted)
				await Import.EndIndex();

			if (CollectionStarted)
				await Import.EndCollection();

			if (DatabaseStarted)
				await Import.EndExport();

			if (FilesStarted)
				await Import.EndFiles();

			await Import.End();
			ShowReport(TabID, Import, Overwrite);
		}

		private static void ShowReport(string TabID, ValidateBackupFile Import, ref DateTime LastReport, bool Overwrite)
		{
			DateTime Now = DateTime.Now;
			if ((Now - LastReport).TotalSeconds >= 1)
			{
				LastReport = Now;
				ShowReport(TabID, Import, Overwrite);
			}
		}

		private static void ShowReport(string TabID, ValidateBackupFile Import, bool Overwrite)
		{
			string Suffix = Overwrite ? "2" : "1";

			if (Import.NrCollections > 0)
				ShowStatus(TabID, "NrCollections" + Suffix, Import.NrCollections.ToString() + " collections.");

			if (Import.NrIndices > 0)
				ShowStatus(TabID, "NrIndices" + Suffix, Import.NrIndices.ToString() + " indices.");

			if (Import.NrObjects > 0)
				ShowStatus(TabID, "NrObjects" + Suffix, Import.NrObjects.ToString() + " objects.");

			if (Import.NrProperties > 0)
				ShowStatus(TabID, "NrProperties" + Suffix, Import.NrProperties.ToString() + " properties.");

			if (Import.NrFiles > 0)
				ShowStatus(TabID, "NrFiles" + Suffix, Import.NrFiles.ToString() + " files (" + Export.FormatBytes(Import.NrFileBytes) + ").");

			if (Import is RestoreBackupFile Restore && Restore.NrObjectsFailed > 0)
				ShowStatus(TabID, "NrFailed" + Suffix, Restore.NrObjectsFailed.ToString() + " objects failed.");
		}

		private static async Task<KeyValuePair<string, object>> ReadValue(XmlReader r)
		{
			string PropertyType = r.LocalName;
			bool ReadSubtree = (PropertyType == "Bin" || PropertyType == "Array" || PropertyType == "Obj");

			if (ReadSubtree)
			{
				r = r.ReadSubtree();
				await r.ReadAsync();
			}

			if (!r.MoveToFirstAttribute())
			{
				if (ReadSubtree)
					r.Dispose();

				throw new Exception("Property attributes missing.");
			}

			string ElementType = null;
			string PropertyName = null;
			object Value = null;

			do
			{
				switch (r.LocalName)
				{
					case "n":
						PropertyName = r.Value;
						break;

					case "v":
						switch (PropertyType)
						{
							case "S":
							case "En":
								Value = r.Value;
								break;

							case "S64":
								Value = Encoding.UTF8.GetString(Convert.FromBase64String(r.Value));
								break;

							case "Null":
								Value = null;
								break;

							case "Bl":
								if (CommonTypes.TryParse(r.Value, out bool bl))
									Value = bl;
								else
								{
									if (ReadSubtree)
										r.Dispose();

									throw new Exception("Invalid boolean value.");
								}
								break;

							case "B":
								if (byte.TryParse(r.Value, out byte b))
									Value = b;
								else
								{
									if (ReadSubtree)
										r.Dispose();

									throw new Exception("Invalid byte value.");
								}
								break;

							case "Ch":
								string s = r.Value;
								if (s.Length == 1)
									Value = s[0];
								else
								{
									if (ReadSubtree)
										r.Dispose();

									throw new Exception("Invalid character value.");
								}
								break;

							case "CIS":
								Value = new CaseInsensitiveString(r.Value);
								break;

							case "CIS64":
								Value = new CaseInsensitiveString(Encoding.UTF8.GetString(Convert.FromBase64String(r.Value)));
								break;

							case "DT":
								if (XML.TryParse(r.Value, out DateTime DT))
									Value = DT;
								else
								{
									if (ReadSubtree)
										r.Dispose();

									throw new Exception("Invalid DateTime value.");
								}
								break;

							case "DTO":
								if (XML.TryParse(r.Value, out DateTimeOffset DTO))
									Value = DTO;
								else
								{
									if (ReadSubtree)
										r.Dispose();

									throw new Exception("Invalid DateTimeOffset value.");
								}
								break;

							case "Dc":
								if (CommonTypes.TryParse(r.Value, out decimal dc))
									Value = dc;
								else
								{
									if (ReadSubtree)
										r.Dispose();

									throw new Exception("Invalid Decimal value.");
								}
								break;

							case "Db":
								if (CommonTypes.TryParse(r.Value, out double db))
									Value = db;
								else
								{
									if (ReadSubtree)
										r.Dispose();

									throw new Exception("Invalid Double value.");
								}
								break;

							case "I2":
								if (short.TryParse(r.Value, out short i2))
									Value = i2;
								else
								{
									if (ReadSubtree)
										r.Dispose();

									throw new Exception("Invalid Int16 value.");
								}
								break;

							case "I4":
								if (int.TryParse(r.Value, out int i4))
									Value = i4;
								else
								{
									if (ReadSubtree)
										r.Dispose();

									throw new Exception("Invalid Int32 value.");
								}
								break;

							case "I8":
								if (long.TryParse(r.Value, out long i8))
									Value = i8;
								else
								{
									if (ReadSubtree)
										r.Dispose();

									throw new Exception("Invalid Int64 value.");
								}
								break;

							case "I1":
								if (sbyte.TryParse(r.Value, out sbyte i1))
									Value = i1;
								else
								{
									if (ReadSubtree)
										r.Dispose();

									throw new Exception("Invalid SByte value.");
								}
								break;

							case "Fl":
								if (CommonTypes.TryParse(r.Value, out float fl))
									Value = fl;
								else
								{
									if (ReadSubtree)
										r.Dispose();

									throw new Exception("Invalid Single value.");
								}
								break;

							case "U2":
								if (ushort.TryParse(r.Value, out ushort u2))
									Value = u2;
								else
								{
									if (ReadSubtree)
										r.Dispose();

									throw new Exception("Invalid UInt16 value.");
								}
								break;

							case "U4":
								if (uint.TryParse(r.Value, out uint u4))
									Value = u4;
								else
								{
									if (ReadSubtree)
										r.Dispose();

									throw new Exception("Invalid UInt32 value.");
								}
								break;

							case "U8":
								if (ulong.TryParse(r.Value, out ulong u8))
									Value = u8;
								else
								{
									if (ReadSubtree)
										r.Dispose();

									throw new Exception("Invalid UInt64 value.");
								}
								break;

							case "TS":
								if (TimeSpan.TryParse(r.Value, out TimeSpan TS))
									Value = TS;
								else
								{
									if (ReadSubtree)
										r.Dispose();

									throw new Exception("Invalid TimeSpan value.");
								}
								break;

							case "Bin":
								if (ReadSubtree)
									r.Dispose();

								throw new Exception("Binary member values are reported using child elements.");

							case "ID":
								if (Guid.TryParse(r.Value, out Guid Id))
									Value = Id;
								else
								{
									if (ReadSubtree)
										r.Dispose();

									throw new Exception("Invalid GUID value.");
								}
								break;

							case "Array":
								if (ReadSubtree)
									r.Dispose();

								throw new Exception("Arrays report values as child elements.");

							case "Obj":
								if (ReadSubtree)
									r.Dispose();

								throw new Exception("Objects report member values as child elements.");

							default:
								if (ReadSubtree)
									r.Dispose();

								throw new Exception("Unexpected property type: " + PropertyType);
						}
						break;

					case "elementType":
					case "type":
						ElementType = r.Value;
						break;

					case "xmlns":
						break;

					default:
						if (ReadSubtree)
							r.Dispose();

						throw new Exception("Unexpected attribute: " + r.LocalName);
				}
			}
			while (r.MoveToNextAttribute());

			if (ElementType != null)
			{
				switch (PropertyType)
				{
					case "Array":
						List<object> List = new List<object>();

						while (await r.ReadAsync())
						{
							if (r.IsStartElement())
							{
								KeyValuePair<string, object> P = await ReadValue(r);
								if (!string.IsNullOrEmpty(P.Key))
								{
									if (ReadSubtree)
										r.Dispose();

									throw new Exception("Arrays do not contain property names.");
								}

								List.Add(P.Value);
							}
							else if (r.NodeType == XmlNodeType.EndElement)
								break;
						}

						Value = List.ToArray();
						break;

					case "Obj":
						GenericObject GenObj = new GenericObject(string.Empty, ElementType, Guid.Empty);
						Value = GenObj;

						while (await r.ReadAsync())
						{
							if (r.IsStartElement())
							{
								KeyValuePair<string, object> P = await ReadValue(r);
								GenObj[P.Key] = P.Value;
							}
							else if (r.NodeType == XmlNodeType.EndElement)
								break;
						}
						break;

					default:
						if (ReadSubtree)
							r.Dispose();

						throw new Exception("Type only valid option for arrays and objects.");
				}
			}
			else if (PropertyType == "Bin")
			{
				MemoryStream Bin = new MemoryStream();

				while (await r.ReadAsync())
				{
					if (r.IsStartElement())
					{
						try
						{
							while (r.LocalName == "Chunk")
							{
								string Base64 = await r.ReadElementContentAsStringAsync();
								byte[] Data = Convert.FromBase64String(Base64);
								Bin.Write(Data, 0, Data.Length);
							}
						}
						catch (Exception ex)
						{
							if (ReadSubtree)
								r.Dispose();

							System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
						}
					}
					else if (r.NodeType == XmlNodeType.EndElement)
						break;
				}

				Value = Bin.ToArray();
			}

			if (ReadSubtree)
				r.Dispose();

			return new KeyValuePair<string, object>(PropertyName, Value);
		}

		private static async Task RestoreBinary(Stream BackupFile, string TabID, ValidateBackupFile Import, bool Overwrite,
			bool OnlySelectedCollections, Array SelectedCollections)
		{
			int Version = BackupFile.ReadByte();
			if (Version != 1)
				throw new Exception("File version not supported.");

			DateTime LastReport = DateTime.Now;
			byte Command;
			bool ImportCollection = !OnlySelectedCollections;

			using (BinaryReader r = new BinaryReader(BackupFile, Encoding.UTF8, true))
			{
				string s = r.ReadString();
				if (s != BinaryExportFormat.Preamble)
					throw new Exception("Invalid backup file.");

				await Import.Start();

				while ((Command = r.ReadByte()) != 0)
				{
					switch (Command)
					{
						case 2: // Database
							string CollectionName;
							string ObjectId;
							string TypeName;
							string FieldName;
							bool Ascending;

							if (Overwrite)
								ShowStatus(TabID, "Restoring database section.");
							else
								ShowStatus(TabID, "Validating database section.");

							await Import.StartExport();

							while (!string.IsNullOrEmpty(CollectionName = r.ReadString()))
							{
								if (OnlySelectedCollections)
									ImportCollection = Array.IndexOf(SelectedCollections, CollectionName) >= 0;

								if (ImportCollection)
								{
									await Import.StartCollection(CollectionName);
									CollectionFound(TabID, CollectionName);
								}

								byte b;

								while ((b = r.ReadByte()) != 0)
								{
									switch (b)
									{
										case 1:
											if (ImportCollection)
												await Import.StartIndex();

											while (!string.IsNullOrEmpty(FieldName = r.ReadString()))
											{
												Ascending = r.ReadBoolean();
											
												if (ImportCollection)
													await Import.ReportIndexField(FieldName, Ascending);
											}

											if (ImportCollection)
												await Import.EndIndex();
											break;

										case 2:
											ObjectId = r.ReadString();
											TypeName = r.ReadString();

											if (ImportCollection)
												await Import.StartObject(ObjectId, TypeName);

											byte PropertyType = r.ReadByte();
											string PropertyName = r.ReadString();
											object PropertyValue;

											while (!string.IsNullOrEmpty(PropertyName))
											{
												PropertyValue = ReadValue(r, PropertyType);

												if (ImportCollection)
													await Import.ReportProperty(PropertyName, PropertyValue);

												PropertyType = r.ReadByte();
												PropertyName = r.ReadString();
											}

											if (ImportCollection)
												await Import.EndObject();
											break;

										default:
											throw new Exception("Unsupported collection section: " + b.ToString());
									}

									ShowReport(TabID, Import, ref LastReport, Overwrite);
								}

								if (ImportCollection)
									await Import.EndCollection();
							}

							await Import.EndExport();
							break;

						case 3: // Files
							string FileName;
							int MaxLen = 256 * 1024;
							byte[] Buffer = new byte[MaxLen];

							if (Overwrite)
								ShowStatus(TabID, "Restoring files section.");
							else
								ShowStatus(TabID, "Validating files section.");

							await Import.StartFiles();

							bool FirstFile = true;

							while (!string.IsNullOrEmpty(FileName = r.ReadString()))
							{
								long Length = r.ReadInt64();

								if (Path.IsPathRooted(FileName))
								{
									if (FileName.StartsWith(Gateway.AppDataFolder))
										FileName = FileName.Substring(Gateway.AppDataFolder.Length);
									else
										throw new Exception("Absolute path names not allowed: " + FileName);
								}

								FileName = Path.Combine(Gateway.AppDataFolder, FileName);

								using (TemporaryFile File = new TemporaryFile())
								{
									while (Length > 0)
									{
										int Nr = r.Read(Buffer, 0, (int)Math.Min(Length, MaxLen));
										Length -= Nr;
										await File.WriteAsync(Buffer, 0, Nr);
									}

									File.Position = 0;
									try
									{
										if (FirstFile && FileName.EndsWith("Gateway.config", StringComparison.CurrentCultureIgnoreCase))
											ImportGatewayConfig(File);
										else
											await Import.ExportFile(FileName, File);

										ShowReport(TabID, Import, ref LastReport, Overwrite);
									}
									catch (Exception ex)
									{
										ShowStatus(TabID, "Unable to restore " + FileName + ": " + ex.Message);
									}

									FirstFile = false;
								}
							}

							await Import.EndFiles();
							break;

						default:
							throw new Exception("Unsupported section: " + Command.ToString());
					}
				}

				await Import.End();
				ShowReport(TabID, Import, Overwrite);
			}
		}

		private static void ImportGatewayConfig(Stream File)
		{
			XmlDocument Doc = new XmlDocument()
			{
				PreserveWhitespace = true
			};
			Doc.Load(File);

			string OriginalFileName = Path.Combine(Gateway.AppDataFolder, "Gateway.config");
			XmlDocument Original = new XmlDocument()
			{
				PreserveWhitespace = true
			};
			Original.Load(OriginalFileName);

			if (Doc.DocumentElement != null && Doc.DocumentElement.LocalName == "GatewayConfiguration")
			{
				foreach (XmlNode N in Doc.DocumentElement.ChildNodes)
				{
					if (N is XmlElement E)
					{
						switch (E.LocalName)
						{
							case "ApplicationName":
								string s = E.InnerText;
								Gateway.ApplicationName = s;
								Original.DocumentElement["ApplicationName"].InnerText = s;
								break;

							case "DefaultPage":
								s = E.InnerText;
								Gateway.DefaultPage = s;
								Original.DocumentElement["DefaultPage"].InnerText = s;
								break;

								// TODO: Ports ?
								// TODO: FileFolders ?
						}
					}
				}
			}

			Original.Save(OriginalFileName);
		}

		private static object ReadValue(BinaryReader r, byte PropertyType)
		{
			switch (PropertyType)
			{
				case BinaryExportFormat.TYPE_BOOLEAN: return r.ReadBoolean();
				case BinaryExportFormat.TYPE_BYTE: return r.ReadByte();
				case BinaryExportFormat.TYPE_INT16: return r.ReadInt16();
				case BinaryExportFormat.TYPE_INT32: return r.ReadInt32();
				case BinaryExportFormat.TYPE_INT64: return r.ReadInt64();
				case BinaryExportFormat.TYPE_SBYTE: return r.ReadSByte();
				case BinaryExportFormat.TYPE_UINT16: return r.ReadUInt16();
				case BinaryExportFormat.TYPE_UINT32: return r.ReadUInt32();
				case BinaryExportFormat.TYPE_UINT64: return r.ReadUInt64();
				case BinaryExportFormat.TYPE_DECIMAL: return r.ReadDecimal();
				case BinaryExportFormat.TYPE_DOUBLE: return r.ReadDouble();
				case BinaryExportFormat.TYPE_SINGLE: return r.ReadSingle();
				case BinaryExportFormat.TYPE_CHAR: return r.ReadChar();
				case BinaryExportFormat.TYPE_STRING: return r.ReadString();
				case BinaryExportFormat.TYPE_ENUM: return r.ReadString();
				case BinaryExportFormat.TYPE_NULL: return null;

				case BinaryExportFormat.TYPE_DATETIME:
					DateTimeKind Kind = (DateTimeKind)((int)r.ReadByte());
					long Ticks = r.ReadInt64();
					return new DateTime(Ticks, Kind);

				case BinaryExportFormat.TYPE_TIMESPAN:
					Ticks = r.ReadInt64();
					return new TimeSpan(Ticks);

				case BinaryExportFormat.TYPE_BYTEARRAY:
					int Count = r.ReadInt32();
					return r.ReadBytes(Count);

				case BinaryExportFormat.TYPE_GUID:
					byte[] Bin = r.ReadBytes(16);
					return new Guid(Bin);

				case BinaryExportFormat.TYPE_DATETIMEOFFSET:
					Kind = (DateTimeKind)((int)r.ReadByte());
					Ticks = r.ReadInt64();
					DateTime DT = new DateTime(Ticks, Kind);
					Ticks = r.ReadInt64();
					Ticks -= Ticks % 600000000;	// Offsets must be in whole minutes.
					TimeSpan TS = new TimeSpan(Ticks);
					return new DateTimeOffset(DT, TS);

				case BinaryExportFormat.TYPE_CI_STRING:
					return new CaseInsensitiveString(r.ReadString());

				case BinaryExportFormat.TYPE_ARRAY:
					r.ReadString(); // Type name
					long NrElements = r.ReadInt64();

					List<object> List = new List<object>();

					while (NrElements > 0)
					{
						NrElements--;
						PropertyType = r.ReadByte();
						List.Add(ReadValue(r, PropertyType));
					}

					return List.ToArray();

				case BinaryExportFormat.TYPE_OBJECT:
					string TypeName = r.ReadString();
					GenericObject Object = new GenericObject(string.Empty, TypeName, Guid.Empty);

					PropertyType = r.ReadByte();
					string PropertyName = r.ReadString();

					while (!string.IsNullOrEmpty(PropertyName))
					{
						Object[PropertyName] = ReadValue(r, PropertyType);

						PropertyType = r.ReadByte();
						PropertyName = r.ReadString();
					}

					return Object;

				default:
					throw new Exception("Unsupported property type: " + PropertyType.ToString());
			}
		}

		private static async Task RestoreCompressed(Stream BackupFile, string TabID, ValidateBackupFile Import, bool Overwrite,
			bool OnlySelectedCollections, Array SelectedCollections)
		{
			using (GZipStream gz = new GZipStream(BackupFile, CompressionMode.Decompress, true))
			{
				await RestoreBinary(gz, TabID, Import, Overwrite, OnlySelectedCollections, SelectedCollections);
			}
		}

		private static async Task<(ICryptoTransform, CryptoStream)> RestoreEncrypted(Stream BackupFile, Stream KeyFile, string TabID, ValidateBackupFile Import,
			bool Overwrite, bool OnlySelectedCollections, Array SelectedCollections)
		{
			XmlDocument Doc = new XmlDocument()
			{
				PreserveWhitespace = true
			};

			try
			{
				Doc.Load(KeyFile);
			}
			catch (Exception)
			{
				throw new Exception("Invalid key file.");
			}

			XmlElement KeyAes256 = Doc.DocumentElement;
			if (KeyAes256.LocalName != "KeyAes256" ||
				KeyAes256.NamespaceURI != Export.ExportNamepace ||
				!KeyAes256.HasAttribute("key") ||
				!KeyAes256.HasAttribute("iv"))
			{
				throw new Exception("Invalid key file.");
			}

			byte[] Key = Convert.FromBase64String(KeyAes256.Attributes["key"].Value);
			byte[] IV = Convert.FromBase64String(KeyAes256.Attributes["iv"].Value);

			ICryptoTransform AesTransform = WebResources.StartExport.aes.CreateDecryptor(Key, IV);
			CryptoStream cs = new CryptoStream(BackupFile, AesTransform, CryptoStreamMode.Read);

			await RestoreCompressed(cs, TabID, Import, Overwrite, OnlySelectedCollections, SelectedCollections);

			return (AesTransform, cs);
		}

		private static async Task DoAnalyze(string TabID)
		{
			ShowStatus(TabID, "Analyzing database.");

			string XmlPath = Path.Combine(Gateway.AppDataFolder, "Restore.xml");
			string HtmlPath = Path.Combine(Gateway.AppDataFolder, "Restore.html");
			string XsltPath = Path.Combine(Gateway.AppDataFolder, "Transforms", "DbStatXmlToHtml.xslt");
			using (FileStream fs = File.Create(XmlPath))
			{
				XmlWriterSettings Settings = XML.WriterSettings(true, false);
				using (XmlWriter w = XmlWriter.Create(fs, Settings))
				{
					await Database.Analyze(w, XsltPath, Gateway.AppDataFolder, false);
					w.Flush();
					fs.Flush();
				}
			}

			XslCompiledTransform Xslt = XSL.LoadTransform(typeof(Gateway).Namespace + ".Transforms.DbStatXmlToHtml.xslt");

			string s = File.ReadAllText(XmlPath);
			s = XSL.Transform(s, Xslt);
			byte[] Bin = WebResources.StartAnalyze.utf8Bom.GetBytes(s);

			File.WriteAllBytes(HtmlPath, Bin);

			ShowStatus(TabID, "Database analysis successfully completed.");

			/*
			int i = s.IndexOf("<body>");
			if (i > 0)
				s = s.Substring(i + 6);

			i = s.IndexOf("</body>");
			if (i > 0)
				s = s.Substring(0, i);

			ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", JSON.Encode(new Dictionary<string, object>()
			{
				{ "html", s },
			}, false), true);
			*/
		}

		/// <summary>
		/// Sets the configuration task as completed.
		/// </summary>
		public override Task MakeCompleted()
		{
			return this.MakeCompleted(this.reloadConfiguration);
		}

		/// <summary>
		/// Simplified configuration by configuring simple default values.
		/// </summary>
		/// <returns>If the configuration was changed.</returns>
		public override Task<bool> SimplifiedConfiguration()
		{
			return Task.FromResult<bool>(true);
		}

	}
}
