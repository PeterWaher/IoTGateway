using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Events;
using Waher.Networking.XMPP.Contracts;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Runtime.Collections;
using Waher.Runtime.Threading;

namespace Waher.Mcp.Identity
{
	/// <summary>
	/// Static class managing the petition cache.
	/// </summary>
	public static class PetitionCache
	{
		/// <summary>
		/// Tries to get a cached object, given its URI and the user name of the user
		/// requesting the object.
		/// </summary>
		/// <param name="UserName">User name.</param>
		/// <param name="Uri">URI</param>
		/// <param name="ContractsClient">Contracts client.</param>
		/// <returns></returns>
		public static async Task<object?> TryGetObject(string UserName, Uri Uri,
			ContractsClient ContractsClient)
		{
			if (Uri.Scheme == "iotid")
				return await TryGetLegalIdentity(UserName, Uri.AbsolutePath);
			else if (Uri.Scheme == "iotsc")
				return await TryGetContract(UserName, Uri.AbsolutePath, ContractsClient);
			else
				return null;
		}

		/// <summary>
		/// Tries to get a cached legal identity, given its Legal ID and the corresponding
		/// user name.
		/// </summary>
		/// <param name="UserName">MCP user name.</param>
		/// <param name="LegalId">Legal Identifier.</param>
		/// <returns>Legal Identity, if found.</returns>
		/// <remarks>A user can only access legal identities earlier authorized, by the
		/// corresponding owner.</remarks>
		public static async Task<LegalIdentity?> TryGetLegalIdentity(
			string UserName, string LegalId)
		{
			string Uri = "iotid:" + LegalId;
			string Key = UserName + ":" + Uri;
			using Semaphore Lock = await Semaphores.BeginWrite(Key);

			CachedPetitionItem? Item = await Database.FindFirstIgnoreRest<CachedPetitionItem>(
				new FilterAnd(
					new FilterFieldEqualTo("McpUserName", UserName),
					new FilterFieldEqualTo("Uri", Uri)));

			if (Item?.Xml is null)
				return null;

			try
			{
				return LegalIdentity.Parse(Item.Xml);
			}
			catch (Exception)
			{
				await Database.Delete(Item);
				return null;
			}
		}

		/// <summary>
		/// Adds a legal identity to the cache.
		/// </summary>
		/// <param name="UserName">MCP user name.</param>
		/// <param name="Identity">Legal identity object.</param>
		/// <returns>If the object was added or updated (true) or if an identical object
		/// was already available in the cache (false).</returns>
		public static async Task<bool> AddLegalIdentity(string UserName, LegalIdentity Identity)
		{
			string Uri = "iotid:" + Identity.Id;
			string Key = UserName + ":" + Uri;
			using Semaphore Lock = await Semaphores.BeginWrite(Key);

			StringBuilder sb = new StringBuilder();
			Identity.Serialize(sb, true, true, true, true, true, true, true);
			string Xml = sb.ToString();

			CachedPetitionItem? Item = await Database.FindFirstIgnoreRest<CachedPetitionItem>(
				new FilterAnd(
					new FilterFieldEqualTo("McpUserName", UserName),
					new FilterFieldEqualTo("Uri", Uri)));

			if (Item is null)
			{
				Item = new CachedPetitionItem()
				{
					McpUserName = UserName,
					Uri = Uri,
					Xml = Xml
				};

				await Database.Insert(Item);

				return true;
			}
			else if (Item.Xml != Xml)
			{
				Item.Xml = Xml;
				await Database.Update(Item);

				return true;
			}
			else
				return false;
		}

