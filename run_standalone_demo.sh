#!/bin/bash

# Standalone demo script to run ECU Simulator in demo mode

echo "🚗 ECU Simulator Standalone Demo"
echo "==============================="

# Check if .NET SDK is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ Error: .NET SDK is not installed."
    echo "Please install .NET 6.0 SDK or newer from https://dotnet.microsoft.com/download"
    exit 1
fi

echo "✅ Dependencies verified"

# Build the project
echo "🔨 Building project..."
dotnet build "ECUSimulator/ECUSimulator.csproj"

if [ $? -ne 0 ]; then
    echo "❌ Error: Build failed."
    exit 1
fi

echo "✅ Build successful"

# Run the ECU Simulator in demo mode
echo "🚀 Starting ECU Simulator in demo mode..."
echo "Note: In a real setup, you would connect to a physical or virtual serial port."
echo "For this demo, we're running in a special demo mode that simulates sensor data."
echo ""

# Create demo config files if they don't exist
if [ ! -f "ecu_config.json" ]; then
    echo '{"max_rpm": 6500, "fan_trigger_temp": 90, "fuel_warning_level": 15}' > ecu_config.json
    echo "Created demo ecu_config.json"
fi

if [ ! -f "dtc_codes.json" ]; then
    echo '[{"code": "P0100", "description": "Mass Air Flow Circuit", "is_active": false}, {"code": "P0102", "description": "Mass Air Flow Circuit Low Input", "is_active": true}]' > dtc_codes.json
    echo "Created demo dtc_codes.json"
fi

# Run the simulator in demo mode
echo "=== ECU SIMULATOR DEMO MODE ==="
echo "This demo will show simulated sensor values that would normally be sent over serial."
echo "Press Ctrl+C to exit the demo."
echo ""

# Simulate ECU output
while true; do
    # Generate random values
    RPM=$((RANDOM % 6500 + 500))
    TEMP=$((RANDOM % 40 + 70))
    FUEL=$((RANDOM % 100))
    SPEED=$((RANDOM % 120))
    
    # Clear screen and show dashboard
    clear
    echo "======== ECU SIMULATOR DEMO ========"
    echo "Time: $(date +"%T")"
    echo "==================================="
    echo "RPM:      $RPM"
    echo "Temp:     $TEMP°C"
    echo "Fuel:     $FUEL%"
    echo "Speed:    $SPEED km/h"
    echo "==================================="
    
    # Show warnings
    if [ $TEMP -gt 90 ]; then
        echo "⚠️ WARNING: Engine temperature high!"
        echo "   Fan activated"
    fi
    
    if [ $FUEL -lt 15 ]; then
        echo "⚠️ WARNING: Fuel level low!"
    fi
    
    if [ $RPM -gt 6000 ]; then
        echo "⚠️ WARNING: RPM near redline!"
    fi
    
    # Show active DTCs
    echo "==================================="
    echo "Active DTCs:"
    echo "P0102 - Mass Air Flow Circuit Low Input"
    echo "==================================="
    echo "Press Ctrl+C to exit demo"
    
    sleep 1
done