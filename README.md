# ExternalMeleeTool

ExternalMeleeTool is a C# library that connects to Super Smash Bros. Melee 
while the game is running in the Dolphin emulator, exposes useful data and control APIs, and enables 
building external tools, overlays, or mods that interact with SSBM in real time. 
It provides access to game state, player data, camera control, and utility functions for 
reading or writing memory exposed by the emulator.

This project is intended for developers building tools, bots, or extensions that need live information from a running Melee process via Dolphin.

## Features

- Connects to a running instance of Super Smash Bros. Melee through Dolphin.

- Reads game memory and game state (player positions, camera, inputs, etc.).

- Marshaling utilities for safely interpreting game data structures.

- Tools and helpers built for external overlays, tools, or automation projects.

- Easy-to-use C# API for interacting with the game.

## Getting Started

### Requirements

- .NET 8.0 SDK or later
- Slippi Dolphin
- Legally obtained Super Smash Bros. Melee ISO

### Installation

```bash
git clone https://github.com/RighteousRyan1/ExternalMeleeTool.git
cd ExternalMeleeTool
```

Open the solution (ExternalMeleeTool.sln) in Visual Studio or your preferred .NET IDE and build the library. 
Then reference the resulting DLL from your own tool or application.

### Connecting to the API

The library exposes a connection API that attaches to Dolphin’s 
process memory and allows reading/writing of SSBM state. 

Below is an example showing how to connect and read the game state:

```cs
using ExternalMeleeTool;

static void Main() {
	// The parameter can take any number of strings
	// GALE01 represents the Vanilla Melee Game ID
	// If a verion or mod of Melee with a different Game ID is used, you can add it to the paraemter list
	// e.g: "GALE01", "GTME01" to support both Vanilla Melee and UnclePunch's Training Mode
	// However, some mods do mess with memory addresses- so some info is likely to be inaccurate if using a modded version
	while (!Dolphinterop.Connect("GALE01")) {
		// This loop is to retry a search every second until Dolphin with Melee is found.
		Console.WriteLine("Waiting for Melee...");
		Thread.Sleep(1000);
	}

	// Now, we can loop and read game state.
	while (Dolphinterop.IsConnected) {
		var matchData = Dolphinterop.GetMatchData();
		var globalData = Dolphinterop.GetGlobalData();

		// We pass in globalData in for both of these so garbage data isn't read
		// when the scene is not correct (e.g: not online, not in a match, etc))
		var onlineData = Dolphinterop.GetOnlineData(globalData);
		var stageData = Dolphinterop.GetStageData(globalData);

		// Now that we have a plentitude of data, we can do whatever we want with it.

		// If you're familiar with reading decompiles, you can get specific data like this:
		// This API does not expose *everything*, so some data you may need to read manually via memory addresses.
		// Example: Get port 1 fighter's current animation frame
		// Note: This in the future most likely will be front-facing
		var p1AnimFrame = Dolphinterop.ReadF32(matchData.Fighters[0].Fighter + 0x894);

		// If there's also a struct not exposed by the API, you can read it manually too.
		// You just need to copy the struct from C and format it for C#.
		// If the size does not match the original struct, pieces of the data may be garbage
		var anonymousStructRead = Dolphinterop.Read<Fighter_x8B0_t>(matchData.Fighters[0].Fighter + 0x8B0);

		// If you want to edit things, you can write to an address as well.
		anonymousStructRead.x0 = 100;
		Dolphinterop.Write<Fighter_x8B0_t>(matchData.Fighters[0].Fighter + 0x8B0, anonymousStructRead);
	}
}
```