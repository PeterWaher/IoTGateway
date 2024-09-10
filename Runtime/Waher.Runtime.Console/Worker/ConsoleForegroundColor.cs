using System;
using System.Threading.Tasks;

namespace Waher.Runtime.Console.Worker
{
	/// <summary>
	/// Changes the console foreground color.
	/// </summary>
	public class ConsoleForegroundColor : WorkItem
	{
		private readonly ConsoleColor foregroundColor;

		/// <summary>
		/// Changes the console foreground color.
		/// </summary>
		/// <param name="ForegroundColor">New foreground color</param>
		public ConsoleForegroundColor(ConsoleColor ForegroundColor)
		{
			this.foregroundColor = ForegroundColor;
		}

		/// <summary>
		/// Executes the console operation.
		/// </summary>
		public override Task Execute()
		{
			System.Console.ForegroundColor = this.foregroundColor;
			return Task.CompletedTask;
		}
	}
}
