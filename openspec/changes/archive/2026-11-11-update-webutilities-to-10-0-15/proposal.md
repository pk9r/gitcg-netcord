# Proposal

## Why

Thư viện `Microsoft.AspNetCore.WebUtilities` phiên bản `10.0.14` chứa lỗ hổng bảo mật nghiêm trọng cao (High severity vulnerability: GHSA-x33c-w48q-qrgc). Lỗ hổng này ảnh hưởng trực tiếp đến dự án `HoyolabHttpClient` và ảnh hưởng gián tiếp (transitive) đến các dự án phụ thuộc như `GitcgPainter`, `GitcgSharp.Shared`, `GitcgSkia`. Cần cập nhật gói này cùng các gói .NET 10.0 runtime / extensions liên quan lên phiên bản `10.0.15` để loại bỏ hoàn toàn cảnh báo bảo mật và đảm bảo an toàn cho toàn bộ solution.

## What Changes

- Cập nhật gói `Microsoft.AspNetCore.WebUtilities` trong `Directory.Packages.props` từ `10.0.14` lên `10.0.15`.
- Đồng bộ các gói phụ thuộc runtime 10.0.x trong `Directory.Packages.props` từ `10.0.14` lên bản vá `10.0.15`:
  - `Microsoft.EntityFrameworkCore.Design`
  - `Microsoft.Extensions.DependencyInjection.Abstractions`
  - `Microsoft.Extensions.Http`
- Thực hiện khôi phục gói (`dotnet restore`) và quét bảo mật (`dotnet list package --vulnerable --include-transitive`) để xác nhận toàn bộ solution không còn package nào chứa lỗ hổng.
- Biên dịch solution (`dotnet build`) để đảm bảo không phát sinh lỗi tương thích hoặc breaking changes.

## Capabilities

### New Capabilities
None.

### Modified Capabilities
None.

## Impact

- **Mã nguồn dự án**: Không yêu cầu thay đổi mã nguồn logic C#.
- **Dependencies**: Cập nhật các PackageVersion trong `Directory.Packages.props` từ `10.0.14` lên `10.0.15`.
- **Bảo mật**: Khắc phục lỗ hổng bảo mật GHSA-x33c-w48q-qrgc trên `Microsoft.AspNetCore.WebUtilities`.

