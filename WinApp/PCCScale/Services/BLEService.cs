using System.Buffers.Binary;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace PCCScale.Services
{
    public class BLEService
    {
        private static readonly Guid ServiceUuid = Guid.Parse("0000A001-CC7A-482A-984A-7F2ED5B3E58F");
        private static readonly Guid WeightUuid = Guid.Parse("0000A002-8E22-4541-9D4C-21EDAE82ED19");
        private static readonly Guid CommandUuid = Guid.Parse("0000A003-8E22-4541-9D4C-21EDAE82ED19");
        private static readonly Guid StatusUuid = Guid.Parse("0000A004-8E22-4541-9D4C-21EDAE82ED19");
        private static CancellationTokenSource? _cts;

        private BluetoothLEAdvertisementWatcher? _watcher;

        private BluetoothLEDevice? _bluetoothLeDevice;

        private GattDeviceService? _service;

        private GattCharacteristic? _weightCharacteristic;
        private GattCharacteristic? _commandCharacteristic;
        private GattCharacteristic? _statusCharacteristic;


        public bool IsConnected => _bluetoothLeDevice?.ConnectionStatus == BluetoothConnectionStatus.Connected;

        public event Action<int, int, int>? WeightValueUpdated;

        public BLEService()
        {
            //ConnectAsync().GetAwaiter().GetResult();
        }

        public async Task<bool> ConnectAsync()
        {
            if (IsConnected)
                return true;

            var addressSource =
                new TaskCompletionSource<ulong>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

            _watcher = new BluetoothLEAdvertisementWatcher();

            void OnReceived(
                BluetoothLEAdvertisementWatcher sender,
                BluetoothLEAdvertisementReceivedEventArgs args)
            {
                if (args.Advertisement.LocalName != "PCCScale")
                    return;

                addressSource.TrySetResult(args.BluetoothAddress);
            }

            _watcher.Received += OnReceived;
            _watcher.Start();

            ulong address;

            try
            {
                address = await addressSource.Task.WaitAsync(
                    TimeSpan.FromSeconds(15));
            }
            catch (TimeoutException)
            {
                return false;
            }
            finally
            {
                _watcher.Stop();
                _watcher.Received -= OnReceived;
                _watcher = null;
            }

            _bluetoothLeDevice =
                await BluetoothLEDevice.FromBluetoothAddressAsync(address);

            if (_bluetoothLeDevice is null)
                return false;

            var serviceResult =
                await _bluetoothLeDevice.GetGattServicesForUuidAsync(
                    ServiceUuid,
                    BluetoothCacheMode.Uncached);

            if (serviceResult.Status != GattCommunicationStatus.Success)
                return false;

            _service = serviceResult.Services.FirstOrDefault();

            if (_service is null)
                return false;

            var characteristicsResult =
                await _service.GetCharacteristicsAsync(
                    BluetoothCacheMode.Uncached);

            if (characteristicsResult.Status != GattCommunicationStatus.Success)
                return false;

            _weightCharacteristic =
                characteristicsResult.Characteristics
                    .FirstOrDefault(c => c.Uuid == WeightUuid);

            _commandCharacteristic =
                characteristicsResult.Characteristics
                    .FirstOrDefault(c => c.Uuid == CommandUuid);

            _statusCharacteristic =
                characteristicsResult.Characteristics
                    .FirstOrDefault(c => c.Uuid == StatusUuid);

            if (_weightCharacteristic is null ||
                _commandCharacteristic is null ||
                _statusCharacteristic is null)
            {
                return false;
            }

            _weightCharacteristic.ValueChanged += OnWeightChanged;

            var subscribeStatus =
                await _weightCharacteristic
                    .WriteClientCharacteristicConfigurationDescriptorAsync(
                        GattClientCharacteristicConfigurationDescriptorValue.Notify);

            return subscribeStatus == GattCommunicationStatus.Success;
        }

        private async Task<bool> SendCommandAsync(CommandEnum command, byte[] payload)
        {
            if (_commandCharacteristic is not null)
            {
                using var writer = new DataWriter();
                writer.WriteByte((byte)command);
                writer.WriteBytes(payload);
                var result = await _commandCharacteristic.WriteValueAsync(writer.DetachBuffer(), GattWriteOption.WriteWithResponse);
                return result == GattCommunicationStatus.Success;
            }
            return false;
        }

        public async Task<bool> SetThresholdAsync(byte rack, byte pan, int threshold)
        {
            byte[] payload = { rack, pan, (byte)(threshold & 0xFF), (byte)((threshold >> 8) & 0xFF), (byte)((threshold >> 16) & 0xFF), (byte)((threshold >> 24) & 0xFF) };
            return await SendCommandAsync(CommandEnum.SetThreshold, payload);
        }

        //public Task DisconnectAsync()
        //{
        //    if (_weightCharacteristic is not null)
        //        _weightCharacteristic.ValueChanged -= OnWeightChanged;

        //    _weightCharacteristic = null;

        //    _service?.Dispose();
        //    _service = null;

        //    return Task.CompletedTask;
        //}

        private void OnWeightChanged(
            GattCharacteristic sender,
            GattValueChangedEventArgs args)
        {
            var data = new byte[args.CharacteristicValue.Length];

            using var reader = DataReader.FromBuffer(args.CharacteristicValue);
            reader.ReadBytes(data);

            if (data.Length < 4)
                return;

            int weight = BinaryPrimitives.ReadInt32LittleEndian(data);

            WeightValueUpdated?.Invoke(1, 3, weight);
        }
    }
}
