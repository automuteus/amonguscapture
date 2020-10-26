[![Build](https://img.shields.io/github/workflow/status/litetex/amonguscapture/Master%20CI)](https://github.com/litetex/amonguscapture/actions?query=workflow%3A%22Master+CI%22)
[![Latest Version](https://img.shields.io/github/v/release/litetex/amonguscapture)](https://github.com/litetex/amonguscapture/releases)
[![Build](https://img.shields.io/github/workflow/status/litetex/amonguscapture/Develop%20CI/develop?label=build%20develop)](https://github.com/litetex/amonguscapture/actions?query=workflow%3A%22Develop+CI%22+branch%3Adevelop)
![Supported: Windows](https://img.shields.io/badge/supported--os-windows-0078d6)

# AmongUsCapture <img src="AmongUsCapture/Icon.ico" width="48">

### ❌**Capture only works with the Official Steam Non-Beta Version of Among Us**❌

Capture of the local Among Us executable state.

## Installation
Only Windows is supported!

Download the [lastest release](https://github.com/litetex/amonguscapture/releases/latest) and execute it.

## Important notes
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

## Releasing
* Create a new pullrequest from ``develop`` to ``master``
  * To control the version: Label that PR with either ``major``, ``minor`` or ``patch``
  * Make sure no Release-Draft with the next version exists
* Merge the PR
* Wait a bit until the Release-Draft is created
  * Important: The version/tag should be identially to the assembly versions of the created assets. If not recreate the assets.
* Release the draft
* Merge back the automatically created PR into ``develop`` so that all changes on the master come back (e.g. if you have a hotfix)
