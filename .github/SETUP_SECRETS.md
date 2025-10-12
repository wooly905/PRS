# GitHub Secrets Setup Guide

## 🔐 Setting up NUGET_API_KEY

To enable automatic publishing to NuGet.org, you need to configure the `NUGET_API_KEY` secret.

### Step-by-Step Guide

#### 1. Get Your NuGet.org API Key

1. Go to https://www.nuget.org
2. Sign in with your account
3. Click on your username (top right) → **API Keys**
4. Click **Create** button

5. Configure the API key:
   - **Key Name:** `PRS GitHub Actions` (or any descriptive name)
   - **Select Scopes:** 
     - ✅ **Push**
     - ✅ **Push new packages and package versions**
   - **Select Packages:**
     - Choose **Select packages** if you want to scope to `wooly905.prs` only (recommended)
     - Or choose **All packages** if you manage multiple packages
   - **Glob Pattern:** `wooly905.prs` (if using Select packages)
   - **Expiration:** Choose 365 days or custom duration

6. Click **Create**

7. **IMPORTANT:** Copy the API key immediately!
   - The key looks like: `oy2...` (long string)
   - You won't be able to see it again
   - Save it in a secure location temporarily

---

#### 2. Add API Key to GitHub Secrets

1. Go to your GitHub repository: `https://github.com/yourusername/PRS`

2. Click on **Settings** tab (top right)

3. In the left sidebar, click **Secrets and variables** → **Actions**

4. Click **New repository secret** button

5. Configure the secret:
   - **Name:** `NUGET_API_KEY` (must be exactly this name)
   - **Secret:** Paste your NuGet API key
   
6. Click **Add secret**

7. **Verify:** You should now see `NUGET_API_KEY` in your secrets list

---

#### 3. Verify Setup

1. Go to **Actions** tab in your repository

2. Click on **prs-release** workflow (left sidebar)

3. Click **Run workflow** button

4. Select branch: `main`

5. Click **Run workflow**

6. Watch the workflow execute:
   - ✅ Build succeeds
   - ✅ All 121 tests pass
   - ✅ Package is created
   - ✅ Package is pushed to NuGet.org
   - ✅ No API key errors

---

## 🔒 Security Best Practices

### ✅ DO:
- Store API keys in GitHub Secrets
- Use scoped API keys (specific packages only)
- Set expiration dates on API keys
- Rotate keys periodically
- Use `--skip-duplicate` flag to handle version conflicts

### ❌ DON'T:
- Never commit API keys to code
- Never put API keys in comments or documentation
- Never share API keys in pull requests
- Never use overly permissive scopes

---

## 🛡️ Secret Management

### Where Secrets are Used

**In `prs-release.yml`:**
```yaml
- name: Push to NuGet.org
  run: dotnet nuget push ./nupkg/*.nupkg 
    --api-key ${{ secrets.NUGET_API_KEY }}   # ← Secret used here
    --source https://api.nuget.org/v3/index.json 
    --skip-duplicate
```

**Security Features:**
- Secret value is never printed in logs
- GitHub automatically masks secret values in output
- Secrets are encrypted at rest
- Only available to workflows in the same repository

---

## 🔄 Rotating API Keys

If you need to rotate your API key:

1. **Create new key on NuGet.org:**
   - Follow steps in section 1
   - Create a new key with same permissions

2. **Update GitHub Secret:**
   - Go to Settings → Secrets and variables → Actions
   - Click on `NUGET_API_KEY`
   - Click **Update secret**
   - Paste new API key
   - Click **Update secret**

3. **Revoke old key on NuGet.org:**
   - Go to your API Keys page
   - Find the old key
   - Click **Delete**

4. **Test new key:**
   - Run the `prs-release` workflow
   - Verify it completes successfully

---

## 📊 Monitoring Releases

### Check NuGet.org

After workflow completes:

1. Go to https://www.nuget.org/packages/wooly905.prs
2. Verify new version appears
3. Check download stats
4. View package details

### GitHub Artifacts

Even if NuGet.org upload fails, you can download the package:

1. Go to workflow run
2. Scroll to **Artifacts** section
3. Download `nuget-package`
4. Manually upload if needed

---

## 🐛 Troubleshooting

### Error: "Invalid API Key"

**Possible causes:**
- API key not added to GitHub Secrets
- Secret name is not exactly `NUGET_API_KEY`
- API key has expired
- API key was revoked

**Solution:**
- Verify secret name is correct (case-sensitive)
- Check API key expiration on NuGet.org
- Regenerate and update secret

### Error: "Package version already exists"

**Cause:** Trying to publish same version twice

**Solution:**
- Increment version in `src/PRS.csproj`
- The `--skip-duplicate` flag will prevent workflow from failing

### Error: "Insufficient permissions"

**Cause:** API key doesn't have push permissions

**Solution:**
- Create new API key with **Push** scope
- Update GitHub Secret

### Tests Failing in CI

**Cause:** Tests pass locally but fail in CI

**Solution:**
- Check test output in Actions tab
- Look for environment-specific issues
- Verify test data files are committed
- Run `dotnet test` locally with Release configuration

---

## 📞 Need Help?

If you encounter issues:

1. Check workflow logs in GitHub Actions
2. Review NuGet.org API key permissions
3. Verify all secrets are correctly configured
4. Check this documentation
5. Open an issue in the repository

---

## ✅ Quick Checklist

Before first release:

- [ ] NuGet.org account created
- [ ] API key generated on NuGet.org
- [ ] `NUGET_API_KEY` added to GitHub Secrets
- [ ] Version updated in `src/PRS.csproj`
- [ ] All tests pass locally (121/121)
- [ ] Code committed and pushed
- [ ] `prs-release` workflow triggered
- [ ] Package appears on NuGet.org
- [ ] Installation tested: `dotnet tool install --global wooly905.prs`

---

**Happy Publishing! 🚀**

For more information, see [workflows/README.md](workflows/README.md)

