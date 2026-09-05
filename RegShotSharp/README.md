# RegShotSharp
## Introduction
Based off the idea of the original RegShot (though more limited). Back around 2015, RegShot stopped working on Windows 10 machines, so I started work on a C# version of RegShot.
I think someone has come out with a working version that works on Windows 10 (maybe 11?) but I wanted to finish this project. 
Is this a finished project? Not at all! Between starting this project almost a decade ago and having stepped away from .NET development for a few years, I'm still relearning how to work .NET projects and learning all the new features that have been added in since than.

## Installation
Requires .NET Framework to be installed on your machine
- Open Command Prompt and navigate to the RegShotSharp directory (where RegShotSharp.csproj is located)
- Run `dotnet restore` to install the required NuGet packages

## Compile and Run
- In same directory as before, run `dotnet build -c Release` to build
- `bin\Release\net8.0-windows\RegShotSharp.exe` to run it (requires adminstrator privledges to run)

## Instructions
Select the Registry Hives to capture and/or compare

### Capture Snapshot
The 'Capture' button will not enable until a 'Capture Directory' has been selected. 
Once a directory has been selected, this will iterate through all the entries in the selected hives.
This process can take a minute or two (or longer on older machines)

### Comparing
The 'Compare' button will not enable until 'Compare To...' has been used to find a previous SQLite3 file to compare against.
Once a valid previous run has been selected, this will run the Capture Snapshot functions and compare those results to the file that was selected (i.e. all the changes since the selected file).
A new SQLite3 DB (with the current snapshot data) and up to two CSV files will be created in the same directory:
	- alteredRegistries.csv: all the registry paths that were added or deleted
	- editedRegistries.csv: all the paths that have changed values

## Known Issues or Things That Should Be Changed
- Error/Exception handling needs rework (yes, I'm aware)
- There are not enough user action checks
- There is no reason for three seperate classes at this point (made since in earlier iterations of this)
- There is logic for storing un-successful registry querying but it is never used....working on it
- The UI looks...basic. I'm more of a backend person, not front end, so working on it but not at the top of my list
- There is only a One-Size-Fits-All for building...working on that too
- UI doesn't resize gracefully.......once again, not a UI person
- CSV output isn't pretty for some of the values (might explore an Excel write to avoid CSV formating issues)