		/// <summary>
		/// Tries to get a cached smart contract, given its Contract ID and the corresponding
		/// user name.
		/// </summary>
		/// <param name="UserName">MCP user name.</param>
		/// <param name="ContractId">Contract Identifier.</param>
		/// <param name="ContractsClient">Contracts client.</param>
		/// <returns>Contract, if found.</returns>
		/// <remarks>A user can only access smart contracts earlier authorized, by the
		/// corresponding parts of the contract.</remarks>
		public static async Task<Contract?> TryGetContract(
			string UserName, string ContractId, ContractsClient ContractsClient)
		{
			string Uri = "iotsc:" + ContractId;
			string Key = UserName + ":" + Uri;
			using Semaphore Lock = await Semaphores.BeginWrite(Key);

			CachedPetitionItem? Item = await Database.FindFirstIgnoreRest<CachedPetitionItem>(
				new FilterAnd(
					new FilterFieldEqualTo("McpUserName", UserName),
					new FilterFieldEqualTo("Uri", Uri)));

			if (Item?.Xml is null)
				return null;

			try
			{
				XmlDocument Doc = new XmlDocument();
				Doc.LoadXml(Item.Xml);
				ParsedContract? Parsed = await Contract.Parse(Doc.DocumentElement, ContractsClient, false);
				if (Parsed?.Contract is null)
				{
					await Database.Delete(Item);
					return null;
				}

				return Parsed.Contract;
			}
			catch (Exception)
			{
				await Database.Delete(Item);
				return null;
			}
		}

		/// <summary>
		/// Adds a smart contract to the cache.
		/// </summary>
		/// <param name="UserName">MCP user name.</param>
		/// <param name="Contract">Contract object.</param>
		/// <returns>If the object was added or updated (true) or if an identical object
		/// was already available in the cache (false).</returns>
		public static async Task<bool> AddContract(string UserName, Contract Contract)
		{
			string Uri = "iotsc:" + Contract.ContractId;
			string Key = UserName + ":" + Uri;
			using Semaphore Lock = await Semaphores.BeginWrite(Key);

			StringBuilder sb = new StringBuilder();
			Contract.Serialize(sb, true, true, true, true, true, true, true);
			string Xml = sb.ToString();

			CachedPetitionItem? Item = await Database.FindFirstIgnoreRest<CachedPetitionItem>(
				new FilterAnd(
					new FilterFieldEqualTo("McpUserName", UserName),
					new FilterFieldEqualTo("Uri", Uri)));

			if (Item is null)
			{
				Item = new CachedPetitionItem()
				{
					McpUserName = UserName,
					Uri = Uri,
					Xml = Xml
				};

				await Database.Insert(Item);

				return true;
			}
			else if (Item.Xml != Xml)
			{
				Item.Xml = Xml;
				await Database.Update(Item);

				return true;
			}
			else
				return false;
		}

		/// <summary>
		/// Gets cached objects for a user.
		/// </summary>
		/// <param name="UserName">User name.</param>
		/// <param name="ContractsClient">Contracts client.</param>
		/// <returns>Array of cached objects.</returns>
		public static async Task<object[]> GetCachedObjects(string UserName, 
			ContractsClient ContractsClient)
		{
			ChunkedList<object> Items = new ChunkedList<object>();

			foreach (CachedPetitionItem Item in await Database.Find<CachedPetitionItem>(
				new FilterFieldEqualTo("McpUserName", UserName)))
			{
				if (Item.Uri is null)
					continue;

				try
				{
					if (Item.Uri.StartsWith("iotid:"))
						Items.Add(LegalIdentity.Parse(Item.Xml));
					else if (Item.Uri.StartsWith("iotsc:"))
					{
						XmlDocument Doc = new XmlDocument();
						Doc.LoadXml(Item.Xml);
						Items.Add(await Contract.Parse(Doc, ContractsClient));
					}
				}
				catch (Exception ex)
				{
					Log.Error("MCP Petition cache contains item with invalid XML.",
						new KeyValuePair<string, object?>("User", UserName),
						new KeyValuePair<string, object?>("URI", Item.Uri),
						new KeyValuePair<string, object?>("Object ID", Item.ObjectID),
						new KeyValuePair<string, object?>("Message", ex.Message));
				}
			}

			return Items.ToArray();
		}
	}
}
