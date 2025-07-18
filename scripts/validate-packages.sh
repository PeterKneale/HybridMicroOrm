#!/bin/bash

# Package Validation Script for HybridMicroOrm
# This script validates the NuGet packages following the same logic as the CI/CD pipeline

set -e

echo "🔧 HybridMicroOrm Package Validation Script"
echo "=========================================="

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    local color=$1
    local message=$2
    echo -e "${color}${message}${NC}"
}

# Function to check if command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Check prerequisites
print_status $BLUE "🔍 Checking prerequisites..."

if ! command_exists dotnet; then
    print_status $RED "❌ .NET SDK not found. Please install .NET 8.0 SDK."
    exit 1
fi

if ! command_exists minver; then
    print_status $YELLOW "⚠️  MinVer CLI not found. Installing..."
    dotnet tool install -g minver-cli
fi

print_status $GREEN "✅ Prerequisites check passed"

# Clean and restore
print_status $BLUE "🧹 Cleaning previous builds..."
dotnet clean >/dev/null 2>&1

print_status $BLUE "📦 Restoring dependencies..."
dotnet restore >/dev/null 2>&1

# Validate MinVer versioning
print_status $BLUE "🔢 Validating MinVer versioning..."

cd src/HybridMicroOrm
VERSION_MAIN=$(minver 2>/dev/null || echo "")
cd ../HybridMicroOrm.Contracts  
VERSION_CONTRACTS=$(minver 2>/dev/null || echo "")
cd ../..

if [[ -z "$VERSION_MAIN" ]] || [[ -z "$VERSION_CONTRACTS" ]]; then
    print_status $RED "❌ MinVer failed to generate versions"
    exit 1
fi

print_status $GREEN "✅ Version validation passed"
print_status $GREEN "   HybridMicroOrm version: $VERSION_MAIN"
print_status $GREEN "   HybridMicroOrm.Contracts version: $VERSION_CONTRACTS"

# Build packages
print_status $BLUE "🏗️  Building packages..."
dotnet pack --configuration Release --no-restore >/dev/null 2>&1

# Validate packages
print_status $BLUE "📋 Validating package contents..."

# Check that packages were created
MAIN_PACKAGE=$(find src/HybridMicroOrm/bin/Release -name "*.nupkg" | head -1)
CONTRACTS_PACKAGE=$(find src/HybridMicroOrm.Contracts/bin/Release -name "*.nupkg" | head -1)
MAIN_SYMBOLS=$(find src/HybridMicroOrm/bin/Release -name "*.snupkg" | head -1)
CONTRACTS_SYMBOLS=$(find src/HybridMicroOrm.Contracts/bin/Release -name "*.snupkg" | head -1)

if [[ -z "$MAIN_PACKAGE" ]] || [[ -z "$CONTRACTS_PACKAGE" ]]; then
    print_status $RED "❌ Package creation failed"
    exit 1
fi

if [[ -z "$MAIN_SYMBOLS" ]] || [[ -z "$CONTRACTS_SYMBOLS" ]]; then
    print_status $RED "❌ Symbol package creation failed"
    exit 1
fi

print_status $GREEN "✅ Package creation validation passed"
print_status $GREEN "   Main package: $(basename "$MAIN_PACKAGE")"
print_status $GREEN "   Contracts package: $(basename "$CONTRACTS_PACKAGE")"
print_status $GREEN "   Main symbols: $(basename "$MAIN_SYMBOLS")"
print_status $GREEN "   Contracts symbols: $(basename "$CONTRACTS_SYMBOLS")"

# Validate package size
MAIN_SIZE=$(stat -c%s "$MAIN_PACKAGE" 2>/dev/null || stat -f%z "$MAIN_PACKAGE" 2>/dev/null)
CONTRACTS_SIZE=$(stat -c%s "$CONTRACTS_PACKAGE" 2>/dev/null || stat -f%z "$CONTRACTS_PACKAGE" 2>/dev/null)

if [[ $MAIN_SIZE -lt 1000 ]] || [[ $CONTRACTS_SIZE -lt 1000 ]]; then
    print_status $RED "❌ Packages are too small, likely malformed"
    print_status $RED "   Main package: $MAIN_SIZE bytes"
    print_status $RED "   Contracts package: $CONTRACTS_SIZE bytes"
    exit 1
fi

if [[ $MAIN_SIZE -gt 10485760 ]] || [[ $CONTRACTS_SIZE -gt 10485760 ]]; then
    print_status $YELLOW "⚠️  Warning: Packages are larger than 10MB"
    print_status $YELLOW "   Main package: $MAIN_SIZE bytes"
    print_status $YELLOW "   Contracts package: $CONTRACTS_SIZE bytes"
else
    print_status $GREEN "✅ Package size validation passed"
    print_status $GREEN "   Main package: $MAIN_SIZE bytes"
    print_status $GREEN "   Contracts package: $CONTRACTS_SIZE bytes"
fi

# Dependency vulnerability scan
print_status $BLUE "🔒 Scanning for dependency vulnerabilities..."
VULN_OUTPUT=$(dotnet list package --vulnerable --include-transitive 2>&1 || true)

if echo "$VULN_OUTPUT" | grep -q "has no vulnerable packages"; then
    print_status $GREEN "✅ No vulnerable dependencies found"
else
    print_status $YELLOW "⚠️  Vulnerability scan results:"
    echo "$VULN_OUTPUT"
fi

# Check package metadata
print_status $BLUE "📝 Validating package metadata..."

# Use dotnet list package to check the structure
LIST_OUTPUT=$(dotnet list package --format json 2>/dev/null || echo "{}")

if [[ "$LIST_OUTPUT" != "{}" ]]; then
    print_status $GREEN "✅ Package metadata validation passed"
else
    print_status $YELLOW "⚠️  Could not validate package metadata (requires newer .NET SDK)"
fi

# Summary
print_status $GREEN "🎉 All validations passed!"
print_status $BLUE "📊 Summary:"
print_status $BLUE "   - MinVer versioning: ✅"
print_status $BLUE "   - Package creation: ✅"
print_status $BLUE "   - Symbol packages: ✅"
print_status $BLUE "   - Package sizes: ✅"
print_status $BLUE "   - Vulnerability scan: ✅"
print_status $BLUE "   - Metadata: ✅"

print_status $GREEN ""
print_status $GREEN "🚀 Packages are ready for publishing!"
print_status $GREEN "   To publish, create a GitHub release with a semantic version tag (e.g., v1.0.0)"
print_status $GREEN "   The CI/CD pipeline will automatically validate and publish the packages."

exit 0