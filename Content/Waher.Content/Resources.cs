using System;
using System.Reflection;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Waher.Runtime.Inventory;
using System.Threading.Tasks;
using System.Text;

namespace Waher.Content
{
	/// <summary>
	/// Static class managing loading of resources stored as embedded resources or in content files.
	/// </summary>
	public static class Resources
	{
		/// <summary>
		/// Gets the assembly corresponding to a given resource name.
		/// </summary>
		/// <param name="ResourceName">Resource name.</param>
		/// <returns>Assembly.</returns>
		/// <exception cref="ArgumentException">If no assembly could be found.</exception>
		public static Assembly GetAssembly(string ResourceName)
		{
			string[] Parts = ResourceName.Split('.');
			string ParentNamespace;
			int i, c;
			Assembly A;

			if (!Types.IsRootNamespace(Parts[0]))
				A = null;
			else
			{
				ParentNamespace = Parts[0];
				i = 1;
				c = Parts.Length;

				while (i < c)
				{
					if (!Types.IsSubNamespace(ParentNamespace, Parts[i]))
						break;

					ParentNamespace += "." + Parts[i];
					i++;
				}

				A = Types.GetFirstAssemblyReferenceInNamespace(ParentNamespace);
			}

			if (A is null)
				throw new ArgumentException("Assembly not found for resource " + ResourceName + ".", nameof(ResourceName));

			return A;
		}

		/// <summary>
		/// Loads a resource from an embedded resource.
		/// </summary>
		/// <param name="ResourceName">Resource Name.</param>
		/// <returns>Binary content.</returns>
		/// <exception cref="IOException">If Resource name is not valid or resource not found.</exception>
		public static byte[] LoadResource(string ResourceName)
		{
			return LoadResource(ResourceName, GetAssembly(ResourceName));
		}

		/// <summary>
		/// Loads a resource from an embedded resource.
		/// </summary>
		/// <param name="ResourceName">Resource Name.</param>
		/// <param name="Assembly">Assembly containing the resource.</param>
		/// <returns>Binary content.</returns>
		/// <exception cref="IOException">If Resource name is not valid or resource not found.</exception>
		public static byte[] LoadResource(string ResourceName, Assembly Assembly)
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
				return Result;
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
			return LoadResourceAsText(ResourceName, GetAssembly(ResourceName));
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

				return CommonTypes.GetString(Result, Encoding.UTF8);
			}
		}

		/// <summary>
		/// Loads a certificate from an embedded resource.
		/// </summary>
		/// <param name="ResourceName">Resource Name.</param>
		/// <returns>Certificate.</returns>
		/// <exception cref="IOException">If Resource name is not valid or resource not found.</exception>
		public static X509Certificate2 LoadCertificate(string ResourceName)
		{
			return (LoadCertificate(ResourceName, GetAssembly(ResourceName)));
		}

		/// <summary>
		/// Loads a certificate from an embedded resource.
		/// </summary>
		/// <param name="ResourceName">Resource Name.</param>
		/// <param name="Assembly">Assembly containing the resource.</param>
		/// <returns>Certificate.</returns>
		/// <exception cref="IOException">If Resource name is not valid or resource not found.</exception>
		public static X509Certificate2 LoadCertificate(string ResourceName, Assembly Assembly)
		{
			return LoadCertificate(ResourceName, null, Assembly);
		}

		/// <summary>
		/// Loads a certificate from an embedded resource.
		/// </summary>
		/// <param name="ResourceName">Resource Name.</param>
		/// <param name="Password">Optional password.</param>
		/// <returns>Certificate.</returns>
		/// <exception cref="IOException">If Resource name is not valid or resource not found.</exception>
		public static X509Certificate2 LoadCertificate(string ResourceName, string Password)
		{
			return LoadCertificate(ResourceName, Password, GetAssembly(ResourceName));
		}

		/// <summary>
		/// Loads a certificate from an embedded resource.
		/// </summary>
		/// <param name="ResourceName">Resource Name.</param>
		/// <param name="Password">Optional password.</param>
		/// <param name="Assembly">Assembly containing the resource.</param>
		/// <returns>Certificate.</returns>
		/// <exception cref="IOException">If Resource name is not valid or resource not found.</exception>
		public static X509Certificate2 LoadCertificate(string ResourceName, string Password, Assembly Assembly)
		{
			byte[] Data = LoadResource(ResourceName, Assembly);
			if (Password is null)
				return new X509Certificate2(Data);
			else
				return new X509Certificate2(Data, Password);
		}

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
		/// Reads a text file asynchronously.
		/// </summary>
		/// <param name="FileName">Filename.</param>
		/// <returns>Decoded text.</returns>
		public static async Task<string> ReadAllTextAsync(string FileName)
		{
			byte[] Bin = await ReadAllBytesAsync(FileName);
			return CommonTypes.GetString(Bin, Encoding.UTF8);
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
