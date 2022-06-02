using System;
using System.Collections.Generic;
using Waher.Content;
using Waher.Networking.Sniffers;
using Waher.Persistence;
using Waher.Persistence.Serialization;

namespace Waher.IoTGateway.Setup.Databases.Sniffing
{
	/// <summary>
	/// Class that can be used to sniff on ledger activity.
	/// </summary>
	public class SniffableLedger : Sniffable
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
		/// <see cref="ISniffable.Add"/>
		/// </summary>
		public override void Add(ISniffer Sniffer)
		{
			base.Add(Sniffer);
			this.CheckEventHandlers();
		}

		/// <summary>
		/// <see cref="ISniffable.AddRange"/>
		/// </summary>
		public override void AddRange(IEnumerable<ISniffer> Sniffers)
		{
			base.AddRange(Sniffers);
			this.CheckEventHandlers();
		}

		/// <summary>
		/// <see cref="ISniffable.Remove"/>
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
					Ledger.CollectionCleared += Ledger_CollectionCleared;
					Ledger.EntryAdded += Ledger_EntryAdded;
					Ledger.EntryUpdated += Ledger_EntryUpdated;
					Ledger.EntryDeleted += Ledger_EntryDeleted;
				}
				else
				{
					Ledger.CollectionCleared -= Ledger_CollectionCleared;
					Ledger.EntryAdded -= Ledger_EntryAdded;
					Ledger.EntryUpdated -= Ledger_EntryUpdated;
					Ledger.EntryDeleted -= Ledger_EntryDeleted;
				}

				this.prevHasSniffers = b;
			}
		}

		private async void Ledger_EntryAdded(object Sender, ObjectEventArgs e)
		{
			try
			{
				this.TransmitText(await SniffableDatabase.GetJSON(e.Object));
			}
			catch (Exception)
			{
				this.TransmitText("Entry of type " + e.Object.GetType().FullName + " added.");
			}
		}

		private async void Ledger_EntryUpdated(object Sender, ObjectEventArgs e)
		{
			try
			{
				this.ReceiveText(await SniffableDatabase.GetJSON(e.Object));
			}
			catch (Exception)
			{
				this.ReceiveText("Entry of type " + e.Object.GetType().FullName + " updated.");
			}
		}

		private async void Ledger_EntryDeleted(object Sender, ObjectEventArgs e)
		{
			try
			{
				this.Error(await SniffableDatabase.GetJSON(e.Object));
			}
			catch (Exception)
			{
				this.Error("Entry of type " + e.Object.GetType().FullName + " deleted.");
			}
		}

		private void Ledger_CollectionCleared(object Sender, CollectionEventArgs e)
		{
			this.Information("Collection has been cleared: " + e.Collection);
		}
	}
}
