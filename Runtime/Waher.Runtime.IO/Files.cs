using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;

namespace Waher.Runtime.IO
{
	/// <summary>
	/// Contains static methods
	/// </summary>
	public static class Files
	{
		/// <summary>
		/// Reads a binary file asynchronously.
		/// </summary>
		/// <param name="FileName">Filename.</param>
		/// <returns>Binary content.</returns>
		public static async Task<byte[]> ReadAllBytesAsync(string FileName)
		{
			using (FileStream fs = File.OpenRead(FileName))
			{
				return await fs.ReadAllAsync();
			}
		}

		/// <summary>
		/// Creates a binary file asynchronously.
		/// </summary>
		/// <param name="FileName">Filename.</param>
		/// <param name="Data">Binary data</param>
		public static Task WriteAllBytesAsync(string FileName, byte[] Data)
		{
			return WriteAllBytesAsync(FileName, Data, 0, Data.Length);
		}

		/// <summary>
		/// Creates a binary file asynchronously.
		/// </summary>
		/// <param name="FileName">Filename.</param>
		/// <param name="Data">Binary data</param>
		/// <param name="Offset">Offset into <paramref name="Data"/> to start.</param>
		/// <param name="Length">Number of bytes to write.</param>
		public static async Task WriteAllBytesAsync(string FileName, byte[] Data, int Offset, int Length)
		{
			using (FileStream fs = File.Create(FileName))
			{
				await fs.WriteAsync(Data, Offset, Length);
			}
		}

		/// <summary>
		/// Appends a binary file asynchronously.
		/// </summary>
		/// <param name="FileName">Filename.</param>
		/// <param name="Data">Binary data</param>
		public static Task AppendAllBytesAsync(string FileName, byte[] Data)
		{
			return AppendAllBytesAsync(FileName, Data, 0, Data.Length);
		}

		/// <summary>
		/// Appends a binary file asynchronously.
		/// </summary>
		/// <param name="FileName">Filename.</param>
		/// <param name="Data">Binary data</param>
		/// <param name="Offset">Offset into <paramref name="Data"/> to start.</param>
		/// <param name="Length">Number of bytes to write.</param>
		public static async Task AppendAllBytesAsync(string FileName, byte[] Data, int Offset, int Length)
		{
			using (FileStream fs = File.OpenWrite(FileName))
			{
				fs.Position = fs.Length;
				await fs.WriteAsync(Data, Offset, Length);
			}
		}

		/// <summary>
		/// Reads a text file asynchronously.
		/// </summary>
		/// <param name="FileName">Filename.</param>
		/// <returns>Decoded text.</returns>
		public static async Task<string> ReadAllTextAsync(string FileName)
		{
			byte[] Bin = await ReadAllBytesAsync(FileName);
			return Strings.GetString(Bin, Encoding.UTF8);
		}

		/// <summary>
		/// Creates a text file asynchronously.
		/// </summary>
		/// <param name="FileName">Filename.</param>
		/// <param name="Text">Text</param>
		public static Task WriteAllTextAsync(string FileName, string Text)
		{
			return WriteAllTextAsync(FileName, Text, Encoding.UTF8);
		}

		/// <summary>
		/// Creates a text file asynchronously.
		/// </summary>
		/// <param name="FileName">Filename.</param>
		/// <param name="Text">Text</param>
		/// <param name="Encoding">Encoding to use</param>
		public static async Task WriteAllTextAsync(string FileName, string Text, Encoding Encoding)
		{
			using (FileStream fs = File.Create(FileName))
			{
				byte[] Preamble = Encoding.GetPreamble();
				byte[] Data = Encoding.GetBytes(Text);
				int i, c = Preamble.Length;

				if (c > 0)
				{
					if (c > Data.Length)
						i = 0;
					else
					{
						for (i = 0; i < c; i++)
						{
							if (Preamble[i] != Data[i])
								break;
						}
					}

					if (i < c)
						await fs.WriteAsync(Preamble, 0, c);
				}

				await fs.WriteAsync(Data, 0, Data.Length);
			}
		}

		/// <summary>
		/// Deletes old files.
		/// </summary>
		/// <param name="FolderName">Folder to search for old files. Search is not recursive.</param>
		/// <param name="MaxAge">Maximum age.</param>
		public static void DeleteOldFiles(string FolderName, TimeSpan MaxAge)
		{
			DeleteOldFiles(FolderName, MaxAge, SearchOption.TopDirectoryOnly, false);
		}

		/// <summary>
		/// Deletes old files.
		/// </summary>
		/// <param name="FolderName">Folder to search for old files.</param>
		/// <param name="MaxAge">Maximum age.</param>
		/// <param name="SearchOption">Search options.</param>
		/// <param name="DeleteEmptySubFolders">If empty sub-folders should be deleted.</param>
		public static void DeleteOldFiles(string FolderName, TimeSpan MaxAge, SearchOption SearchOption, bool DeleteEmptySubFolders)
		{
			if (string.IsNullOrEmpty(FolderName))
				FolderName = ".";

			FolderName = Path.GetFullPath(FolderName);
			if (FolderName[FolderName.Length - 1] != Path.DirectorySeparatorChar)
				FolderName += Path.DirectorySeparatorChar;

			string[] Files = Directory.GetFiles(FolderName, "*.*", SearchOption);
			Dictionary<string, bool> Folders = new Dictionary<string, bool>();
			double MaxDays = MaxAge.TotalDays;

			foreach (string FileName in Files)
			{
				if ((DateTime.UtcNow - File.GetLastWriteTimeUtc(FileName)).TotalDays >= MaxDays)
				{
					try
					{
						File.Delete(FileName);
						Folders[Path.GetDirectoryName(FileName)] = true;
					}
					catch (IOException ex)
					{
						Log.Error("Unable to delete file: " + ex.Message, FileName);
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
					}
				}
			}

			if (DeleteEmptySubFolders && SearchOption != SearchOption.TopDirectoryOnly)
			{
				foreach (string Folder in Folders.Keys)
				{
					if (Folder.Length <= FolderName.Length)
						continue;

					try
					{
						IEnumerable<string> Entries = Directory.EnumerateFileSystemEntries(Folder);
						
						using (IEnumerator<string> Enumerator = Entries.GetEnumerator())
						{
							if (Enumerator.MoveNext())
								continue;

							try
							{
								Directory.Delete(Folder);
							}
							catch (IOException ex)
							{
								Log.Error("Unable to delete folder: " + ex.Message, Folder);
							}
						}
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
					}
				}
			}
		}
	}
}
