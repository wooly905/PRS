# GitHub Workflows Documentation

This directory contains GitHub Actions workflows for the PRS project.

## 📋 Workflows

### 1. `build.yml` - Continuous Integration

**Triggers:** 
- Push to `main` branch
- Pull requests to `main` branch

**Steps:**
1. ✅ Checkout code
2. ✅ Install .NET 8.0
3. ✅ Restore dependencies
4. ✅ Build solution (Release mode)
5. ✅ **Run unit tests** (121 tests)
6. ✅ Update package version
7. ✅ Create and merge version update PR

**Purpose:** Ensures code quality and automatically manages version numbers.

---

### 2. `prs-release.yml` - NuGet Package Release

**Triggers:** 
- Manual trigger only (`workflow_dispatch`)

**Steps:**
1. ✅ Checkout code
2. ✅ Install .NET 8.0
3. ✅ Restore dependencies
4. ✅ Build solution (Release mode)
5. ✅ **Run unit tests** (must pass 100%)
6. ✅ Generate test report
7. ✅ Pack NuGet package
8. ✅ Display package information
9. ✅ **Push to NuGet.org** (using API key)
10. ✅ Upload package as artifact

**Purpose:** Create and publish NuGet packages to nuget.org.

---

## 🔐 Required Secrets

For the release workflow to work, you need to configure the following GitHub Secret:

### NUGET_API_KEY

**How to set up:**

