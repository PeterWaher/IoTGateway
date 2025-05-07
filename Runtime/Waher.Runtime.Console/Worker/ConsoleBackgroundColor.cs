using System;
using System.Threading;
using System.Threading.Tasks;
using Waher.Runtime.Queue;

namespace Waher.Runtime.Console.Worker
{
	/// <summary>
	/// Changes the console background color.
	/// </summary>
	public class ConsoleBackgroundColor : WorkItem
	{
		private readonly ConsoleColor backgroundColor;

		/// <summary>
		/// Changes the console background color.
		/// </summary>
		/// <param name="BackgroundColor">New background color</param>
		public ConsoleBackgroundColor(ConsoleColor BackgroundColor)
		{
			this.backgroundColor = BackgroundColor;
		}

		/// <summary>
		/// Executes the console operation.
		/// </summary>
		/// <param name="Cancel">Cancellation token.</param>
		public override Task Execute(CancellationToken Cancel)
		{
			System.Console.BackgroundColor = this.backgroundColor;
			return Task.CompletedTask;
		}
	}
}
