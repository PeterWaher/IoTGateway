using System;
using System.Collections.Generic;
using System.IO;

namespace Waher.Content.SystemFiles
{
	/// <summary>
	/// Static class helping modules to find files installed on the system.
	/// </summary>
	public static class FileSystem
	{
		/// <summary>
		/// Finds files in a set of folders, and optionally, their subfolders. This method only finds files in folders
		/// the application as access rights to.
		/// </summary>
		/// <param name="Folders">Folders to search in.</param>
		/// <param name="Pattern">Pattern to search for.</param>
		/// <param name="IncludeSubfolders">If subfolders are to be included.</param>
		/// <param name="BreakOnFirst">If the search should break when it finds the first file.</param>
		/// <returns>Files matching the pattern found in the corresponding folders.</returns>
		public static string[] FindFiles(Environment.SpecialFolder[] Folders, string Pattern, bool IncludeSubfolders, bool BreakOnFirst)
		{
			return FindFiles(GetFolders(Folders), Pattern, IncludeSubfolders, BreakOnFirst ? 1 : int.MaxValue);
		}

		/// <summary>
		/// Finds files in a set of folders, and optionally, their subfolders. This method only finds files in folders
		/// the application as access rights to.
		/// </summary>
		/// <param name="Folders">Folders to search in.</param>
		/// <param name="Pattern">Pattern to search for.</param>
		/// <param name="IncludeSubfolders">If subfolders are to be included.</param>
		/// <param name="BreakOnFirst">If the search should break when it finds the first file.</param>
		/// <returns>Files matching the pattern found in the corresponding folders.</returns>
		public static string[] FindFiles(string[] Folders, string Pattern, bool IncludeSubfolders, bool BreakOnFirst)
		{
			return FindFiles(Folders, Pattern, IncludeSubfolders, BreakOnFirst ? 1 : int.MaxValue);
		}

		/// <summary>
		/// Finds files in a set of folders, and optionally, their subfolders. This method only finds files in folders
		/// the application as access rights to.
		/// </summary>
		/// <param name="Folders">Folders to search in.</param>
		/// <param name="Pattern">Pattern to search for.</param>
		/// <param name="IncludeSubfolders">If subfolders are to be included.</param>
		/// <param name="MaxCount">Maximum number of files to return.</param>
		/// <returns>Files matching the pattern found in the corresponding folders.</returns>
		public static string[] FindFiles(string[] Folders, string Pattern, bool IncludeSubfolders, int MaxCount)
		{
			return FindFiles(Folders, Pattern, IncludeSubfolders ? int.MaxValue : 0, MaxCount);
		}

		/// <summary>
		/// Finds files in a set of folders, and optionally, their subfolders. This method only finds files in folders
		/// the application as access rights to.
		/// </summary>
		/// <param name="Folders">Folders to search in.</param>
		/// <param name="Pattern">Pattern to search for.</param>
		/// <param name="SubfolderDepth">Maximum folder depth to search.</param>
		/// <param name="MaxCount">Maximum number of files to return.</param>
		/// <returns>Files matching the pattern found in the corresponding folders.</returns>
		public static string[] FindFiles(string[] Folders, string Pattern, int SubfolderDepth, int MaxCount)
		{
			if (MaxCount <= 0)
				throw new ArgumentException("Must be positive.", nameof(MaxCount));

			LinkedList<KeyValuePair<string, int>> ToProcess = new LinkedList<KeyValuePair<string, int>>();
			Dictionary<string, bool> Processed = new Dictionary<string, bool>(StringComparer.CurrentCultureIgnoreCase);
			List<string> Result = new List<string>();
			int Count = 0;

			foreach (string Folder in Folders)
				ToProcess.AddLast(new KeyValuePair<string, int>(Folder, SubfolderDepth));

			while (!(ToProcess.First is null))
			{
				KeyValuePair<string, int> Processing = ToProcess.First.Value;
				string Folder = Processing.Key;
				int Depth = Processing.Value;

				ToProcess.RemoveFirst();
				if (Processed.ContainsKey(Folder))
					continue;

				Processed[Folder] = true;

				try
				{
					string[] Names = Directory.GetFiles(Folder, Pattern, SearchOption.TopDirectoryOnly);

					foreach (string FileName in Names)
					{
						Result.Add(FileName);
						if (++Count >= MaxCount)
							return Result.ToArray();
					}

					if (Depth-- > 0)
					{
						Names = Directory.GetDirectories(Folder);

						foreach (string SubFolder in Names)
							ToProcess.AddLast(new KeyValuePair<string, int>(SubFolder, Depth));
					}
				}
				catch (Exception)
				{
					// Ignore
				}
			}

			return Result.ToArray();
		}

