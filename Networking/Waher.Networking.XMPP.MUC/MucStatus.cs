using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.MUC
{
	/// <summary>
	/// MUC Status, as defined in https://xmpp.org/registrar/mucstatus.html
	/// </summary>
	public enum MucStatus
	{
		/// <summary>
		/// Inform user that any occupant is allowed to see the user's full JID
		/// </summary>
		FullJidVisisble = 100,

		/// <summary>
		/// Inform user that his or her affiliation changed while not in the room
		/// </summary>
		AffiliationChanged = 101,

		/// <summary>
		/// Inform occupants that room now shows unavailable members
		/// </summary>
		ShowsUnavailableMembers = 102,

		/// <summary>
		/// Inform occupants that room now does not show unavailable members
		/// </summary>
		DoesNotShowUnavailableMembers = 103,

		/// <summary>
		/// Inform occupants that a non-privacy-related room configuration change has occurred
		/// </summary>
		NonPrivacyRelatedConfigurationChange = 104,

		/// <summary>
		/// Inform user that presence refers to one of its own room occupants
		/// </summary>
		OwnPresence = 110,

		/// <summary>
		/// Inform occupants that room logging is now enabled
		/// </summary>
		LoggingEnabled = 170,

		/// <summary>
		/// Inform occupants that room logging is now disabled
		/// </summary>
		LoggingDisabled = 171,

		/// <summary>
		/// Inform occupants that the room is now non-anonymous
		/// </summary>
		RoomNonAnonymous = 172,

		/// <summary>
		/// Inform occupants that the room is now semi-anonymous
		/// </summary>
		RoomSemiAnonymous = 173,

		/// <summary>
		/// Inform occupants that the room is now fully-anonymous
		/// </summary>
		RoomAnonymous = 174,

		/// <summary>
		/// Inform user that a new room has been created
		/// </summary>
		Created = 201,

		/// <summary>
		/// Inform user that the service has assigned or modified the occupant's roomnick
		/// </summary>
		NickModified = 210,

		/// <summary>
		/// Inform user that he or she has been banned from the room
		/// </summary>
		Banned = 301,

		/// <summary>
		/// Inform all occupants of new room nickname
		/// </summary>
		NewRoomNickName = 303,

		/// <summary>
		/// Inform user that he or she has been kicked from the room
		/// </summary>
		Kicked = 307,

		/// <summary>
		/// Inform user that he or she is being removed from the room because of an affiliation change
		/// </summary>
		RemovedDueToAffiliationChange = 321,

		/// <summary>
		/// Inform user that he or she is being removed from the room because the room has been changed to members-only and the user is not a member
		/// </summary>
		RemovedDueToNonMembership = 322,

		/// <summary>
		/// Inform user that he or she is being removed from the room because of a system shutdown
		/// </summary>
		RemovedDueToSystemShutdown = 332
	}
}
