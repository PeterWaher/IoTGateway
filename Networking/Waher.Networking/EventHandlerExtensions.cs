using System;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.Sniffers;

namespace Waher.Networking
{
	/// <summary>
	/// Asynchronous version of <see cref="EventArgs"/>.
	/// </summary>
	public delegate Task EventHandlerAsync(object Sender, EventArgs e);

	/// <summary>
	/// Asynchronous version of <see cref="EventArgs"/> with a typed event arguments.
	/// </summary>
	public delegate Task EventHandlerAsync<T>(object Sender, T e);

	/// <summary>
	/// Connection error event handler delegate.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="Exception">Information about error received.</param>
	public delegate Task ExceptionEventHandler(object Sender, Exception Exception);

	/// <summary>
	/// Callback function with one argument.
	/// </summary>
	/// <typeparam name="ArgT">Argument type.</typeparam>
	/// <param name="Arg">Argument</param>
	public delegate void Callback<ArgT>(ArgT Arg);

	/// <summary>
	/// Asynchronous callback function with one argument.
	/// </summary>
	/// <typeparam name="ArgT">Argument type.</typeparam>
	/// <param name="Arg">Argument</param>
	public delegate Task CallbackAsync<ArgT>(ArgT Arg);

	/// <summary>
	/// Static class with method extensions simplifying raising events.
	/// </summary>
	public static class EventHandlerExtensions
	{
		#region Synchronous event handlers

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static Task<bool> Raise(this EventHandler EventHandler, object Sender)
		{
			return Raise(EventHandler, Sender, EventArgs.Empty);
		}

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <param name="e">Event arguments.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static async Task<bool> Raise(this EventHandler EventHandler, object Sender, EventArgs e)
		{
			if (EventHandler is null)
			{
				if (Sender is ISniffable Sniffable)
					await Sniffable.Warning("No event handler registered.");
			}
			else
			{
				try
				{
					EventHandler(Sender, e);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <param name="e">Event arguments.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static async Task<bool> Raise<T>(this EventHandler<T> EventHandler, object Sender, T e)
		{
			if (EventHandler is null)
			{
				if (Sender is ISniffable Sniffable)
					await Sniffable.Warning("No event handler registered.");
			}
			else
			{
				try
				{
					EventHandler(Sender, e);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);

					if (Sender is ISniffable Sniffable)
						await Sniffable.Exception(ex);

					return false;
				}
			}

			return true;
		}

		#endregion

		#region Asynchronous event handlers

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static Task<bool> Raise(this EventHandlerAsync EventHandler, object Sender)
		{
			return Raise(EventHandler, Sender, EventArgs.Empty);
		}

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <param name="e">Event arguments.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static async Task<bool> Raise(this EventHandlerAsync EventHandler, object Sender, EventArgs e)
		{
			if (EventHandler is null)
			{
				if (Sender is ISniffable Sniffable)
					await Sniffable.Warning("No event handler registered.");
			}
			else
			{
				try
				{
					await EventHandler(Sender, e);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);

					if (Sender is ISniffable Sniffable)
						await Sniffable.Exception(ex);

					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <param name="e">Event arguments.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static async Task<bool> Raise<T>(this EventHandlerAsync<T> EventHandler, object Sender, T e)
		{
			if (EventHandler is null)
			{
				if (Sender is ISniffable Sniffable)
					await Sniffable.Warning("No event handler registered.");
			}
			else
			{
				try
				{
					await EventHandler(Sender, e);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);

					if (Sender is ISniffable Sniffable)
						await Sniffable.Exception(ex);
					
					return false;
				}
			}

			return true;
		}

		#endregion

		#region Synchronous callback functions

		/// <summary>
		/// Calls a callback function. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="Callback">Callback function, or null if not defined.</param>
		/// <param name="Argument">Argument</param>
		public static void Call<ArgT>(this Callback<ArgT> Callback, ArgT Argument)
		{
			if (!(Callback is null))
			{
				try
				{
					Callback(Argument);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}
		}

		#endregion

		#region Asynchronous callback functions

		/// <summary>
		/// Calls a callback function. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="Callback">Callback function, or null if not defined.</param>
		/// <param name="Argument">Argument</param>
		public static async Task Call<ArgT>(this CallbackAsync<ArgT> Callback, ArgT Argument)
		{
			if (!(Callback is null))
			{
				try
				{
					await Callback(Argument);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}
		}

		#endregion

	}
}
