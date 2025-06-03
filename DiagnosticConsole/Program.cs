using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace DiagnosticConsole
{
    class Program
    {
        private static SerialPort _serialPort;
        private static bool _isRunning = true;
        private static bool _isMonitoring = false;
        private static Timer _monitorTimer;
        private static string _logFilePath = "diagnostic_log.csv";

        static void Main(string[] args)
        {
            Console.WriteLine("🔧 ECU Diagnostic Console Starting...");

            // Check if port name is provided
            if (args.Length < 1)
            {
                Console.WriteLine("Error: Please provide a serial port name.");
                Console.WriteLine("Usage: dotnet run -- /dev/ttys002");
                return;
            }

            string portName = args[0];
            Console.WriteLine($"Connecting to ECU on port: {portName}");

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
                Console.WriteLine("Connected to ECU successfully.");

                // Initialize log file if it doesn't exist
                if (!File.Exists(_logFilePath))
                {
                    using (StreamWriter writer = new StreamWriter(_logFilePath))
                    {
                        writer.WriteLine("Timestamp,Speed,RPM,Temperature,FuelLevel");
                    }
                    Console.WriteLine($"Created log file: {_logFilePath}");
                }

                // Start the main menu
                ShowMainMenu();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                _monitorTimer?.Dispose();
                _serialPort?.Close();
                Console.WriteLine("Diagnostic Console shut down.");
            }
        }

        private static void ShowMainMenu()
        {
            while (_isRunning)
            {
                Console.Clear();
                Console.WriteLine("========================================");
                Console.WriteLine("🚗 ECU DIAGNOSTIC CONSOLE");
                Console.WriteLine("========================================");
                Console.WriteLine("1. Monitor Live ECU Data");
                Console.WriteLine("2. Configure ECU Parameters");
                Console.WriteLine("3. Read Diagnostic Trouble Codes");
                Console.WriteLine("4. Clear Diagnostic Trouble Codes");
                Console.WriteLine("5. Apply Configuration Patch");
                Console.WriteLine("6. Export Diagnostic Log");
                Console.WriteLine("7. Exit");
                Console.WriteLine("========================================");
                Console.Write("Select an option: ");

                string input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        MonitorLiveData();
                        break;
                    case "2":
                        ConfigureECUParameters();
                        break;
                    case "3":
                        ReadDTCs();
                        break;
                    case "4":
                        ClearDTCs();
                        break;
                    case "5":
                        ApplyConfigPatch();
                        break;
                    case "6":
                        ExportDiagnosticLog();
                        break;
                    case "7":
                        _isRunning = false;
                        break;
                    default:
                        Console.WriteLine("Invalid option. Press any key to continue...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private static void MonitorLiveData()
        {
            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("📊 LIVE ECU DATA MONITOR");
            Console.WriteLine("========================================");
            Console.WriteLine("Press any key to stop monitoring...");
            Console.WriteLine();

            _isMonitoring = true;

            // Set up timer to request data periodically
            _monitorTimer = new Timer(RequestData, null, 0, 1000);

            // Wait for key press to stop monitoring
            Console.ReadKey(true);
            _isMonitoring = false;
            _monitorTimer.Dispose();

            Console.WriteLine("\nMonitoring stopped. Press any key to return to menu...");
            Console.ReadKey();
        }

        private static void RequestData(object state)
        {
            if (_serialPort != null && _serialPort.IsOpen && _isMonitoring)
            {
                try
                {
                    // Clear any existing data in the buffer
                    _serialPort.DiscardInBuffer();

                    // Send request for data
                    _serialPort.WriteLine("GET_DATA");

                    // Wait for response
                    Thread.Sleep(100);

                    // Read response
                    string response = _serialPort.ReadLine().Trim();
                    ParseAndDisplayData(response);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error requesting data: {ex.Message}");
                }
            }
        }

        private static void ParseAndDisplayData(string data)
        {
            try
            {
                // Parse the data string (format: SPEED:72,RPM:2200,TEMP:87,FUEL:65)
                var parts = data.Split(',');
                int speed = 0, rpm = 0, temp = 0, fuel = 0;

                foreach (var part in parts)
                {
                    var keyValue = part.Split(':');
                    if (keyValue.Length == 2)
                    {
                        switch (keyValue[0])
                        {
                            case "SPEED":
                                speed = int.Parse(keyValue[1]);
                                break;
                            case "RPM":
                                rpm = int.Parse(keyValue[1]);
                                break;
                            case "TEMP":
                                temp = int.Parse(keyValue[1]);
                                break;
                            case "FUEL":
                                fuel = int.Parse(keyValue[1]);
                                break;
                        }
                    }
                }

                // Clear previous line and display data
                Console.SetCursorPosition(0, Console.CursorTop - (Console.CursorTop > 0 ? 1 : 0));
                Console.WriteLine(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, Console.CursorTop - 1);

                // Display data with color coding
                Console.Write($"Speed: {speed} km/h | ");
                
                // Color code RPM
                Console.Write("RPM: ");
                if (rpm > 5000)
                    Console.ForegroundColor = ConsoleColor.Red;
                else if (rpm > 3500)
                    Console.ForegroundColor = ConsoleColor.Yellow;
                else
                    Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(rpm);
                Console.ResetColor();
                
                Console.Write(" | Temperature: ");
                // Color code temperature
                if (temp > 95)
                    Console.ForegroundColor = ConsoleColor.Red;
                else if (temp > 85)
                    Console.ForegroundColor = ConsoleColor.Yellow;
                else
                    Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"{temp}°C");
                Console.ResetColor();
                
                Console.Write(" | Fuel: ");
                // Color code fuel level
                if (fuel < 15)
                    Console.ForegroundColor = ConsoleColor.Red;
                else if (fuel < 30)
                    Console.ForegroundColor = ConsoleColor.Yellow;
                else
                    Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"{fuel}%");
                Console.ResetColor();
                Console.WriteLine();

                // Log data to file
                LogData(speed, rpm, temp, fuel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing data: {ex.Message}");
            }
        }

        private static void LogData(int speed, int rpm, int temp, int fuel)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(_logFilePath, true))
                {
                    writer.WriteLine($"{DateTime.Now},{speed},{rpm},{temp},{fuel}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging data: {ex.Message}");
            }
        }

        private static void ConfigureECUParameters()
        {
            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("⚙️ CONFIGURE ECU PARAMETERS");
            Console.WriteLine("========================================");
            Console.WriteLine("1. Set Maximum RPM");
            Console.WriteLine("2. Set Fan Trigger Temperature");
            Console.WriteLine("3. Set Fuel Warning Level");
            Console.WriteLine("4. Return to Main Menu");
            Console.WriteLine("========================================");
            Console.Write("Select an option: ");

            string input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    SetMaxRPM();
                    break;
                case "2":
                    SetFanTriggerTemp();
                    break;
                case "3":
                    SetFuelWarningLevel();
                    break;
                case "4":
                    return;
                default:
                    Console.WriteLine("Invalid option. Press any key to continue...");
                    Console.ReadKey();
                    ConfigureECUParameters();
                    break;
            }
        }

        private static void SetMaxRPM()
        {
            Console.Write("Enter new maximum RPM value: ");
            if (int.TryParse(Console.ReadLine(), out int maxRpm))
            {
                try
                {
                    _serialPort.WriteLine($"SET_MAX_RPM:{maxRpm}");
                    Thread.Sleep(100);
                    string response = _serialPort.ReadLine().Trim();

                    if (response.StartsWith("MAX_RPM_SET:"))
                    {
                        Console.WriteLine($"Maximum RPM set to {maxRpm}");
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error setting max RPM: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a number.");
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            ConfigureECUParameters();
        }

        private static void SetFanTriggerTemp()
        {
            Console.Write("Enter new fan trigger temperature (°C): ");
            if (int.TryParse(Console.ReadLine(), out int fanTemp))
            {
                try
                {
                    _serialPort.WriteLine($"SET_FAN_TEMP:{fanTemp}");
                    Thread.Sleep(100);
                    string response = _serialPort.ReadLine().Trim();

                    if (response.StartsWith("FAN_TEMP_SET:"))
                    {
                        Console.WriteLine($"Fan trigger temperature set to {fanTemp}°C");
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error setting fan temperature: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a number.");
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            ConfigureECUParameters();
        }

        private static void SetFuelWarningLevel()
        {
            Console.Write("Enter new fuel warning level (%): ");
            if (int.TryParse(Console.ReadLine(), out int fuelLevel))
            {
                try
                {
                    _serialPort.WriteLine($"SET_FUEL_WARNING:{fuelLevel}");
                    Thread.Sleep(100);
                    string response = _serialPort.ReadLine().Trim();

                    if (response.StartsWith("FUEL_WARNING_SET:"))
                    {
                        Console.WriteLine($"Fuel warning level set to {fuelLevel}%");
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error setting fuel warning level: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a number.");
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            ConfigureECUParameters();
        }

        private static void ReadDTCs()
        {
            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("🔍 DIAGNOSTIC TROUBLE CODES");
            Console.WriteLine("========================================");

            try
            {
                _serialPort.WriteLine("GET_DTC");
                Thread.Sleep(100);
                string response = _serialPort.ReadLine().Trim();

                if (response == "NO_DTC")
                {
                    Console.WriteLine("No diagnostic trouble codes found.");
                }
                else
                {
                    var dtcCodes = response.Split(";;");
                    foreach (var dtc in dtcCodes)
                    {
                        if (!string.IsNullOrWhiteSpace(dtc))
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine(dtc);
                            Console.ResetColor();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading DTCs: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey();
        }

        private static void ClearDTCs()
        {
            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("🧹 CLEAR DIAGNOSTIC TROUBLE CODES");
            Console.WriteLine("========================================");
            Console.Write("Are you sure you want to clear all DTCs? (y/n): ");

            string input = Console.ReadLine().ToLower();
            if (input == "y" || input == "yes")
            {
                try
                {
                    _serialPort.WriteLine("CLEAR_DTC");
                    Thread.Sleep(100);
                    string response = _serialPort.ReadLine().Trim();

                    if (response == "DTC_CLEARED")
                    {
                        Console.WriteLine("All diagnostic trouble codes have been cleared.");
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error clearing DTCs: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Operation cancelled.");
            }

            Console.WriteLine("Press any key to return to menu...");
            Console.ReadKey();
        }

        private static void ApplyConfigPatch()
        {
            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("📝 APPLY CONFIGURATION PATCH");
            Console.WriteLine("========================================");
            Console.WriteLine("Enter JSON patch (example: {\"max_rpm\": 7000}):");

            string patchJson = Console.ReadLine();

            try
            {
                // Validate JSON format
                JsonDocument.Parse(patchJson);

                _serialPort.WriteLine($"APPLY_PATCH:{patchJson}");
                Thread.Sleep(100);
                string response = _serialPort.ReadLine().Trim();

                if (response == "PATCH_APPLIED")
                {
                    Console.WriteLine("Configuration patch applied successfully.");
                }
                else
                {
                    Console.WriteLine($"Error: {response}");
                }
            }
            catch (JsonException)
            {
                Console.WriteLine("Error: Invalid JSON format.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying patch: {ex.Message}");
            }

            Console.WriteLine("Press any key to return to menu...");
            Console.ReadKey();
        }

        private static void ExportDiagnosticLog()
        {
            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("📊 EXPORT DIAGNOSTIC LOG");
            Console.WriteLine("========================================");

            if (File.Exists(_logFilePath))
            {
                string exportPath = $"ecu_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                File.Copy(_logFilePath, exportPath);
                Console.WriteLine($"Log exported to: {exportPath}");

                // Display log statistics
                try
                {
                    var lines = File.ReadAllLines(_logFilePath).Skip(1).ToList(); // Skip header
                    if (lines.Count > 0)
                    {
                        Console.WriteLine($"\nLog Statistics:");
                        Console.WriteLine($"- Total entries: {lines.Count}");
                        Console.WriteLine($"- First entry: {lines.First().Split(',')[0]}");
                        Console.WriteLine($"- Last entry: {lines.Last().Split(',')[0]}");
                    }
                    else
                    {
                        Console.WriteLine("\nLog is empty.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error analyzing log: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("No log file found.");
            }

            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey();
        }
    }
}