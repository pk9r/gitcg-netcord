# Design

## Context

Dự án `GitcgSkia` thực hiện chuyển đổi dữ liệu bộ bài TCG (Genius Invokation) thành hình ảnh PNG thông qua thư viện `SkiaSharp`. Xem `proposal.md` để biết động lực thay đổi.

Hiện tại trong `GitcgSkia`:
- `SimplestDeckImageCreator.cs` đã áp dụng kiến trúc chuẩn: định nghĩa `Utils.DefaultSamplingOptions`, tải trước hình ảnh thẻ bài bất đồng bộ qua `Task.WhenAll`, sau đó vẽ tuần tự lên `SKCanvas`.
- Ba tệp nguồn còn lại vẫn đang sử dụng API cũ của SkiaSharp bị đánh dấu `[Obsolete]` (CS0618):
  - `ImageCacheServiceExtension.cs`: Các hàm mở rộng biên/icon dùng `DrawBitmap` cũ.
  - `GameBackgroundDeckImageCreator.cs`: Dùng `DrawBitmap` và `DrawText` cũ; gọi vẽ trên `canvas` trực tiếp từ các tác vụ chạy đồng thời.
  - `GenshincardsDeckImageCreator.cs`: Dùng `DrawBitmap` và `DrawText` cũ; gọi vẽ trên `canvas` trực tiếp từ các tác vụ chạy đồng thời.

## Goals / Non-Goals

**Goals:**
- Loại bỏ hoàn toàn 18 cảnh báo `CS0618` trong `GitcgSkia`.
- Chuẩn hóa các lời gọi `SKCanvas.DrawBitmap` với `SKSamplingOptions` phù hợp (`Utils.DefaultSamplingOptions`).
- Chuẩn hóa các lời gọi `SKCanvas.DrawText` với `SKTextAlign.Left` để giữ nguyên vị trí tọa độ căn chữ trước đó.
- Khắc phục nguy cơ xung đột đa luồng khi vẽ lên `SKCanvas` bằng cách tách biệt pha Preload (bất đồng bộ song song) và pha Render (tuần tự đơn luồng).
- Giữ nguyên độ nét, màu sắc và bố cục của tất cả các layout hình ảnh xuất ra.

**Non-Goals:**
- Tái cấu trúc toàn bộ kiến trúc từ `SKBitmap` sang `SKImage` (tránh phá vỡ giao diện của `ImageCacheService` và các mô hình chia sẻ).
- Thay đổi `GitcgPainter` (dự án này sử dụng ImageSharp, không phụ thuộc SkiaSharp).
- Thay đổi các API public options như `SimplestDeckImageOptions`, `GameBackgroundDeckImageOptions`, `GenshincardsDeckImageOptions`.

## Decisions

### Quyết định 1: Sử dụng `Utils.DefaultSamplingOptions` cho tất cả các lời gọi `DrawBitmap`
- **Lý do**: `Utils.DefaultSamplingOptions` đã được khai báo sẵn trong `GitcgSkia/Utils.cs` với cấu hình:
  ```csharp
  public static readonly SKSamplingOptions DefaultSamplingOptions = new(
      filter: SKFilterMode.Linear,
      mipmap: SKMipmapMode.Linear
  );
  ```
  Cấu hình này tương đương với bộ lọc song tuyến tính có mipmap (`SKFilterQuality.Medium`), đảm bảo chất lượng hình ảnh sắc nét, mượt mà khi thu phóng thẻ bài mà không gây tốn tài nguyên CPU như bộ lọc cubic (`Mitchell`).
- **Phương án thay thế**: `SKSamplingOptions.Default` (Nearest-neighbor, làm vỡ hạt hình ảnh thu nhỏ).

