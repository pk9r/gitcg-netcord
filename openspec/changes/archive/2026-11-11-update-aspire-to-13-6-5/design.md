# Design

## Context

Dự án sử dụng Central Package Management (CPM) thông qua `Directory.Packages.props`.
Hiện tại:
- `src/AppHost/AppHost.csproj` khai báo SDK: `<Project Sdk="Aspire.AppHost.Sdk/13.6.4">`, tự động kéo theo gói `Aspire.Hosting.AppHost` 13.6.4 và `Aspire.Hosting` 13.6.4.
- `Directory.Packages.props` quản lý 4 gói Aspire phụ thuộc:
  - `Aspire.Hosting.PostgreSQL` (13.6.4)
  - `Aspire.Hosting.Redis` (13.6.4)
  - `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL` (13.6.4)
  - `Aspire.StackExchange.Redis.DistributedCaching` (13.6.4)

Xem [proposal.md](proposal.md) để biết thêm về bối cảnh lỗ hổng bảo mật GHSA-f7vh-h5g3-7jrh.

## Goals / Non-Goals

**Goals:**
- Nâng cấp đồng bộ `Aspire.AppHost.Sdk` và tất cả các gói Aspire lên phiên bản `13.6.5`.
- Giải quyết triệt để cảnh báo lỗ hổng High severity GHSA-f7vh-h5g3-7jrh trong dự án `AppHost`.
- Đảm bảo toàn bộ solution build sạch và không bị ảnh hưởng hành vi thực thi.

**Non-Goals:**
- Không cập nhật các gói bên ngoài Aspire (chẳng hạn như cảnh báo lỗ hổng của `Microsoft.AspNetCore.WebUtilities` 10.0.14) trong phạm vi change này để giữ tính cô lập và an toàn cho từng thay đổi.
- Không thay đổi mã nguồn logic trong `AppHost.cs`, `Program.cs` hay các service khác.

## Decisions

### 1. Đồng bộ toàn bộ phiên bản Aspire lên 13.6.5
- **Quyết định**: Cập nhật cả `Aspire.AppHost.Sdk` trong `AppHost.csproj` và các gói `Aspire.*` trong `Directory.Packages.props` từ `13.6.4` lên `13.6.5`.
- **Lý do**: Các thành phần trong hệ sinh thái Aspire (Hosting, Components, SDK) có sự phụ thuộc chặt chẽ về targets, analyzer và runtime contracts. Việc giữ cùng một phiên bản 13.6.5 giúp tránh xung đột transitive package và lỗi nạp assembly.
- **Phương án thay thế đã cân nhắc**: Chỉ ép transitive dependency `Aspire.Hosting.AppHost` trong `Directory.Packages.props` mà giữ nguyên SDK 13.6.4. Cách này không sạch và có thể gây xung đột MSBuild targets do SDK cung cấp.

### 2. Giữ nguyên mô hình Central Package Management
- **Quyết định**: Tiếp tục khai báo phiên bản tập trung tại `Directory.Packages.props` cho các thư viện Aspire và chỉ định nghĩa phiên bản SDK tại `src/AppHost/AppHost.csproj`.
- **Lý do**: Phù hợp với chuẩn quản lý dependencies hiện tại của repository.

## Risks / Trade-offs

- **[Risk]** Cache NuGet cũ làm trình biên dịch giữ lại gói 13.6.4 hoặc phát sinh cảnh báo lỗi thời.
  - **Mitigation**: Chạy lệnh `dotnet restore --force` sau khi chỉnh sửa file cấu hình.
- **[Risk]** Khác biệt nhỏ về hành vi khởi tạo container giữa các patch versions.
  - **Mitigation**: Chạy `dotnet build` và kiểm tra cấu trúc tài nguyên của AppHost để xác nhận tính ổn định.

## Migration Plan

1. Sửa `src/AppHost/AppHost.csproj`: đổi `Aspire.AppHost.Sdk/13.6.4` sang `Aspire.AppHost.Sdk/13.6.5`.
2. Sửa `Directory.Packages.props`: cập nhật các thuộc tính `Version="13.6.4"` của các gói Aspire thành `Version="13.6.5"`.
3. Chạy `dotnet restore` toàn bộ solution.
4. Chạy `dotnet list package --vulnerable` để xác nhận AppHost không còn dính GHSA-f7vh-h5g3-7jrh.
5. Chạy `dotnet build` để đảm bảo biên dịch thành công 100%.
- **Rollback**: Hoàn tác các thay đổi trên `AppHost.csproj` và `Directory.Packages.props` bằng Git nếu phát sinh lỗi không tương thích.

