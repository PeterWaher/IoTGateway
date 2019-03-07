using System;
using System.Collections.Generic;
using System.Net;

namespace Waher.Networking.Cluster
{
	internal class LockInfo
	{
		public string Resource;
		public bool Locked;
		public LinkedList<LockInfoRec> Queue = new LinkedList<LockInfoRec>();
	}

	internal class LockInfoRec
	{
		public LockInfo Info;
		public DateTime Timeout;
		public ClusterResourceLockEventHandler Callback;
		public IPEndPoint LockedBy;
		public object State;
		public bool TimeoutScheduled = false;
	}
}
