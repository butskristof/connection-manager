# Connection Manager

Command line tool to make working with remote connections such as SSH easier. Manage a collection of connection profiles and use them to quickly connect to servers.

## Installation

By default, the install script puts the tool in `~/tools/connection-manager` and generates a `connection-manager` executable. Both the location and name can be customized with command line options. 
The install script will:
- do a self-contained release build
- move the app files into the specified directory
- create a symlink to the executable in `~/usr/local/bin`

On first start, an `app.db` file will be created in the installation directory to store your connection profiles.  
To update, run the install script again. This will preserve the application database so no data is lost.

```bash
# Install to ~/tools/connection-manager
./install.sh

# Custom installation directory and executable name
./install.sh --install-dir ~/apps --name conn-mgr

# With verbose output
./install.sh --verbose
```

## Usage

After installation, you can run the tool from your shell by using the name you specified during installation (default is `connection-manager`).
A rich terminal UI will guide you through managing your connections and connecting to servers.

## Development

### Technology stack
- .NET 9
- Spectre.Console for terminal UI
- EF Core with SQLite

See [CLAUDE.md](CLAUDE.md) for detailed architectural guidance and development patterns.

