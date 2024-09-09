using System;
using System.Threading.Tasks;
using Waher.Events;

namespace Waher.Runtime.Console.Worker
{
	/// <summary>
	/// Performs custom writing
	/// </summary>
	public class ConsoleOutCustomAsyncWriter : WorkItem
	{
		private readonly CustomAsyncWriter writer;

		/// <summary>
		/// Performs custom writing
		/// </summary>
		/// <param name="Writer">Custom writer.</param>
		public ConsoleOutCustomAsyncWriter(CustomAsyncWriter Writer)
		{
			this.writer = Writer;
		}

		/// <summary>
		/// Executes the console operation.
		/// </summary>
		public override async Task Execute()
		{
			try
			{
				await this.writer(System.Console.Out);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}
	}
}
