# connection-manager

This is a simple CLI tool to launch remote (SSH) connections from the terminal without having to provide credentials every time. 

## Features
* Pick from a list of preconfigured connections and start a session
* Manage the saved connections (host, port, username, password or SSH key, ...)
* Start an ad-hoc connection using e.g. the default SSH key or a given set of credentials

## Installation

### Quick Install (Recommended)
```bash
# Install to ~/tools/connection-manager with default settings
./install.sh

# Custom installation directory and executable name
./install.sh --install-dir ~/apps --name conn-mgr

# Install with verbose output
./install.sh --verbose
```

### What the Install Script Does
The `install.sh` script creates a complete, self-contained installation:

1. **Builds a self-contained executable** - No .NET runtime required on target system
2. **Creates installation directory** - Default: `~/tools/connection-manager`
3. **Preserves your data** - Existing database is kept during updates
4. **Generates wrapper script** - Ensures configuration files are found correctly
5. **Creates global symlink** - Adds executable to `/usr/local/bin` for system-wide access

### Installation Structure
```
~/tools/connection-manager/
├── ConnectionManager.Cli          # Self-contained executable
├── connection-manager             # Wrapper script
├── appsettings.json              # Configuration
├── app.db                        # Your connection profiles (preserved during updates)
└── *.dll                         # Runtime dependencies
```

### Usage After Installation
```bash
# Run from anywhere after installation
connection-manager

# Or use custom name if specified during install
conn-mgr
```

### Updating
Simply run the install script again - your connection profiles and settings will be preserved:
```bash
./install.sh  # Updates installation, keeps your data
```

## Technology 
* .NET 9 / C# 13 
* EF Core with SQLite storage 
* Spectre.Console 
