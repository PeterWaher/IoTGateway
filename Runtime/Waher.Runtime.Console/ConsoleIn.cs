using System;
using System.Threading.Tasks;
using Waher.Runtime.Console.Worker;

namespace Waher.Runtime.Console
{
	/// <summary>
	/// Serializes input from <see cref="System.Console.In"/>, and assures modules are not dead-locked in case the Console gets locked by
	/// the user.
	/// </summary>
	public static class ConsoleIn
	{
		/// <summary>
		/// Reads the next character without changing the state of the reader or the character
		/// source. Returns the next available character without actually reading it from
		/// the reader.
		/// </summary>
		/// <returns>An integer representing the next character to be read, or -1 if no more characters
		/// are available or the reader does not support seeking.</returns>
		public static int Peek()
		{
			return PeekAsync().Result;
		}

		/// <summary>
		/// Reads the next character without changing the state of the reader or the character
		/// source. Returns the next available character without actually reading it from
		/// the reader.
		/// </summary>
		/// <returns>An integer representing the next character to be read, or -1 if no more characters
		/// are available or the reader does not support seeking.</returns>
		public static async Task<int> PeekAsync()
		{
			TaskCompletionSource<int> Result = new TaskCompletionSource<int>();
			if (await ConsoleWorker.Queue(new ConsoleInPeekCharacter(Result)))
				return await Result.Task;
			else
				return -1;
		}

		/// <summary>
		/// Reads the next character from the text reader and advances the character position by one character.
		/// </summary>
		/// <returns>The next character from the text reader, or -1 if no more characters are available. 
		/// The default implementation returns -1.</returns>
		public static int Read()
		{
			return ReadAsync().Result;
		}

		/// <summary>
		/// Reads the next character from the text reader and advances the character position by one character.
		/// </summary>
		/// <returns>The next character from the text reader, or -1 if no more characters are available. 
		/// The default implementation returns -1.</returns>
		public static async Task<int> ReadAsync()
		{
			TaskCompletionSource<int> Result = new TaskCompletionSource<int>();
			if (await ConsoleWorker.Queue(new ConsoleInReadCharacter(Result)))
				return await Result.Task;
			else
				return -1;
		}

		/// <summary>
		/// Reads a specified maximum number of characters from the current reader and writes
		/// the data to a buffer, beginning at the specified index.
		/// </summary>
		/// <param name="buffer">When this method returns, contains the specified character array with the values
		/// between index and (index + count - 1) replaced by the characters read from the
		/// current source.</param>
		/// <param name="index">The position in buffer at which to begin writing.</param>
		/// <param name="count">The maximum number of characters to read. If the end of the reader is reached
		/// before the specified number of characters is read into the buffer, the method
		/// returns.</param>
		/// <returns>The number of characters that have been read. The number will be less than or
		/// equal to count, depending on whether the data is available within the reader.
		/// This method returns 0 (zero) if it is called when no more characters are left
		/// to read.</returns>
		public static int Read(char[] buffer, int index, int count)
		{
			return ReadAsync(buffer, index, count).Result;
		}

		/// <summary>
		/// Reads a specified maximum number of characters from the current text reader asynchronously
		/// and writes the data to a buffer, beginning at the specified index.
		/// </summary>
		/// <param name="buffer">When this method returns, contains the specified character array with the values
		/// between index and (index + count - 1) replaced by the characters read from the
		/// current source.</param>
		/// <param name="index">The position in buffer at which to begin writing.</param>
		/// <param name="count">The maximum number of characters to read. If the end of the text is reached before
		/// the specified number of characters is read into the buffer, the current method
		/// returns.</param>
		/// <returns>A task that represents the asynchronous read operation. The value of the TResult
		/// parameter contains the total number of bytes read into the buffer. The result
		/// value can be less than the number of bytes requested if the number of bytes currently
		/// available is less than the requested number, or it can be 0 (zero) if the end
		/// of the text has been reached.</returns>
		public static async Task<int> ReadAsync(char[] buffer, int index, int count)
		{
			CheckArguments(buffer, index, count);

			TaskCompletionSource<int> Result = new TaskCompletionSource<int>();
			if (await ConsoleWorker.Queue(new ConsoleInRead(buffer, index, count, Result)))
				return await Result.Task;
			else
				return 0;
		}

		private static void CheckArguments(Array buffer, int index, int count)
		{
			if (buffer is null)
				throw new ArgumentNullException(nameof(buffer));

			if (index < 0)
				throw new ArgumentOutOfRangeException(nameof(index));

			if (count < 0)
				throw new ArgumentOutOfRangeException(nameof(count));

			if (index > buffer.Length)
				throw new ArgumentException(nameof(index));

			if (index + count > buffer.Length)
				throw new ArgumentException(nameof(count));
		}

