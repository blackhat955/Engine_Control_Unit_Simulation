# Installation Guide: ECU Simulator & Diagnostic Toolchain

This guide will help you set up the ECU Simulator and Diagnostic Console on macOS.

## Prerequisites

Before you begin, ensure you have the following installed on your macOS system:

1. **macOS** - Monterey (12.0) or newer recommended
2. **.NET 6.0 SDK** or newer - Required to build and run the C# applications
3. **socat** - Required to create virtual serial ports

## Step 1: Install .NET SDK

If you don't have .NET SDK installed:

1. Visit [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)
2. Download and install the .NET 6.0 SDK or newer for macOS
3. Verify the installation by opening Terminal and running:

```bash
dotnet --version
```

You should see a version number of 6.0.0 or higher.

## Step 2: Install socat

socat is used to create virtual serial ports for communication between the ECU Simulator and Diagnostic Console.

1. Install Homebrew if you don't have it already:

```bash
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
```

2. Install socat using Homebrew:

```bash
brew install socat
```

3. Verify the installation:

```bash
socat -V
```

You should see the socat version information.

## Step 3: Clone or Download the Repository

1. Clone the repository or download and extract the ZIP file to your preferred location.

```bash
git clone https://github.com/yourusername/ecu-simulator.git
cd ecu-simulator
```

Or if you downloaded a ZIP file, extract it and navigate to the extracted folder in Terminal.

## Step 4: Build the Solution

1. Build the solution using the .NET CLI:

```bash
dotnet build
```

This will restore all required packages and build both the ECU Simulator and Diagnostic Console projects.

## Step 5: Run the Simulation

You can run the simulation in two ways:

### Option 1: Using the Provided Script (Recommended)

1. Make the script executable (if not already):

```bash
chmod +x run_simulation.sh
```

2. Run the script:

```bash
./run_simulation.sh
```

This script will:
- Create virtual serial ports using socat
- Build the solution
- Start the ECU Simulator in one terminal window
- Start the Diagnostic Console in another terminal window

### Option 2: Manual Setup

1. Open a Terminal window and create virtual serial ports:

```bash
socat -d -d pty,raw,echo=0 pty,raw,echo=0
```

This will output something like:
```
2023/01/01 12:00:00 socat[12345] N PTY is /dev/ttys001
2023/01/01 12:00:00 socat[12345] N PTY is /dev/ttys002
```

Note the two port names (e.g., `/dev/ttys001` and `/dev/ttys002`).

2. In a new Terminal window, start the ECU Simulator with the first port:

```bash
cd "path/to/ECU APP"
dotnet run --project ECUSimulator -- /dev/ttys001
```

3. In another Terminal window, start the Diagnostic Console with the second port:

```bash
cd "path/to/ECU APP"
dotnet run --project DiagnosticConsole -- /dev/ttys002
```

## Troubleshooting

### Serial Port Issues

If you encounter issues with the serial ports:

1. Make sure socat is running and has created the virtual ports
2. Check that the port names are correct in your commands
3. Try different port names if the suggested ones don't work

### Build Errors

If you encounter build errors:

1. Make sure you have the correct .NET SDK version installed
2. Try restoring packages manually:

```bash
dotnet restore
```

3. Check for any error messages in the build output

### Runtime Errors

If the applications run but don't communicate:

1. Verify both applications are using the correct paired ports
2. Check that socat is still running
3. Restart both applications

## Next Steps

Once you have the ECU Simulator and Diagnostic Console running:

1. Explore the Diagnostic Console menu options
2. Try applying configuration patches from the sample_patches directory
3. Monitor live ECU data
4. Read and clear diagnostic trouble codes

For information on extending the system with new features, see the [EXTENDING.md](EXTENDING.md) guide.

---

If you encounter any issues not covered in this guide, please open an issue on the GitHub repository or contact the project maintainers.