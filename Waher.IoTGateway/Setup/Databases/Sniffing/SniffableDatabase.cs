using System;
using System.Collections.Generic;
using Waher.Content;
using Waher.Networking.Sniffers;
using Waher.Persistence;
using Waher.Persistence.Serialization;

namespace Waher.IoTGateway.Setup.Databases.Sniffing
{
	/// <summary>
	/// Class that can be used to sniff on database updates.
	/// </summary>
	public class SniffableDatabase : Sniffable
	{
		private bool prevHasSniffers = false;

		/// <summary>
		/// Class that can be used to sniff on database updates.
		/// </summary>
		public SniffableDatabase()
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
					Database.CollectionCleared += Database_CollectionCleared;
					Database.CollectionRepaired += Database_CollectionRepaired;
					Database.ObjectDeleted += Database_ObjectDeleted;
					Database.ObjectInserted += Database_ObjectInserted;
					Database.ObjectUpdated += Database_ObjectUpdated;
				}
				else
				{
					Database.CollectionCleared -= Database_CollectionCleared;
					Database.CollectionRepaired -= Database_CollectionRepaired;
					Database.ObjectDeleted -= Database_ObjectDeleted;
					Database.ObjectInserted -= Database_ObjectInserted;
					Database.ObjectUpdated -= Database_ObjectUpdated;
				}

				this.prevHasSniffers = b;
			}
		}

		private async void Database_ObjectInserted(object Sender, ObjectEventArgs e)
		{
			try
			{
				GenericObject Obj = await Database.Generalize(e.Object);
				string s = JSON.Encode(Obj, true);
				this.TransmitText(s);
			}
			catch (Exception)
			{
				this.TransmitText("Object of type " + e.Object.GetType().FullName + " inserted.");
			}
		}

		private async void Database_ObjectUpdated(object Sender, ObjectEventArgs e)
		{
			try
			{
				GenericObject Obj = await Database.Generalize(e.Object);
				string s = JSON.Encode(Obj, true);
				this.ReceiveText(s);
			}
			catch (Exception)
			{
				this.ReceiveText("Object of type " + e.Object.GetType().FullName + " updated.");
			}
		}

		private async void Database_ObjectDeleted(object Sender, ObjectEventArgs e)
		{
			try
			{
				GenericObject Obj = await Database.Generalize(e.Object);
				string s = JSON.Encode(Obj, true);
				this.Error(s);
			}
			catch (Exception)
			{
				this.Error("Object of type " + e.Object.GetType().FullName + " deleted.");
			}
		}

		private async void Database_CollectionRepaired(object Sender, CollectionRepairedEventArgs e)
		{
			try
			{
				this.Warning("Collection has been repaired: " + e.Collection);

				if (!(e.Flagged is null))
				{
					foreach (FlagSource Source in e.Flagged)
						await this.Exception("Flagged " + Source.Count.ToString() + " time(s) for:" + Source.Reason + "\r\n\r\n" + Source.StackTrace);
				}
			}
			catch (Exception)
			{
				// Ignore.
			}
		}

		private void Database_CollectionCleared(object Sender, CollectionEventArgs e)
		{
			this.Information("Collection has been cleared: " + e.Collection);
		}
	}
}
