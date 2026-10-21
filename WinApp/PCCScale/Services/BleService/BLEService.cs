using Microsoft.Extensions.Options;
using System.Buffers.Binary;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Networking.Connectivity;
using Windows.Storage.Streams;

namespace PCCScale.Services.BleService
{
    public class BLEService
    {
        private readonly SemaphoreSlim _connectLock = new(1, 1);
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private Task? _autoConnnectLoopTask;

        private readonly BleOptions _options;
        private BluetoothLEAdvertisementWatcher? _watcher;

        private BluetoothLEDevice? _bluetoothLeDevice;

        private GattDeviceService? _service;

        private GattCharacteristic? _weightCharacteristic;
        private GattCharacteristic? _commandCharacteristic;
        private GattCharacteristic? _statusCharacteristic;


        public bool IsConnected;
        public event Action<BleConnectionStateEnum>? ConnectionStateChanged;
        public event Action<int, int, int>? WeightValueUpdated;

        public BLEService(IOptions<BleOptions> options) => _options = options.Value;
        public void Connect() => _autoConnnectLoopTask = AutoConnectLoopAsync(_cts.Token);
        private async Task<bool> ConnectAsync()
        {
            if (IsConnected)
                return true;

            var addressSource = new TaskCompletionSource<ulong>(TaskCreationOptions.RunContinuationsAsynchronously);

            _watcher = new BluetoothLEAdvertisementWatcher();

            void OnReceived(
                BluetoothLEAdvertisementWatcher sender,
                BluetoothLEAdvertisementReceivedEventArgs args)
            {
                if (args.Advertisement.LocalName != _options.DeviceName)
                    return;

                addressSource.TrySetResult(args.BluetoothAddress);
            }

            _watcher.Received += OnReceived;
            _watcher.Start();

            ulong address;

            try
            {
                address = await addressSource.Task.WaitAsync(TimeSpan.FromSeconds(15));
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

            _bluetoothLeDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(address);
            _bluetoothLeDevice.ConnectionStatusChanged += DeviceConnectionStatusChanged;
            if (_bluetoothLeDevice is null)
                return false;

            var serviceResult = await _bluetoothLeDevice.GetGattServicesForUuidAsync(_options.ServiceUuid, BluetoothCacheMode.Uncached);

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
                    .FirstOrDefault(c => c.Uuid == _options.WeightUuid);

            _commandCharacteristic =
                characteristicsResult.Characteristics
                    .FirstOrDefault(c => c.Uuid == _options.CommandUuid);

            _statusCharacteristic =
                characteristicsResult.Characteristics
                    .FirstOrDefault(c => c.Uuid == _options.StatusUuid);

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

        private void OnWeightChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            var data = new byte[args.CharacteristicValue.Length];

            using var reader = DataReader.FromBuffer(args.CharacteristicValue);
            reader.ReadBytes(data);

            if (data.Length < 4)
                return;

            int weight = BinaryPrimitives.ReadInt32LittleEndian(data);

            WeightValueUpdated?.Invoke(1, 3, weight);
        }

        private async Task AutoConnectLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (!IsConnected)
                    {
                        ConnectionStateChanged?.Invoke(BleConnectionStateEnum.Connecting);
                        await TryConnectAsync(token);
                    }
                }
                catch (OperationCanceledException)
                    when (token.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception _) { }

                await Task.Delay(TimeSpan.FromSeconds(5), token);
            }
        }
        private async Task<bool> TryConnectAsync(CancellationToken cancellationToken)
        {
            await _connectLock.WaitAsync(cancellationToken);

            try
            {
                if (IsConnected)
                    return true;

                return await ConnectAsync();
            }
            finally
            {
                _connectLock.Release();
            }
        }

        private void DeviceConnectionStatusChanged(BluetoothLEDevice device, object obj)
        {
            IsConnected = device.ConnectionStatus switch
            {
                BluetoothConnectionStatus.Connected => true,
                BluetoothConnectionStatus.Disconnected => false,
                _ => throw new NotImplementedException()
            };
            ConnectionStateChanged?.Invoke(device.ConnectionStatus == BluetoothConnectionStatus.Connected ? BleConnectionStateEnum.Connected : BleConnectionStateEnum.Disconnected);
        }
    }
}
