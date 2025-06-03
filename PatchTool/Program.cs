using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace PatchTool
{
    class Program
    {
        private static SerialPort? serialPort;
        private static string logFilePath = "patch_log.txt";
        private static bool isConnected = false;

        static void Main(string[] args)
        {
            Console.WriteLine("===== PatchTool v1.0 =====");
            Console.WriteLine("A utility for firmware updates over serial connection");
            Console.WriteLine();

            try
            {
                // Start logging
                File.WriteAllText(logFilePath, $"PatchTool session started at {DateTime.Now}\n");
                LogMessage("PatchTool initialized");

                // Main program flow
                ScanSerialPorts();
                ConnectToSerialPort();
                
                if (isConnected)
                {
                    CheckDeviceVersion();
                    SendFirmwareUpdate();
                    CloseConnection();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
                LogMessage($"ERROR: {ex.Message}");
            }
            finally
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    serialPort.Close();
                    LogMessage("Serial port closed");
                }
                LogMessage("PatchTool session ended");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        static void ScanSerialPorts()
        {
            Console.WriteLine("Scanning for available serial ports...");
            LogMessage("Scanning for available serial ports");

            string[] ports = SerialPort.GetPortNames();

            if (ports.Length == 0)
            {
                Console.WriteLine("No serial ports found.");
                LogMessage("No serial ports found");
                return;
            }

            Console.WriteLine("Available ports:");
            for (int i = 0; i < ports.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {ports[i]}");
            }
            LogMessage($"Found {ports.Length} serial ports: {string.Join(", ", ports)}");
        }

        static void ConnectToSerialPort()
        {
            Console.Write("\nEnter port name or number to connect (e.g., /dev/ttys003 or 1): ");
            string input = Console.ReadLine() ?? "";
            LogMessage($"User selected port: {input}");

            string portName;
            if (int.TryParse(input, out int portNumber))
            {
                string[] ports = SerialPort.GetPortNames();
                if (portNumber > 0 && portNumber <= ports.Length)
                {
                    portName = ports[portNumber - 1];
                }
                else
                {
                    Console.WriteLine("Invalid port number.");
                    LogMessage("Invalid port number entered");
                    return;
                }
            }
            else
            {
                portName = input;
            }

            try
            {
                serialPort = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One)
                {
                    ReadTimeout = 2000,
                    WriteTimeout = 2000
                };

                serialPort.Open();
                isConnected = true;
                Console.WriteLine($"Connected to {portName}");
                LogMessage($"Connected to {portName} at 9600 baud");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to port: {ex.Message}");
                LogMessage($"Error connecting to port: {ex.Message}");
            }
        }

        static void CheckDeviceVersion()
        {
            if (serialPort == null || !serialPort.IsOpen) return;

            Console.WriteLine("\nChecking device version...");
            LogMessage("Sending GET_VER command");

            try
            {
                // Clear any existing data
                serialPort.DiscardInBuffer();
                
                // Send version request command
                serialPort.WriteLine("GET_VER");
                LogMessage("Sent: GET_VER");
                
                // Wait for response
                Thread.Sleep(500);
                
                // Read response
                string response = serialPort.ReadLine().Trim();
                Console.WriteLine($"Device version: {response}");
                LogMessage($"Received version: {response}");

                // Optional: Version verification logic could be added here
                if (response != "FW_1.0.3")
                {
                    Console.WriteLine("Warning: Expected version FW_1.0.3 but got " + response);
                    LogMessage($"Version mismatch. Expected FW_1.0.3, got {response}");
                    
                    Console.Write("Continue with firmware update anyway? (y/n): ");
                    string answer = Console.ReadLine()?.ToLower() ?? "n";
                    
                    if (answer != "y")
                    {
                        Console.WriteLine("Firmware update aborted.");
                        LogMessage("Firmware update aborted by user due to version mismatch");
                        isConnected = false;
                    }
                }
            }
            catch (TimeoutException)
            {
                Console.WriteLine("Timeout waiting for device response.");
                LogMessage("Timeout waiting for version response");
                isConnected = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking version: {ex.Message}");
                LogMessage($"Error checking version: {ex.Message}");
                isConnected = false;
            }
        }

        static void SendFirmwareUpdate()
        {
            if (serialPort == null || !serialPort.IsOpen || !isConnected) return;

            string firmwarePath = "firmware.hex";
            
            // Check if firmware file exists in current directory
            if (!File.Exists(firmwarePath))
            {
                // Try with absolute path
                firmwarePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "firmware.hex");
                
                // If still not found, try one directory up (project root)
                if (!File.Exists(firmwarePath))
                {
                    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    var parentDir = Directory.GetParent(baseDir);
                    if (parentDir?.Parent != null)
                    {
                        firmwarePath = Path.Combine(parentDir.Parent.FullName, "firmware.hex");
                    }
                    else
                    {
                        Console.WriteLine("Firmware file not found. Please place firmware.hex in the application directory.");
                        LogMessage("Firmware file not found");
                        return;
                    }
                    
                    if (!File.Exists(firmwarePath))
                    {
                        Console.WriteLine("Firmware file not found. Please place firmware.hex in the application directory.");
                        LogMessage("Firmware file not found");
                        return;
                    }
                }
            }

            Console.WriteLine($"\nSending firmware update from {firmwarePath}...");
            LogMessage($"Starting firmware update from {firmwarePath}");

            try
            {
                string[] firmwareLines = File.ReadAllLines(firmwarePath);
                int totalLines = firmwareLines.Length;
                int successCount = 0;

                Console.WriteLine($"Sending {totalLines} lines of firmware data...");
                LogMessage($"Firmware file contains {totalLines} lines");

                // Send firmware initialization command
                serialPort.WriteLine("START_FIRMWARE");
                LogMessage("Sent: START_FIRMWARE");
                Thread.Sleep(500);

                // Read response
                string response = serialPort.ReadLine().Trim();
                LogMessage($"Received: {response}");

                if (response != "READY")
                {
                    Console.WriteLine("Device not ready for firmware update.");
                    LogMessage("Device not ready for firmware update");
                    return;
                }

                // Send each line of the firmware file
                for (int i = 0; i < totalLines; i++)
                {
                    string line = firmwareLines[i];
                    string command = $"FIRMWARE:{line}";
                    
                    serialPort.WriteLine(command);
                    LogMessage($"Sent: {command}");
                    
                    // Wait for response
                    Thread.Sleep(100);
                    response = serialPort.ReadLine().Trim();
                    LogMessage($"Received: {response}");

                    if (response == "OK")
                    {
                        successCount++;
                        Console.Write($"\rProgress: {i + 1}/{totalLines} lines sent");
                    }
                    else
                    {
                        Console.WriteLine($"\nError sending line {i + 1}: {response}");
                        LogMessage($"Error on line {i + 1}: {response}");
                        
                        // Simple retry logic
                        Console.WriteLine("Retrying...");
                        LogMessage("Retrying failed line");
                        
                        serialPort.WriteLine(command);
                        LogMessage($"Resent: {command}");
                        
                        Thread.Sleep(100);
                        response = serialPort.ReadLine().Trim();
                        LogMessage($"Received: {response}");
                        
                        if (response == "OK")
                        {
                            successCount++;
                            Console.Write($"\rProgress: {i + 1}/{totalLines} lines sent");
                        }
                        else
                        {
                            Console.WriteLine($"\nFailed to send line {i + 1} after retry.");
                            LogMessage($"Failed to send line {i + 1} after retry");
                        }
                    }
                }

                Console.WriteLine($"\n\nFirmware update completed. {successCount}/{totalLines} lines successfully sent.");
                LogMessage($"Firmware update completed. {successCount}/{totalLines} lines successfully sent");

                // Send firmware completion command
                serialPort.WriteLine("END_FIRMWARE");
                LogMessage("Sent: END_FIRMWARE");
                Thread.Sleep(500);
                
                response = serialPort.ReadLine().Trim();
                Console.WriteLine($"Device response: {response}");
                LogMessage($"Final response: {response}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during firmware update: {ex.Message}");
                LogMessage($"Error during firmware update: {ex.Message}");
            }
        }

        static void CloseConnection()
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
                Console.WriteLine("\nSerial connection closed.");
                LogMessage("Serial connection closed");
            }
        }

        static void LogMessage(string message)
        {
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n";
            File.AppendAllText(logFilePath, logEntry);
        }
    }
}
