# Proposal

## Why

Khi biên dịch giải pháp (solution), dự án `GitcgSkia` phát sinh 18 cảnh báo biên dịch `CS0618` do sử dụng các hàm đã lỗi thời (obsolete) của thư viện `SkiaSharp` (phiên bản `4.154.0`):
- `SKCanvas.DrawBitmap` các overload cũ không có tham số lấy mẫu hình ảnh (`SKSamplingOptions`), vốn trước đây dựa vào thuộc tính ngầm định `SKPaint.FilterQuality`.
- `SKCanvas.DrawText` overload cũ thiếu tham số căn lề văn bản tường minh (`SKTextAlign`).

Ngoài ra, qua khảo sát mã nguồn, việc vẽ ảnh trong `GameBackgroundDeckImageCreator` và `GenshincardsDeckImageCreator` đang gọi đồng thời các phương thức vẽ trên cùng một đối tượng `SKCanvas` từ nhiều tác vụ bất đồng bộ (`Task.WhenAll`), gây nguy cơ tranh chấp tài nguyên (race conditions / non-thread-safe) trong SkiaSharp. Cần hiện đại hóa các API SkiaSharp và chuẩn hóa mô hình render (tải trước tài nguyên song song, vẽ tuần tự như `SimplestDeckImageCreator`) để loại bỏ hoàn toàn các cảnh báo `CS0618` và nâng cao tính ổn định của hệ thống.

## What Changes

- Cập nhật tất cả các lời gọi `SKCanvas.DrawBitmap` trong `GitcgSkia` sang overload hiện đại nhận tham số `SKSamplingOptions` (sử dụng `Utils.DefaultSamplingOptions`).
- Cập nhật tất cả các lời gọi `SKCanvas.DrawText` trong `GitcgSkia` sang overload tường minh có tham số `SKTextAlign` (sử dụng `SKTextAlign.Left` phù hợp với hành vi mặc định trước đó).
- Tách biệt rõ ràng pha tải tài nguyên ảnh bất đồng bộ (Preload phase) và pha vẽ canvas (Render phase) trong `GameBackgroundDeckImageCreator` và `GenshincardsDeckImageCreator`, đảm bảo an toàn đơn luồng khi thao tác trên `SKCanvas`.
- Xác thực giải pháp sau khi thay đổi bằng `dotnet build` và kiểm tra kết quả render hình ảnh để đảm bảo không còn cảnh báo `CS0618` và không làm thay đổi chất lượng/bố cục hình ảnh xuất ra.

## Capabilities

### New Capabilities
None.

### Modified Capabilities
None.

## Impact

- **Mã nguồn bị ảnh hưởng**:
  - `src/GitcgSkia/Extensions/ImageCacheServiceExtension.cs`
  - `src/GitcgSkia/ImageCreators/Deck/GameBackground/GameBackgroundDeckImageCreator.cs`
  - `src/GitcgSkia/ImageCreators/Deck/Genshincards/GenshincardsDeckImageCreator.cs`
- **APIs & Dependencies**: Không thay đổi phiên bản package hay interface bên ngoài (`IDeckImageCreationService`, `IDeckImageCreator`).
- **Chất lượng & Khả năng tương thích**: Triệt tiêu 18 cảnh báo biên dịch CS0618, giữ nguyên bố cục và chất lượng đồ họa thẻ bài.
