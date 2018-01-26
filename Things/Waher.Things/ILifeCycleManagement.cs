using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Things
{
	/// <summary>
	/// Interface for nodes whose life cycle can be provisioned.
	/// </summary>
    public interface ILifeCycleManagement : INode
    {
		/// <summary>
		/// If node can be provisioned.
		/// </summary>
		bool IsProvisioned
		{
			get;
		}

		/// <summary>
		/// Who the owner of the node is. The empty string means the node has no owner.
		/// </summary>
		string Owner
		{
			get;
		}

		/// <summary>
		/// If the node is public.
		/// </summary>
		bool IsPublic
		{
			get;
		}

		/// <summary>
		/// Gets meta-data about the node.
		/// </summary>
		/// <returns>Meta data.</returns>
		Task<KeyValuePair<string, object>[]> GetMetaData();

		/// <summary>
		/// Called when node has been claimed by an owner.
		/// </summary>
		/// <param name="Owner">Owner</param>
		/// <param name="IsPublic">If node is public.</param>
		Task Claimed(string Owner, bool IsPublic);

		/// <summary>
		/// Called when node has been disowned by its owner.
		/// </summary>
		Task Disowned();

		/// <summary>
		/// Called when node has been removed from the registry.
		/// </summary>
		Task Removed();
	}
}
