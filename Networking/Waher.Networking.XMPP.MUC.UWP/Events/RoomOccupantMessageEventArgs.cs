using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.MUC
{
	/// <summary>
	/// Delegate for MUC Room occupant event handlers.
	/// </summary>
	/// <param name="Sender"></param>
	/// <param name="e"></param>
	/// <returns></returns>
	public delegate Task RoomOccupantMessageEventHandler(object Sender, RoomOccupantMessageEventArgs e);

	/// <summary>
	/// Message from a MUC room occupant.
	/// </summary>
	public class RoomOccupantMessageEventArgs : RoomMessageEventArgs
	{
		private readonly string nickName;

		/// <summary>
		/// Message from a MUC room occupant.
		/// </summary>
		/// <param name="e">Message event arguments.</param>
		/// <param name="RoomId">Room ID</param>
		/// <param name="Domain">Domain hosting the room.</param>
		/// <param name="NickName">Nick-name of occupant.</param>
		public RoomOccupantMessageEventArgs(MessageEventArgs e, string RoomId, 
			string Domain, string NickName)
			: base(e, RoomId, Domain)
		{
			this.nickName = NickName;
		}

		/// <summary>
		/// Nick-name of occupant.
		/// </summary>
		public string NickName => this.nickName;
	}
}
