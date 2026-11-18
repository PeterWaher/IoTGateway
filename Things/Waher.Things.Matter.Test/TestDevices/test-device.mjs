/*
 * Matter test device, based on the open-source matter.js reference implementation of the Matter specification
 * (https://github.com/project-chip/matter.js), maintained under the Connectivity Standards Alliance (CSA) Project CHIP
 * GitHub organization.
 *
 * The device uses the publicly known test credentials of the Matter SDK: test vendor ID 0xFFF1, test device attestation
 * certificates, test certification declaration, and (by default) setup passcode 20202021 and discriminator 3840.
 *
 * The device is composed of several endpoints, each implementing a common sensor or actuator device type. Behavior is
 * implemented by the reference cluster servers of matter.js. Selected measurements are simulated, to allow tests of
 * subscriptions. The root endpoint implements the Time Synchronization cluster, to allow tests of time configuration
 * during commissioning.
 *
 * Configuration is made using environment variables:
 *
 *     TEST_DEVICE_PORT           UDP port of the device (default 5540).
 *     TEST_DEVICE_PASSCODE       Setup passcode (default 20202021).
 *     TEST_DEVICE_DISCRIMINATOR  Discriminator (default 3840).
 *     TEST_DEVICE_ID             Unique ID of the device, also used as storage key (default "waher-test-device").
 *     TEST_DEVICE_SIMULATE       If "1", sensor values are updated periodically.
 *     TEST_DEVICE_BRIDGE         If "1", the device acts as a bridge with bridged devices.
 *     MATTER_STORAGE_PATH        Folder where matter.js stores the state of the device.
 *
 * When the device is ready, it prints a line "READY" followed by its manual pairing code and QR code payload to stdout.
 * Commands can be sent to the device through stdin, one per line:
 *
 *     temperature <value>      Sets the measured temperature (°C).
 *     humidity <value>         Sets the measured relative humidity (%).
 *     contact <true|false>     Sets the state of the contact sensor.
 *     occupancy <true|false>   Sets the occupancy state.
 *     press                    Simulates a short press on the generic switch.
 *     quit                     Shuts down the device.
 */

import { ServerNode, Endpoint, VendorId } from "@matter/main";
import {
	ExtendedColorLightDevice,
	ExtendedColorLightRequirements,
	TemperatureSensorDevice,
	HumiditySensorDevice,
	ContactSensorDevice,
	OccupancySensorDevice,
	OccupancySensorRequirements,
	DoorLockDevice,
	WindowCoveringDevice,
	WindowCoveringRequirements,
	ThermostatDevice,
	ThermostatRequirements,
	FanDevice,
	LightSensorDevice,
	PressureSensorDevice,
	FlowSensorDevice,
	GenericSwitchDevice,
	GenericSwitchRequirements,
	OnOffPlugInUnitDevice
} from "@matter/main/devices";
import { AggregatorEndpoint } from "@matter/main/endpoints";
import { BridgedDeviceBasicInformationServer } from "@matter/main/behaviors/bridged-device-basic-information";
import { PowerSourceServer } from "@matter/main/behaviors/power-source";
import { PowerSource } from "@matter/main/clusters/power-source";
import { DoorLock } from "@matter/main/clusters/door-lock";
import { Thermostat } from "@matter/main/clusters/thermostat";
import { OccupancySensing } from "@matter/main/clusters/occupancy-sensing";
import { FanControl } from "@matter/main/clusters/fan-control";
import { TimeSynchronizationServer } from "@matter/main/behaviors/time-synchronization";
import { createInterface } from "node:readline";

/**
 * Time synchronization server accepting UTC time from administrators.
 */
class TestTimeSynchronizationServer extends TimeSynchronizationServer {
	setUtcTime(request) {
		this.state.utcTime = request.utcTime;
		this.state.granularity = request.granularity;
	}
}

const env = process.env;
const port = Number(env.TEST_DEVICE_PORT ?? 5540);
const passcode = Number(env.TEST_DEVICE_PASSCODE ?? 20202021);
const discriminator = Number(env.TEST_DEVICE_DISCRIMINATOR ?? 3840);
const id = env.TEST_DEVICE_ID ?? "waher-test-device";
const simulate = env.TEST_DEVICE_SIMULATE === "1";
const bridge = env.TEST_DEVICE_BRIDGE === "1";

const server = await ServerNode.create(ServerNode.RootEndpoint.with(TestTimeSynchronizationServer), {
	id,
	network: { port },
	commissioning: { passcode, discriminator },
	productDescription: {
		name: bridge ? "Waher Test Bridge" : "Waher Test Device",
		deviceType: bridge ? AggregatorEndpoint.deviceType : ExtendedColorLightDevice.deviceType,
	},
	basicInformation: {
		vendorName: "matter.js",
		vendorId: VendorId(0xfff1),
		productName: bridge ? "Waher Test Bridge" : "Waher Test Device",
		productId: bridge ? 0x8001 : 0x8000,
		nodeLabel: bridge ? "Test Bridge" : "Test Device",
		serialNumber: "WTD-" + id,
		uniqueId: id,
		hardwareVersion: 1,
		hardwareVersionString: "1.0",
		softwareVersion: 1,
		softwareVersionString: "1.0.0",
		productUrl: "https://github.com/PeterWaher/IoTGateway",
	},
	timeSynchronization: {
		utcTime: null,
		granularity: 0,
	},
});