		/// <summary>
		/// Reads a specified maximum number of characters from the current text reader and
		/// writes the data to a buffer, beginning at the specified index.
		/// </summary>
		/// <param name="buffer">When this method returns, this parameter contains the specified character array
		/// with the values between index and (index + count -1) replaced by the characters
		/// read from the current source.</param>
		/// <param name="index">The position in buffer at which to begin writing.</param>
		/// <param name="count">The maximum number of characters to read.</param>
		/// <returns>The number of characters that have been read. The number will be less than or
		/// equal to count, depending on whether all input characters have been read.</returns>
		public static int ReadBlock(char[] buffer, int index, int count)
		{
			return ReadBlockAsync(buffer, index, count).Result;
		}

		/// <summary>
		/// Reads a specified maximum number of characters from the current text reader asynchronously
		/// and writes the data to a buffer, beginning at the specified index.
		/// </summary>
		/// <param name="buffer">
		/// When this method returns, contains the specified character array with the values
		/// between index and (index + count - 1) replaced by the characters read from the
		/// current source.
		/// </param>
		/// <param name="index">The position in buffer at which to begin writing.</param>
		/// <param name="count">The maximum number of characters to read. If the end of the text is reached before
		/// the specified number of characters is read into the buffer, the current method
		/// returns.</param>
		/// <returns>A task that represents the asynchronous read operation. The value of the TResult
		/// parameter contains the total number of bytes read into the buffer. The result
		/// value can be less than the number of bytes requested if the number of bytes currently
		/// available is less than the requested number, or it can be 0 (zero) if the end
		/// of the text has been reached.</returns>
		public static async Task<int> ReadBlockAsync(char[] buffer, int index, int count)
		{
			CheckArguments(buffer, index, count);

			TaskCompletionSource<int> Result = new TaskCompletionSource<int>();
			if (await ConsoleWorker.Queue(new ConsoleInReadBlock(buffer, index, count, Result)))
				return await Result.Task;
			else
				return 0;
		}

		/// <summary>
		/// Reads a line of characters from the text reader and returns the data as a string.
		/// </summary>
		/// <returns>The next line from the reader, or null if all characters have been read.</returns>
		public static string ReadLine()
		{
			return ReadLineAsync().Result;
		}

		/// <summary>
		/// Reads a line of characters asynchronously and returns the data as a string.
		/// </summary>
		/// <returns>A task that represents the asynchronous read operation. The value of the TResult
		/// parameter contains the next line from the text reader, or is null if all of the
		/// characters have been read.</returns>
		public static async Task<string> ReadLineAsync()
		{
			TaskCompletionSource<string> Result = new TaskCompletionSource<string>();
			if (await ConsoleWorker.Queue(new ConsoleInReadLine(Result)))
				return await Result.Task;
			else
				return null;
		}

		/// <summary>
		/// Reads all characters from the current position to the end of the text reader
		/// and returns them as one string.
		/// </summary>
		/// <returns>A string that contains all characters from the current position to the end of
		/// the text reader.</returns>
		public static string ReadToEnd()
		{
			return ReadToEndAsync().Result;
		}

		/// <summary>
		/// Reads all characters from the current position to the end of the text reader
		/// asynchronously and returns them as one string.
		/// </summary>
		/// <returns>A task that represents the asynchronous read operation. The value of the TResult
		/// parameter contains a string with the characters from the current position to
		/// the end of the text reader.</returns>
		public static async Task<string> ReadToEndAsync()
		{
			TaskCompletionSource<string> Result = new TaskCompletionSource<string>();
			if (await ConsoleWorker.Queue(new ConsoleInReadToEnd(Result)))
				return await Result.Task;
			else
				return string.Empty;
		}

		/// <summary>
		/// Reads a key press
		/// </summary>
		/// <returns>Key information</returns>
		public static ConsoleKeyInfo ReadKey()
		{
			return ReadKeyAsync().Result;
		}

		/// <summary>
		/// Reads a key press
		/// </summary>
		/// <returns>Key information</returns>
		public static ConsoleKeyInfo ReadKey(bool intercept)
		{
			return ReadKeyAsync(intercept).Result;
		}

		/// <summary>
		/// Reads a key press
		/// </summary>
		/// <returns>Key information</returns>
		public static Task<ConsoleKeyInfo> ReadKeyAsync()
		{
			return ReadKeyAsync(false);
		}

		/// <summary>
		/// Reads a key press
		/// </summary>
		/// <returns>Key information</returns>
		public static async Task<ConsoleKeyInfo> ReadKeyAsync(bool intercept)
		{
			TaskCompletionSource<ConsoleKeyInfo> Result = new TaskCompletionSource<ConsoleKeyInfo>();
			if (await ConsoleWorker.Queue(new ConsoleInReadKey(intercept, Result)))
				return await Result.Task;
			else
				return new ConsoleKeyInfo();
		}

	}
}
