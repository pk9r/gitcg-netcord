# Design

## Context

Solution sử dụng mô hình Central Package Management (CPM) thông qua `Directory.Packages.props`.
Gói `Microsoft.AspNetCore.WebUtilities` đang được quản lý tập trung ở phiên bản `10.0.14`, được sử dụng trực tiếp bởi `HoyolabHttpClient` và gián tiếp bởi `GitcgPainter`, `GitcgSharp.Shared`, `GitcgSkia`.
Cùng nhóm này có các thư viện runtime Microsoft 10.0.14:
- `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.Extensions.DependencyInjection.Abstractions`
- `Microsoft.Extensions.Http`

Xem [proposal.md](proposal.md) để biết thêm về động lực bảo mật (GHSA-x33c-w48q-qrgc).

## Goals / Non-Goals

**Goals:**
- Cập nhật `Microsoft.AspNetCore.WebUtilities` lên phiên bản `10.0.15` để xử lý triệt để lỗ hổng High severity GHSA-x33c-w48q-qrgc.
- Đồng bộ các gói phụ thuộc runtime Microsoft 10.0.x trong `Directory.Packages.props` lên `10.0.15`.
- Đảm bảo `dotnet list package --vulnerable --include-transitive` không còn cảnh báo lỗ hổng nào trong toàn bộ solution.
- Đảm bảo solution biên dịch thành công 100% không phát sinh lỗi.

**Non-Goals:**
- Không nâng cấp lên phiên bản 11.0.0 của các gói trên vì target framework hiện tại của các dự án là `net10.0`.
- Không thay đổi mã nguồn logic trong các dự án.

## Decisions

### 1. Nâng cấp lên bản vá 10.0.15 thay vì phiên bản 11.0.0
- **Quyết định**: Chọn phiên bản `10.0.15` cho `Microsoft.AspNetCore.WebUtilities` và các gói liên quan.
- **Lý do**: Đây là bản vá an toàn (security patch release) tương thích trực tiếp với `net10.0`, không có breaking API changes, đồng thời đã khắc phục hoàn toàn lỗ hổng GHSA-x33c-w48q-qrgc.
- **Phương án thay thế đã cân nhắc**: Nâng cấp lên 11.0.0 -> Có thể gây xung đột phụ thuộc hoặc yêu cầu nâng target framework toàn solution.

### 2. Đồng bộ các thư viện Microsoft 10.0.14 trong CPM
- **Quyết định**: Cùng lúc nâng `Microsoft.EntityFrameworkCore.Design`, `Microsoft.Extensions.DependencyInjection.Abstractions`, `Microsoft.Extensions.Http` từ `10.0.14` lên `10.0.15`.
- **Lý do**: Giữ tính nhất quán về phiên bản các thành phần nền tảng của Microsoft trong cùng một solution.

## Risks / Trade-offs

- **[Risk]** Cache NuGet cũ làm sai lệch kết quả kiểm tra.
  - **Mitigation**: Chạy `dotnet restore` và xác nhận lại với `dotnet list package --vulnerable --include-transitive`.

## Migration Plan

1. Cập nhật `Directory.Packages.props` cho các thuộc tính `Version="10.0.14"` sang `Version="10.0.15"`.
2. Khôi phục packages: `dotnet restore`.
3. Kiểm tra danh sách lỗ hổng: `dotnet list package --vulnerable --include-transitive`.
4. Biên dịch: `dotnet build`.
- **Rollback**: Hoàn tác `Directory.Packages.props` qua Git (`git checkout Directory.Packages.props`).

