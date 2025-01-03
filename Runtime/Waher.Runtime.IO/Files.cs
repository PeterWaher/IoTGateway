using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;

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
		/// Loads a text resource from an embedded resource.
		/// </summary>
		/// <param name="ResourceName">Resource Name.</param>
		/// <returns>Text content.</returns>
		/// <exception cref="IOException">If Resource name is not valid or resource not found.</exception>
		public static string LoadResourceAsText(string ResourceName)
		{
			return LoadResourceAsText(ResourceName, Types.GetAssemblyForResource(ResourceName));
		}

		/// <summary>
		/// Loads a text resource from an embedded resource.
		/// </summary>
		/// <param name="ResourceName">Resource Name.</param>
		/// <param name="Assembly">Assembly containing the resource.</param>
		/// <returns>Text content.</returns>
		/// <exception cref="IOException">If Resource name is not valid or resource not found.</exception>
		public static string LoadResourceAsText(string ResourceName, Assembly Assembly)
		{
			using (Stream f = Assembly.GetManifestResourceStream(ResourceName))
			{
				if (f is null)
					throw new ArgumentException("Resource not found: " + ResourceName, nameof(ResourceName));

				if (f.Length > int.MaxValue)
					throw new ArgumentException("Resource size exceeds " + int.MaxValue.ToString() + " bytes.", nameof(ResourceName));

				int Len = (int)f.Length;
				byte[] Result = new byte[Len];
				f.ReadAll(Result, 0, Len);

				return Strings.GetString(Result, Encoding.UTF8);
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

	}
}