### Quyết định 2: Chỉ định tường minh `SKTextAlign.Left` cho các lời gọi `DrawText`
- **Lý do**: Lời gọi cũ `SKCanvas.DrawText(string, SKPoint, SKFont, SKPaint)` trước đây ngầm định sử dụng thuộc tính `paint.TextAlign`, mặc định là `SKTextAlign.Left`. Việc truyền trực tiếp `SKTextAlign.Left` vào overload mới:
  ```csharp
  canvas.DrawText(text, p, SKTextAlign.Left, font, paint);
  ```
  sẽ loại bỏ cảnh báo `CS0618` và bảo đảm 100% tọa độ hiển thị tên thẻ, số lượng `×quantity`, điểm kỹ năng và tên tác giả không bị xê dịch.
- **Phương án thay thế**: Đặt `paint.TextAlign = SKTextAlign.Left` nhưng SkiaSharp vẫn yêu cầu overload mới trong các phiên bản cập nhật.

### Quyết định 3: Chuẩn hóa quy trình render theo mô hình Preload -> Render
- **Lý do**: `SKCanvas` trong SkiaSharp duy trì trạng thái ngữ cảnh vẽ (matrix, clipping, paint state) và hoàn toàn **không an toàn khi truy cập đồng thời** (non-thread-safe). Cả `GameBackgroundDeckImageCreator` và `GenshincardsDeckImageCreator` sẽ được tái cấu trúc theo mô hình 2 pha:
  1. **Phase 1 (Preload - Parallel Async)**: Thực hiện tải và xử lý hình ảnh thẻ bài/icon thông qua `imageCacheService` với `Task.WhenAll`. Kết quả trả về danh sách các đối tượng dữ liệu vẽ (đã sẵn sàng `SKBitmap`).
  2. **Phase 2 (Render - Sequential Sync)**: Lặp qua danh sách đã nạp và gọi `canvas.DrawBitmap`, `canvas.DrawText` tuần tự trên luồng chính.
- **Phương án thay thế**: Khóa `lock(canvas)` trong các task con (dễ gây nghẽn hiệu năng và phức tạp hóa luồng async/await).

## Risks / Trade-offs

- **[Quản lý vòng đời bộ nhớ SKBitmap khi Preload]** → *Giảm thiểu*: Các bitmap được tải trong Phase 1 sẽ được giải phóng ngay sau khi render và encode xong bằng `using` hoặc khối dọn dẹp tập trung. Vì số lượng thẻ trong 1 bộ bài TCG cố định (3 thẻ nhân vật, 30 thẻ hành động), lượng bộ nhớ tiêu thụ là hoàn toàn xác định và an toàn.
- **[Tọa độ chữ khi thêm `SKTextAlign.Left`]** → *Giảm thiểu*: Tọa độ trong mã nguồn vốn dĩ đã được tính toán dựa trên căn trái; giữ nguyên tọa độ gốc `p`.

## Migration Plan

1. **`ImageCacheServiceExtension.cs`**:
   - Cập nhật hàm `BorderIcon` truyền `Utils.DefaultSamplingOptions` vào `canvas.DrawBitmap`.
2. **`GameBackgroundDeckImageCreator.cs`**:
   - Cập nhật dòng 36 vẽ background với `Utils.DefaultSamplingOptions`.
   - Cập nhật dòng 132 vẽ tên tác giả với `SKTextAlign.Left`.
   - Chuyển đổi `LoadAndDrawCardAsync`: tách thành nạp bitmap trước (`LoadBorderedIconAsync`), sau đó vẽ tuần tự lên canvas với `Utils.DefaultSamplingOptions`.
3. **`GenshincardsDeckImageCreator.cs`**:
   - Tách pha tải ảnh role/action card song song, sau đó vẽ tuần tự lên canvas.
   - Cập nhật tất cả các vị trí `canvas.DrawBitmap` sang overload nhận `Utils.DefaultSamplingOptions`.
   - Cập nhật tất cả các vị trí `canvas.DrawText` sang overload nhận `SKTextAlign.Left`.
4. **Kiểm tra và xác thực**:
   - Chạy `dotnet build src/GitcgSkia/GitcgSkia.csproj` xác nhận 0 lỗi, 0 cảnh báo CS0618.
   - Chạy `dotnet build` toàn bộ solution.
