using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;

namespace Waher.Client.WPF.Controls.Logs
{
	public class LogSink : EventSink
	{
		private readonly LogView view;

		public LogSink(LogView View)
			: base("Main Window Log")
		{
			this.view = View;
		}

		public override Task Queue(Event Event)
		{
			this.view.Add(new LogItem(Event));
			return Task.CompletedTask;
		}
	}
}
