# Design

## Context

Dự án `gitcg-netcord` bao gồm 10 projects C# (1 AppHost, 7 library/application projects trong `src/`, và 2 test projects trong `tests/`). Việc quản lý phiên bản thư viện được tập trung hóa thông qua Central Package Management (CPM) tại `Directory.Packages.props`.

Môi trường phát triển hiện tại đã cài đặt .NET SDK 11.0.100 (`11.0.100 [C:\Program Files\dotnet\sdk]`). Toàn bộ 76 unit/integration tests hiện tại đều vượt qua khi chạy trên `net11.0`.

Trong quá trình khởi chạy runtime với .NET Aspire, đã phát hiện lỗi không tương thích nhị phân giữa `Microsoft.EntityFrameworkCore 11.0.0` và `Npgsql.EntityFrameworkCore.PostgreSQL 10.0.3` (thuộc `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL 13.6.5`) dẫn đến `MissingMethodException` trong `JsonTypeMapping..ctor`.

Xem `proposal.md` để biết thêm về mục tiêu và bối cảnh nâng cấp.

## Goals / Non-Goals

**Goals:**
- Nâng cấp đồng bộ `TargetFramework` của toàn bộ 10 projects lên `net11.0`.
- Cập nhật các gói thư viện Microsoft BCL, OpenApi, Resilience, Caching trong `Directory.Packages.props` lên phiên bản `11.0.0`.
- Ổn định các gói Entity Framework Core (`Microsoft.EntityFrameworkCore.Design`, `Microsoft.EntityFrameworkCore.InMemory`) ở phiên bản `10.0.14` để đảm bảo tương thích nhị phân 100% với `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL 13.6.5` và `Npgsql.EntityFrameworkCore.PostgreSQL 10.0.3`.
- Nâng cấp base image trong `src/GitcgNetCord.MainApp/Dockerfile` lên .NET 11 Alpine.
- Cập nhật định nghĩa stack trong `openspec/config.yaml` (.NET 11, C# 15).
- Đảm bảo toàn bộ solution biên dịch sạch sẽ không lỗi, 100% test cases (76 tests) vượt qua thành công, và ứng dụng khởi chạy Healthy (HTTP 200 tại `/health`) dưới Aspire.

**Non-Goals:**
- Không thay đổi mã nguồn các modules lệnh Discord hay giao diện đồ họa.
- Không thay đổi schema cơ sở dữ liệu PostgreSQL (không tạo thêm EF Core migrations mới).
- Không nâng cấp các gói Aspire vượt qua 13.6.5 (phiên bản hiện tại ổn định và hỗ trợ đầy đủ .NET 11).
- Không tự ý tái cấu trúc hoặc áp dụng các cú pháp mới của C# 15 nằm ngoài phạm vi tương thích của bản nâng cấp.

## Decisions

### Quyết định 1: Nâng cấp TargetFramework đồng loạt trên toàn bộ solution
- **Lựa chọn**: Đổi `<TargetFramework>net10.0</TargetFramework>` thành `<TargetFramework>net11.0</TargetFramework>` trên tất cả 10 projects cùng lúc.
- **Lý do**: Solution có mạng lưới ProjectReference dày đặc giữa MainApp, GitcgSkia, HoyolabHttpClient, ServiceDefaults, SharedUtils và các test projects. Việc nâng cấp không đồng bộ sẽ gây ra cảnh báo/lỗi MSBuild về target framework mismatch khi project bậc cao reference project bậc thấp hơn.
- **Phương án thay thế**: Nâng cấp từng project hoặc multi-targeting (`net10.0;net11.0`). Bị loại bỏ vì dự án là ứng dụng độc lập (standalone service), không phải thư viện NuGet public cần hỗ trợ nhiều runtime cũ.

### Quyết định 2: Tách biệt phiên bản BCL (11.0.0) và EF Core (10.0.14) cho PostgreSQL Provider
- **Lựa chọn**:
  - Cập nhật các package BCL & Web lên `11.0.0`:
    - `Microsoft.Extensions.Caching.Hybrid`: `11.0.0`
    - `Microsoft.AspNetCore.OpenApi`: `11.0.0`
    - `Microsoft.Extensions.Http.Resilience`: `11.0.0`
    - `Microsoft.Extensions.ServiceDiscovery`: `11.0.0`
    - `Microsoft.Extensions.DependencyInjection.Abstractions`: `11.0.0`
    - `Microsoft.Extensions.Http`: `11.0.0`
    - `Microsoft.AspNetCore.WebUtilities`: `11.0.0`
  - Giữ nguyên các package EF Core ở `10.0.14`:
    - `Microsoft.EntityFrameworkCore.Design`: `10.0.14`
    - `Microsoft.EntityFrameworkCore.InMemory`: `10.0.14`
    - Gỡ bỏ ghim transitive `Microsoft.EntityFrameworkCore` 11.0.0 và `Microsoft.EntityFrameworkCore.Relational` 11.0.0.
    - Gỡ bỏ `<PackageReference Include="Microsoft.EntityFrameworkCore" />` trực tiếp trong `GitcgNetCord.MainApp.csproj`.
- **Lý do**: `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL 13.6.5` mang theo `Npgsql.EntityFrameworkCore.PostgreSQL 10.0.3` (provider Npgsql 11 chưa ra mắt RTM). Npgsql 10.0.3 gọi constructor `JsonTypeMapping..ctor(string, Type, DbType?)` vốn đã bị thay đổi trong EF Core 11.0.0, gây ra `MissingMethodException` tại runtime. Nền tảng .NET 11 hỗ trợ chạy mượt mà các thư viện EF Core 10.x mà không gặp bất kỳ xung đột nhị phân nào.
- **Phương án thay thế**: Sử dụng `Npgsql.EntityFrameworkCore.PostgreSQL 11.0.0-rc.3`. Bị loại bỏ vì gói này là prerelease và ghim cứng phiên bản cụ thể `11.0.0-rc.3.26522.148`, xung đột với bản EF Core 11 RTM và không an toàn cho production.

### Quyết định 3: Giữ nguyên các gói bên thứ ba và hệ sinh thái Aspire 13.6.5
- **Lựa chọn**: Giữ nguyên:
  - `Aspire.Hosting.*` và `Aspire.Npgsql.*` / `Aspire.StackExchange.Redis.*` ở phiên bản `13.6.5`.
  - `NetCord.Hosting.Services` ở phiên bản `1.0.0-beta.29`.
  - `SkiaSharp` và `SkiaSharp.NativeAssets.Linux.NoDependencies` ở phiên bản `4.154.0`.
  - `Scalar.AspNetCore` ở phiên bản `2.21.0`.
  - `Microsoft.SemanticKernel` ở phiên bản `1.81.0`.
- **Lý do**: Các gói này tương thích hoàn toàn với .NET 11 runtime mà không có bất kỳ breaking changes nào. Việc cô lập phạm vi chỉ nâng cấp runtime core giúp giảm thiểu tối đa rủi ro hồi quy.

### Quyết định 4: Nâng cấp Dockerfile base images lên 11.0-alpine
- **Lựa chọn**:
  - `FROM mcr.microsoft.com/dotnet/aspnet:11.0-alpine AS base`
  - `FROM mcr.microsoft.com/dotnet/sdk:11.0-alpine AS build`
- **Lý do**: Đảm bảo hình ảnh Docker release của MainApp biên dịch và vận hành trên runtime .NET 11 Alpine chính thức.

## Risks / Trade-offs

- **[Risk: Không tương thích nhị phân giữa Npgsql 10.x và EF Core 11.x (`MissingMethodException`)]**
  - *Mitigation*: Giữ `Microsoft.EntityFrameworkCore.Design` và `Microsoft.EntityFrameworkCore.InMemory` ở `10.0.14`. Điều này đảm bảo Npgsql 10.0.3 liên kết chính xác với assembly EF Core 10.x mà vẫn chạy trên nền `net11.0`.
- **[Risk: Ngoại lệ trong BackgroundService dừng toàn bộ Host trong .NET 11]**
  - *Mitigation*: Đã rà soát mã nguồn `DailySnapshotWorker.cs` và `VersionSnapshotWorker.cs`. Cả hai background worker đều đã có khối `try-catch (Exception ex) when (ex is not OperationCanceledException)` chuẩn mực bọc ngoài từng vòng lặp xử lý, ghi log lỗi mà không để ngoại lệ văng ra ngoài dừng Host.
- **[Risk: Microsoft.OpenApi v3 thay đổi object model]**
  - *Mitigation*: `Program.cs` chỉ gọi các extension method `builder.Services.AddOpenApi()` và `app.MapOpenApi()` được cung cấp bởi ASP.NET Core, không tương tác trực tiếp với DOM types của `Microsoft.OpenApi`, đảm bảo biên dịch và tạo schema OpenAPI 3.2.0 trơn tru.
- **[Risk: Yêu cầu phần cứng CPU tối thiểu của .NET 11 x86-64-v2]**
  - *Mitigation*: Hạ tầng hosting và môi trường Docker hiện đại đều chạy trên các vi xử lý x86-64 hỗ trợ tối thiểu SSE4.2 (các vi xử lý từ sau năm 2013).

## Migration Plan

1. Cập nhật `Directory.Packages.props`:
   - Nâng cấp các gói BCL/Web lên `11.0.0`.
   - Giữ `Microsoft.EntityFrameworkCore.Design` và `InMemory` ở `10.0.14`.
   - Gỡ bỏ transitive pins và direct package references thừa của EF Core 11.
2. Cập nhật `<TargetFramework>net11.0</TargetFramework>` trong 10 file `.csproj`.
3. Cập nhật `src/GitcgNetCord.MainApp/Dockerfile` sang base image 11.0-alpine.
4. Cập nhật ngữ cảnh trong `openspec/config.yaml`.
5. Chạy `dotnet restore` và `dotnet build` để kiểm tra biên dịch sạch (0 errors, 0 warnings không mong muốn).
6. Chạy `dotnet test` trên `GitcgNetCord.MainApp.Tests` và `GitcgSkia.Tests` để xác minh toàn bộ 76 test cases vượt qua thành công.
7. Khởi chạy thử nghiệm qua Aspire CLI (`aspire restart` hoặc qua dashboard) và xác nhận endpoint `/health` trả lời Healthy (HTTP 200).
