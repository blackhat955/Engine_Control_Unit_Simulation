# ECU Simulator & Diagnostic Toolchain

> A cross-platform toolchain for simulating and diagnosing vehicle Engine Control Units (ECUs) - designed specifically for macOS using .NET 6+ and local serial communication.

##  What Is This?

This project provides a complete simulation environment for automotive ECU diagnostics without requiring physical hardware. It consists of two main components that communicate via virtual serial ports:

1. **Vehicle ECU Simulator** - A console application that mimics real-time vehicle sensor data
2. **Diagnostic Console Application** - A C# tool that connects to the simulator for monitoring and configuration

Perfect for embedded developers, automotive students, firmware testers, or anyone interested in ECU diagnostics without needing actual vehicle hardware.

![ECU Simulator Demo](https://via.placeholder.com/800x400?text=ECU+Simulator+Demo)

## 🔧 System Components

### 1. Vehicle ECU Simulator

This console application streams mock sensor data in real-time:

```
SPEED:72,RPM:2200,TEMP:87,FUEL:65
```

It accepts commands over serial connection:
- `GET_DATA` - Returns current sensor readings
- `SET_MAX_RPM:6000` - Configures maximum RPM threshold
- `GET_DTC` - Returns diagnostic trouble codes (e.g., P0300)
- `CLEAR_DTC` - Clears stored fault codes

Configuration is stored in `ecu_config.json`:

```json
{
  "max_rpm": 6000,
  "fan_trigger_temp": 95,
  "fuel_warning_level": 15
}
```

Diagnostic Trouble Codes are stored in a log file:
```
[P0300] Random Misfire
[P0420] Catalyst System Efficiency Below Threshold
```

### 2. C# Diagnostic Console Application

A menu-driven interface that connects to the simulator via virtual serial ports:

- 📊 Display live sensor data with real-time updates
- ⚙️ Configure ECU parameters (RPM limits, temperature thresholds)
- 🔍 Read and clear diagnostic trouble codes
- 📝 Apply firmware-style JSON patches
- 📈 Log vehicle health data and export as CSV

## Key Features

- **macOS Support** - Uses .NET 6+ and `socat` for virtual COM port creation
- **Pure C# Implementation** - No external dependencies or libraries
- **Extensible Design** - Easily add new sensors, commands, or diagnostic features
- **Local Simulation** - No CAN bus or hardware required
- **Educational Value** - Perfect for learning ECU diagnostics and firmware patching

## Sample "Patch" Ideas

The system is designed to be extended with JSON-based patches. Some ideas:

- Add O2_SENSOR or BATTERY_VOLTAGE support
- Implement auto fan control when TEMP > 95°C
- Create a trip mileage estimator based on speed over time
- Build a high-temperature alert system (>100°C)
- Add CSV export functionality for mechanic analysis

## 🛠️ Technical Implementation

- **Console UI** - Optionally enhanced with ANSI color codes
- **File I/O** - Implemented via `System.IO`
- **JSON Handling** - Using `System.Text.Json`
- **Serial Communication** - Via `System.IO.Ports`
- **Virtual Serial Ports** - Created using `socat` on macOS

## Getting Started

### Prerequisites

- .NET 6 SDK or newer
- macOS (tested on Monterey and newer)
- `socat` utility (install via `brew install socat`)

### Setup Virtual Serial Ports

```bash
# Create a pair of virtual serial ports
socat -d -d pty,raw,echo=0 pty,raw,echo=0
```

This will create two linked ports (e.g., `/dev/ttys001` and `/dev/ttys002`)

### Running the Simulator

```bash
# Start the ECU Simulator (use the first port from socat)
dotnet run --project ECUSimulator -- /dev/ttys001
```

### Running the Diagnostic Tool

```bash
# Start the Diagnostic Console (use the second port from socat)
dotnet run --project DiagnosticConsole -- /dev/ttys002
```

## Why This Project Matters

- **Realistic Simulation** - Experience ECU diagnostics without physical hardware
- **Educational Tool** - Learn about automotive systems, DTCs, and firmware patching
- **Development Sandbox** - Test diagnostic algorithms and firmware updates safely
- **Cross-Platform** - Works on macOS with minimal setup

## 📚 Project Structure

```
├── ECUSimulator/
│   ├── Program.cs              # Main simulator entry point
│   ├── ECUSimulator.cs         # Core simulation logic
│   ├── SensorDataGenerator.cs  # Generates mock sensor data
│   └── ecu_config.json         # Configuration file
│
├── DiagnosticConsole/
│   ├── Program.cs              # Main diagnostic tool entry point
│   ├── DiagnosticManager.cs    # Core diagnostic logic
│   ├── SerialConnector.cs      # Serial port communication
│   └── ConfigPatcher.cs        # JSON patch application logic
│
└── Common/
    ├── Models/                 # Shared data models
    └── Utilities/              # Shared utility functions
```

## Contributing

Contributions are welcome! Some ideas:

- Add new sensor types
- Implement additional diagnostic commands
- Create more sophisticated firmware patches
- Improve the console UI
- Add support for other platforms

## License

This project is licensed under the MIT License - see the LICENSE file for details.

---

##  Ready to Get Started?

Clone this repository, set up your virtual serial ports with `socat`, and start exploring the world of ECU diagnostics without leaving your development environment! Whether you're learning about automotive systems or prototyping diagnostic tools, this simulator provides a realistic environment for experimentation.

Feel free to fork, extend, and build upon this foundation for your own projects or educational purposes. We welcome feedback, feature contributions, and creative uses of this toolchain!