const endpoints = {};

if (bridge) {
	const aggregator = new Endpoint(AggregatorEndpoint, { id: "aggregator" });
	await server.add(aggregator);

	endpoints.lamp = new Endpoint(OnOffPlugInUnitDevice.with(BridgedDeviceBasicInformationServer), {
		id: "lamp",
		bridgedDeviceBasicInformation: {
			nodeLabel: "Bridged Lamp",
			productName: "Lamp",
			vendorName: "matter.js",
			uniqueId: id + "-lamp",
			reachable: true,
		},
	});
	await aggregator.add(endpoints.lamp);

	endpoints.temperature = new Endpoint(TemperatureSensorDevice.with(BridgedDeviceBasicInformationServer), {
		id: "temperature",
		bridgedDeviceBasicInformation: {
			nodeLabel: "Bridged Thermometer",
			productName: "Thermometer",
			vendorName: "matter.js",
			uniqueId: id + "-temperature",
			reachable: true,
		},
		temperatureMeasurement: { measuredValue: 1900, minMeasuredValue: -4000, maxMeasuredValue: 8500 },
	});
	await aggregator.add(endpoints.temperature);
} else {
	endpoints.light = new Endpoint(
		ExtendedColorLightDevice.with(
			ExtendedColorLightRequirements.ColorControlServer.with("HueSaturation", "Xy", "ColorTemperature")),
		{
			id: "light",
			onOff: { onOff: false },
			levelControl: { currentLevel: 127 },
			colorControl: {
				colorMode: 2,
				enhancedColorMode: 2,
				colorTemperatureMireds: 250,
				colorTempPhysicalMinMireds: 153,
				colorTempPhysicalMaxMireds: 500,
				coupleColorTempToLevelMinMireds: 153,
				startUpColorTemperatureMireds: null,
				currentHue: 0,
				currentSaturation: 0,
				currentX: 24939,
				currentY: 24701,
			},
		});
	await server.add(endpoints.light);

	endpoints.temperature = new Endpoint(TemperatureSensorDevice, {
		id: "temperature",
		temperatureMeasurement: { measuredValue: 2150, minMeasuredValue: -4000, maxMeasuredValue: 8500 },
	});
	await server.add(endpoints.temperature);

	endpoints.humidity = new Endpoint(HumiditySensorDevice, {
		id: "humidity",
		relativeHumidityMeasurement: { measuredValue: 4550, minMeasuredValue: 0, maxMeasuredValue: 10000 },
	});
	await server.add(endpoints.humidity);

	endpoints.contact = new Endpoint(ContactSensorDevice, {
		id: "contact",
		booleanState: { stateValue: true },
	});
	await server.add(endpoints.contact);

	endpoints.occupancy = new Endpoint(
		OccupancySensorDevice.with(OccupancySensorRequirements.OccupancySensingServer.with("PassiveInfrared")),
		{
			id: "occupancy",
			occupancySensing: {
				occupancy: { occupied: false },
				occupancySensorType: OccupancySensing.OccupancySensorType.Pir,
				occupancySensorTypeBitmap: { pir: true },
			},
		});
	await server.add(endpoints.occupancy);

	endpoints.pressure = new Endpoint(PressureSensorDevice, {
		id: "pressure",
		pressureMeasurement: { measuredValue: 1013, minMeasuredValue: 300, maxMeasuredValue: 1100 },
	});
	await server.add(endpoints.pressure);

	endpoints.illuminance = new Endpoint(LightSensorDevice, {
		id: "illuminance",
		illuminanceMeasurement: { measuredValue: 30001, minMeasuredValue: 1, maxMeasuredValue: 50001 },
	});
	await server.add(endpoints.illuminance);

	endpoints.flow = new Endpoint(FlowSensorDevice, {
		id: "flow",
		flowMeasurement: { measuredValue: 123, minMeasuredValue: 0, maxMeasuredValue: 1000 },
	});
	await server.add(endpoints.flow);

	endpoints.lock = new Endpoint(DoorLockDevice, {
		id: "lock",
		doorLock: {
			lockState: DoorLock.LockState.Locked,
			lockType: DoorLock.LockType.DeadBolt,
			actuatorEnabled: true,
			operatingMode: DoorLock.OperatingMode.Normal,
			supportedOperatingModes: { noRemoteLockUnlock: true, normal: false, vacation: true, privacy: true, passage: true, alwaysSet: 2047 },
		},
	});
	await server.add(endpoints.lock);

	endpoints.cover = new Endpoint(
		WindowCoveringDevice.with(WindowCoveringRequirements.WindowCoveringServer.with("Lift", "PositionAwareLift")),
		{
			id: "cover",
			windowCovering: {
				currentPositionLiftPercent100ths: 5000,
				targetPositionLiftPercent100ths: 5000,
			},
		});
	await server.add(endpoints.cover);

	endpoints.thermostat = new Endpoint(
		ThermostatDevice.with(ThermostatRequirements.ThermostatServer.with("Heating", "Cooling")),
		{
			id: "thermostat",
			thermostat: {
				localTemperature: 2000,
				occupiedHeatingSetpoint: 2100,
				occupiedCoolingSetpoint: 2600,
				minHeatSetpointLimit: 700,
				maxHeatSetpointLimit: 3000,
				minCoolSetpointLimit: 1600,
				maxCoolSetpointLimit: 3200,
				absMinHeatSetpointLimit: 700,
				absMaxHeatSetpointLimit: 3000,
				absMinCoolSetpointLimit: 1600,
				absMaxCoolSetpointLimit: 3200,
				controlSequenceOfOperation: Thermostat.ControlSequenceOfOperation.CoolingAndHeating,
				systemMode: Thermostat.SystemMode.Heat,
			},
		});
	await server.add(endpoints.thermostat);

	endpoints.fan = new Endpoint(FanDevice, {
		id: "fan",
		fanControl: {
			fanMode: FanControl.FanMode.Off,
			fanModeSequence: FanControl.FanModeSequence.OffLowMedHigh,
			percentSetting: 0,
			percentCurrent: 0,
		},
	});
	await server.add(endpoints.fan);

	endpoints.switch = new Endpoint(
		GenericSwitchDevice.with(GenericSwitchRequirements.SwitchServer.with("MomentarySwitch", "MomentarySwitchRelease")),
		{
			id: "switch",
			switch: { numberOfPositions: 2, currentPosition: 0 },
		});
	await server.add(endpoints.switch);

	endpoints.plug = new Endpoint(OnOffPlugInUnitDevice.with(PowerSourceServer.with("Battery")), {
		id: "plug",
		onOff: { onOff: true },
		powerSource: {
			status: PowerSource.PowerSourceStatus.Active,
			order: 0,
			description: "Battery",
			batPercentRemaining: 180,
			batChargeLevel: PowerSource.BatChargeLevel.Ok,
			batReplacementNeeded: false,
			batReplaceability: PowerSource.BatReplaceability.UserReplaceable,
			batVoltage: 3000,
			endpointList: [],
		},
	});
	await server.add(endpoints.plug);
}

