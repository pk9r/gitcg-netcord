# Proposal

## Why

Phiên bản `Aspire.Hosting.AppHost` 13.6.4 (cùng với gói transitive `Aspire.Hosting` 13.6.4) hiện chứa lỗ hổng bảo mật mức độ nghiêm trọng cao (High severity vulnerability: GHSA-f7vh-h5g3-7jrh). Cần nâng cấp toàn bộ hệ sinh thái .NET Aspire trong dự án lên bản vá `13.6.5` để loại bỏ lỗ hổng bảo mật này và đảm bảo an toàn cho môi trường runtime và orchestration.

## What Changes

- Nâng cấp `Aspire.AppHost.Sdk` trong `src/AppHost/AppHost.csproj` từ `13.6.4` lên `13.6.5`.
- Cập nhật phiên bản các gói Aspire tập trung trong `Directory.Packages.props` từ `13.6.4` lên `13.6.5`:
  - `Aspire.Hosting.PostgreSQL`
  - `Aspire.Hosting.Redis`
  - `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL`
  - `Aspire.StackExchange.Redis.DistributedCaching`
- Chạy khôi phục gói (`dotnet restore`) và quét bảo mật (`dotnet list package --vulnerable`) để xác nhận các cảnh báo bảo mật liên quan đến Aspire đã được khắc phục hoàn toàn.
- Kiểm tra biên dịch toàn bộ solution (`dotnet build`) và chạy thử AppHost (`dotnet run --project src/AppHost/AppHost.csproj --no-build` hoặc tương đương) để đảm bảo không có breaking changes.

## Capabilities

### New Capabilities
None.

### Modified Capabilities
None.

## Impact

- **Mã nguồn dự án**: Không yêu cầu thay đổi mã C# nghiệp vụ; đây là bản cập nhật bảo mật patch release (13.6.4 -> 13.6.5).
- **Dependencies**: Cập nhật `Aspire.AppHost.Sdk` và các gói Aspire Hosting / Integration trong `Directory.Packages.props` và `src/AppHost/AppHost.csproj`.
- **Hệ thống runtime**: Loại bỏ lỗ hổng bảo mật GHSA-f7vh-h5g3-7jrh trên `Aspire.Hosting.AppHost` và `Aspire.Hosting`.

