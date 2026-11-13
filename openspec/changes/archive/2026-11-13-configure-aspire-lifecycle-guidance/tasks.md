# Tasks

## 1. OpenSpec Configuration Updates

- [x] 1.1 Update `openspec/config.yaml` to add Aspire Process Ownership rules forbidding `aspire stop` and `aspire start` under `operations.apply.guidance`
- [x] 1.2 Update `openspec/config.yaml` to add resource-level isolation (`aspire resource main stop/start`) and scoped testing guidance for resolving Windows DLL file locks
- [x] 1.3 Validate `openspec/config.yaml` syntax and verify that `openspec instructions apply` outputs the updated guidance correctly

## 2. Verification

- [x] 2.1 Run `openspec validate configure-aspire-lifecycle-guidance` and verify the change structure and schemas pass validation
- [x] 2.2 Execute and pass the test suite with scoped `dotnet test tests/GitcgNetCord.MainApp.Tests/GitcgNetCord.MainApp.Tests.csproj`