		/// <summary>
		/// Gets the physical locations of special folders.
		/// </summary>
		/// <param name="Folders">Special folders.</param>
		/// <param name="AppendWith">Append result with this array of folders.</param>
		/// <returns>Physical locations. Only the physical locations of defined special folders are returned.</returns>
		public static string[] GetFolders(Environment.SpecialFolder[] Folders, params string[] AppendWith)
		{
			List<string> Result = new List<string>();

			foreach (Environment.SpecialFolder Folder in Folders)
			{
				string Path = Environment.GetFolderPath(Folder, Environment.SpecialFolderOption.None);
				if (!string.IsNullOrEmpty(Path))
				{
					// In 64-bit operating systems, the 32-bit folder can be returned anyway, if the process is running in 32 bit.

					if (Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess)
					{
						switch (Folder)
						{
							case Environment.SpecialFolder.CommonProgramFiles:
							case Environment.SpecialFolder.ProgramFiles:
							case Environment.SpecialFolder.System:
								if (Path.EndsWith(" (x86)"))
								{
									Path = Path.Substring(0, Path.Length - 6);
									if (!Directory.Exists(Path))
										continue;
								}
								break;
						}
					}

					Result.Add(Path);
				}
			}

			foreach (string Path in AppendWith)
				Result.Add(Path);

			return Result.ToArray();
		}

		/// <summary>
		/// Finds the latest file matching a search pattern, by searching in a set of folders, and optionally, their subfolders. 
		/// This method only finds files in folders the application as access rights to. If no file is found, the empty string
		/// is returned.
		/// </summary>
		/// <param name="Folders">Folders to search in.</param>
		/// <param name="Pattern">Pattern to search for.</param>
		/// <param name="IncludeSubfolders">If subfolders are to be included.</param>
		/// <returns>Latest file if found, empty string if not.</returns>
		public static string FindLatestFile(Environment.SpecialFolder[] Folders, string Pattern, bool IncludeSubfolders)
		{
			return FindLatestFile(GetFolders(Folders), Pattern, IncludeSubfolders);
		}

		/// <summary>
		/// Finds the latest file matching a search pattern, by searching in a set of folders, and optionally, their subfolders. 
		/// This method only finds files in folders the application as access rights to. If no file is found, the empty string
		/// is returned.
		/// </summary>
		/// <param name="Folders">Folders to search in.</param>
		/// <param name="Pattern">Pattern to search for.</param>
		/// <param name="IncludeSubfolders">If subfolders are to be included.</param>
		/// <returns>Latest file if found, empty string if not.</returns>
		public static string FindLatestFile(string[] Folders, string Pattern, bool IncludeSubfolders)
		{
			return FindLatestFile(Folders, Pattern, IncludeSubfolders ? int.MaxValue : 0);
		}

		/// <summary>
		/// Finds the latest file matching a search pattern, by searching in a set of folders, and optionally, their subfolders. 
		/// This method only finds files in folders the application as access rights to. If no file is found, the empty string
		/// is returned.
		/// </summary>
		/// <param name="Folders">Folders to search in.</param>
		/// <param name="Pattern">Pattern to search for.</param>
		/// <param name="SubfolderDepth">Maximum folder depth to search.</param>
		/// <returns>Latest file if found, empty string if not.</returns>
		public static string FindLatestFile(string[] Folders, string Pattern, int SubfolderDepth)
		{
			string[] Files = FindFiles(Folders, Pattern, SubfolderDepth, int.MaxValue);
			string Result = string.Empty;
			DateTime BestTP = DateTime.MinValue;
			DateTime TP;

			foreach (string FilePath in Files)
			{
				TP = File.GetCreationTimeUtc(FilePath);
				if (TP > BestTP)
				{
					BestTP = TP;
					Result = FilePath;
				}
			}

			return Result;
		}
	}
}
