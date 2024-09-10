using System;
using System.Threading.Tasks;
using Waher.Events;

namespace Waher.Runtime.Console.Worker
{
	/// <summary>
	/// Performs custom writing
	/// </summary>
	public class ConsoleOutCustomWriter : WorkItem
	{
		private readonly CustomWriter writer;
		private readonly ConsoleColor? foregroundColor;
		private readonly ConsoleColor? backgroundColor;

		/// <summary>
		/// Performs custom writing
		/// </summary>
		/// <param name="Writer">Custom writer.</param>
		/// <param name="ForegroundColor">Optional Foreground Color to use.</param>
		/// <param name="BackgroundColor">Optional Background Color to use.</param>
		public ConsoleOutCustomWriter(CustomWriter Writer, ConsoleColor? ForegroundColor, ConsoleColor? BackgroundColor)
		{
			this.writer = Writer;
			this.foregroundColor = ForegroundColor;
			this.backgroundColor = BackgroundColor;
		}

		/// <summary>
		/// Executes the console operation.
		/// </summary>
		public override Task Execute()
		{
			try
			{
				ConsoleColor FgBak = System.Console.ForegroundColor;
				ConsoleColor BgBak = System.Console.BackgroundColor;

				if (this.foregroundColor.HasValue)
					System.Console.ForegroundColor = this.foregroundColor.Value;

				if (this.backgroundColor.HasValue)
					System.Console.BackgroundColor = this.backgroundColor.Value;

				this.writer(System.Console.Out);

				System.Console.ForegroundColor = FgBak;
				System.Console.BackgroundColor = BgBak;
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}

			return Task.CompletedTask;
		}
	}
}
