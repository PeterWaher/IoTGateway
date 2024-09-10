using System;
using System.Threading.Tasks;

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
		public override Task Execute()
		{
			System.Console.BackgroundColor = this.backgroundColor;
			return Task.CompletedTask;
		}
	}
}
