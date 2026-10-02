import asyncio
from bleak import BleakScanner, BleakClient


async def main():
    print("Searching for PCCScale...")

    device = await BleakScanner.find_device_by_name("PCCScale", timeout=10.0)

    if device is None:
        print("PCCScale not found")
        return

    print(f"Found: {device.name}  {device.address}")
    print("Connecting...")

    async with BleakClient(device, timeout=15.0) as client:

        print("Connected:", client.is_connected)

        print("\nGATT services:")

        for service in client.services:
            print(f"\nSERVICE: {service.uuid}")

            for characteristic in service.characteristics:
                print(
                    f"  CHARACTERISTIC: {characteristic.uuid}"
                    f"  {characteristic.properties}"
                )

        print("\nKeeping connection open for 30 seconds...")
        await asyncio.sleep(30)


asyncio.run(main())
