using System;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Runtime.Queue;

namespace Waher.Runtime.Console.Worker
{
	/// <summary>
	/// Abstract base class for custom writers
	/// </summary>
	public abstract class ConsoleCustomWriter : WorkItem
	{
		private readonly ConsoleColor? foregroundColor;
		private readonly ConsoleColor? backgroundColor;

		/// <summary>
		/// Abstract base class for custom writers
		/// </summary>
		/// <param name="ForegroundColor">Optional Foreground Color to use.</param>
		/// <param name="BackgroundColor">Optional Background Color to use.</param>
		public ConsoleCustomWriter(ConsoleColor? ForegroundColor, ConsoleColor? BackgroundColor)
		{
			this.foregroundColor = ForegroundColor;
			this.backgroundColor = BackgroundColor;
		}

		/// <summary>
		/// Executes the console operation.
		/// </summary>
		/// <param name="Cancel">Cancellation token.</param>
		public override sealed async Task Execute(CancellationToken Cancel)
		{
			try
			{
				ConsoleColor FgBak = System.Console.ForegroundColor;
				ConsoleColor BgBak = System.Console.BackgroundColor;

				if (this.foregroundColor.HasValue)
					System.Console.ForegroundColor = this.foregroundColor.Value;

				if (this.backgroundColor.HasValue)
					System.Console.BackgroundColor = this.backgroundColor.Value;

				await this.DoWrite();

				System.Console.ForegroundColor = FgBak;
				System.Console.BackgroundColor = BgBak;
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		/// <summary>
		/// Method that does the actual custom writing.
		/// </summary>
		protected abstract Task DoWrite();

	}
}
