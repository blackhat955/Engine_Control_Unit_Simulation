# Extending the ECU Simulator & Diagnostic Toolchain

This guide provides instructions on how to extend the ECU Simulator and Diagnostic Console with new features. The system is designed to be modular and easily extensible.

## Table of Contents

1. [Adding New Sensors](#adding-new-sensors)
2. [Adding New Commands](#adding-new-commands)
3. [Creating Custom Patches](#creating-custom-patches)
4. [Implementing Advanced Features](#implementing-advanced-features)
5. [Best Practices](#best-practices)

## Adding New Sensors

The ECU Simulator can be extended to simulate additional sensors beyond the default set (Speed, RPM, Temperature, Fuel Level).

### Step 1: Modify the ECU Simulator

1. Open `ECUSimulator/Program.cs`
2. Locate the `SendSensorData` method
3. Add your new sensor to the data generation:

```csharp
// Generate random sensor data
int speed = _random.Next(0, 120);
int rpm = _random.Next(800, _config.MaxRpm);
int temp = _random.Next(70, 110);
int fuel = _random.Next(0, 100);
// Add your new sensor here
float o2Sensor = (float)(_random.Next(75, 100) / 100.0); // Example O2 sensor (0.75-1.00)

// Format data string
string data = $"SPEED:{speed},RPM:{rpm},TEMP:{temp},FUEL:{fuel},O2:{o2Sensor:F2}";
```

### Step 2: Update the ECU Configuration

1. Add the new sensor configuration to the `ECUConfig` class:

```csharp
public class ECUConfig
{
    [JsonPropertyName("max_rpm")]
    public int MaxRpm { get; set; }

    [JsonPropertyName("fan_trigger_temp")]
    public int FanTriggerTemp { get; set; }

    [JsonPropertyName("fuel_warning_level")]
    public int FuelWarningLevel { get; set; }
    
    [JsonPropertyName("o2_sensor_enabled")]
    public bool O2SensorEnabled { get; set; }
    
    [JsonPropertyName("o2_sensor_warning_threshold")]
    public float O2SensorWarningThreshold { get; set; }
}
```

### Step 3: Update the Diagnostic Console

1. Modify the `ParseAndDisplayData` method in `DiagnosticConsole/Program.cs` to handle the new sensor:

```csharp
private static void ParseAndDisplayData(string data)
{
    try
    {
        // Parse the data string
        var parts = data.Split(',');
        int speed = 0, rpm = 0, temp = 0, fuel = 0;
        float o2 = 0.0f;

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
                    case "O2":
                        o2 = float.Parse(keyValue[1]);
                        break;
                }
            }
        }

        // Display data with color coding
        // ... existing code ...
        
        // Add O2 sensor display
        if (o2 > 0)
        {
            Console.Write(" | O2: ");
            if (o2 < 0.80)
                Console.ForegroundColor = ConsoleColor.Red;
            else if (o2 > 0.95)
                Console.ForegroundColor = ConsoleColor.Yellow;
            else
                Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{o2:F2}");
            Console.ResetColor();
        }
        
        Console.WriteLine();

        // Log data to file
        LogData(speed, rpm, temp, fuel, o2);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error parsing data: {ex.Message}");
    }
}
```

## Adding New Commands

You can extend the command set that the ECU Simulator responds to.

### Step 1: Add Command Processing

1. Open `ECUSimulator/Program.cs`
2. Locate the `ProcessCommand` method
3. Add a new command case:

```csharp
else if (command == "GET_O2_STATUS")
{
    if (_config.O2SensorEnabled)
    {
        float o2Value = (float)(_random.Next(75, 100) / 100.0);
        _serialPort.WriteLine($"O2_STATUS:{o2Value:F2}");
    }
    else
    {
        _serialPort.WriteLine("O2_SENSOR_DISABLED");
    }
}
else if (command.StartsWith("ENABLE_O2_SENSOR:"))
{
    string valueStr = command.Substring("ENABLE_O2_SENSOR:".Length);
    if (bool.TryParse(valueStr, out bool enabled))
    {
        _config.O2SensorEnabled = enabled;
        SaveConfig();
        _serialPort.WriteLine($"O2_SENSOR_ENABLED:{enabled}");
    }
    else
    {
        _serialPort.WriteLine("ERROR:Invalid value");
    }
}
```

### Step 2: Update the Diagnostic Console

1. Add a new menu option in the `ConfigureECUParameters` method:

```csharp
Console.WriteLine("4. Configure O2 Sensor");
Console.WriteLine("5. Return to Main Menu");
```

2. Add a new method to handle the command:

```csharp
private static void ConfigureO2Sensor()
{
    Console.Clear();
    Console.WriteLine("========================================");
    Console.WriteLine("⚙️ CONFIGURE O2 SENSOR");
    Console.WriteLine("========================================");
    Console.WriteLine("1. Enable O2 Sensor");
    Console.WriteLine("2. Disable O2 Sensor");
    Console.WriteLine("3. Set Warning Threshold");
    Console.WriteLine("4. Return to Configuration Menu");
    Console.WriteLine("========================================");
    Console.Write("Select an option: ");

    string input = Console.ReadLine();

    switch (input)
    {
        case "1":
            _serialPort.WriteLine("ENABLE_O2_SENSOR:true");
            Thread.Sleep(100);
            string response1 = _serialPort.ReadLine().Trim();
            Console.WriteLine("O2 Sensor enabled.");
            break;
        case "2":
            _serialPort.WriteLine("ENABLE_O2_SENSOR:false");
            Thread.Sleep(100);
            string response2 = _serialPort.ReadLine().Trim();
            Console.WriteLine("O2 Sensor disabled.");
            break;
        case "3":
            SetO2WarningThreshold();
            break;
        case "4":
            return;
        default:
            Console.WriteLine("Invalid option.");
            break;
    }

    Console.WriteLine("Press any key to continue...");
    Console.ReadKey();
    ConfigureO2Sensor();
}
```

## Creating Custom Patches

Patches are JSON files that can modify the ECU configuration. Here's how to create custom patches:

### Basic Patch Structure

Patches should follow the same structure as the ECU configuration file but only include the parameters you want to change:

```json
{
  "max_rpm": 7000,
  "fan_trigger_temp": 92,
  "new_parameter": "value"
}
```

### Implementing Patch Support for New Parameters

1. Update the `ECUConfig` class with new parameters
2. Modify the `APPLY_PATCH` command handler to handle new parameters:

```csharp
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
        
        // Handle new parameters
        // For boolean properties, we need to check if they exist in the JSON
        if (patchJson.Contains("o2_sensor_enabled"))
            _config.O2SensorEnabled = patchConfig.O2SensorEnabled;
            
        if (patchConfig.O2SensorWarningThreshold > 0)
            _config.O2SensorWarningThreshold = patchConfig.O2SensorWarningThreshold;
        
        SaveConfig();
        _serialPort.WriteLine("PATCH_APPLIED");
    }
    catch (JsonException)
    {
        _serialPort.WriteLine("ERROR:Invalid JSON patch");
    }
}
```

## Implementing Advanced Features

Here are some ideas for advanced features you can implement:

### 1. Trip Computer

Implement a trip computer that calculates:
- Distance traveled based on speed over time
- Average speed
- Fuel economy

### 2. Automated Testing

Create a testing framework that:
- Simulates various driving conditions
- Injects faults to test DTC generation
- Verifies system responses

### 3. Data Visualization

Add a data visualization component that:
- Plots sensor data in real-time
- Shows historical trends
- Exports data for external analysis

### 4. Advanced Diagnostics

Implement more sophisticated diagnostic features:
- Freeze frame data capture when DTCs occur
- Simulated sensor correlation analysis
- Predictive maintenance alerts

## Best Practices

1. **Maintain Backward Compatibility**: Ensure new features don't break existing functionality
2. **Error Handling**: Implement robust error handling for all new features
3. **Documentation**: Document all new features, commands, and configuration options
4. **Testing**: Test new features thoroughly before submitting pull requests
5. **Code Style**: Follow the existing code style and patterns

---

By following these guidelines, you can extend the ECU Simulator and Diagnostic Console with new features while maintaining compatibility with the existing codebase. Happy coding!