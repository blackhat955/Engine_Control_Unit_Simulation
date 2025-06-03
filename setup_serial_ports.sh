#!/bin/bash

# Check if socat is installed
if ! command -v socat &> /dev/null; then
    echo "Error: socat is not installed."
    echo "Please install it using: brew install socat"
    exit 1
fi

echo "Creating virtual serial port pair..."
echo "Press Ctrl+C to terminate when done testing."
echo ""

# Run socat to create virtual serial port pair
socat -d -d pty,raw,echo=0 pty,raw,echo=0