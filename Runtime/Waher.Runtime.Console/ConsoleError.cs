using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Console.Worker;
using Waher.Runtime.Queue;

namespace Waher.Runtime.Console
{
	/// <summary>
	/// Serializes output to <see cref="System.Console.Error"/>, and assures modules are not dead-locked in case the Console gets locked by
	/// the user.
	/// </summary>
	public static class ConsoleError
	{
		/// <summary>
		/// The character encoding in which the output is written.
		/// </summary>
		public static Encoding Encoding => System.Console.Error.Encoding;

		/// <summary>
		/// Gets an object that controls formatting.
		/// </summary>
		public static IFormatProvider FormatProvider => System.Console.Error.FormatProvider;

		/// <summary>
		/// The line terminator string for the current TextWriter.
		/// </summary>
		public static string NewLine => System.Console.Error.NewLine;

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="value">Value to be written.</param>
		public static void Write(string value)
		{
			if (!string.IsNullOrEmpty(value))
				ConsoleWorker.Forward(new ConsoleErrorWriteString(value));
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
			ConsoleWorker.Forward(new ConsoleErrorCustomWriter(Writer, ForegroundColor, BackgroundColor));
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
		/// <param name="format">Value to be written.</param>
		/// <param name="arg">Argument</param>
		public static void Write(string format, params object[] arg)
		{
			Write(string.Format(format, arg));
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="format">Value to be written.</param>
		/// <param name="arg0">Argument</param>
		/// <param name="arg1">Argument</param>
		/// <param name="arg2">Argument</param>
		public static void Write(string format, object arg0, object arg1, object arg2)
		{
			Write(string.Format(format, arg0, arg1, arg2));
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="format">Value to be written.</param>
		/// <param name="arg0">Argument</param>
		/// <param name="arg1">Argument</param>
		public static void Write(string format, object arg0, object arg1)
		{
			Write(string.Format(format, arg0, arg1));
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="format">Value to be written.</param>
		/// <param name="arg0">Argument</param>
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
		/// <param name="buffer">Array of characters.</param>
		/// <param name="index">Starting offset into buffer.</param>
		/// <param name="count">Number of characters.</param>
		public static void Write(char[] buffer, int index, int count)
		{
			Write(new string(buffer, index, count));
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="buffer">Array of characters.</param>
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
				WorkItem Item = new ConsoleErrorWriteString(value.ToString());

				if (!await ConsoleWorker.Forward(Item))
					return;

				await Item.Wait();
			}
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="buffer">Array of characters.</param>
		/// <param name="index">Starting offset into buffer.</param>
		/// <param name="count">Number of characters.</param>
		public static Task WriteAsync(char[] buffer, int index, int count)
		{
			return WriteAsync(new string(buffer, index, count));
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="buffer">Array of characters.</param>
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
			WorkItem Item = new ConsoleErrorCustomAsyncWriter(Writer, ForegroundColor, BackgroundColor);

			if (!await ConsoleWorker.Forward(Item))
				return;

			await Item.Wait();
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
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
			ConsoleWorker.Forward(new ConsoleErrorWriteLineString(value));
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
		/// <param name="format">Value to be written.</param>
		/// <param name="arg">Argument</param>
		public static void WriteLine(string format, params object[] arg)
		{
			WriteLine(string.Format(format, arg));
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="format">Value to be written.</param>
		/// <param name="arg0">Argument</param>
		/// <param name="arg1">Argument</param>
		/// <param name="arg2">Argument</param>
		public static void WriteLine(string format, object arg0, object arg1, object arg2)
		{
			WriteLine(string.Format(format, arg0, arg1, arg2));
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="format">Value to be written.</param>
		/// <param name="arg0">Argument</param>
		/// <param name="arg1">Argument</param>
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
		/// <param name="format">Value to be written.</param>
		/// <param name="arg0">Argument</param>
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
		/// <param name="buffer">Array of characters.</param>
		/// <param name="index">Starting offset into buffer.</param>
		/// <param name="count">Number of characters.</param>
		public static void WriteLine(char[] buffer, int index, int count)
		{
			WriteLine(new string(buffer, index, count));
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="buffer">Array of characters.</param>
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
			WorkItem Item = new ConsoleErrorWriteLineString(value ?? string.Empty);

			if (!await ConsoleWorker.Forward(Item))
				return;

			await Item.Wait();
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
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
		/// <param name="buffer">Array of characters.</param>
		public static Task WriteLineAsync(char[] buffer)
		{
			return WriteLineAsync(new string(buffer));
		}

		/// <summary>
		/// Queues a value to be written to the console output.
		/// </summary>
		/// <param name="buffer">Array of characters.</param>
		/// <param name="index">Starting offset into buffer.</param>
		/// <param name="count">Number of characters.</param>
		public static Task WriteLineAsync(char[] buffer, int index, int count)
		{
			return WriteLineAsync(new string(buffer, index, count));
		}

		/// <summary>
		/// Emits a Beep sounds.
		/// </summary>
		public static void Beep()
		{
			ConsoleWorker.Forward(new ConsoleBeep());
		}

		/// <summary>
		/// Provides a <see cref="TextWriter"/> instance, that writes to <see cref="ConsoleError"/>.
		/// </summary>
		public static TextWriter Writer
		{
			get => new ConsoleErrorTextWriter();
		}

		/// <summary>
		/// Text writer that writes to <see cref="ConsoleError"/>
		/// </summary>
		private class ConsoleErrorTextWriter : TextWriter
		{
			public ConsoleErrorTextWriter()
			{
			}

			public override Encoding Encoding => ConsoleError.Encoding;
			public override void Write(char value) => ConsoleError.Write(value);
			public override void Write(ulong value) => ConsoleError.Write(value);
			public override void Write(uint value) => ConsoleError.Write(value);
			public override void Write(string format, params object[] arg) => ConsoleError.Write(format, arg);
			public override void Write(string format, object arg0, object arg1, object arg2) => ConsoleError.Write(format, arg0, arg1, arg2);
			public override void Write(string format, object arg0, object arg1) => ConsoleError.Write(format, arg0, arg1);
			public override void Write(string format, object arg0) => ConsoleError.Write(format, arg0);
			public override void Write(string value) => ConsoleError.Write(value);
			public override void Write(float value) => ConsoleError.Write(value);
			public override void Write(long value) => ConsoleError.Write(value);
			public override void Write(int value) => ConsoleError.Write(value);
			public override void Write(double value) => ConsoleError.Write(value);
			public override void Write(decimal value) => ConsoleError.Write(value);
			public override void Write(char[] buffer, int index, int count) => ConsoleError.Write(buffer, index, count);
			public override void Write(char[] buffer) => ConsoleError.Write(buffer);
			public override void Write(bool value) => ConsoleError.Write(value);
			public override void Write(object value) => ConsoleError.Write(value);
			public override Task WriteAsync(string value) => ConsoleError.WriteAsync(value);
			public override Task WriteAsync(char[] buffer, int index, int count) => ConsoleError.WriteAsync(buffer, index, count);
			public override Task WriteAsync(char value) => ConsoleError.WriteAsync(value);
			public override void WriteLine() => ConsoleError.WriteLine();
			public override void WriteLine(ulong value) => ConsoleError.WriteLine(value);
			public override void WriteLine(uint value) => ConsoleError.WriteLine(value);
			public override void WriteLine(string format, params object[] arg) => ConsoleError.WriteLine(format, arg);
			public override void WriteLine(string format, object arg0, object arg1, object arg2) => ConsoleError.WriteLine(format, arg0, arg1, arg2);
			public override void WriteLine(string format, object arg0, object arg1) => ConsoleError.WriteLine(format, arg0, arg1);
			public override void WriteLine(string value) => ConsoleError.WriteLine(value);
			public override void WriteLine(float value) => ConsoleError.WriteLine(value);
			public override void WriteLine(string format, object arg0) => ConsoleError.WriteLine(format, arg0);
			public override void WriteLine(long value) => ConsoleError.WriteLine(value);
			public override void WriteLine(int value) => ConsoleError.WriteLine(value);
			public override void WriteLine(double value) => ConsoleError.WriteLine(value);
			public override void WriteLine(decimal value) => ConsoleError.WriteLine(value);
			public override void WriteLine(char[] buffer, int index, int count) => ConsoleError.WriteLine(buffer, index, count);
			public override void WriteLine(char[] buffer) => ConsoleError.WriteLine(buffer);
			public override void WriteLine(char value) => ConsoleError.WriteLine(value);
			public override void WriteLine(object value) => ConsoleError.WriteLine(value);
			public override void WriteLine(bool value) => ConsoleError.WriteLine(value);
			public override Task WriteLineAsync() => ConsoleError.WriteLineAsync();
			public override Task WriteLineAsync(char value) => ConsoleError.WriteLineAsync(value);
			public override Task WriteLineAsync(char[] buffer, int index, int count) => ConsoleError.WriteLineAsync(buffer, index, count);
			public override Task WriteLineAsync(string value) => ConsoleError.WriteLineAsync(value);
		}
	}
}
