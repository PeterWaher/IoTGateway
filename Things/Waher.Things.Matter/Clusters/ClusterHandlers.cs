using System;
using System.Collections.Generic;
using Waher.Runtime.Inventory;

namespace Waher.Things.Matter.Clusters
{
	/// <summary>
	/// Finds cluster handlers for clusters. Built-in handlers are complemented by handlers available through
	/// <see cref="Types"/>, which makes it possible to add new handlers, or override built-in handlers, by
	/// implementing <see cref="IClusterHandler"/> in other modules.
	/// </summary>
	public static class ClusterHandlers
	{
		private static readonly Dictionary<uint, IClusterHandler> handlers = new Dictionary<uint, IClusterHandler>();
		private static readonly GenericClusterHandler generic = new GenericClusterHandler();
		private static readonly IClusterHandler[] builtIn = new IClusterHandler[]
		{
			new IdentifyHandler(),
			new OnOffHandler(),
			new LevelControlHandler(),
			new ColorControlHandler(),
			new TemperatureMeasurementHandler(),
			new RelativeHumidityMeasurementHandler(),
			new PressureMeasurementHandler(),
			new FlowMeasurementHandler(),
			new IlluminanceMeasurementHandler(),
			new OccupancySensingHandler(),
			new BooleanStateHandler(),
			new ConcentrationMeasurementHandler(),
			new AirQualityHandler(),
			new PowerSourceHandler(),
			new ElectricalPowerMeasurementHandler(),
			new ElectricalEnergyMeasurementHandler(),
			new ThermostatHandler(),
			new FanControlHandler(),
			new WindowCoveringHandler(),
			new DoorLockHandler(),
			new SwitchHandler(),
			new ModeHandler(),
			new ValveHandler(),
			new BasicInformationHandler(),
			new GeneralDiagnosticsHandler(),
			new NetworkDiagnosticsHandler(),
			new InfrastructureClusterHandler()
		};

		static ClusterHandlers()
		{
			Types.OnInvalidated += (Sender, e) => ClearCache();
		}

		/// <summary>
		/// Gets the best cluster handler for a cluster.
		/// </summary>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <returns>Cluster handler.</returns>
		public static IClusterHandler Get(uint ClusterId)
		{
			lock (handlers)
			{
				if (handlers.TryGetValue(ClusterId, out IClusterHandler Handler))
					return Handler;
			}

			IClusterHandler Best = null;
			Grade BestGrade = Grade.NotAtAll;

			foreach (IClusterHandler Handler in builtIn)
			{
				Grade Grade = Handler.Supports(ClusterId);

				if (Grade > BestGrade)
				{
					Best = Handler;
					BestGrade = Grade;
				}
			}

			try
			{
				IClusterHandler External = Types.FindBest<IClusterHandler, uint>(ClusterId);

				if (!(External is null))
				{
					Grade Grade = External.Supports(ClusterId);

					if (Grade > BestGrade && !IsBuiltIn(External.GetType()))
					{
						Best = External;
						BestGrade = Grade;
					}
				}
			}
			catch (Exception)
			{
				// Types not initialized, or no external handlers available.
			}

			if (Best is null)
				Best = generic;

			lock (handlers)
			{
				handlers[ClusterId] = Best;
			}

			return Best;
		}

		private static bool IsBuiltIn(Type T)
		{
			foreach (IClusterHandler Handler in builtIn)
			{
				if (Handler.GetType() == T)
					return true;
			}

			return T == typeof(GenericClusterHandler);
		}

		/// <summary>
		/// Clears the cache of cluster handlers.
		/// </summary>
		public static void ClearCache()
		{
			lock (handlers)
			{
				handlers.Clear();
			}
		}
	}
}