1. **Get your NuGet.org API Key:**
   - Go to https://www.nuget.org
   - Sign in to your account
   - Navigate to your profile → **API Keys**
   - Click **Create** to generate a new API key
   - Give it a name (e.g., "PRS GitHub Actions")
   - Select scopes: **Push** (and optionally **Push new packages and package versions**)
   - Copy the generated API key (you won't see it again!)

2. **Add to GitHub Secrets:**
   - Go to your GitHub repository
   - Click **Settings** → **Secrets and variables** → **Actions**
   - Click **New repository secret**
   - **Name:** `NUGET_API_KEY`
   - **Value:** Paste your NuGet.org API key
   - Click **Add secret**

**Security Notes:**
- ✅ API keys are encrypted and never exposed in logs
- ✅ Only repository administrators can view/edit secrets
- ✅ Secrets are not passed to workflows from forked repositories
- ✅ Use `--skip-duplicate` to avoid errors on re-publishing same version

---

## 🚀 How to Use

### Running the Release Workflow

1. **Ensure all tests pass locally:**
   ```bash
   dotnet test tests/PRS.Tests.csproj
   ```

2. **Update version number** in `src/PRS.csproj`:
   ```xml
   <PackageVersion>8.3.0.16</PackageVersion>
   ```

3. **Commit and push changes:**
   ```bash
   git add src/PRS.csproj
   git commit -m "Bump version to 8.3.0.16"
   git push origin main
   ```

4. **Trigger manual release:**
   - Go to GitHub repository
   - Click **Actions** tab
   - Select **prs-release** workflow
   - Click **Run workflow**
   - Select branch (usually `main`)
   - Click **Run workflow**

5. **Monitor the workflow:**
   - Watch the workflow execution
   - All tests must pass (121/121)
   - Package will be automatically pushed to NuGet.org
   - Check NuGet.org for your published package

---

## 📦 NuGet Package Details

### Package Information
- **Package ID:** `wooly905.prs`
- **Tool Command:** `prs`
- **Target Framework:** .NET 8.0
- **Package Type:** .NET Global Tool

### Installation
```bash
dotnet tool install --global wooly905.prs
```

### Update
```bash
dotnet tool update --global wooly905.prs
```

---

## 🧪 Test Integration

Both workflows now include unit test execution:

- **build.yml:** Tests run on every push/PR
- **prs-release.yml:** Tests must pass before publishing

**Test Requirements:**
- All 121 tests must pass (100%)
- Test results are published to GitHub Actions UI
- Failed tests will stop the workflow

---

## 🛡️ Safety Features

### Release Workflow (`prs-release.yml`)
- ✅ **Manual trigger only** - Prevents accidental releases
- ✅ **Tests must pass** - No broken packages published
- ✅ **Skip duplicate** - Won't fail if version already exists
- ✅ **Artifact upload** - Package saved for 30 days
- ✅ **Secure API key** - Uses GitHub Secrets

### Build Workflow (`build.yml`)
- ✅ **Automatic** - Runs on every push/PR
- ✅ **Tests included** - Catches issues early
- ✅ **Version management** - Automated version bumping

---

## 📝 Workflow Variables

### Environment Variables
```yaml
env:
  Solution_Name: PRS.sln
  Project_Path: src/PRS.csproj
  Tests_Path: tests/PRS.Tests.csproj
```

### Secrets Required
```yaml
secrets:
  NUGET_API_KEY  # NuGet.org API key (required for prs-release.yml)
  GITHUB_TOKEN   # Automatically provided by GitHub
```

---

## 🔄 Release Process Flow

```
1. Developer updates version in PRS.csproj
2. Commits and pushes to main
3. Manually triggers prs-release workflow
4. Workflow starts:
   ├─ Checkout code
   ├─ Install .NET 8.0
   ├─ Restore dependencies
   ├─ Build (Release mode)
   ├─ Run tests (must pass all 121)
   ├─ Pack NuGet package
   ├─ Push to NuGet.org ✅
   └─ Upload artifact (backup)
5. Package is live on NuGet.org
6. Users can install: dotnet tool install --global wooly905.prs
```

---

## 📊 Test Reporting

Test results are published to GitHub Actions with:
- ✅ Test count and pass rate
- ✅ Detailed failure information (if any)
- ✅ Test execution time
- ✅ Coverage information

**Test Reporter:** Uses `dorny/test-reporter@v1` for nice UI display

---

## ⚠️ Important Notes

### Before Publishing
1. ✅ **Increment version** in `src/PRS.csproj`
2. ✅ **Update CHANGELOG** if you have one
3. ✅ **Run tests locally** to ensure 100% pass
4. ✅ **Test the tool locally** with `dotnet pack` and `dotnet tool install`

### Version Numbering
Current format: `Major.Minor.Build.Revision`
- Example: `8.3.0.16`
- **Must be unique** for each NuGet publish
- **Cannot republish** same version (use `--skip-duplicate` to handle this)

### API Key Permissions
Your NuGet API key should have:
- ✅ **Push** permission (minimum)
- ✅ **Push new packages** permission (if publishing new package)
- ✅ Scoped to specific package (recommended) or all packages

---

## 🐛 Troubleshooting

### "API key not found" error
**Problem:** `NUGET_API_KEY` secret not configured

**Solution:** Follow the setup steps above to add the secret

### "Package version already exists" error
**Problem:** Trying to publish same version twice

**Solution:** 
- Increment version in `src/PRS.csproj`
- Or use `--skip-duplicate` flag (already included)

### Tests failing in CI but passing locally
**Problem:** Environment differences

**Solution:**
- Check test output in Actions tab
- Ensure all dependencies are in the project
- Verify test data files are included

### Package not showing on NuGet.org
**Problem:** Publishing might take a few minutes

**Solution:**
- Wait 5-10 minutes for NuGet.org to process
- Check workflow logs for errors
- Verify API key has correct permissions

---

## 📖 Additional Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [NuGet Package Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/publish-a-package)
- [GitHub Secrets](https://docs.github.com/en/actions/security-guides/encrypted-secrets)
- [.NET Global Tools](https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools)

---

## ✅ Checklist for First Release

- [ ] Create NuGet.org account
- [ ] Generate NuGet API key
- [ ] Add `NUGET_API_KEY` to GitHub Secrets
- [ ] Update version in `src/PRS.csproj`
- [ ] Ensure all tests pass (121/121)
- [ ] Trigger `prs-release` workflow manually
- [ ] Verify package appears on NuGet.org
- [ ] Test installation: `dotnet tool install --global wooly905.prs`

---

**Last Updated:** October 2025  
**Test Count:** 121 (100% pass rate)  
**Package Format:** .NET Global Tool

