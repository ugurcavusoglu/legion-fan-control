# Legion Fan Control

Open-source fan control application for Lenovo Legion laptops.

## Features

- Real-time CPU & GPU temperature monitoring
- Fan speed monitoring (RPM & %)
- Thermal mode switching (Quiet / Balanced / Performance)
- System tray support
- Auto-updates every 2 seconds

## Requirements

- Windows 10/11
- Lenovo Legion laptop (tested on Legion 5 Gen 6)
- .NET 8 Desktop Runtime
- **Must be run as Administrator** (required for WMI fan access)

## Supported Models

Any Lenovo Legion laptop with `LENOVO_GAMEZONE_DATA` and `LENOVO_FAN_METHOD` WMI classes:
- Legion 5 / 5 Pro (Gen 5, 6, 7, 8)
- Legion 7 / 7i
- Legion Slim 5 / 7
- IdeaPad Gaming 3 (partial support)

## Build

```bash
git clone https://github.com/ugurcavusoglu/legion-fan-control
cd legion-fan-control/LegionFanControl
dotnet build
dotnet run
```

## How it works

Uses Lenovo's official WMI interface (`root\WMI`) — the same API that Lenovo Vantage uses internally. No kernel drivers, no unsafe code.

## License

MIT
