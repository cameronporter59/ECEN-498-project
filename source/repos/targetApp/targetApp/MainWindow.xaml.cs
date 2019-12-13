using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using System.Diagnostics;
using Windows.Storage.Streams;
using Windows.Security.Cryptography;

namespace targetApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
 
    public static class Globals
    {
        public static int state = 0; // global state, 0 is not hit, 1 is hit
        
    }
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            
            BluetoothLEAdvertisementWatcher Watcher = new BluetoothLEAdvertisementWatcher();

            Watcher.ScanningMode = BluetoothLEScanningMode.Active;
            Watcher.Received += Watcher_Received;
            Watcher.Start();
            
        }

        private async void Watcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            //Debug.WriteLine($"Found New Device: {args.Advertisement.LocalName}");
            if (args.Advertisement.LocalName == "Thermometer Example")
            {
                //Debug.WriteLine($"Found target: {args.Advertisement.LocalName}");
                BluetoothLEDevice target = await BluetoothLEDevice.FromBluetoothAddressAsync(args.BluetoothAddress);
                
                GattDeviceServicesResult result = await target.GetGattServicesAsync(BluetoothCacheMode.Uncached);

                if (result.Status == GattCommunicationStatus.Success)
                {
                    var services = result.Services;
                    //Debug.WriteLine(String.Format("Found {0} services", services.Count));
                    foreach (var service in services)
                    {
                        string serviceIdentity = "00001809-0000-1000-8000-00805f9b34fb";
                        string currentIdentity = service.Uuid.ToString();
                        //Debug.WriteLine($"Service: {service.Uuid}");
                        if (serviceIdentity.Equals(currentIdentity)){

                            Debug.WriteLine("Connected to Service!");

                            var accessStatus = await service.RequestAccessAsync();
                            if (accessStatus == DeviceAccessStatus.Allowed)
                            {

                            }

                                GattCharacteristicsResult characteristicsResult = await service.GetCharacteristicsAsync(BluetoothCacheMode.Uncached);
                            var characteristics = characteristicsResult.Characteristics;
                            foreach (var characteristic in characteristics)
                                
                            {
                                var res = await characteristic.GetDescriptorsAsync(BluetoothCacheMode.Uncached);
                                var props = characteristic.CharacteristicProperties;
                                if(props.HasFlag(GattCharacteristicProperties.Indicate))
                                {
                                    await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Indicate);
                                    characteristic.ValueChanged += Characteristic_ValueChanged;
                                                           
                                }
                                
                            }
                            
                            
                        }
                    }
                }
            }

        }

        private void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            
            byte[] data;
            CryptographicBuffer.CopyToByteArray(args.CharacteristicValue, out data);
            Debug.WriteLine($"New values: {data[0]}, {data[1]}, {data[2]}, {data[3]}, {data[4]}");
            if (data[1] > 8)
            {
                Globals.state = 1;
            }
            
            if (Globals.state == 1)
            {
                Debug.WriteLine("Target HIT!");
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Hitmarker.Visibility = Visibility.Visible;
                });
                Globals.state = 0;
            }
            else
            {
                Globals.state = 0;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Hitmarker.Visibility = Visibility.Collapsed;
                });
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        
    }
}
