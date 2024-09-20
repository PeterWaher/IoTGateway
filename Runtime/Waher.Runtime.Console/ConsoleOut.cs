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
	/// Serializes output to <see cref="System.Console.Out"/>, and assures modules are not dead-locked in case the Console gets locked by
	/// the user.
	/// </summary>
	public static class ConsoleOut
	{
		private static ConsoleColor foregroundColor = System.Console.ForegroundColor;
		private static ConsoleColor backgroundColor = System.Console.BackgroundColor;

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
		/// Width of window.
		/// </summary>
		public static int WindowWidth => System.Console.WindowWidth;

		/// <summary>
		/// Left edge of window.
		/// </summary>
		public static int WindowLeft => System.Console.WindowLeft;

		/// <summary>
		/// Top edge of window.
		/// </summary>
		public static int WindowTop => System.Console.WindowTop;

		/// <summary>
		/// Height of window.
		/// </summary>
		public static int WindowHeight => System.Console.WindowHeight;

		/// <summary>
		/// Left position of cursor.
		/// </summary>
		public static int CursorLeft => System.Console.CursorLeft;

		/// <summary>
		/// Top position of cursor.
		/// </summary>
		public static int CursorTop => System.Console.CursorTop;

		/// <summary>
		/// Size of cursor.
		/// </summary>
		public static int CursorSize => System.Console.CursorSize;

		/// <summary>
		/// Console foreground color.
		/// </summary>
		public static ConsoleColor ForegroundColor
		{
			get => foregroundColor;
			set
			{
				foregroundColor = value;
				ConsoleWorker.Queue(new ConsoleForegroundColor(value));
			}
		}

		/// <summary>
		/// Console background color.
		/// </summary>
		public static ConsoleColor BackgroundColor
		{
			get => backgroundColor;
			set
			{
				backgroundColor = value;
				ConsoleWorker.Queue(new ConsoleBackgroundColor(value));
			}
		}

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
				WorkItem Item = new ConsoleOutWriteString(value.ToString());

				if (!await ConsoleWorker.Queue(Item))
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
			WorkItem Item = new ConsoleOutCustomAsyncWriter(Writer, ForegroundColor, BackgroundColor);

			if (!await ConsoleWorker.Queue(Item))
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
			WorkItem Item = new ConsoleOutWriteLineString(value ?? string.Empty);

			if (!await ConsoleWorker.Queue(Item))
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
			ConsoleWorker.Queue(new ConsoleBeep());
		}

		/// <summary>
		/// Clears all buffers for the current writer and causes any buffered data to be
		/// written to the underlying device.
		/// </summary>
		public static void Flush()
		{
			Flush(false);
		}

		/// <summary>
		/// Clears all buffers for the current writer and causes any buffered data to be
		/// written to the underlying device.
		/// </summary>
		/// <param name="Terminate">If console serialization should be terminated.</param>
		public static void Flush(bool Terminate)
		{
			FlushAsync(Terminate).Wait();
		}

		/// <summary>
		/// Asynchronously clears all buffers for the current writer and causes any buffered
		/// data to be written to the underlying device.
		/// </summary>
		/// <returns>A task that represents the asynchronous flush operation.</returns>
		public static Task FlushAsync()
		{
			return FlushAsync(false);
		}

		/// <summary>
		/// Asynchronously clears all buffers for the current writer and causes any buffered
		/// data to be written to the underlying device.
		/// </summary>
		/// <returns>A task that represents the asynchronous flush operation.</returns>
		public static async Task FlushAsync(bool Terminate)
		{
			if (!ConsoleWorker.Terminating)
			{
				TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();
				if (await ConsoleWorker.Queue(new ConsoleFlush(Terminate, Result)))
					await Result.Task;
			}
		}

		/// <summary>
		/// Provides a <see cref="TextWriter"/> instance, that writes to <see cref="ConsoleOut"/>.
		/// </summary>
		public static TextWriter Writer
		{
			get => new ConsoleOutTextWriter();
		}

		/// <summary>
		/// Text writer that writes to <see cref="ConsoleOut"/>
		/// </summary>
		private class ConsoleOutTextWriter : TextWriter
		{
			public ConsoleOutTextWriter()
			{
			}

			public override Encoding Encoding => ConsoleOut.Encoding;
			public override void Flush() => ConsoleOut.Flush();
			public override Task FlushAsync() => ConsoleOut.FlushAsync();
			public override void Write(char value) => ConsoleOut.Write(value);
			public override void Write(ulong value) => ConsoleOut.Write(value);
			public override void Write(uint value) => ConsoleOut.Write(value);
			public override void Write(string format, params object[] arg) => ConsoleOut.Write(format, arg);
			public override void Write(string format, object arg0, object arg1, object arg2) => ConsoleOut.Write(format, arg0, arg1, arg2);
			public override void Write(string format, object arg0, object arg1) => ConsoleOut.Write(format, arg0, arg1);
			public override void Write(string format, object arg0) => ConsoleOut.Write(format, arg0);
			public override void Write(string value) => ConsoleOut.Write(value);
			public override void Write(float value) => ConsoleOut.Write(value);
			public override void Write(long value) => ConsoleOut.Write(value);
			public override void Write(int value) => ConsoleOut.Write(value);
			public override void Write(double value) => ConsoleOut.Write(value);
			public override void Write(decimal value) => ConsoleOut.Write(value);
			public override void Write(char[] buffer, int index, int count) => ConsoleOut.Write(buffer, index, count);
			public override void Write(char[] buffer) => ConsoleOut.Write(buffer);
			public override void Write(bool value) => ConsoleOut.Write(value);
			public override void Write(object value) => ConsoleOut.Write(value);
			public override Task WriteAsync(string value) => ConsoleOut.WriteAsync(value);
			public override Task WriteAsync(char[] buffer, int index, int count) => ConsoleOut.WriteAsync(buffer, index, count);
			public override Task WriteAsync(char value) => ConsoleOut.WriteAsync(value);
			public override void WriteLine() => ConsoleOut.WriteLine();
			public override void WriteLine(ulong value) => ConsoleOut.WriteLine(value);
			public override void WriteLine(uint value) => ConsoleOut.WriteLine(value);
			public override void WriteLine(string format, params object[] arg) => ConsoleOut.WriteLine(format, arg);
			public override void WriteLine(string format, object arg0, object arg1, object arg2) => ConsoleOut.WriteLine(format, arg0, arg1, arg2);
			public override void WriteLine(string format, object arg0, object arg1) => ConsoleOut.WriteLine(format, arg0, arg1);
			public override void WriteLine(string value) => ConsoleOut.WriteLine(value);
			public override void WriteLine(float value) => ConsoleOut.WriteLine(value);
			public override void WriteLine(string format, object arg0) => ConsoleOut.WriteLine(format, arg0);
			public override void WriteLine(long value) => ConsoleOut.WriteLine(value);
			public override void WriteLine(int value) => ConsoleOut.WriteLine(value);
			public override void WriteLine(double value) => ConsoleOut.WriteLine(value);
			public override void WriteLine(decimal value) => ConsoleOut.WriteLine(value);
			public override void WriteLine(char[] buffer, int index, int count) => ConsoleOut.WriteLine(buffer, index, count);
			public override void WriteLine(char[] buffer) => ConsoleOut.WriteLine(buffer);
			public override void WriteLine(char value) => ConsoleOut.WriteLine(value);
			public override void WriteLine(object value) => ConsoleOut.WriteLine(value);
			public override void WriteLine(bool value) => ConsoleOut.WriteLine(value);
			public override Task WriteLineAsync() => ConsoleOut.WriteLineAsync();
			public override Task WriteLineAsync(char value) => ConsoleOut.WriteLineAsync(value);
			public override Task WriteLineAsync(char[] buffer, int index, int count) => ConsoleOut.WriteLineAsync(buffer, index, count);
			public override Task WriteLineAsync(string value) => ConsoleOut.WriteLineAsync(value);
		}
	}
}
