# SessionValidation

`SessionValidation` is a .NET 11 RC1 Blazor Web App used to validate session configuration and recovery behavior for two ASP.NET Core component features:

- Session-backed `[SupplyParameterFromTempData]`
- `[SupplyParameterFromSession]`

The sample uses static server-side rendering, an in-memory distributed cache, and a 15-second session idle timeout. It covers normal reads, missing session middleware, expiry, deleted cookies, corrupt bytes, and valid JSON with an invalid stored-value shape.

## Prerequisites

- .NET SDK `11.0.100-rc.1.26425.128` or a compatible .NET 11 RC1 SDK
- A Chromium-based browser with DevTools
- PowerShell on Windows for the documented evidence commands

The required SDK is pinned in [global.json](global.json).

## Build and run

From the repository root:

```powershell
dotnet build .\SessionValidation.sln
dotnet run --project .\SessionValidation
```

Open `http://localhost:5188` after the application starts.

## Validation pages

| Route | Purpose | Seeded value |
|---|---|---|
| `/session-parameter` | Reads a nullable `[SupplyParameterFromSession]` property | `session-valid` |
| `/tempdata-session` | Reads nullable session-backed TempData | `tempdata-valid` |

The test-only mutation endpoints overwrite the exact session entry consumed by each reader:

```text
POST /_validation/corrupt/{key}
POST /_validation/wrong-shape/{key}
```

The relevant keys are `session_message` and `__BlazorTempData`.

## Configuration

Session services and the distributed-memory backing store remain registered for every scenario. Middleware can be disabled independently with:

```json
{
  "Validation": {
    "UseSession": false
  }
}
```

This isolates the expected missing-middleware `InvalidOperationException` from service-registration failures. Restore the setting to `true` for all other scenarios.

Component diagnostics are configured at Debug level during validation. Keep the application console visible while testing so warnings and exceptions can be captured.

## Test coverage

- Baseline reads with and without forced navigation
- UI-initiated and direct URL navigation
- Missing `UseSession` for both features
- Session idle-timeout expiry
- Deleted `.AspNetCore.Session` cookie
- Corrupt bytes in each feature's actual storage key
- Valid JSON with the wrong stored-envelope shape
- Comparison of normal absence with logged deserialization failures

The results and linked artifacts are in [Evidence/TestReport.md](Evidence/TestReport.md).

## Current result

All required session and TempData behavior checks pass. The report records an unrelated diagnostic issue where baseline logs unexpectedly execute `/not-found`; this does not prevent either feature from returning its expected value or HTTP 200 response.