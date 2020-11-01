[![Build](https://img.shields.io/github/workflow/status/denverquane/amonguscapture/Check%20Build)](https://github.com/denverquane/amonguscapture/actions?query=workflow%3A%22Check+Build%22+branch%3Amaster)
# AmongUsCapture <img src="assets/icon.ico" width="40">

Capture of the local Among Us executable state.

## Installation
Download the [lastest release](https://github.com/litetex/amonguscapture/releases/latest) and execute it.

## Important notes
### ❌**Capture only works with the Official Steam Non-Beta Version of Among Us**❌
### Antivirus detection
Windows Defender may flag this executable as Trojan:Win32/Emali.A!cl, you can make an exception for this file depending on your version of Windows

##### Windows 10:
Windows Security → Virus & threat protection → Virus & threat protection settings → Exclusions → Add exclusion

Then browse to the AmongUsCapture.exe file and confirm that you want to create the exception.

*** 

## Developing
### Tools for developing
* [Visual Studio](https://visualstudio.microsoft.com/vs/)
* [.NET Core 3.1 SDK](https://dotnet.microsoft.com/download/dotnet-core/3.1) (should be installed automatically by Visual Studio)
* [SonarLint](https://www.sonarlint.org/visualstudio/)
* CheatEngine to get the memory addresses 

### How it works
* The state of the game is recognized from the game's memory
* The infos are sent via SocketIO to [automuteus](https://github.com/denverquane/automuteus)
