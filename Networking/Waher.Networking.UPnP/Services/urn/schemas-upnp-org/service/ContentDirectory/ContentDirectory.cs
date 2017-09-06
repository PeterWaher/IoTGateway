using System;
using System.Collections.Generic;

namespace Waher.Networking.UPnP.Services
{
//#pragma warning disable
	/// <summary>
	/// Generated from SCPD
	/// </summary>
	public class ContentDirectory
	{
		private ServiceDescriptionDocument service;
		private UPnPAction actionGetSearchCapabilities = null;
		private UPnPAction actionGetSortCapabilities = null;
		private UPnPAction actionGetSystemUpdateID = null;
		private UPnPAction actionBrowse = null;
		private UPnPAction actionSearch = null;
		private UPnPAction actionUpdateObject = null;
		private UPnPAction actionX_GetRemoteSharingStatus = null;

		/// <summary>
		/// Generated from SCPD
		/// </summary>
		public ContentDirectory(ServiceDescriptionDocument Service)
		{
			this.service = Service;
		}

		/// <summary>
		/// Generated from SCPD
		/// </summary>
		public void GetSearchCapabilities(out string SearchCaps)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionGetSearchCapabilities == null)
				actionGetSearchCapabilities = this.service.GetAction("GetSearchCapabilities");

			this.actionGetSearchCapabilities.Invoke(out OutputValues);

			SearchCaps = (string)OutputValues["SearchCaps"];
		}

		/// <summary>
		/// Generated from SCPD
		/// </summary>
		public void GetSortCapabilities(out string SortCaps)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionGetSortCapabilities == null)
				actionGetSortCapabilities = this.service.GetAction("GetSortCapabilities");

			this.actionGetSortCapabilities.Invoke(out OutputValues);

			SortCaps = (string)OutputValues["SortCaps"];
		}

		/// <summary>
		/// Generated from SCPD
		/// </summary>
		public void GetSystemUpdateID(out uint Id)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionGetSystemUpdateID == null)
				actionGetSystemUpdateID = this.service.GetAction("GetSystemUpdateID");

			this.actionGetSystemUpdateID.Invoke(out OutputValues);

			Id = (uint)OutputValues["Id"];
		}

		/// <summary>
		/// Generated from SCPD
		/// </summary>
		public void Browse(string ObjectID, string BrowseFlag, string Filter, uint StartingIndex, uint RequestedCount, string SortCriteria, out string Result, out uint NumberReturned, out uint TotalMatches, out uint UpdateID)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionBrowse == null)
				actionBrowse = this.service.GetAction("Browse");

			this.actionBrowse.Invoke(out OutputValues,
				new KeyValuePair<string, object>("ObjectID", ObjectID),
				new KeyValuePair<string, object>("BrowseFlag", BrowseFlag),
				new KeyValuePair<string, object>("Filter", Filter),
				new KeyValuePair<string, object>("StartingIndex", StartingIndex),
				new KeyValuePair<string, object>("RequestedCount", RequestedCount),
				new KeyValuePair<string, object>("SortCriteria", SortCriteria));

			Result = (string)OutputValues["Result"];
			NumberReturned = (uint)OutputValues["NumberReturned"];
			TotalMatches = (uint)OutputValues["TotalMatches"];
			UpdateID = (uint)OutputValues["UpdateID"];
		}

		/// <summary>
		/// Generated from SCPD
		/// </summary>
		public void Search(string ContainerID, string SearchCriteria, string Filter, uint StartingIndex, uint RequestedCount, string SortCriteria, out string Result, out uint NumberReturned, out uint TotalMatches, out uint UpdateID)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionSearch == null)
				actionSearch = this.service.GetAction("Search");

			this.actionSearch.Invoke(out OutputValues,
				new KeyValuePair<string, object>("ContainerID", ContainerID),
				new KeyValuePair<string, object>("SearchCriteria", SearchCriteria),
				new KeyValuePair<string, object>("Filter", Filter),
				new KeyValuePair<string, object>("StartingIndex", StartingIndex),
				new KeyValuePair<string, object>("RequestedCount", RequestedCount),
				new KeyValuePair<string, object>("SortCriteria", SortCriteria));

			Result = (string)OutputValues["Result"];
			NumberReturned = (uint)OutputValues["NumberReturned"];
			TotalMatches = (uint)OutputValues["TotalMatches"];
			UpdateID = (uint)OutputValues["UpdateID"];
		}

		/// <summary>
		/// Generated from SCPD
		/// </summary>
		public void UpdateObject(string ObjectID, string CurrentTagValue, string NewTagValue)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionUpdateObject == null)
				actionUpdateObject = this.service.GetAction("UpdateObject");

			this.actionUpdateObject.Invoke(out OutputValues,
				new KeyValuePair<string, object>("ObjectID", ObjectID),
				new KeyValuePair<string, object>("CurrentTagValue", CurrentTagValue),
				new KeyValuePair<string, object>("NewTagValue", NewTagValue));
		}

		/// <summary>
		/// Generated from SCPD
		/// </summary>
		public void X_GetRemoteSharingStatus(out bool Status)
		{
			Dictionary<string, object> OutputValues = new Dictionary<string, object>();
			
			if (actionX_GetRemoteSharingStatus == null)
				actionX_GetRemoteSharingStatus = this.service.GetAction("X_GetRemoteSharingStatus");

			this.actionX_GetRemoteSharingStatus.Invoke(out OutputValues);

			Status = (bool)OutputValues["Status"];
		}
	}
//#pragma warning restore
}