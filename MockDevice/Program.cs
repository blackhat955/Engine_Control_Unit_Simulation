using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Collections.Generic;

namespace MockDevice
{
    class Program
    {
        private static SerialPort? serialPort;
        private static string firmwareVersion = "FW_1.0.3";
        private static string receivedFirmwarePath = "received_firmware.hex";
        private static bool isRunning = true;

        static void Main(string[] args)
        {
            Console.WriteLine("===== MockDevice v1.0 =====");
            Console.WriteLine("A simulated device for testing firmware updates");
            Console.WriteLine();

            // Clear any previous received firmware file
            if (File.Exists(receivedFirmwarePath))
            {
                File.Delete(receivedFirmwarePath);
            }

            try
            {
                ScanSerialPorts();
                ConnectToSerialPort();
                
                if (serialPort != null && serialPort.IsOpen)
                {
                    Console.WriteLine("\nMockDevice is running. Press Ctrl+C to exit.");
                    Console.WriteLine($"Current firmware version: {firmwareVersion}");
                    
                    // Set up console cancellation
                    Console.CancelKeyPress += (sender, e) => {
                        e.Cancel = true;
                        isRunning = false;
                    };
                    
                    // Main processing loop
                    ProcessCommands();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
            }
            finally
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    serialPort.Close();
                    Console.WriteLine("Serial port closed.");
                }
            }

            Console.WriteLine("\nMockDevice stopped.");
        }

        static void ScanSerialPorts()
        {
            Console.WriteLine("Available serial ports:");
            string[] ports = SerialPort.GetPortNames();

            if (ports.Length == 0)
            {
                Console.WriteLine("No serial ports found.");
                return;
            }

            for (int i = 0; i < ports.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {ports[i]}");
            }
        }

        static void ConnectToSerialPort()
        {
            Console.Write("\nEnter port name or number to listen on (e.g., /dev/ttys004 or 1): ");
            string input = Console.ReadLine() ?? "";

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
                    ReadTimeout = -1, // Infinite timeout for reading
                    NewLine = "\n"   // Set newline character
                };

                serialPort.Open();
                Console.WriteLine($"Listening on {portName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to port: {ex.Message}");
            }
        }

        static void ProcessCommands()
        {
            if (serialPort == null || !serialPort.IsOpen) return;

            bool receivingFirmware = false;
            int firmwareLinesReceived = 0;
            List<string> firmwareLines = new List<string>();

            while (isRunning)
            {
                try
                {
                    // Read incoming command
                    string command = serialPort.ReadLine().Trim();
                    Console.WriteLine($"Received: {command}");

                    // Process command
                    if (command == "GET_VER")
                    {
                        Console.WriteLine($"Sending version: {firmwareVersion}");
                        serialPort.WriteLine(firmwareVersion);
                    }
                    else if (command == "START_FIRMWARE")
                    {
                        Console.WriteLine("Starting firmware reception...");
                        receivingFirmware = true;
                        firmwareLinesReceived = 0;
                        firmwareLines.Clear();
                        serialPort.WriteLine("READY");
                    }
                    else if (command == "END_FIRMWARE")
                    {
                        receivingFirmware = false;
                        Console.WriteLine($"Firmware reception completed. {firmwareLinesReceived} lines received.");
                        
                        // Save received firmware to file
                        File.WriteAllLines(receivedFirmwarePath, firmwareLines);
                        Console.WriteLine($"Firmware saved to {receivedFirmwarePath}");
                        
                        // Simulate processing time
                        Thread.Sleep(1000);
                        serialPort.WriteLine("UPDATE_SUCCESS");
                    }
                    else if (command.StartsWith("FIRMWARE:"))
                    {
                        if (receivingFirmware)
                        {
                            string firmwareLine = command.Substring(9); // Remove "FIRMWARE:" prefix
                            firmwareLines.Add(firmwareLine);
                            firmwareLinesReceived++;
                            
                            // Simulate occasional errors (approximately 10% of the time)
                            if (new Random().Next(10) == 0)
                            {
                                Console.WriteLine("Simulating error response...");
                                serialPort.WriteLine("ERROR");
                            }
                            else
                            {
                                serialPort.WriteLine("OK");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Received firmware data without START_FIRMWARE command");
                            serialPort.WriteLine("ERROR: Not in firmware reception mode");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Unknown command: {command}");
                        serialPort.WriteLine("UNKNOWN_COMMAND");
                    }
                }
                catch (TimeoutException)
                {
                    // Timeout is normal, just continue
                }
                catch (IOException ex)
                {
                    // Port might be closed or disconnected
                    Console.WriteLine($"IO Error: {ex.Message}");
                    isRunning = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }
}
