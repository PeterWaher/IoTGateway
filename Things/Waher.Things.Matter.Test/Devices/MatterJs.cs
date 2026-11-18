using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Things.Matter.Test.Devices
{
	/// <summary>
	/// Prepares the environment for running Matter test devices, based on the open-source matter.js reference
	/// implementation (https://github.com/project-chip/matter.js). Node.js (v20 or later) must be installed.
	///
	/// The matter.js packages are installed (using npm) into the TestDevices folder of the test output directory
	/// the first time tests are run. Alternatively, the environment variable MATTER_JS_HOME can point to a folder
	/// where matter.js (@matter/main) has already been installed (i.e. a folder containing a node_modules folder).
	///
	/// If Node.js or matter.js is not available, tests requiring test devices are reported as inconclusive.
	/// </summary>
	public static class MatterJs
	{
		/// <summary>
		/// Name of the test device script.
		/// </summary>
		public const string ScriptFileName = "test-device.mjs";

		private static readonly SemaphoreSlim synchObject = new SemaphoreSlim(1, 1);
		private static string scriptFolder = null;
		private static string errorMessage = null;

		/// <summary>
		/// Gets the folder from which test devices are to be run. If test devices cannot be run, the test is
		/// marked as inconclusive.
		/// </summary>
		/// <returns>Folder containing the test device script, with matter.js available in a node_modules folder.</returns>
		public static async Task<string> GetScriptFolderAsync()
		{
			await synchObject.WaitAsync();
			try
			{
				if (scriptFolder is null && errorMessage is null)
				{
					try
					{
						scriptFolder = await PrepareAsync();
					}
					catch (Exception ex)
					{
						errorMessage = ex.Message;
					}
				}
			}
			finally
			{
				synchObject.Release();
			}

			if (!(errorMessage is null))
				Assert.Inconclusive("Matter test devices not available: " + errorMessage);

			return scriptFolder;
		}

		private static async Task<string> PrepareAsync()
		{
			string SourceFolder = Path.Combine(AppContext.BaseDirectory, "TestDevices");
			string SourceScript = Path.Combine(SourceFolder, ScriptFileName);

			if (!File.Exists(SourceScript))
				throw new Exception("Test device script not found: " + SourceScript);

			(int ExitCode, string Output) = await RunAsync("node", "--version", SourceFolder, TimeSpan.FromSeconds(30));
			if (ExitCode != 0)
				throw new Exception("Node.js not found. Install Node.js (v20 or later) to run tests against Matter test devices.");

			Console.Out.WriteLine("Node.js version: " + Output.Trim());

			string Folder;
			string Home = Environment.GetEnvironmentVariable("MATTER_JS_HOME");

			if (!string.IsNullOrEmpty(Home))
			{
				if (!IsMatterJsInstalled(Home))
					throw new Exception("MATTER_JS_HOME does not contain matter.js (node_modules/@matter/main): " + Home);

				File.Copy(SourceScript, Path.Combine(Home, ScriptFileName), true);
				Folder = Home;
			}
			else
			{
				if (!IsMatterJsInstalled(SourceFolder))
				{
					Console.Out.WriteLine("Installing matter.js test device packages. This may take a few minutes.");

					(ExitCode, Output) = await RunAsync("npm", "install --no-audit --no-fund --loglevel=error", SourceFolder,
						TimeSpan.FromMinutes(10));

					if (ExitCode != 0 || !IsMatterJsInstalled(SourceFolder))
						throw new Exception("Unable to install matter.js using npm: " + Output);
				}

				Folder = SourceFolder;
			}

			if (!SupportsIPv6())
			{
				Console.Out.WriteLine("Host does not support IPv6. Patching matter.js to run test devices on IPv4 only.");
				MatterJsIpv4Patch.Apply(Path.Combine(Folder, "node_modules"));
			}

			return Folder;
		}

		private static bool IsMatterJsInstalled(string Folder)
		{
			return File.Exists(Path.Combine(Folder, "node_modules", "@matter", "main", "package.json"));
		}

		/// <summary>
		/// Checks if IPv6 UDP sockets can be created on the host.
		/// </summary>
		/// <returns>If IPv6 is supported.</returns>
		public static bool SupportsIPv6()
		{
			if (!Socket.OSSupportsIPv6)
				return false;

			try
			{
				using Socket S = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
				S.Bind(new IPEndPoint(IPAddress.IPv6Any, 0));
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// Runs a command, and returns the exit code and output.
		/// </summary>
		/// <param name="Command">Command.</param>
		/// <param name="Arguments">Arguments.</param>
		/// <param name="WorkingFolder">Working folder.</param>
		/// <param name="Timeout">Timeout.</param>
		/// <returns>Exit code and output.</returns>
		public static async Task<(int, string)> RunAsync(string Command, string Arguments, string WorkingFolder, TimeSpan Timeout)
		{
			ProcessStartInfo StartInfo = CreateStartInfo(Command, Arguments, WorkingFolder);
			StringBuilder Output = new StringBuilder();

			StartInfo.RedirectStandardInput = false;

			using Process P = new Process()
			{
				StartInfo = StartInfo
			};

			P.OutputDataReceived += (Sender, e) =>
			{
				if (!(e.Data is null))
				{
					lock (Output)
					{
						Output.AppendLine(e.Data);
					}
				}
			};

			P.ErrorDataReceived += (Sender, e) =>
			{
				if (!(e.Data is null))
				{
					lock (Output)
					{
						Output.AppendLine(e.Data);
					}
				}
			};

			try
			{
				if (!P.Start())
					return (-1, "Unable to start " + Command);
			}
			catch (Exception ex)
			{
				return (-1, ex.Message);
			}

			P.BeginOutputReadLine();
			P.BeginErrorReadLine();

			using CancellationTokenSource Cancel = new CancellationTokenSource(Timeout);

			try
			{
				await P.WaitForExitAsync(Cancel.Token);
			}
			catch (OperationCanceledException)
			{
				try
				{
					P.Kill(true);
				}
				catch (Exception)
				{
					// Ignore
				}

				return (-1, "Timeout running " + Command + ".\r\n" + Output.ToString());
			}

			lock (Output)
			{
				return (P.ExitCode, Output.ToString());
			}
		}

		/// <summary>
		/// Creates process start information for a command, taking the platform into account.
		/// </summary>
		/// <param name="Command">Command.</param>
		/// <param name="Arguments">Arguments.</param>
		/// <param name="WorkingFolder">Working folder.</param>
		/// <returns>Process start information.</returns>
		public static ProcessStartInfo CreateStartInfo(string Command, string Arguments, string WorkingFolder)
		{
			ProcessStartInfo StartInfo;

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && Command == "npm")
				StartInfo = new ProcessStartInfo("cmd.exe", "/c npm " + Arguments);
			else
				StartInfo = new ProcessStartInfo(Command, Arguments);

			StartInfo.WorkingDirectory = WorkingFolder;
			StartInfo.UseShellExecute = false;
			StartInfo.CreateNoWindow = true;
			StartInfo.RedirectStandardOutput = true;
			StartInfo.RedirectStandardError = true;
			StartInfo.RedirectStandardInput = true;
			StartInfo.StandardOutputEncoding = Encoding.UTF8;
			StartInfo.StandardErrorEncoding = Encoding.UTF8;

			return StartInfo;
		}

		/// <summary>
		/// Gets a free UDP port, available on both IPv4 and IPv6 (if supported).
		/// </summary>
		/// <returns>Port number.</returns>
		public static int GetFreeUdpPort()
		{
			bool IPv6 = SupportsIPv6();

			for (int i = 0; i < 100; i++)
			{
				using Socket S4 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				S4.Bind(new IPEndPoint(IPAddress.Any, 0));
				int Port = ((IPEndPoint)S4.LocalEndPoint).Port;

				if (!IPv6)
					return Port;

				try
				{
					using Socket S6 = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
					S6.Bind(new IPEndPoint(IPAddress.IPv6Any, Port));
					return Port;
				}
				catch (SocketException)
				{
					// Try another port.
				}
			}

			throw new Exception("Unable to find a free UDP port.");
		}
	}
}
