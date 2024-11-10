using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking;
using Waher.Networking.Sniffers;
using Waher.Persistence;
using Waher.Persistence.FullTextSearch;
using Waher.Persistence.Serialization;

namespace Waher.IoTGateway.Setup.Databases.Sniffing
{
    /// <summary>
    /// Class that can be used to sniff on database updates.
    /// </summary>
    public class SniffableDatabase : CommunicationLayer
	{
		private bool prevHasSniffers = false;

		/// <summary>
		/// Class that can be used to sniff on database updates.
		/// </summary>
		public SniffableDatabase()
			: base(false)
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
					Database.CollectionCleared += this.Database_CollectionCleared;
					Database.CollectionRepaired += this.Database_CollectionRepaired;
					Database.ObjectDeleted += this.Database_ObjectDeleted;
					Database.ObjectInserted += this.Database_ObjectInserted;
					Database.ObjectUpdated += this.Database_ObjectUpdated;

					Search.ObjectAddedToIndex += this.Search_ObjectAddedToIndex;
					Search.ObjectUpdatedInIndex += this.Search_ObjectUpdatedInIndex;
					Search.ObjectRemovedFromIndex += this.Search_ObjectRemovedFromIndex;
				}
				else
				{
					Database.CollectionCleared -= this.Database_CollectionCleared;
					Database.CollectionRepaired -= this.Database_CollectionRepaired;
					Database.ObjectDeleted -= this.Database_ObjectDeleted;
					Database.ObjectInserted -= this.Database_ObjectInserted;
					Database.ObjectUpdated -= this.Database_ObjectUpdated;

					Search.ObjectAddedToIndex -= this.Search_ObjectAddedToIndex;
					Search.ObjectUpdatedInIndex -= this.Search_ObjectUpdatedInIndex;
					Search.ObjectRemovedFromIndex -= this.Search_ObjectRemovedFromIndex;
				}

				this.prevHasSniffers = b;
			}
		}

		internal static async Task<string> GetJSON(object Object)
		{
			GenericObject Obj = await Database.Generalize(Object);
			Dictionary<string, object> Obj2 = new Dictionary<string, object>()
			{
				{ "ObjectId", Obj.ObjectId },
				{ "TypeName", Obj.TypeName },
				{ "CollectionName", Obj.CollectionName }
			};

			foreach (KeyValuePair<string, object> P in Obj)
				Obj2[P.Key] = P.Value;

			return JSON.Encode(Obj2, true);
		}

		private async void Database_ObjectInserted(object Sender, ObjectEventArgs e)
		{
			try
			{
				await this.TransmitText(await GetJSON(e.Object));
			}
			catch (Exception)
			{
				await this.TransmitText("Object of type " + e.Object.GetType().FullName + " inserted.");
			}
		}

		private async void Database_ObjectUpdated(object Sender, ObjectEventArgs e)
		{
			try
			{
				await this.ReceiveText(await GetJSON(e.Object));
			}
			catch (Exception)
			{
				await this.ReceiveText("Object of type " + e.Object.GetType().FullName + " updated.");
			}
		}

		private async void Database_ObjectDeleted(object Sender, ObjectEventArgs e)
		{
			try
			{
				await this.Error(await GetJSON(e.Object));
			}
			catch (Exception)
			{
				await this.Error("Object of type " + e.Object.GetType().FullName + " deleted.");
			}
		}

		private async Task Database_CollectionRepaired(object Sender, CollectionRepairedEventArgs e)
		{
			try
			{
				await this.Warning("Collection has been repaired: " + e.Collection);

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

		private Task Database_CollectionCleared(object Sender, CollectionEventArgs e)
		{
			return this.Information("Collection has been cleared: " + e.Collection);
		}

		private Task Search_ObjectAddedToIndex(object Sender, ObjectReferenceEventArgs e)
		{
			return this.Information("Object added to full-text-search index: " + e.Reference.IndexCollection);
		}

		private Task Search_ObjectUpdatedInIndex(object Sender, ObjectReferenceEventArgs e)
		{
			return this.Information("Object updated in full-text-search index: " + e.Reference.IndexCollection);
		}

		private Task Search_ObjectRemovedFromIndex(object Sender, ObjectReferenceEventArgs e)
		{
			return this.Information("Object removed from full-text-search index: " + e.Reference.IndexCollection);
		}
	}
}
