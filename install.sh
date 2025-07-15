#!/bin/bash
# Enable strict mode: exit on error, exit on undefined variable, fail on any pipe command failure
set -euo pipefail

# Default configuration
DEFAULT_INSTALL_DIR="$HOME/tools"
DEFAULT_NAME="connection-manager"
CLI_PROJECT_PATH="src/Cli"
SYMLINK_DIR="/usr/local/bin"

# Configuration variables
INSTALL_DIR="$DEFAULT_INSTALL_DIR"
NAME="$DEFAULT_NAME"
VERBOSE=false

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Print functions
print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1" >&2
}

print_verbose() {
    if [ "$VERBOSE" = true ]; then
        echo -e "${BLUE}[VERBOSE]${NC} $1"
    fi
}

# Usage function
usage() {
    cat << EOF
Usage: $0 [OPTIONS]

Install or update the Connection Manager CLI tool.

OPTIONS:
    --install-dir DIR    Installation directory (default: $DEFAULT_INSTALL_DIR)
    --name NAME          Executable name (default: $DEFAULT_NAME)
    --verbose           Enable verbose output
    --help              Show this help message

EXAMPLES:
    $0                                          # Install with defaults
    $0 --install-dir ~/apps --name conn-mgr    # Custom installation
    $0 --verbose                               # Install with verbose output

The script will:
1. Build a self-contained release of the application
2. Install to the specified directory
3. Create a wrapper script for easy execution
4. Create a symlink in $SYMLINK_DIR for global access
5. Preserve existing database during updates

EOF
}

# Parse command line arguments
parse_args() {
    while [[ $# -gt 0 ]]; do
        case $1 in
            --install-dir)
                INSTALL_DIR="$2"
                shift 2
                ;;
            --name)
                NAME="$2"
                shift 2
                ;;
            --verbose)
                VERBOSE=true
                shift
                ;;
            --help)
                usage
                exit 0
                ;;
            *)
                print_error "Unknown option: $1"
                usage
                exit 1
                ;;
        esac
    done
}

# Validate environment
validate_environment() {
    print_info "Validating environment..."
    
    # Check if we're in the project root
    if [ ! -f "ConnectionManager.slnx" ] || [ ! -d "$CLI_PROJECT_PATH" ]; then
        print_error "Please run this script from the project root directory"
        exit 1
    fi
    
    # Check if dotnet is available
    if ! command -v dotnet &> /dev/null; then
        print_error "dotnet CLI is not installed or not in PATH"
        exit 1
    fi
    
    # Check dotnet version
    local dotnet_version=$(dotnet --version)
    print_verbose "Found dotnet version: $dotnet_version"
    
    # Expand tilde in paths
    INSTALL_DIR=$(eval echo "$INSTALL_DIR")
    local target_dir="$INSTALL_DIR/$NAME"
    
    print_verbose "Install directory: $target_dir"
    print_verbose "Executable name: $NAME"
    print_verbose "Symlink location: $SYMLINK_DIR/$NAME"
}

# Detect runtime identifier
detect_runtime() {
    local os=$(uname -s)
    local arch=$(uname -m)
    
    case "$os" in
        Darwin)
            case "$arch" in
                x86_64) echo "osx-x64" ;;
                arm64) echo "osx-arm64" ;;
                *) print_error "Unsupported macOS architecture: $arch"; exit 1 ;;
            esac
            ;;
        Linux)
            case "$arch" in
                x86_64) echo "linux-x64" ;;
                aarch64) echo "linux-arm64" ;;
                *) print_error "Unsupported Linux architecture: $arch"; exit 1 ;;
            esac
            ;;
        *)
            print_error "Unsupported operating system: $os"
            exit 1
            ;;
    esac
}

# Build the application
build_application() {
    local runtime="$1"
    print_info "Building self-contained application for $runtime..."
    
    # Clean previous builds
    print_verbose "Cleaning previous builds..."
    dotnet clean --configuration Release --verbosity quiet
    
    # Restore packages
    print_verbose "Restoring packages..."
    dotnet restore --verbosity quiet
    
    # Publish self-contained
    print_info "Publishing self-contained application..."
    dotnet publish "$CLI_PROJECT_PATH" \
        --configuration Release \
        --runtime "$runtime" \
        --self-contained \
        --output "$CLI_PROJECT_PATH/bin/Release/net9.0/$runtime/publish" \
        --verbosity quiet
    
    if [ $? -eq 0 ]; then
        print_success "Build completed successfully"
    else
        print_error "Build failed"
        exit 1
    fi
}

# Preserve existing database
preserve_database() {
    local target_dir="$1"
    local db_file="$target_dir/app.db"
    
    if [ -f "$db_file" ]; then
        print_info "Preserving existing database..."
        cp "$db_file" "$db_file.backup"
        print_verbose "Database backed up to: $db_file.backup"
    else
        print_verbose "No existing database found to preserve"
    fi
}