async function setTemperature(value) {
	await endpoints.temperature.set({ temperatureMeasurement: { measuredValue: Math.round(value * 100) } });
}

async function setHumidity(value) {
	if (endpoints.humidity)
		await endpoints.humidity.set({ relativeHumidityMeasurement: { measuredValue: Math.round(value * 100) } });
}

async function press() {
	const sw = endpoints.switch;
	if (!sw)
		return;

	await sw.act(agent => agent.switch.events.initialPress.emit({ newPosition: 1 }, agent.context));
	await sw.set({ switch: { currentPosition: 1 } });
	await new Promise(resolve => setTimeout(resolve, 100));
	await sw.act(agent => agent.switch.events.shortRelease.emit({ previousPosition: 1 }, agent.context));
	await sw.set({ switch: { currentPosition: 0 } });
}

await server.start();

const pairing = server.state.commissioning.pairingCodes;
console.log("READY " + pairing.manualPairingCode + " " + pairing.qrPairingCode);

let simulationTimer = undefined;
let closing = false;

async function shutdown() {
	if (closing)
		return;

	closing = true;

	if (simulationTimer)
		clearInterval(simulationTimer);

	try {
		await server.close();
	} catch (e) {
		console.error(e);
	}

	process.exit(0);
}

if (simulate) {
	let t = 21.5;
	simulationTimer = setInterval(() => {
		t = t >= 25 ? 20 : t + 0.1;
		setTemperature(t).catch(e => console.error(e));
	}, 1000);
}

const rl = createInterface({ input: process.stdin });

rl.on("line", async line => {
	const parts = line.trim().split(/\s+/);

	try {
		switch (parts[0]) {
			case "temperature":
				await setTemperature(Number(parts[1]));
				break;

			case "humidity":
				await setHumidity(Number(parts[1]));
				break;

			case "contact":
				if (endpoints.contact)
					await endpoints.contact.set({ booleanState: { stateValue: parts[1] === "true" } });
				break;

			case "occupancy":
				if (endpoints.occupancy)
					await endpoints.occupancy.set({ occupancySensing: { occupancy: { occupied: parts[1] === "true" } } });
				break;

			case "press":
				await press();
				break;

			case "quit":
				await shutdown();
				return;

			default:
				return;
		}

		console.log("OK " + parts[0]);
	} catch (e) {
		console.log("ERROR " + parts[0] + ": " + e);
	}
});

rl.on("close", shutdown);
process.on("SIGTERM", shutdown);
process.on("SIGINT", shutdown);
