# Proposal

## Why

Hệ thống hiện đang chạy trên .NET 10 (`net10.0`) trong khi môi trường máy phát triển và hệ sinh thái .NET đã chính thức phát hành .NET 11 SDK (`11.0.100`) cùng các gói thư viện chuẩn 11.0 (ASP.NET Core, Microsoft.Extensions 11). Việc nâng cấp lên .NET 11 giúp tận dụng những cải tiến hiệu năng vượt bậc của JIT compiler (tối ưu hóa `x86-64-v3` ReadyToRun, vectorization, giảm GC throughput), đồng thời đảm bảo toàn bộ dự án đồng bộ với toolchain mới nhất, sẵn sàng tiếp nhận các tính năng ngôn ngữ C# 15.

## What Changes

- **TargetFramework**: Nâng cấp toàn bộ 10 projects trong solution từ `net10.0` lên `net11.0`.
- **Central Package Management**:
  - Cập nhật các gói BCL và web của Microsoft trong `Directory.Packages.props` lên phiên bản `11.0.0`:
    - `Microsoft.Extensions.Caching.Hybrid` (10.11.0 -> 11.0.0)
    - `Microsoft.AspNetCore.OpenApi` (10.0.15 -> 11.0.0)
    - `Microsoft.Extensions.Http.Resilience` (10.11.0 -> 11.0.0)
    - `Microsoft.Extensions.ServiceDiscovery` (10.11.0 -> 11.0.0)
    - `Microsoft.Extensions.DependencyInjection.Abstractions` (10.0.15 -> 11.0.0)
    - `Microsoft.Extensions.Http` (10.0.15 -> 11.0.0)
    - `Microsoft.AspNetCore.WebUtilities` (10.0.15 -> 11.0.0)
  - Duy trì các gói EF Core (`Microsoft.EntityFrameworkCore.Design`, `Microsoft.EntityFrameworkCore.InMemory`) ở phiên bản `10.0.14` để đảm bảo tương thích nhị phân tuyệt đối với `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL 13.6.5` và `Npgsql.EntityFrameworkCore.PostgreSQL 10.0.3` (tránh lỗi runtime `MissingMethodException` trong `JsonTypeMapping`), đồng thời gỡ bỏ các khai báo ghim trung gian EF Core 11.0.0 không tương thích.
- **Containerization**: Cập nhật `src/GitcgNetCord.MainApp/Dockerfile` sử dụng base image `mcr.microsoft.com/dotnet/aspnet:11.0-alpine` và `mcr.microsoft.com/dotnet/sdk:11.0-alpine`.
- **Configuration & Context**: Cập nhật ngữ cảnh công nghệ trong `openspec/config.yaml` (.NET 11, C# 15).
- **SemVer Impact & Breaking Changes**:
  - **SemVer Impact**: **MAJOR** (Nâng cấp framework nền tảng và môi trường runtime/SDK).
  - **BREAKING (Build/Runtime Requirement)**: Yêu cầu bắt buộc môi trường máy phát triển và CI/CD phải có .NET 11 SDK (`11.0.100+`).
  - **BREAKING (Hardware Baseline)**: .NET 11 nâng mức baseline x86/x64 tối thiểu lên `x86-64-v2` (yêu cầu SSE4.2, POPCNT, các CPU sau 2013).
  - User-facing commands và API nghiệp vụ trên Discord: **Không thay đổi (Non-breaking)**.
- **Boundaries**:
  - **In-Scope**:
    - Nâng cấp TFM cho 10 file `.csproj`.
    - Cập nhật phiên bản các package 11.0 (BCL/OpenApi) và ổn định EF Core ở 10.0.14 trong `Directory.Packages.props`.
    - Cập nhật base image trong `src/GitcgNetCord.MainApp/Dockerfile`.
    - Cập nhật `openspec/config.yaml`.
    - Kiểm tra `dotnet restore`, `dotnet build` và chạy toàn bộ test suites (`dotnet test`).
    - Xác minh ứng dụng khởi động runtime và endpoint `/health` trả về Healthy (HTTP 200).
  - **Out-of-Scope**:
    - Không thay đổi mã nguồn logic nghiệp vụ bot, handler, commands hay UI rendering.
    - Không thay đổi schema cơ sở dữ liệu PostgreSQL (không phát sinh EF Core migrations mới).
    - Không nâng cấp provider Npgsql lên các bản prerelease (`11.0.0-rc.3`) chưa ổn định.
    - Không kích hoạt release tag hay triển khai production trực tiếp trong change này.

## Capabilities

### New Capabilities
None.

### Modified Capabilities
None.

## Impact

- **Affected Code & Configuration**:
  - 10 file `.csproj` (`src/AppHost`, `src/GitcgNetCord.MainApp`, `src/GitcgPainter`, `src/GitcgSharp.Shared`, `src/GitcgSkia`, `src/HoyolabHttpClient`, `src/ServiceDefaults`, `src/SharedUtils`, `tests/GitcgNetCord.MainApp.Tests`, `tests/GitcgSkia.Tests`).
  - `Directory.Packages.props`.
  - `src/GitcgNetCord.MainApp/Dockerfile`.
  - `openspec/config.yaml`.
- **Infrastructure Requirements**:
  - Không phát sinh tài nguyên hạ tầng mới (PostgreSQL, Redis vẫn giữ nguyên trong Aspire AppHost).
  - Yêu cầu runner và container runtime hỗ trợ .NET 11.
