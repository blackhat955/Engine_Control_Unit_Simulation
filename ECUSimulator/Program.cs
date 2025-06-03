using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Collections.Generic;

namespace ECUSimulator
{
    class Program
    {
        private static SerialPort _serialPort;
        private static Random _random = new Random();
        private static ECUConfig _config;
        private static List<DTCCode> _dtcCodes;
        private static string _configPath = "ecu_config.json";
        private static string _dtcPath = "dtc_codes.json";
        private static bool _isRunning = true;
        private static Timer _dataTimer;

        static void Main(string[] args)
        {
            Console.WriteLine("🚗 ECU Simulator Starting...");

            // Check if port name is provided
            if (args.Length < 1)
            {
                Console.WriteLine("Error: Please provide a serial port name.");
                Console.WriteLine("Usage: dotnet run -- /dev/ttys001");
                return;
            }

            string portName = args[0];
            Console.WriteLine($"Initializing on port: {portName}");

            // Load configuration
            LoadConfig();
            
            // Load DTC codes
            LoadDTCCodes();

            try
            {
                // Initialize serial port
                _serialPort = new SerialPort(portName)
                {
                    BaudRate = 9600,
                    Parity = Parity.None,
                    StopBits = StopBits.One,
                    DataBits = 8,
                    Handshake = Handshake.None,
                    ReadTimeout = 500,
                    WriteTimeout = 500
                };

                _serialPort.Open();
                Console.WriteLine("Serial port opened successfully.");

                // Set up data timer to send periodic updates
                _dataTimer = new Timer(SendSensorData, null, 0, 1000);

                // Start listening for commands
                Console.WriteLine("Listening for commands...");
                ListenForCommands();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                _dataTimer?.Dispose();
                _serialPort?.Close();
                Console.WriteLine("ECU Simulator shut down.");
            }
        }

