# Playwright MCP Setup

## Overview
Playwright MCP server is installed in this project for browser automation via Claude Code. This enables Claude to control browsers for testing and interacting with the ASP.NET Core CMS admin interface.

## Installation
The Playwright MCP server has been installed and configured. To verify or update:

```bash
# Update Playwright MCP
npm update @playwright/mcp

# Update browsers
npx playwright install --with-deps
```

## Configuration
MCP server is configured in `~/.claude/config/mcp_settings.json`:

```json
{
  "mcpServers": {
    "playwright": {
      "command": "npx",
      "args": ["-y", "@playwright/mcp"],
      "cwd": "/home/justin/Projects/comjustinspicer",
      "env": {
        "PLAYWRIGHT_BROWSERS_PATH": "0"
      }
    }
  }
}
```

**Note:** Restart Claude Code after any changes to the MCP configuration.

## Development Server URLs
- **HTTPS:** https://localhost:7046
- **HTTP:** http://localhost:5063

## Admin Authentication
- **Login URL:** https://localhost:7046/Identity/Account/Login
- **Email:** admin@justinspicer.com
- **Password:** Check `appsettings.Development.json` (not committed to git)

## Admin Routes
- `/admin/contentblocks` - Content block management
- `/admin/article` - Article management
- `/admin/contentzones` - Content zone management
- `/admin/pages` - Page management

## Bulma CSS Selectors
The admin interface uses Bulma CSS framework with predictable selectors for automation:

### Forms
- `.field` - Form field container
- `.control` - Input control wrapper
- `.input` - Text input
- `.textarea` - Textarea
- `.select` - Select dropdown

### Buttons
- `.button` - Base button class
- `.button.is-primary` - Primary action button
- `.button.is-danger` - Danger/delete button
- `.button.is-link` - Link-styled button

### Navigation
- `.navbar` - Navigation bar
- `.navbar-menu` - Nav menu container
- `.navbar-item` - Nav menu item

### Tables
- `.table` - Base table class
- `.table.is-striped` - Striped table rows

## CKEditor Integration
The admin interface uses CKEditor 5 (v46.1.1) for rich text editing. When automating rich text fields:
- Wait for CKEditor to initialize before interacting
- Use accessibility API to interact with editor content

## Example Automation Patterns

### Login Flow
```typescript
// Navigate to login page
await page.goto('https://localhost:7046/Identity/Account/Login');

// Fill credentials
await page.fill('input[name="Input.Email"]', 'admin@justinspicer.com');
await page.fill('input[name="Input.Password"]', 'YOUR_PASSWORD_HERE');

// Submit and wait for redirect
await page.click('button[type="submit"]');
await page.waitForURL(/^(?!.*\/Identity\/Account\/Login)/);
```

### Create Content Block
```typescript
// Navigate to content blocks admin
await page.goto('https://localhost:7046/admin/contentblocks');

// Click create/add button
await page.click('.button.is-primary');

// Fill form fields using Bulma selectors
await page.fill('.input[name="Title"]', 'New Content Block');
// ... fill other fields

// Submit form
await page.click('button[type="submit"].button.is-primary');
```

### Take Screenshot
```typescript
// Navigate to page
await page.goto('https://localhost:7046');

// Take full page screenshot
await page.screenshot({ path: 'homepage.png', fullPage: true });
```

## Testing with Claude Code

After restarting Claude Code, test the MCP connection with these prompts:

1. **Basic navigation:**
   ```
   Navigate to https://localhost:7046 and describe what you see
   ```

2. **Take screenshot:**
   ```
   Take a screenshot of the homepage
   ```

3. **Login test:**
   ```
   Login to the admin panel at /Identity/Account/Login using the credentials from appsettings.Development.json
   ```

4. **Admin interaction:**
   ```
   Navigate to /admin/contentblocks and list all buttons visible on the page
   ```

## Troubleshooting

### Browser Not Found
If you see errors about missing browsers:
```bash
npx playwright install chromium
```

### MCP Server Not Connecting
1. Check `~/.claude/config/mcp_settings.json` for syntax errors
2. Verify the working directory path is correct
3. Restart Claude Code
4. Check Claude Code logs: `ls -la ~/.claude/debug/`

### HTTPS Certificate Errors
The development server uses a self-signed certificate. Playwright MCP handles this automatically, but if you encounter issues, you can configure it to ignore HTTPS errors in `playwright.config.ts` (see advanced configuration below).

### Port Conflicts
Verify the development server is running on the expected ports:
```bash
dotnet run --project Comjustinspicer.Web/Comjustinspicer.Web.csproj
```

Check `Comjustinspicer.Web/Properties/launchSettings.json` for configured URLs.

## Advanced Configuration (Optional)

### Create Playwright Config
For more advanced settings, create `playwright.config.ts` in the project root:

```typescript
import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './playwright-tests',
  use: {
    baseURL: 'https://localhost:7046',
    ignoreHTTPSErrors: true, // For dev cert
  },
  webServer: {
    command: 'dotnet run --project Comjustinspicer.Web/Comjustinspicer.Web.csproj',
    url: 'https://localhost:7046',
    timeout: 120000,
    reuseExistingServer: !process.env.CI,
  },
});
```

### Install Additional Browsers
```bash
# Firefox
npx playwright install firefox

# WebKit (Safari)
npx playwright install webkit

# All browsers
npx playwright install
```

### Enable Debug Mode
For detailed logging:
```bash
DEBUG=pw:api npx @playwright/mcp
```

## Security Considerations

1. **Credentials:** Admin passwords are stored in `appsettings.Development.json` which is excluded from git via `.gitignore`
2. **Session Management:** Playwright maintains browser context and sessions persist across automation steps
3. **HTTPS Certificates:** Development uses self-signed certificates which Playwright accepts automatically

## Future Enhancements

### E2E Test Suite
Create a dedicated test directory for Playwright tests:
```
playwright-tests/
├── admin/
│   ├── contentblocks.spec.ts
│   ├── articles.spec.ts
│   └── auth.spec.ts
└── helpers/
    └── auth.ts
```

### CI/CD Integration
Add Playwright tests to GitHub Actions:
1. Install Node.js in workflow
2. Install Playwright browsers
3. Run tests against Docker container

### NUnit Integration
Investigate Playwright .NET bindings for integration with existing NUnit test framework.

## Resources

- [Playwright MCP Documentation](https://github.com/microsoft/playwright-mcp)
- [Playwright Documentation](https://playwright.dev/)
- [Claude Code MCP Guide](https://docs.anthropic.com/en/docs/claude-code/mcp)
- [Bulma CSS Documentation](https://bulma.io/documentation/)

## Support

For issues or questions:
- Check Playwright MCP GitHub issues: https://github.com/microsoft/playwright-mcp/issues
- Review Claude Code documentation: https://docs.anthropic.com/en/docs/claude-code
- Check project-specific issues in this repository
