using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Runtime.Console;
using Waher.Runtime.Inventory;
using Waher.Runtime.IO;

namespace Waher.Utility.Upload
{
	class Program
	{
		/// <summary>
		/// Uploads a package (optionally signed) to a broker.
		/// 
		/// Command line switches:
		/// 
		/// -p PACKAGE_FILENAME    Package file name.
		/// -s SIGNATURE_FILENAME  Optional package signature file name.
		/// -h HOST                Host name of broker to upload to.
		/// -n PORT                Port number of broker to upload to, 
		///                        if not using the default port number.
		/// -a                     Account name to use for authentication.
		/// -l                     Account password to use for authentication.
		/// -d                     If package should be downloadable via
		///                        the web interface.
		/// -?                     Help.
		/// </summary>
		static async Task<int> Main(string[] args)
		{
			string PackageFileName = null;
			string SignatureFileName = null;
			string HostName = null;
			string UserName = null;
			string Password = null;
			string s;
			int Port = 0;
			int i = 0;
			int c = args.Length;
			bool Help = false;
			bool Downloadable = false;

			try
			{
				Types.Initialize(
					typeof(InternetContent).Assembly,
					typeof(Program).Assembly);

				while (i < c)
				{
					s = args[i++].ToLower();

					switch (s)
					{
						case "-p":
							if (i >= c)
								throw new Exception("Missing package file name.");

							if (string.IsNullOrEmpty(PackageFileName))
								PackageFileName = args[i++];
							else
								throw new Exception("Only one package file name allowed.");
							break;

						case "-s":
							if (i >= c)
								throw new Exception("Missing signature file name.");

							if (string.IsNullOrEmpty(SignatureFileName))
								SignatureFileName = args[i++];
							else
								throw new Exception("Only one signature file name allowed.");
							break;

						case "-h":
							if (i >= c)
								throw new Exception("Missing host name.");

							if (string.IsNullOrEmpty(HostName))
								HostName = args[i++];
							else
								throw new Exception("Only one host name allowed.");
							break;

						case "-n":
							if (i >= c)
								throw new Exception("Missing port number.");

							if (!int.TryParse(args[i++], out int j) || j <= 0 || j > 65535)
								throw new Exception("Invalid port number.");

							if (Port == 0)
								Port = j;
							else
								throw new Exception("Only one port number allowed.");
							break;

						case "-a":
							if (i >= c)
								throw new Exception("Missing account name.");

							if (string.IsNullOrEmpty(UserName))
								UserName = args[i++];
							else
								throw new Exception("Only one user name allowed.");
							break;

						case "-l":
							if (i >= c)
								throw new Exception("Missing password.");

							if (string.IsNullOrEmpty(Password))
								Password = args[i++];
							else
								throw new Exception("Only one password allowed.");
							break;

						case "-d":
							Downloadable = true;
							break;

						case "-?":
							Help = true;
							break;

						default:
							throw new Exception("Unrecognized switch: " + s);
					}
				}

				if (Help || c == 0)
				{
					ConsoleOut.WriteLine("Uploads a package (optionally signed) to a broker.");
					ConsoleOut.WriteLine();
					ConsoleOut.WriteLine("Command line switches:");
					ConsoleOut.WriteLine();
					ConsoleOut.WriteLine("-p PACKAGE_FILENAME    Package file name.");
					ConsoleOut.WriteLine("-s SIGNATURE_FILENAME  Optional package signature file name.");
					ConsoleOut.WriteLine("-h HOST                Host name of broker to upload to.");
					ConsoleOut.WriteLine("-n PORT                Port number of broker to upload to, ");
					ConsoleOut.WriteLine("                       if not using the default port number.");
					ConsoleOut.WriteLine("-a                     Account name to use for authentication.");
					ConsoleOut.WriteLine("-l                     Account password to use for authentication.");
					ConsoleOut.WriteLine("-d                     If package should be downloadable via");
					ConsoleOut.WriteLine("                       the web interface.");
					ConsoleOut.WriteLine("-?                     Help.");
					return 0;
				}

				if (string.IsNullOrEmpty(HostName))
					throw new Exception("Host name not specified.");

				if (string.IsNullOrEmpty(PackageFileName))
					throw new Exception("Package file name not specified.");

				if (string.IsNullOrEmpty(UserName))
					throw new Exception("Account name not specified.");

				if (string.IsNullOrEmpty(Password))
					throw new Exception("Account password not specified.");

				PackageFileName = Path.GetFullPath(PackageFileName);

				StringBuilder sb = new();

				sb.Append("https://");
				sb.Append(HostName);

				if (Port > 0)
				{
					sb.Append(':');
					sb.Append(Port);
				}

				sb.Append('/');

				Uri Login = new(sb.ToString() + "Login");
				Uri Logout = new(sb.ToString() + "Logout");
				Uri UploadPackage = new(sb.ToString() + "UploadPackage");
				Uri UploadSignature = new(sb.ToString() + "UploadSignature");
				string TabID = Guid.NewGuid().ToString();
				string HttpSessionID = Guid.NewGuid().ToString();

				ConsoleOut.WriteLine("Logging in.");

				ContentResponse LoginResponse = await InternetContent.PostAsync(Login,
					new Dictionary<string, string>()
					{
						{ "UserName", UserName },
						{ "Password", Password }
					},
					new KeyValuePair<string, string>("Cookie", "HttpSessionID=" + HttpSessionID));

				LoginResponse.AssertOk();

				using FileStream PackageStream = new(PackageFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
				int BufSize = 4 * 1024 * 1024;
				long Length = PackageStream.Length;
				long Position = 0;
				int BlockNr = 0;
				byte[] Buffer = null;

				ConsoleOut.WriteLine("Uploading package file.");

				do
				{
					ConsoleOut.Write('.');

					c = (int)Math.Min(BufSize, Length - Position);
					if (Buffer is null || Buffer.Length != c)
						Buffer = new byte[c];

					await PackageStream.ReadAllAsync(Buffer, 0, c);
					Position += c;

					ContentBinaryResponse Response = await InternetContent.PostAsync(
						UploadPackage, Buffer, "application/octet-stream",
						new KeyValuePair<string, string>("Cookie", "HttpSessionID=" + HttpSessionID),
						new KeyValuePair<string, string>("X-FileName", Path.GetFileName(PackageFileName)),
						new KeyValuePair<string, string>("X-TabID", TabID),
						new KeyValuePair<string, string>("X-BlockNr", BlockNr++.ToString()),
						new KeyValuePair<string, string>("X-More", Position < Length ? "1" : "0"),
						new KeyValuePair<string, string>("X-Downloadable", Downloadable ? "1" : "0"),
						new KeyValuePair<string, string>("X-GenSign", string.IsNullOrEmpty(SignatureFileName) ? "1" : "0"));

					Response.AssertOk();
				}
				while (Position < Length);

				ConsoleOut.WriteLine();

				if (!string.IsNullOrEmpty(SignatureFileName))
				{
					using FileStream SignatureStream = new(SignatureFileName, FileMode.Open, FileAccess.Read, FileShare.Read);

					Length = SignatureStream.Length;
					if (Length > int.MaxValue)
						throw new Exception("Signature file too large.");

					if (Buffer is null || Buffer.Length != Length)
						Buffer = new byte[Length];

					ConsoleOut.WriteLine("Uploading signature file.");

					await SignatureStream.ReadAllAsync(Buffer, 0, (int)Length);

					ContentBinaryResponse Response = await InternetContent.PostAsync(
						UploadSignature, Buffer, "application/octet-stream",
						new KeyValuePair<string, string>("Cookie", "HttpSessionID=" + HttpSessionID),
						new KeyValuePair<string, string>("X-FileName", Path.GetFileName(SignatureFileName)),
						new KeyValuePair<string, string>("X-TabID", TabID),
						new KeyValuePair<string, string>("X-BlockNr", "0"),
						new KeyValuePair<string, string>("X-More", "0"),
						new KeyValuePair<string, string>("X-Downloadable", Downloadable ? "1" : "0"));

					Response.AssertOk();
				}

				ConsoleOut.WriteLine("Package uploaded successfully.");
				ConsoleOut.WriteLine("Logging out.");

				ContentResponse LogoutResponse = await InternetContent.PostAsync(Logout,
					string.Empty,
					new KeyValuePair<string, string>("Cookie", "HttpSessionID=" + HttpSessionID));

				LoginResponse.AssertOk();

				return 0;
			}
			catch (Exception ex)
			{
				ConsoleOut.WriteLine();

				ConsoleColor Bak = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Red;

				ConsoleOut.WriteLine(ex.Message);

				Console.ForegroundColor = Bak;
				return -1;
			}
			finally
			{
				ConsoleOut.Flush(true);
			}
		}
	}
}