        private static void LoadConfig()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    string json = File.ReadAllText(_configPath);
                    _config = JsonSerializer.Deserialize<ECUConfig>(json);
                    Console.WriteLine("Configuration loaded successfully.");
                }
                else
                {
                    // Create default config
                    _config = new ECUConfig
                    {
                        MaxRpm = 6000,
                        FanTriggerTemp = 95,
                        FuelWarningLevel = 15
                    };
                    SaveConfig();
                    Console.WriteLine("Default configuration created.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading configuration: {ex.Message}");
                _config = new ECUConfig
                {
                    MaxRpm = 6000,
                    FanTriggerTemp = 95,
                    FuelWarningLevel = 15
                };
            }
        }

        private static void SaveConfig()
        {
            try
            {
                string json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_configPath, json);
                Console.WriteLine("Configuration saved.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving configuration: {ex.Message}");
            }
        }

        private static void LoadDTCCodes()
        {
            try
            {
                if (File.Exists(_dtcPath))
                {
                    string json = File.ReadAllText(_dtcPath);
                    _dtcCodes = JsonSerializer.Deserialize<List<DTCCode>>(json);
                    Console.WriteLine("DTC codes loaded successfully.");
                }
                else
                {
                    // Create default DTC codes
                    _dtcCodes = new List<DTCCode>
                    {
                        new DTCCode { Code = "P0300", Description = "Random Misfire", IsActive = true },
                        new DTCCode { Code = "P0420", Description = "Catalyst System Efficiency Below Threshold", IsActive = true }
                    };
                    SaveDTCCodes();
                    Console.WriteLine("Default DTC codes created.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading DTC codes: {ex.Message}");
                _dtcCodes = new List<DTCCode>
                {
                    new DTCCode { Code = "P0300", Description = "Random Misfire", IsActive = true },
                    new DTCCode { Code = "P0420", Description = "Catalyst System Efficiency Below Threshold", IsActive = true }
                };
            }
        }

        private static void SaveDTCCodes()
        {
            try
            {
                string json = JsonSerializer.Serialize(_dtcCodes, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_dtcPath, json);
                Console.WriteLine("DTC codes saved.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving DTC codes: {ex.Message}");
            }
        }

        private static void SendSensorData(object state)
        {
            if (_serialPort != null && _serialPort.IsOpen && _isRunning)
            {
                try
                {
                    // Generate random sensor data
                    int speed = _random.Next(0, 120);
                    int rpm = _random.Next(800, _config.MaxRpm);
                    int temp = _random.Next(70, 110);
                    int fuel = _random.Next(0, 100);

                    // Format data string
                    string data = $"SPEED:{speed},RPM:{rpm},TEMP:{temp},FUEL:{fuel}";

                    // Send data over serial port
                    _serialPort.WriteLine(data);
                    Console.WriteLine($"Sent: {data}");

                    // Check for conditions that might trigger DTCs
                    if (temp > _config.FanTriggerTemp + 10)
                    {
                        // Add high temperature DTC if not already present
                        if (!_dtcCodes.Exists(d => d.Code == "P0217"))
                        {
                            _dtcCodes.Add(new DTCCode { Code = "P0217", Description = "Engine Coolant Over Temperature", IsActive = true });
                            SaveDTCCodes();
                            Console.WriteLine("Added P0217 DTC: Engine Coolant Over Temperature");
                        }
                    }

                    if (fuel < _config.FuelWarningLevel)
                    {
                        // Add low fuel DTC if not already present
                        if (!_dtcCodes.Exists(d => d.Code == "P0460"))
                        {
                            _dtcCodes.Add(new DTCCode { Code = "P0460", Description = "Fuel Level Sensor Circuit", IsActive = true });
                            SaveDTCCodes();
                            Console.WriteLine("Added P0460 DTC: Fuel Level Sensor Circuit");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending data: {ex.Message}");
                }
            }
        }

        private static void ListenForCommands()
        {
            while (_isRunning)
            {
                try
                {
                    if (_serialPort.IsOpen && _serialPort.BytesToRead > 0)
                    {
                        string command = _serialPort.ReadLine().Trim();
                        Console.WriteLine($"Received command: {command}");
                        ProcessCommand(command);
                    }
                }
                catch (TimeoutException)
                {
                    // Timeout is normal, continue
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading command: {ex.Message}");
                }

                Thread.Sleep(100); // Small delay to prevent CPU hogging
            }
        }

        private static void ProcessCommand(string command)
        {
            try
            {
                if (command == "GET_DATA")
                {
                    // Generate random sensor data
                    int speed = _random.Next(0, 120);
                    int rpm = _random.Next(800, _config.MaxRpm);
                    int temp = _random.Next(70, 110);
                    int fuel = _random.Next(0, 100);

                    // Format data string
                    string data = $"SPEED:{speed},RPM:{rpm},TEMP:{temp},FUEL:{fuel}";
                    _serialPort.WriteLine(data);
                }
                else if (command.StartsWith("SET_MAX_RPM:"))
                {
                    string valueStr = command.Substring("SET_MAX_RPM:".Length);
                    if (int.TryParse(valueStr, out int maxRpm))
                    {
                        _config.MaxRpm = maxRpm;
                        SaveConfig();
                        _serialPort.WriteLine($"MAX_RPM_SET:{maxRpm}");
                    }
                    else
                    {
                        _serialPort.WriteLine("ERROR:Invalid RPM value");
                    }
                }
                else if (command.StartsWith("SET_FAN_TEMP:"))
                {
                    string valueStr = command.Substring("SET_FAN_TEMP:".Length);
                    if (int.TryParse(valueStr, out int fanTemp))
                    {
                        _config.FanTriggerTemp = fanTemp;
                        SaveConfig();
                        _serialPort.WriteLine($"FAN_TEMP_SET:{fanTemp}");
                    }
                    else
                    {
                        _serialPort.WriteLine("ERROR:Invalid temperature value");
                    }
                }
                else if (command.StartsWith("SET_FUEL_WARNING:"))
                {
                    string valueStr = command.Substring("SET_FUEL_WARNING:".Length);
                    if (int.TryParse(valueStr, out int fuelLevel))
                    {
                        _config.FuelWarningLevel = fuelLevel;
                        SaveConfig();
                        _serialPort.WriteLine($"FUEL_WARNING_SET:{fuelLevel}");
                    }
                    else
                    {
                        _serialPort.WriteLine("ERROR:Invalid fuel level value");
                    }
                }
                else if (command == "GET_DTC")
                {
                    if (_dtcCodes.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var dtc in _dtcCodes)
                        {
                            if (dtc.IsActive)
                            {
                                sb.Append($"[{dtc.Code}] {dtc.Description};;");
                            }
                        }
                        _serialPort.WriteLine(sb.ToString());
                    }
                    else
                    {
                        _serialPort.WriteLine("NO_DTC");
                    }
                }
                else if (command == "CLEAR_DTC")
                {
                    foreach (var dtc in _dtcCodes)
                    {
                        dtc.IsActive = false;
                    }
                    SaveDTCCodes();
                    _serialPort.WriteLine("DTC_CLEARED");
                }
                else if (command == "GET_CONFIG")
                {
                    string configJson = JsonSerializer.Serialize(_config);
                    _serialPort.WriteLine(configJson);
                }
                else if (command.StartsWith("APPLY_PATCH:"))
                {
                    string patchJson = command.Substring("APPLY_PATCH:".Length);
                    try
                    {
                        // Parse and apply the patch
                        var patchConfig = JsonSerializer.Deserialize<ECUConfig>(patchJson);
                        
                        // Apply non-null values from the patch
                        if (patchConfig.MaxRpm > 0)
                            _config.MaxRpm = patchConfig.MaxRpm;
                        
                        if (patchConfig.FanTriggerTemp > 0)
                            _config.FanTriggerTemp = patchConfig.FanTriggerTemp;
                        
                        if (patchConfig.FuelWarningLevel > 0)
                            _config.FuelWarningLevel = patchConfig.FuelWarningLevel;
                        
                        SaveConfig();
                        _serialPort.WriteLine("PATCH_APPLIED");
                    }
                    catch (JsonException)
                    {
                        _serialPort.WriteLine("ERROR:Invalid JSON patch");
                    }
                }
                else
                {
                    _serialPort.WriteLine("ERROR:Unknown command");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing command: {ex.Message}");
                try
                {
                    _serialPort.WriteLine("ERROR:Internal error");
                }
                catch
                {
                    // Ignore write errors here
                }
            }
        }
    }

    public class ECUConfig
    {
        [JsonPropertyName("max_rpm")]
        public int MaxRpm { get; set; }

        [JsonPropertyName("fan_trigger_temp")]
        public int FanTriggerTemp { get; set; }

        [JsonPropertyName("fuel_warning_level")]
        public int FuelWarningLevel { get; set; }
    }

    public class DTCCode
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }
    }
}