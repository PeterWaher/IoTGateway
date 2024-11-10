using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking;
using Waher.Networking.Sniffers;
using Waher.Persistence;

namespace Waher.IoTGateway.Setup.Databases.Sniffing
{
    /// <summary>
    /// Class that can be used to sniff on ledger activity.
    /// </summary>
    public class SniffableLedger : CommunicationLayer
	{
		private bool prevHasSniffers = false;

		/// <summary>
		/// Class that can be used to sniff on ledger activity.
		/// </summary>
		public SniffableLedger()
			: base()
		{
		}

		/// <summary>
		/// <see cref="ICommunicationLayer.Add"/>
		/// </summary>
		public override void Add(ISniffer Sniffer)
		{
			base.Add(Sniffer);
			this.CheckEventHandlers();
		}

		/// <summary>
		/// <see cref="ICommunicationLayer.AddRange"/>
		/// </summary>
		public override void AddRange(IEnumerable<ISniffer> Sniffers)
		{
			base.AddRange(Sniffers);
			this.CheckEventHandlers();
		}

		/// <summary>
		/// <see cref="ICommunicationLayer.Remove"/>
		/// </summary>
		public override bool Remove(ISniffer Sniffer)
		{
			bool Result = base.Remove(Sniffer);
			this.CheckEventHandlers();
			return Result;
		}

		private void CheckEventHandlers()
		{
			bool b = this.HasSniffers;

			if (this.prevHasSniffers != b)
			{
				if (b)
				{
					Ledger.CollectionCleared += this.Ledger_CollectionCleared;
					Ledger.EntryAdded += this.Ledger_EntryAdded;
					Ledger.EntryUpdated += this.Ledger_EntryUpdated;
					Ledger.EntryDeleted += this.Ledger_EntryDeleted;
				}
				else
				{
					Ledger.CollectionCleared -= this.Ledger_CollectionCleared;
					Ledger.EntryAdded -= this.Ledger_EntryAdded;
					Ledger.EntryUpdated -= this.Ledger_EntryUpdated;
					Ledger.EntryDeleted -= this.Ledger_EntryDeleted;
				}

				this.prevHasSniffers = b;
			}
		}

		private async void Ledger_EntryAdded(object Sender, ObjectEventArgs e)
		{
			try
			{
				await this.TransmitText(await SniffableDatabase.GetJSON(e.Object));
			}
			catch (Exception)
			{
				await this.TransmitText("Entry of type " + e.Object.GetType().FullName + " added.");
			}
		}

		private async void Ledger_EntryUpdated(object Sender, ObjectEventArgs e)
		{
			try
			{
				await this.ReceiveText(await SniffableDatabase.GetJSON(e.Object));
			}
			catch (Exception)
			{
				await this.ReceiveText("Entry of type " + e.Object.GetType().FullName + " updated.");
			}
		}

		private async void Ledger_EntryDeleted(object Sender, ObjectEventArgs e)
		{
			try
			{
				await this.Error(await SniffableDatabase.GetJSON(e.Object));
			}
			catch (Exception)
			{
				await this.Error("Entry of type " + e.Object.GetType().FullName + " deleted.");
			}
		}

		private Task Ledger_CollectionCleared(object Sender, CollectionEventArgs e)
		{
			return this.Information("Collection has been cleared: " + e.Collection);
		}
	}
}
