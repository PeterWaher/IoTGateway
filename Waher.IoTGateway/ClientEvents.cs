using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Networking.HTTP;
using Waher.Runtime.Cache;

namespace Waher.IoTGateway
{
	public class ClientEvents : HttpAsynchronousResource, IHttpPostMethod
	{
		public ClientEvents()
			: base("/ClientEvents")
		{
		}

		public override bool HandlesSubPaths
		{
			get
			{
				return false;
			}
		}

		public override bool UserSessions
		{
			get
			{
				return true;
			}
		}

		public void POST(HttpRequest Request, HttpResponse Response)
		{
			if (!Request.HasData || Request.Session == null)
				throw new BadRequestException();

			// TODO: Check User authenticated

			object Obj = Request.DecodeData();
			string Location = Obj as string;
			string TabID = Request.Header["X-TabID"];

			if (Location == null || string.IsNullOrEmpty(TabID))
				throw new BadRequestException();

			Uri Uri = new Uri(Location);
			string Resource = Uri.LocalPath;
			List<KeyValuePair<string, string>> Query = null;
			string s;

			if (!string.IsNullOrEmpty(Uri.Query))
			{
				Query = new List<KeyValuePair<string, string>>();
				int i;

				s = Uri.Query;
				if (s.StartsWith("?"))
					s = s.Substring(1);

				foreach (string Part in s.Split('&'))
				{
					i = Part.IndexOf('=');
					if (i < 0)
						Query.Add(new KeyValuePair<string, string>(Part, string.Empty));
					else
						Query.Add(new KeyValuePair<string, string>(Part.Substring(0, i), Part.Substring(i + 1)));
				}
			}

			Response.ContentType = "application/json";

			Dictionary<string, List<KeyValuePair<string, string>>> TabIds;
			TabQueue Queue;

			if (!eventsByTabID.TryGetValue(TabID, out Queue))
			{
				Queue = new TabQueue(TabID);
				eventsByTabID[TabID] = Queue;
			}

			lock (locationByTabID)
			{
				if (!locationByTabID.TryGetValue(TabID, out s) || s != Resource)
					locationByTabID[TabID] = Resource;
			}

			lock (tabIdsByLocation)
			{
				if (!tabIdsByLocation.TryGetValue(Resource, out TabIds))
				{
					TabIds = new Dictionary<string, List<KeyValuePair<string, string>>>();
					tabIdsByLocation[Resource] = TabIds;
				}

				TabIds[TabID] = Query;
			}

			StringBuilder Json = null;

			lock (Queue)
			{
				if (Queue.Queue.First != null)
				{
					foreach (string Event in Queue.Queue)
					{
						if (Json == null)
							Json = new StringBuilder("[");
						else
							Json.Append(',');

						Json.Append(Event);
					}

					Queue.Queue.Clear();
					Queue.Response = null;
				}
				else
					Queue.Response = Response;
			}

			if (Json != null)
			{
				timeoutByTabID.Remove(TabID);

				Json.Append(']');
				Response.Write(Json.ToString());
				Response.SendResponse();
				Response.Dispose();
			}
			else
				timeoutByTabID[TabID] = Queue;
		}

		private static Cache<string, TabQueue> eventsByTabID = GetQueueCache();
		private static Cache<string, TabQueue> timeoutByTabID = GetTimeoutCache();
		private static Dictionary<string, string> locationByTabID = new Dictionary<string, string>();
		private static Dictionary<string, Dictionary<string, List<KeyValuePair<string, string>>>> tabIdsByLocation = 
			new Dictionary<string, Dictionary<string, List<KeyValuePair<string, string>>>>(StringComparer.OrdinalIgnoreCase);

		private static Cache<string, TabQueue> GetTimeoutCache()
		{
			Cache<string, TabQueue> Result = new Cache<string, TabQueue>(int.MaxValue, TimeSpan.MaxValue, new TimeSpan(0, 0, 20));

			Result.Removed += TimeoutCacheItem_Removed;

			return Result;
		}

		private static void TimeoutCacheItem_Removed(object Sender, CacheItemEventArgs<string, TabQueue> e)
		{
			if (e.Reason == RemovedReason.NotUsed)
			{
				HttpResponse Response = e.Value.Response;

				if (Response != null)
				{
					try
					{
						e.Value.Response = null;

						Response.Write("[{\"type\":\"NOP\"}]");
						Response.SendResponse();
					}
					catch (Exception)
					{
						// Ignore
					}
					finally
					{
						Response.Dispose();
					}
				}
			}
		}

		private static Cache<string, TabQueue> GetQueueCache()
		{
			Cache<string, TabQueue> Result = new Cache<string, TabQueue>(int.MaxValue, TimeSpan.MaxValue, new TimeSpan(0, 0, 30));

			Result.Removed += QueueCacheItem_Removed;

			return Result;
		}

		private static void QueueCacheItem_Removed(object Sender, CacheItemEventArgs<string, TabQueue> e)
		{
			TabQueue Queue = e.Value;
			string TabID = Queue.TabID;
			string Location;
			Dictionary<string, List<KeyValuePair<string, string>>> TabIDs;

			Queue.Queue.Clear();

			lock (locationByTabID)
			{
				if (locationByTabID.TryGetValue(TabID, out Location))
					locationByTabID.Remove(TabID);
				else
					Location = null;
			}

			if (Location != null)
			{
				lock (tabIdsByLocation)
				{
					if (tabIdsByLocation.TryGetValue(Location, out TabIDs))
					{
						if (TabIDs.Remove(TabID) && TabIDs.Count == 0)
							tabIdsByLocation.Remove(Location);
					}
				}
			}
		}

