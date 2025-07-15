# KEL103Driver

**KEL103Driver** is a C# library designed to control and monitor the **Korad KEL-103 Programmable DC Electronic Load** over a network. It provides high-level functions to discover the KEL-103 on a LAN, send commands via UDP (the device’s network protocol), and retrieve measurements. The repository also includes a basic Windows utility for testing and a TCP bridge server to facilitate integration with standard instrument control tools like NI/VISA. This allows developers and end users to easily integrate the KEL-103 electronic load into automated test setups or custom applications.

<img src="https://media2.giphy.com/media/v1.Y2lkPTc5MGI3NjExOXpkZGF4d3gyNGVpM3JnYzk1enl4cmJhOWFqeHZnZHkyajUzZ3FtaSZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/x11VtP2QJrVPeOxcSx/giphy.gif">
KEL103 with a sine wave input
<br>
<br>
<img src="https://private-user-images.githubusercontent.com/2748872/466365284-caed7a4e-befa-4b36-bddf-0af279fbac09.png?jwt=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJnaXRodWIuY29tIiwiYXVkIjoicmF3LmdpdGh1YnVzZXJjb250ZW50LmNvbSIsImtleSI6ImtleTUiLCJleHAiOjE3NTI1NjU1OTgsIm5iZiI6MTc1MjU2NTI5OCwicGF0aCI6Ii8yNzQ4ODcyLzQ2NjM2NTI4NC1jYWVkN2E0ZS1iZWZhLTRiMzYtYmRkZi0wYWYyNzlmYmFjMDkucG5nP1gtQW16LUFsZ29yaXRobT1BV1M0LUhNQUMtU0hBMjU2JlgtQW16LUNyZWRlbnRpYWw9QUtJQVZDT0RZTFNBNTNQUUs0WkElMkYyMDI1MDcxNSUyRnVzLWVhc3QtMSUyRnMzJTJGYXdzNF9yZXF1ZXN0JlgtQW16LURhdGU9MjAyNTA3MTVUMDc0MTM4WiZYLUFtei1FeHBpcmVzPTMwMCZYLUFtei1TaWduYXR1cmU9ZDhlODhhYTQxM2Y4M2JiNzEwYjA2ZWFlNjAwMzQ2YmFhZjFjMjA3YmUzZWJlYjYyNjU0Njk3MGIyYzkwMDllNiZYLUFtei1TaWduZWRIZWFkZXJzPWhvc3QifQ.kMDR_RG1_1c2DzrzZcEpvIIF0aDR2uOe1AUEWHoCxP4">
Photo of the KEL103DriverUtility example application

## Features and Functionality

- **Network Discovery:** Automatically locate the KEL-103 on the local network by broadcasting a discovery message.
- **UDP Communication:** Communicate with the KEL-103 over its UDP command port.
- **Control of Modes and Setpoints:** Support for all operating modes of the KEL-103:
  - Constant Current (CC)
  - Constant Voltage (CV)
  - Constant Power (CW)
  - Constant Resistance (CR)
- **Load On/Off Control:** Toggle the electronic load input on or off programmatically.
- **Measurement and Monitoring:** Read real-time measurements from the device.
- **Thread-Safe Client Access:** The library manages a single UDP client for efficiency.
- **Test Utility (GUI):** A Windows Forms app demonstrates how to use the library in a GUI.
- **VISA Integration via TCP Server:** Bridges UDP to TCP for integration with PyVISA and NI MAX.
- **Configuration Persistence:** Network settings can be saved in a JSON config file.

## PyVISA Example

The commands to test the TCP bridge server with PyVISA are:

```python
import pyvisa
rm = pyvisa.ResourceManager()
rm.list_resources('?*')
# prints a list of available resources

my_instr = rm.open_resource('TCPIP0::your_ip_address::5025::SOCKET')
my_instr.read_termination = '\n'
my_instr.write_termination = '\n'

# Test commands: it should respond to the IDN query and change to CV mode
print(my_instr.query('*IDN?'))
my_instr.write(':FUNC CV')
```

## Installation

1. Clone or download the repository.
2. Open `KEL103Driver.sln` in Visual Studio 2017 or later (.NET Framework 4.6.1).
3. Restore NuGet packages (e.g., `Newtonsoft.Json`).
4. Build the solution to produce the library, utility, and TCP server.
5. Optionally run the TCP server for VISA integration.

## Basic Usage

```csharp
// Discover the device
IPAddress kel103IP = KEL103Driver.KEL103Tools.FindLoadAddress();

// Set mode to Constant Current
await KEL103Driver.KEL103Command.SetSystemMode(kel103IP, 0);

// Turn the load input ON
await KEL103Driver.KEL103Command.SetLoadInputSwitchState(kel103IP, true);

// Set current to 2.0 A
await KEL103Driver.KEL103Command.SetConstantCurrentTarget(kel103IP, 2.0);

// Read measurements
double voltage = await KEL103Driver.KEL103Command.MeasureVoltage(kel103IP);
```

### Using the State Tracker
```csharp
KEL103Driver.KEL103StateTracker.NewKEL103StateAvailable += state => {
    Console.WriteLine($"[{state.TimeStamp}] V={state.Voltage} V, I={state.Current} A, P={state.Power} W");
};
KEL103Driver.KEL103StateTracker.Start();
// To stop:
// KEL103Driver.KEL103StateTracker.Stop();
```

## Contribution

Contributions are welcome. Please open an issue or pull request if you have suggestions or improvements.

## License

This project is licensed under the GNU GPL v3.0.
