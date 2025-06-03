# Serial Communication Firmware Update System

This project consists of two C# applications for simulating firmware updates over serial connections:

1. **PatchTool**: A utility for sending firmware updates to devices
2. **MockDevice**: A simulated device that receives firmware updates

## Prerequisites

### .NET SDK

This project requires the .NET SDK. If you don't have it installed, you can download it from:

- [Download .NET for macOS](https://dotnet.microsoft.com/download/dotnet)

Verify your installation by running:

```bash
dotnet --version
```

### Virtual Serial Ports

To test the applications without physical hardware, you'll need to create virtual serial ports. On macOS, this can be done using `socat`.

1. Install socat (if not already installed):

```bash
brew install socat
```

2. Create a pair of virtual serial ports:

```bash
socat -d -d pty,raw,echo=0 pty,raw,echo=0
```

This command will output something like:

```
2023/05/01 12:34:56 socat[12345] N PTY is /dev/ttys003
2023/05/01 12:34:56 socat[12345] N PTY is /dev/ttys004
```

Note these port names as you'll need them when running the applications.

## Building the Applications

1. Clone or download this repository
2. Navigate to the project directory
3. Build the solution:

```bash
dotnet build
```

## Running the Applications

### Step 1: Create Virtual Serial Ports

Open a terminal and run:

```bash
socat -d -d pty,raw,echo=0 pty,raw,echo=0
```

Note the two port names that are displayed (e.g., `/dev/ttys003` and `/dev/ttys004`).

### Step 2: Start the MockDevice

Open a new terminal window and run:

```bash
cd /path/to/EmbeddedSys
dotnet run --project MockDevice
```

When prompted, enter one of the port names from Step 1 (e.g., `/dev/ttys004`).

### Step 3: Run the PatchTool

Open another terminal window and run:

```bash
cd /path/to/EmbeddedSys
dotnet run --project PatchTool
```

When prompted, enter the other port name from Step 1 (e.g., `/dev/ttys003`).

## Sample Workflow

1. The PatchTool will scan for available serial ports
2. Connect to your chosen port
3. Send a version check command (`GET_VER`)
4. The MockDevice will respond with its version (`FW_1.0.3`)
5. PatchTool will send the firmware update from `firmware.hex`
6. MockDevice will receive the firmware and save it to `received_firmware.hex`
7. Communication logs will be saved to `patch_log.txt`

## Project Structure

- `PatchTool/`: The firmware update tool
  - `Program.cs`: Main application code
  - `PatchTool.csproj`: Project file

- `MockDevice/`: The simulated device
  - `Program.cs`: Main application code
  - `MockDevice.csproj`: Project file

- `firmware.hex`: Sample firmware file

## Features

### PatchTool

- Scans for available serial ports
- Connects to a chosen port
- Sends version check command
- Loads and sends firmware file line-by-line
- Logs all communication to a file
- Handles basic error responses
- Implements retry logic for failed transmissions

### MockDevice

- Listens on a specified serial port
- Responds to version check commands
- Receives and saves firmware data
- Simulates occasional errors (10% chance)
- Provides feedback on command processing

## Extending the Project

Possible enhancements:

- Add checksum validation for firmware lines
- Implement more sophisticated error handling
- Add support for different firmware formats
- Create a GUI interface for the tools
- Add encryption for secure firmware updates