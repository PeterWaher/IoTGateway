using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Waher.Events;
using Waher.Events.Console;

namespace Waher.IoTGateway.Console
{
	/// <summary>
	/// A console application version of the IoT gateway. It's easy to use and experiment with.
	/// </summary>
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				System.Console.ForegroundColor = ConsoleColor.White;

				System.Console.Out.WriteLine("Welcome to the Internet of Things Gateway server application.");
				System.Console.Out.WriteLine(new string('-', 79));
				System.Console.Out.WriteLine("This server application will help you manage IoT devices and");
				System.Console.Out.WriteLine("create dynamic content that you can publish on the Internet.");
				System.Console.Out.WriteLine("It also provides programming interfaces (API) which allow you");
				System.Console.Out.WriteLine("to dynamically and securely interact with the devices and the");
				System.Console.Out.WriteLine("content you publish.");

				Log.Register(new ConsoleEventSink(false));
				Log.RegisterExceptionToUnnest(typeof(System.Runtime.InteropServices.ExternalException));

				if (!Gateway.Start(true))
				{
					System.Console.Out.WriteLine();
					System.Console.Out.WriteLine("Gateway being started in another process.");
					return;
				}

				ManualResetEvent Done = new ManualResetEvent(false);
				System.Console.CancelKeyPress += (sender, e) => Done.Set();

				try
				{
					SetConsoleCtrlHandler((ControlType) =>
					{
						switch (ControlType)
						{
							case CtrlTypes.CTRL_BREAK_EVENT:
							case CtrlTypes.CTRL_CLOSE_EVENT:
							case CtrlTypes.CTRL_C_EVENT:
							case CtrlTypes.CTRL_SHUTDOWN_EVENT:
								Done.Set();
								break;

							case CtrlTypes.CTRL_LOGOFF_EVENT:
								break;
						}

						return true;
					}, true);
				}
				catch (Exception)
				{
					Log.Error("Unable to register CTRL-C control handler.");
				}

				while (!Done.WaitOne(1000))
					;
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
			finally
			{
				Gateway.Stop();
				Log.Terminate();
			}
		}

		#region unmanaged

		// https://msdn.microsoft.com/en-us/library/windows/desktop/ms686016(v=vs.85).aspx
		// https://msdn.microsoft.com/en-us/library/windows/desktop/ms683242(v=vs.85).aspx

		[DllImport("Kernel32")]
		public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);
		public delegate bool HandlerRoutine(CtrlTypes CtrlType);

		public enum CtrlTypes
		{
			CTRL_C_EVENT = 0,
			CTRL_BREAK_EVENT = 1,
			CTRL_CLOSE_EVENT = 2,
			CTRL_LOGOFF_EVENT = 5,
			CTRL_SHUTDOWN_EVENT = 6
		}

		#endregion
	}
}
