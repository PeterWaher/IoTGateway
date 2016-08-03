using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Deployment.WindowsInstaller;

namespace Waher.IoTGateway.Installers
{
	public class CustomActions
	{
		[CustomAction]
		public static ActionResult CreateEventSource(Session Session)
		{
			Session.Log("Checking event sources.");

			try
			{
				if (!EventLog.Exists("IoTGateway") || !EventLog.SourceExists("IoTGateway"))
				{
					Session.Log("Creating event source.");
					EventLog.CreateEventSource(new EventSourceCreationData("IoTGateway", "IoTGateway"));
					Session.Log("Event source created.");
				}

				return ActionResult.Success;
			}
			catch (Exception ex)
			{
				Session.Log("Unable to create event source. Error reported: " + ex.Message);
				return ActionResult.Failure;
			}
		}

		[CustomAction]
		public static ActionResult DeleteEventSource(Session Session)
		{
			Session.Log("Checking event sources.");

			if (EventLog.Exists("IoTGateway"))
			{
				try
				{
					Session.Log("Deleting event log.");
					EventLog.Delete("IoTGateway");
					Session.Log("Event log deleted.");
				}
				catch (Exception ex)
				{
					Session.Log("Unable to delete event log. Error reported: " + ex.Message);
				}
			}

			if (EventLog.SourceExists("IoTGateway"))
			{
				try
				{
					Session.Log("Deleting event source.");
					EventLog.DeleteEventSource("IoTGateway");
					Session.Log("Event source deleted.");
				}
				catch (Exception ex)
				{
					Session.Log("Unable to delete event source. Error reported: " + ex.Message);
					// Ignore.
				}
			}

			return ActionResult.Success;
		}
	}
}
