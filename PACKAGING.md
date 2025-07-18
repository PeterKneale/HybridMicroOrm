# Package Publishing Guide

This document describes the improved NuGet package publishing workflow for HybridMicroOrm.

## Overview

The HybridMicroOrm project now follows NuGet packaging best practices with:

- **Automatic Semantic Versioning**: Using MinVer for Git-based versioning
- **Conditional Publishing**: Packages are only published on releases, not every push
- **Enhanced Package Metadata**: Complete package information and symbol packages
- **Package Validation**: Comprehensive validation before publishing

## Versioning Strategy

### Automatic Versioning with MinVer

The project uses [MinVer](https://github.com/adamralph/minver) for automatic semantic versioning based on Git tags and commits.

**Configuration:**
- `MinVerDefaultPreReleaseIdentifiers`: `alpha` (for pre-release versions)
- `MinVerTagPrefix`: `v` (expects tags like `v1.0.0`)

**Version Examples:**
- No tags: `0.0.0-alpha.1`, `0.0.0-alpha.2`, etc.
- With tag `v1.0.0`: `1.0.0`, `1.0.1-alpha.1`, etc.
- With tag `v1.1.0-beta.1`: `1.1.0-beta.1`, `1.1.0-beta.2`, etc.

### Creating Releases

To publish a new version:

1. **Create and push a tag:**
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

2. **Create a GitHub Release:**
   - Go to GitHub > Releases > "Create a new release"
   - Choose the tag you created
   - Add release notes
   - Publish the release

3. **Automatic Publishing:**
   - The workflow will automatically trigger on release creation
   - Packages will be validated and published to NuGet.org

## Package Metadata

Both packages include comprehensive metadata:

### HybridMicroOrm Package
- **Description**: A lightweight, JSON-based micro ORM for PostgreSQL with built-in multi-tenant support
- **Repository**: https://github.com/PeterKneale/HybridMicroOrm
- **License**: MIT
- **Tags**: postgres, sql, json, multi-tenant, saas, dapper, table, storage
- **Symbol Package**: Yes (`.snupkg`)

### HybridMicroOrm.Contracts Package
- **Description**: Contracts and interfaces for HybridMicroOrm
- **Repository**: https://github.com/PeterKneale/HybridMicroOrm
- **License**: MIT
- **Tags**: postgres, sql, json, multi-tenant, saas, dapper, table, storage
- **Symbol Package**: Yes (`.snupkg`)

## Publishing Workflow

### When Packages are Published

Packages are **only** published when:
1. A GitHub release is created/published
2. Manual workflow dispatch is triggered on the `main` branch

Packages are **not** published on:
- Regular pushes to main
- Pull requests
- Pushes to feature branches

### Validation Steps

Before publishing, packages undergo comprehensive validation:

1. **MinVer Version Validation**
   - Ensures MinVer can generate valid semantic versions
   - Verifies versions are not empty

2. **Package Creation Validation**
   - Confirms both `.nupkg` and `.snupkg` files are created
   - Validates package files for both projects

3. **Package Size Validation**
   - Ensures packages are not empty (> 1KB)
   - Warns if packages are unusually large (> 10MB)

4. **Dependency Vulnerability Scan**
   - Scans all dependencies for known vulnerabilities
   - Reports any security issues

5. **Symbol Package Validation**
   - Confirms symbol packages are created for debugging support

### Manual Publishing

For emergency releases or testing, you can manually trigger publishing:

1. Go to GitHub > Actions > "Build" workflow
2. Click "Run workflow"
3. Select the `main` branch
4. Click "Run workflow"

This will build, validate, and publish packages with the current MinVer-generated version.

## Local Development

### Building Packages Locally

```bash
# Clean previous builds
dotnet clean

# Build and pack with Release configuration
dotnet pack --configuration Release

# Check generated packages
find . -name "*.nupkg" -o -name "*.snupkg"
```

### Checking Versions Locally

```bash
# Install MinVer CLI tool
dotnet tool install -g minver-cli

# Check version for main project
cd src/HybridMicroOrm && minver

# Check version for contracts project  
cd ../HybridMicroOrm.Contracts && minver
```

### Local Validation

You can run the same validation logic locally:

```bash
# Build packages
dotnet pack --configuration Release --no-restore

# Run package validation
./scripts/validate-packages.sh  # (if you create this script)
```

## Best Practices

### For Maintainers

1. **Use Semantic Versioning**: Create meaningful tags like `v1.0.0`, `v1.1.0`, `v2.0.0`
2. **Write Release Notes**: Always include comprehensive release notes
3. **Test Before Release**: Validate packages work correctly before creating releases
4. **Monitor Vulnerability Scans**: Address any dependency vulnerabilities promptly

### For Contributors

1. **Don't Modify Versions**: Never hardcode `PackageVersion` in `.csproj` files
2. **Update Dependencies Carefully**: Be mindful of version ranges and compatibility
3. **Test Package Changes**: Verify package builds work correctly
4. **Follow Conventional Commits**: Use clear commit messages for better release notes

## Troubleshooting

### Common Issues

1. **MinVer not generating expected version**
   - Ensure git tags follow semantic versioning (`v1.0.0`)
   - Check that fetch-depth is 0 in workflow
   - Verify tag prefix configuration matches

2. **Package validation failing**
   - Check that all required files are being created
   - Verify package sizes are reasonable
   - Review build output for errors

3. **Publishing not triggered**
   - Ensure the release is published (not just created as draft)
   - Check that the workflow file is on the main branch
   - Verify GitHub Actions are enabled

### Getting Help

- Check GitHub Actions logs for detailed error information
- Review MinVer documentation: https://github.com/adamralph/minver
- Consult NuGet best practices: https://learn.microsoft.com/en-us/nuget/create-packages/package-authoring-best-practices