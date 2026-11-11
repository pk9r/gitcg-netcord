# Tasks

## 1. ImageCacheServiceExtension Modernization

- [x] 1.1 Cập nhật phương thức `BorderIcon` trong `src/GitcgSkia/Extensions/ImageCacheServiceExtension.cs` để truyền `Utils.DefaultSamplingOptions` vào `canvas.DrawBitmap` cho cả `iconResized` và `border`. Xác thực bằng `dotnet build src/GitcgSkia/GitcgSkia.csproj` rằng 2 cảnh báo CS0618 tại file này được giải quyết.

## 2. GameBackgroundDeckImageCreator Modernization & Concurrency Safety

- [x] 2.1 Cập nhật vẽ ảnh nền (`background`) với `Utils.DefaultSamplingOptions` và vẽ tên tác giả (`DrawText`) với `SKTextAlign.Left` trong `src/GitcgSkia/ImageCreators/Deck/GameBackground/GameBackgroundDeckImageCreator.cs`.
- [x] 2.2 Tái cấu trúc quy trình render trong `GameBackgroundDeckImageCreator.cs`: tải trước các bitmap thẻ nhân vật và thẻ hành động song song bằng `Task.WhenAll`, sau đó duyệt vẽ tuần tự lên `canvas` sử dụng `DrawBitmap` kèm `Utils.DefaultSamplingOptions`. Xác thực rằng không còn cảnh báo CS0618 nào trong file này.

## 3. GenshincardsDeckImageCreator Modernization & Concurrency Safety

- [x] 3.1 Tái cấu trúc quy trình render trong `src/GitcgSkia/ImageCreators/Deck/Genshincards/GenshincardsDeckImageCreator.cs` tách biệt pha tải trước tài nguyên (role cards, action cards, element icons) bằng `Task.WhenAll`, sau đó vẽ tuần tự lên `canvas`.
- [x] 3.2 Cập nhật toàn bộ các lời gọi `SKCanvas.DrawBitmap` trong `GenshincardsDeckImageCreator.cs` (thẻ bài, nhãn tên thẻ, biểu tượng nguyên tố kỹ năng) sang overload nhận `Utils.DefaultSamplingOptions`.
- [x] 3.3 Cập nhật toàn bộ các lời gọi `SKCanvas.DrawText` trong `GenshincardsDeckImageCreator.cs` (tên thẻ bài, chỉ số kỹ năng `SkillValue`/`SkillValue2`, số lượng thẻ `×quantity`) sang overload nhận `SKTextAlign.Left`. Xác thực rằng không còn cảnh báo CS0618 nào trong file này.

## 4. Verification & Validation

- [x] 4.1 Biên dịch riêng dự án `GitcgSkia` (`dotnet build src/GitcgSkia/GitcgSkia.csproj`) và kiểm tra rằng 0 cảnh báo CS0618 được sinh ra từ SkiaSharp.
- [x] 4.2 Biên dịch toàn bộ solution (`dotnet build`) và chạy kiểm thử (`dotnet test`) để đảm bảo không phát sinh lỗi biên dịch hay hồi quy logic.

## Workflow follow-up

- Lưu trữ (archive) change sau khi quá trình áp dụng (apply) và kiểm thử hoàn tất thành công.
