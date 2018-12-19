using System;
using System.Text;
using System.Collections.Generic;
using System.Net;
using Waher.Networking.PeerToPeer;

namespace Waher.Networking.PeerToPeer
{
	/// <summary>
	/// Class containing information about a player.
	/// </summary>
	public class Player : IEnumerable<KeyValuePair<string, string>>
	{
		private int index = -1;
		private Guid playerId;
		private IPEndPoint publicEndpoint;
		private IPEndPoint localEndpoint;
		private Dictionary<string, string> playerMetaInfo;
		private PeerConnection connection = null;

		internal Player(Guid PlayerId, IPEndPoint PublicEndpoint, IPEndPoint LocalEndpoint, params KeyValuePair<string, string>[] PlayerMetaInfo)
		{
			this.playerId = PlayerId;
			this.publicEndpoint = PublicEndpoint;
			this.localEndpoint = LocalEndpoint;
			this.playerMetaInfo = new Dictionary<string, string>();

			foreach (KeyValuePair<string, string> P in PlayerMetaInfo)
				this.playerMetaInfo[P.Key] = P.Value;
		}

		/// <summary>
		/// Player index.
		/// </summary>
		public int Index
		{
			get { return this.index; }
			internal set { this.index = value; }
		}

		/// <summary>
		/// Player ID
		/// </summary>
		public Guid PlayerId
		{
			get { return this.playerId; }
		}

		/// <summary>
		/// Public Endpoint
		/// </summary>
		public IPEndPoint PublicEndpoint
		{
			get { return this.publicEndpoint; }
		}

		/// <summary>
		/// Local Endpoint
		/// </summary>
		public IPEndPoint LocalEndpoint
		{
			get { return this.localEndpoint; }
		}

		internal void SetEndpoints(IPEndPoint PublicEndpoint, IPEndPoint LocalEndpoint)
		{
			this.publicEndpoint = PublicEndpoint;
			this.localEndpoint = LocalEndpoint;
		}

		internal IPEndPoint GetExpectedEndpoint(PeerToPeerNetwork Network)
		{
			if (IPAddress.Equals(this.publicEndpoint.Address, Network.ExternalAddress))
				return this.localEndpoint;
			else
				return this.publicEndpoint;
		}

		/// <summary>
		/// Number of meta information tags.
		/// </summary>
		public int Count
		{
			get { return this.playerMetaInfo.Count; }
		}

		/// <summary>
		/// Gets an enumerator for player meta information.
		/// </summary>
		/// <returns>Enumerator</returns>
		public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
		{
			return this.playerMetaInfo.GetEnumerator();
		}

		/// <summary>
		/// Gets an enumerator for player meta information.
		/// </summary>
		/// <returns>Enumerator</returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.playerMetaInfo.GetEnumerator();
		}

		/// <summary>
		/// Gets the meta value, given its meta key. If key is not found, the empty string is returned.
		/// </summary>
		/// <param name="Key">Meta information key.</param>
		/// <returns>Meta information value.</returns>
		public string this[string Key]
		{
			get
			{
				string Value;

				if (this.playerMetaInfo.TryGetValue(Key, out Value))
					return Value;
				else
					return string.Empty;
			}
		}

		/// <summary>
		/// Peer connection, if any.
		/// </summary>
		public PeerConnection Connection
		{
			get { return this.connection; }
			internal set { this.connection = value; }
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			StringBuilder sb = null;

			foreach (KeyValuePair<string, string> P in this.playerMetaInfo)
			{
				if (sb is null)
				{
					sb = new StringBuilder(this.publicEndpoint.ToString());
					sb.Append(":");
				}
				else
					sb.Append(", ");

				sb.Append(P.Key);
				sb.Append('=');
				sb.Append(P.Value);
			}

			if (sb is null)
				return base.ToString();
			else
				return sb.ToString();
		}

	}
}
