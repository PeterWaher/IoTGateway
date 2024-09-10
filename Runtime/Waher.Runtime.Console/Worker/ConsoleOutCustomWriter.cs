using System;
using System.Threading.Tasks;

namespace Waher.Runtime.Console.Worker
{
	/// <summary>
	/// Performs custom writing
	/// </summary>
	public class ConsoleOutCustomWriter : ConsoleCustomWriter
	{
		private readonly CustomWriter writer;

		/// <summary>
		/// Performs custom writing
		/// </summary>
		/// <param name="Writer">Custom writer.</param>
		/// <param name="ForegroundColor">Optional Foreground Color to use.</param>
		/// <param name="BackgroundColor">Optional Background Color to use.</param>
		public ConsoleOutCustomWriter(CustomWriter Writer, ConsoleColor? ForegroundColor, ConsoleColor? BackgroundColor)
			: base(ForegroundColor, BackgroundColor) 
		{
			this.writer = Writer;
		}

		/// <summary>
		/// Method that does the actual custom writing.
		/// </summary>
		protected override Task DoWrite()
		{
			this.writer(System.Console.Out);
			return Task.CompletedTask;
		}
	}
}
