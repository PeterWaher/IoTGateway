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

		/// <summary>
		/// Performs custom writing
		/// </summary>
		/// <param name="Writer">Custom writer.</param>
		public ConsoleOutCustomWriter(CustomWriter Writer)
		{
			this.writer = Writer;
		}

		/// <summary>
		/// Executes the console operation.
		/// </summary>
		public override Task Execute()
		{
			try
			{
				this.writer(System.Console.Out);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}

			return Task.CompletedTask;
		}
	}
}
