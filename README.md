# LPJ-Guess Runner

Run many LPJ-Guess simulations with modified parameter values.

## Archived

This repository is now read-only. The runner tool has now been migrated to the
[GUI](https://github.com/hie-dave/lpjg-gui) repository. Future bugfixes are
unlikely to be backported to this repository.

## Building

To build in debug mode:

```bash
cd src
dotnet build # debug build
dotnet publish -c Release -r ${runtime_id} # release build
```

Where runtime_id is one of: linux-x64, osx-x64, osx-arm64, win-x64,
etc. This will produce output in the `bin` directory; the exact path will depend
on how the executable was built (ie build configuration, runtime ID, etc).

## Running

The runner requires a single argument: the path to a .toml configuration
file, which contains instructions on how to run the model. An example .toml file
is included in this repository (`example.toml`). This file explains the required
parameters and their effects.
