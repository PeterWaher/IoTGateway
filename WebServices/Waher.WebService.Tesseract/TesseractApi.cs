using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Events;
using Waher.Runtime.Inventory;
using Waher.Runtime.Temporary;
using Waher.Runtime.Timing;
using Waher.Security;

namespace Waher.WebService.Tesseract
{
	/// <summary>
	/// Class providing a web API for OCR using Tesseract, installed on the server.
	/// </summary>
	public class TesseractApi : IModule
	{
		private const string FolderPrefix = "Tesseract";
		private const string ExecutableName = "tesseract.exe";

		private static readonly Random rnd = new Random();
		private static Scheduler scheduler = null;
		private static bool disposeScheduler = false;
		private static string tesseractExe = null;
		private static string tesseractFolder = null;
		private static string imagesFolder = null;
		private static bool hasImagesFolder = false;

		/// <summary>
		/// Class providing a web API for OCR using Tesseract, installed on the server.
		/// </summary>
		public TesseractApi()
		{
		}

		/// <summary>
		/// Starts the module.
		/// </summary>
		public Task Start()
		{
			try
			{
				string ExeFolder = SearchForInstallationFolder();
				string ImagesFolder = null;

				if (Types.TryGetModuleParameter("AppData", out object Obj) && Obj is string AppDataFolder)
					ImagesFolder = Path.Combine(AppDataFolder, FolderPrefix);

				if (scheduler is null)
				{
					if (Types.TryGetModuleParameter("Scheduler", out Obj) && Obj is Scheduler Scheduler)
					{
						scheduler = Scheduler;
						disposeScheduler = false;
					}
					else
					{
						scheduler = new Scheduler();
						disposeScheduler = true;
					}
				}

				if (string.IsNullOrEmpty(ExeFolder))
					Log.Warning("Tesseract not found. Tesseract support will not be available via the Tesseract API.");
				else
				{
					SetInstallationPaths(ExeFolder, ImagesFolder);

					Log.Informational("Tesseract found. Tesseract API added.",
						new KeyValuePair<string, object>("Folder", ExeFolder));
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Stops the module.
		/// </summary>
		public Task Stop()
		{
			if (disposeScheduler)
			{
				scheduler?.Dispose();
				scheduler = null;
				disposeScheduler = false;
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Sets the installation folder of Tesseract.
		/// </summary>
		/// <param name="ExePath">Path to executable file.</param>
		/// <exception cref="Exception">If trying to set the installation folder to a different folder than the one set previously.
		/// The folder can only be set once, for security reasons.</exception>
		public static void SetInstallationPaths(string ExePath, string ImagesFolder)
		{
			if (!string.IsNullOrEmpty(tesseractExe) && ExePath != tesseractExe)
				throw new Exception("Tesseract executable path has already been set.");

			tesseractExe = ExePath;
			tesseractFolder = Path.GetDirectoryName(ExePath);
			imagesFolder = ImagesFolder;
			hasImagesFolder = !string.IsNullOrEmpty(imagesFolder);

			if (hasImagesFolder)
			{
				if (!Directory.Exists(imagesFolder))
					Directory.CreateDirectory(imagesFolder);

				DeleteOldFiles(TimeSpan.FromDays(7));
			}
		}

		private static void DeleteOldFiles(object P)
		{
			if (P is TimeSpan MaxAge)
				DeleteOldFiles(MaxAge, true);
		}

		/// <summary>
		/// Deletes generated files older than <paramref name="MaxAge"/>.
		/// </summary>
		/// <param name="MaxAge">Age limit.</param>
		/// <param name="Reschedule">If rescheduling should be done.</param>
		public static void DeleteOldFiles(TimeSpan MaxAge, bool Reschedule)
		{
			DateTime Limit = DateTime.Now - MaxAge;
			int Count = 0;

			foreach (string FileName in Directory.GetFiles(imagesFolder, "*.*"))
			{
				if (File.GetLastAccessTime(FileName) < Limit)
				{
					try
					{
						File.Delete(FileName);
						Count++;
					}
					catch (Exception ex)
					{
						Log.Error("Unable to delete old file: " + ex.Message, FileName);
					}
				}
			}

			if (Count > 0)
				Log.Informational(Count.ToString() + " old file(s) deleted.", imagesFolder);

			if (Reschedule)
			{
				lock (rnd)
				{
					scheduler.Add(DateTime.Now.AddDays(rnd.NextDouble() * 2), DeleteOldFiles, MaxAge);
				}
			}
		}

		/// <summary>
		/// Searches for the installation folder on the local machine.
		/// </summary>
		/// <returns>Installation folder, if found, null otherwise.</returns>
		public static string SearchForInstallationFolder()
		{
			string InstallationFolder;

			InstallationFolder = SearchForInstallationFolder(Environment.SpecialFolder.ProgramFilesX86);
			if (string.IsNullOrEmpty(InstallationFolder))
			{
				InstallationFolder = SearchForInstallationFolder(Environment.SpecialFolder.ProgramFiles);
				if (string.IsNullOrEmpty(InstallationFolder))
				{
					InstallationFolder = SearchForInstallationFolder(Environment.SpecialFolder.Programs);
					if (string.IsNullOrEmpty(InstallationFolder))
					{
						InstallationFolder = SearchForInstallationFolder(Environment.SpecialFolder.CommonProgramFilesX86);
						if (string.IsNullOrEmpty(InstallationFolder))
						{
							InstallationFolder = SearchForInstallationFolder(Environment.SpecialFolder.CommonProgramFiles);
							if (string.IsNullOrEmpty(InstallationFolder))
								InstallationFolder = SearchForInstallationFolder(Environment.SpecialFolder.CommonPrograms);
						}
					}
				}
			}

			return InstallationFolder;
		}

		private static string SearchForInstallationFolder(Environment.SpecialFolder SpecialFolder)
		{
			string Folder;

			try
			{
				Folder = Environment.GetFolderPath(SpecialFolder);
			}
			catch (Exception)
			{
				return null; // Folder not defined for the operating system.
			}

			if (String.IsNullOrEmpty(Folder))
				return null;

			if (!Directory.Exists(Folder))
				return null;

			string FolderName;
			string[] SubFolders;

			try
			{
				SubFolders = Directory.GetDirectories(Folder);
			}
			catch (UnauthorizedAccessException)
			{
				return null;
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				return null;
			}

			foreach (string SubFolder in SubFolders)
			{
				FolderName = Path.GetFileName(SubFolder);
				if (!FolderName.StartsWith(FolderPrefix, StringComparison.CurrentCultureIgnoreCase))
					continue;

				string ExePath = Path.Combine(FolderName, ExecutableName);
				if (File.Exists(ExePath))
					return ExePath;
			}

			return null;
		}

		private async Task<object> PerformOcr(byte[] Image, string ContentType, PageSegmentationMode? PageSegmentationMode,
			string Language)
		{
			string Extension = InternetContent.GetFileExtension(ContentType);
			TemporaryFile TempFile = null;
			string ResultFileName = null;
			string FileName;

			try
			{
				if (hasImagesFolder)
				{
					string Hash = Hashes.ComputeSHA256HashString(Image);
					FileName = Path.Combine(imagesFolder, Path.ChangeExtension(Hash, Extension));
				}
				else
				{
					TempFile = new TemporaryFile(Extension);
					await TempFile.WriteAsync(Image, 0, Image.Length);

					FileName = TempFile.FileName;
				}

				ResultFileName = Path.ChangeExtension(FileName, "txt");

				if (!(imagesFolder is null) && File.Exists(ResultFileName))
					return await Resources.ReadAllTextAsync(ResultFileName);
				else
				{
					if (!string.IsNullOrEmpty(imagesFolder))
						await Resources.WriteAllBytesAsync(FileName, Image);

					StringBuilder Arguments = new StringBuilder();

					Arguments.Append('"');
					Arguments.Append(FileName);
					Arguments.Append("\" \"");
					Arguments.Append(ResultFileName);
					Arguments.Append('"');

					if (!string.IsNullOrEmpty(Language))
					{
						Arguments.Append(" -l ");
						Arguments.Append(Language);
					}

					if (PageSegmentationMode.HasValue)
					{
						Arguments.Append(" -psm ");
						Arguments.Append(((int)PageSegmentationMode.Value).ToString());
					}

					ProcessStartInfo ProcessInformation = new ProcessStartInfo()
					{
						FileName = tesseractExe,
						Arguments = Arguments.ToString(),
						UseShellExecute = false,
						RedirectStandardError = true,
						RedirectStandardOutput = true,
						WorkingDirectory = tesseractFolder,
						CreateNoWindow = true,
						WindowStyle = ProcessWindowStyle.Hidden
					};

					Process P = new Process();
					TaskCompletionSource<string> ResultSource = new TaskCompletionSource<string>();

					P.ErrorDataReceived += (sender, e) =>
					{
						Log.Error("Unable to perform OCR: " + e.Data);
						ResultSource.TrySetResult(null);
					};

					P.Exited += async (sender, e) =>
					{
						try
						{
							if (P.ExitCode != 0)
							{
								Log.Error("Unable to perform OCR. Exit code: " + P.ExitCode.ToString());
								ResultSource.TrySetResult(null);
							}
							else
							{
								string Result = await Resources.ReadAllTextAsync(ResultFileName);
								ResultSource.TrySetResult(Result);
							}
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
						}
					};

					Task _ = Task.Delay(10000).ContinueWith(Prev => ResultSource.TrySetException(new TimeoutException("Tesseract process did not terminate properly.")));

					P.StartInfo = ProcessInformation;
					P.EnableRaisingEvents = true;
					P.Start();

					return await ResultSource.Task;
				}
			}
			finally
			{
				if (!(TempFile is null))
				{
					TempFile.Dispose();

					if (!string.IsNullOrEmpty(ResultFileName))
						File.Delete(ResultFileName);
				}
			}
		}
	}
}
