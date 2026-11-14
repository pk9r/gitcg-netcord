# Tasks

## 1. OpenSpec Configuration Updates

- [x] 1.1 Update `openspec/config.yaml` to replace reactive lock recovery with proactive Session-Level Aspire Resource Lifecycle guidance under `operations.apply.guidance`
- [x] 1.2 Validate `openspec/config.yaml` syntax and verify that `openspec instructions apply` outputs the updated session-level guidance correctly

## 2. Verification

- [x] 2.1 Run `openspec validate optimize-aspire-apply-lifecycle` and verify the change structure and schemas pass validation
- [x] 2.2 Execute and pass the test suite with scoped `dotnet test tests/GitcgNetCord.MainApp.Tests/GitcgNetCord.MainApp.Tests.csproj`
