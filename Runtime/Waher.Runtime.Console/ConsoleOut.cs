using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Console.Worker;

namespace Waher.Runtime.Console
{
	/// <summary>
	/// Delegate for custom writers.
	/// </summary>
	/// <param name="Output">Where output should be written.</param>
	public delegate void CustomWriter(TextWriter Output);

	/// <summary>
	/// Delegate for asynchronous custom writers.
	/// </summary>
	/// <param name="Output">Where output should be written.</param>
	public delegate Task CustomAsyncWriter(TextWriter Output);

	/// <summary>
	/// Serializes output to <see cref="Console.Out"/>, and assures modules are not dead-locked in case the Console gets locked by
	/// the user.
	/// </summary>
	public static class ConsoleOut
	{
		/// <summary>
		/// The character encoding in which the output is written.
		/// </summary>
		public static Encoding Encoding => System.Console.Out.Encoding;

		/// <summary>
		/// Gets an object that controls formatting.
		/// </summary>
		public static IFormatProvider FormatProvider => System.Console.Out.FormatProvider;

		/// <summary>
		/// The line terminator string for the current TextWriter.
		/// </summary>
		public static string NewLine => System.Console.Out.NewLine;

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static void Write(string value)
		{
			if (!string.IsNullOrEmpty(value))
				ConsoleWorker.Queue(new ConsoleOutWriteString(value));
		}

		/// <summary>
		/// Queues a custom writer to the console output.
		/// </summary>
		/// <param name="Writer">Callback method that will perform the actual writing.</param>
		public static void Write(CustomWriter Writer)
		{
			Write(Writer, null, null);
		}

		/// <summary>
		/// Queues a custom writer to the console output.
		/// </summary>
		/// <param name="Writer">Callback method that will perform the actual writing.</param>
		/// <param name="ForegroundColor">Optional Foreground Color to use.</param>
		/// <param name="BackgroundColor">Optional Background Color to use.</param>
		public static void Write(CustomWriter Writer, ConsoleColor? ForegroundColor, ConsoleColor? BackgroundColor)
		{
			ConsoleWorker.Queue(new ConsoleOutCustomWriter(Writer, ForegroundColor, BackgroundColor));
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static void Write(ulong value)
		{
			Write(value.ToString());
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static void Write(uint value)
		{
			Write(value.ToString());
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static void Write(string format, params object[] arg)
		{
			Write(string.Format(format, arg));
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static void Write(string format, object arg0, object arg1, object arg2)
		{
			Write(string.Format(format, arg0, arg1, arg2));
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static void Write(string format, object arg0, object arg1)
		{
			Write(string.Format(format, arg0, arg1));
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static void Write(string format, object arg0)
		{
			Write(string.Format(format, arg0));
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static void Write(float value)
		{
			Write(value.ToString());
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static void Write(long value)
		{
			Write(value.ToString());
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static void Write(int value)
		{
			Write(value.ToString());
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static void Write(double value)
		{
			Write(value.ToString());
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static void Write(decimal value)
		{
			Write(value.ToString());
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static void Write(char[] buffer, int index, int count)
		{
			Write(new string(buffer, index, count));
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static void Write(char[] buffer)
		{
			Write(new string(buffer));
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static void Write(char value)
		{
			Write(new string(value, 1));
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static void Write(bool value)
		{
			Write(value.ToString());
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static void Write(object value)
		{
			if (!(value is null))
				Write(value.ToString());
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static async Task WriteAsync(string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				WorkItem Item = new ConsoleOutWriteString(value.ToString());

				if (!await ConsoleWorker.Queue(Item))
					return;

				await Item.Wait();
			}
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static Task WriteAsync(char[] buffer, int index, int count)
		{
			return WriteAsync(new string(buffer, index, count));
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static Task WriteAsync(char[] buffer)
		{
			return WriteAsync(new string(buffer));
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static Task WriteAsync(char value)
		{
			return WriteAsync(new string(value, 1));
		}

		/// <summary>
		/// Queues a custom writer to the console output.
		/// </summary>
		/// <param name="Writer">Callback method that will perform the actual writing.</param>
		public static Task WriteAsync(CustomAsyncWriter Writer)
		{
			return WriteAsync(Writer, null, null);
		}

		/// <summary>
		/// Queues a custom writer to the console output.
		/// </summary>
		/// <param name="Writer">Callback method that will perform the actual writing.</param>
		/// <param name="ForegroundColor">Optional Foreground Color to use.</param>
		/// <param name="BackgroundColor">Optional Background Color to use.</param>
		public static async Task WriteAsync(CustomAsyncWriter Writer, ConsoleColor? ForegroundColor, ConsoleColor? BackgroundColor)
		{
			WorkItem Item = new ConsoleOutCustomAsyncWriter(Writer, ForegroundColor, BackgroundColor);

			if (!await ConsoleWorker.Queue(Item))
				return;

			await Item.Wait();
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static void WriteLine()
		{
			WriteLine(string.Empty);
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static void WriteLine(string value)
		{
			ConsoleWorker.Queue(new ConsoleOutWriteLineString(value));
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static void WriteLine(ulong value)
		{
			WriteLine(value.ToString());
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static void WriteLine(uint value)
		{
			WriteLine(value.ToString());
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static void WriteLine(string format, params object[] arg)
		{
			WriteLine(string.Format(format, arg));
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static void WriteLine(string format, object arg0, object arg1, object arg2)
		{
			WriteLine(string.Format(format, arg0, arg1, arg2));
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static void WriteLine(string format, object arg0, object arg1)
		{
			WriteLine(string.Format(format, arg0, arg1));
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static void WriteLine(float value)
		{
			WriteLine(value.ToString());
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static void WriteLine(string format, object arg0)
		{
			WriteLine(string.Format(format, arg0));
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static void WriteLine(long value)
		{
			WriteLine(value.ToString());
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static void WriteLine(int value)
		{
			WriteLine(value.ToString());
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static void WriteLine(double value)
		{
			WriteLine(value.ToString());
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static void WriteLine(decimal value)
		{
			WriteLine(value.ToString());
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static void WriteLine(char[] buffer, int index, int count)
		{
			WriteLine(new string(buffer, index, count));
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static void WriteLine(char[] buffer)
		{
			WriteLine(new string(buffer));
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static void WriteLine(char value)
		{
			WriteLine(new string(value, 1));
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static void WriteLine(object value)
		{
			if (!(value is null))
				WriteLine(value.ToString());
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static void WriteLine(bool value)
		{
			WriteLine(value.ToString());
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static async Task WriteLineAsync(string value)
		{
			WorkItem Item = new ConsoleOutWriteLineString(value ?? string.Empty);

			if (!await ConsoleWorker.Queue(Item))
				return;

			await Item.Wait();
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static Task WriteLineAsync()
		{
			return WriteLineAsync(string.Empty);
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static Task WriteLineAsync(char value)
		{
			return WriteLineAsync(new string(value, 1));
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static Task WriteLineAsync(char[] buffer)
		{
			return WriteLineAsync(new string(buffer));
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static Task WriteLineAsync(char[] buffer, int index, int count)
		{
			return WriteLineAsync(new string(buffer, index, count));
		}

		/// <summary>
		/// Emits a Beep sounds.
		/// </summary>
		public static void Beep()
		{
			ConsoleWorker.Queue(new ConsoleBeep());
		}
	}
}
