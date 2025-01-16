using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Waher.Networking.Sniffers;
using Waher.Networking.Sniffers.Model;

namespace Waher.Client.WPF.Controls.Sniffers
{
	public class TabSniffer(SnifferView View) : SnifferBase
	{
		private readonly SnifferView view = View;
		private string? snifferId = null;

		public SnifferView View => this.view;

		public string? SnifferId
		{
			get => this.snifferId;
			set => this.snifferId = value;
		}

		public override Task Process(SnifferRxBinary Event)
		{
			this.view.Add(new SniffItem(Event.Timestamp, SniffItemType.DataReceived, HexToString(Event.Data, Event.Offset, Event.Count), 
				CloneSection(Event.Data, Event.Offset, Event.Count), Colors.White, Colors.Navy));

			return Task.CompletedTask;
		}

		public override Task Process(SnifferTxBinary Event)
		{
			this.view.Add(new SniffItem(Event.Timestamp, SniffItemType.DataTransmitted, HexToString(Event.Data, Event.Offset, Event.Count), 
				CloneSection(Event.Data, Event.Offset, Event.Count), Colors.Black, Colors.White));
			return Task.CompletedTask;
		}

		internal static string HexToString(byte[] Data, int Offset, int Count)
		{
			StringBuilder Output = new();
			int i = 0;
			byte b;

			while (Count-- > 0)
			{
				b = Data[Offset++];

				if (i > 0)
					Output.Append(' ');

				Output.Append(b.ToString("X2"));

				i = (i + 1) & 31;
				if (i == 0)
					Output.AppendLine();
			}

			return Output.ToString().TrimEnd();
		}

		public override Task Process(SnifferRxText Event)
		{
			this.view.Add(new SniffItem(Event.Timestamp, SniffItemType.TextReceived, Event.Text, null, Colors.White, Colors.Navy));
			return Task.CompletedTask;
		}

		public override Task Process(SnifferTxText Event)
		{
			this.view.Add(new SniffItem(Event.Timestamp, SniffItemType.TextTransmitted, Event.Text, null, Colors.Black, Colors.White));
			return Task.CompletedTask;
		}

		public override Task Process(SnifferInformation Event)
		{
			this.view.Add(new SniffItem(Event.Timestamp, SniffItemType.Information,	Event.Text, null, Colors.Yellow, Colors.DarkGreen));
			return Task.CompletedTask;
		}

		public override Task Process(SnifferWarning Event)
		{
			this.view.Add(new SniffItem(Event.Timestamp, SniffItemType.Warning, Event.Text, null, Colors.Black, Colors.Yellow));
			return Task.CompletedTask;
		}

		public override Task Process(SnifferError Event)
		{
			this.view.Add(new SniffItem(Event.Timestamp, SniffItemType.Error, Event.Text, null, Colors.White, Colors.Red));
			return Task.CompletedTask;
		}

		public override Task Process(SnifferException Event)
		{
			this.view.Add(new SniffItem(Event.Timestamp, SniffItemType.Exception, Event.Text, null, Colors.White, Colors.DarkRed));
			return Task.CompletedTask;
		}
	}
}