# Install the application
install_application() {
    local publish_dir="$1"
    local target_dir="$2"
    
    print_info "Installing application to: $target_dir"
    
    # Create target directory
    mkdir -p "$target_dir"
    
    # Copy all files from publish directory
    print_verbose "Copying files from: $publish_dir"
    cp -r "$publish_dir"/* "$target_dir/"
    
    # Make the main executable file executable
    chmod +x "$target_dir/ConnectionManager.Cli"
    
    print_success "Application files installed"
}

# Restore preserved database
restore_database() {
    local target_dir="$1"
    local db_file="$target_dir/app.db"
    local backup_file="$db_file.backup"
    
    if [ -f "$backup_file" ]; then
        print_info "Restoring preserved database..."
        mv "$backup_file" "$db_file"
        print_verbose "Database restored from backup"
    else
        print_verbose "No database backup to restore"
    fi
}

# Generate wrapper script
generate_wrapper() {
    local target_dir="$1"
    local wrapper_file="$target_dir/$NAME"
    
    print_info "Generating wrapper script..."
    
    cat > "$wrapper_file" << 'EOF'
#!/bin/bash
# Auto-generated wrapper script for Connection Manager CLI
# This script ensures the application runs from its install directory
# so that relative paths in configuration work correctly.

# Get the directory where this script lives, resolving symlinks
SCRIPT_SOURCE="${BASH_SOURCE[0]}"
while [ -h "$SCRIPT_SOURCE" ]; do
    SCRIPT_DIR="$(cd "$(dirname "$SCRIPT_SOURCE")" && pwd)"
    SCRIPT_SOURCE="$(readlink "$SCRIPT_SOURCE")"
    [[ $SCRIPT_SOURCE != /* ]] && SCRIPT_SOURCE="$SCRIPT_DIR/$SCRIPT_SOURCE"
done
SCRIPT_DIR="$(cd "$(dirname "$SCRIPT_SOURCE")" && pwd)"

# Change to the script directory and run the application
cd "$SCRIPT_DIR" && exec ./ConnectionManager.Cli "$@"
EOF
    
    # Make wrapper executable
    chmod +x "$wrapper_file"
    
    print_success "Wrapper script created: $wrapper_file"
}

# Create symlink
create_symlink() {
    local target_dir="$1"
    local name="$2"
    local wrapper_file="$target_dir/$name"
    local symlink_path="$SYMLINK_DIR/$name"
    
    print_info "Creating symlink: $symlink_path"
    
    # Check if symlink directory exists and is writable
    if [ ! -d "$SYMLINK_DIR" ]; then
        print_error "Symlink directory $SYMLINK_DIR does not exist"
        print_info "You can manually create a symlink with:"
        print_info "  sudo ln -sf '$wrapper_file' '$symlink_path'"
        return 1
    fi
    
    if [ ! -w "$SYMLINK_DIR" ]; then
        print_warning "No write permission to $SYMLINK_DIR, trying with sudo..."
        if sudo ln -sf "$wrapper_file" "$symlink_path"; then
            print_success "Symlink created with sudo"
        else
            print_error "Failed to create symlink with sudo"
            print_info "You can manually create the symlink with:"
            print_info "  sudo ln -sf '$wrapper_file' '$symlink_path'"
            return 1
        fi
    else
        # Remove existing symlink if it exists
        if [ -L "$symlink_path" ]; then
            print_verbose "Removing existing symlink: $symlink_path"
            rm "$symlink_path"
        fi
        
        # Create new symlink
        if ln -sf "$wrapper_file" "$symlink_path"; then
            print_success "Symlink created successfully"
        else
            print_error "Failed to create symlink"
            return 1
        fi
    fi
}

# Main function
main() {
    print_info "Connection Manager CLI Installation Script"
    print_info "=========================================="
    
    # Parse arguments
    parse_args "$@"
    
    # Validate environment
    validate_environment
    
    # Detect runtime
    local runtime=$(detect_runtime)
    print_info "Detected runtime: $runtime"
    
    # Set up paths
    local target_dir="$INSTALL_DIR/$NAME"
    local publish_dir="$CLI_PROJECT_PATH/bin/Release/net9.0/$runtime/publish"
    
    print_info "Starting installation to: $target_dir"
    
    # Build the application
    build_application "$runtime"
    
    # Preserve existing database if it exists
    preserve_database "$target_dir"
    
    # Install the application
    install_application "$publish_dir" "$target_dir"
    
    # Restore preserved database
    restore_database "$target_dir"
    
    # Generate wrapper script
    generate_wrapper "$target_dir"
    
    # Create symlink
    create_symlink "$target_dir" "$NAME"
    
    print_success "Installation completed successfully!"
    print_info "You can now run: $NAME"
}

# Run main function with all arguments
main "$@"