import asyncio
from bleak import BleakScanner

devices = {}


def callback(device, adv):
    devices[device.address] = (device, adv)


async def main():
    print("Scanning...\n")

    async with BleakScanner(callback):
        await asyncio.sleep(10)

    results = sorted(devices.values(), key=lambda x: x[1].rssi, reverse=True)

    for device, adv in results:
        print(
            f"{adv.rssi:4} dBm | "
            f"{adv.local_name or 'None':20} | "
            f"{device.address}"
        )

asyncio.run(main())