		/// <summary>
		/// Returns a list of resources that are currently open.
		/// </summary>
		/// <returns>List of resources that are currently open.</returns>
		public static string[] GetOpenLocations()
		{
			string[] Result;

			lock (tabIdsByLocation)
			{
				Result = new string[tabIdsByLocation.Count];
				tabIdsByLocation.Keys.CopyTo(Result, 0);
			}

			return Result;
		}

		/// <summary>
		/// Gets the Tab IDs of all tabs that display a particular resource.
		/// </summary>
		/// <param name="Location">Resource.</param>
		/// <param name="QueryFilter">Query parameter filter.</param>
		/// <returns>Tab IDs</returns>
		public static string[] GetTabIDsForLocation(string Location, params KeyValuePair<string, string>[] QueryFilter)
		{
			Dictionary<string, List<KeyValuePair<string, string>>> TabIDs;
			string[] Result;

			lock (tabIdsByLocation)
			{
				if (tabIdsByLocation.TryGetValue(Location, out TabIDs))
				{
					if (QueryFilter == null || QueryFilter.Length == 0)
					{
						Result = new string[TabIDs.Count];
						TabIDs.Keys.CopyTo(Result, 0);
					}
					else
					{
						List<string> Match = new List<string>();
						bool Found;
						bool IsMatch;

						foreach (KeyValuePair<string, List<KeyValuePair<string, string>>> P in TabIDs)
						{
							IsMatch = true;

							foreach (KeyValuePair<string, string> Q in QueryFilter)
							{
								Found = false;

								foreach (KeyValuePair<string, string> Q2 in P.Value)
								{
									if (Q2.Key == Q.Key && Q2.Value == Q.Value)
									{
										Found = true;
										break;
									}
								}

								if (!Found)
								{
									IsMatch = false;
									break;
								}
							}

							if (IsMatch)
								Match.Add(P.Key);
						}

						Result = Match.ToArray();
					}
				}
				else
					Result = new string[0];
			}

			return Result;
		}

		/// <summary>
		/// Gets the Tab IDs of all tabs that display a set of resources.
		/// </summary>
		/// <param name="Locations">Resources.</param>
		/// <returns>Tab IDs</returns>
		public static string[] GetTabIDsForLocations(params string[] Locations)
		{
			switch (Locations.Length)
			{
				case 0:
					return new string[0];

				case 1:
					return GetTabIDsForLocation(Locations[0]);

				default:
					Dictionary<string, bool> Result = new Dictionary<string, bool>();
					Dictionary<string, List<KeyValuePair<string, string>>> TabIDs;

					lock (tabIdsByLocation)
					{
						foreach (string Location in Locations)
						{
							if (tabIdsByLocation.TryGetValue(Location, out TabIDs))
							{
								foreach (string TabID in TabIDs.Keys)
									Result[TabID] = true;
							}
						}
					}

					string[] Result2 = new string[Result.Count];
					Result.Keys.CopyTo(Result2, 0);

					return Result2;
			}
		}

		/// <summary>
		/// Gets all open Tab IDs.
		/// </summary>
		/// <returns>Tab IDs.</returns>
		public static string[] GetTabIDs()
		{
			string[] Result;

			lock (locationByTabID)
			{
				Result = new string[locationByTabID.Count];
				locationByTabID.Keys.CopyTo(Result, 0);
			}

			return Result;
		}

		private class TabQueue
		{
			public string TabID;
			public LinkedList<string> Queue = new LinkedList<string>();
			public HttpResponse Response = null;

			public TabQueue(string ID)
			{
				this.TabID = ID;
			}
		}

		/// <summary>
		/// Puses an event to a set of Tabs, given their Tab IDs.
		/// </summary>
		/// <param name="TabIDs">Tab IDs.</param>
		/// <param name="Type">Event Type.</param>
		/// <param name="Data">Event Data (as plain text).</param>
		public static void PushEvent(string[] TabIDs, string Type, string Data)
		{
			PushEvent(TabIDs, Type, Data, false);
		}

		/// <summary>
		/// Puses an event to a set of Tabs, given their Tab IDs.
		/// </summary>
		/// <param name="TabIDs">Tab IDs.</param>
		/// <param name="Type">Event Type.</param>
		/// <param name="Data">Event Data (as plain text, or as JSON).</param>
		/// <param name="DataIsJson">If <paramref name="Data"/> is JSON or plain text.</param>
		public static void PushEvent(string[] TabIDs, string Type, string Data, bool DataIsJson)
		{
			StringBuilder Json = new StringBuilder();

			Json.Append("{\"type\":\"");
			Json.Append(Type);
			Json.Append("\",\"data\":");

			if (DataIsJson)
				Json.Append(Data);
			else
			{
				Json.Append('"');
				Json.Append(CommonTypes.JsonStringEncode(Data));
				Json.Append('"');
			}

			Json.Append('}');

			string s = Json.ToString();
			TabQueue Queue;

			if (TabIDs == null)
				TabIDs = eventsByTabID.GetKeys();

			foreach (string TabID in TabIDs)
			{
				if (eventsByTabID.TryGetValue(TabID, out Queue))
				{
					lock (Queue)
					{
						if (Queue.Response != null)
						{
							try
							{
								Queue.Response.Write("[" + Json + "]");
								Queue.Response.SendResponse();
								Queue.Response.Dispose();
								Queue.Response = null;
							}
							catch (Exception)
							{
								// Ignore
							}
						}
						else
							Queue.Queue.AddLast(s);
					}

					timeoutByTabID.Remove(TabID);
				}
			}
		}


	}
}
