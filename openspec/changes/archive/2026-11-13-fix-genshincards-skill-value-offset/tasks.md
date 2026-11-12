# Tasks

## 1. Coordinate Offset Fix in GenshincardsDeckImageCreator

- [x] 1.1 Cập nhật tọa độ điểm vẽ của `actionCard.SkillValue` trong nhánh thẻ có 2 skill values tại `src/GitcgSkia/ImageCreators/Deck/Genshincards/GenshincardsDeckImageCreator.cs` thành `skillValuePosition + new SKSizeI(-11, 24)` và xác minh mã nguồn không có lỗi biên dịch.

## 2. Regression Testing & Aspire Run Verification

- [x] 2.1 Viết unit test trong `tests/GitcgSkia.Tests/` để kiểm tra kết xuất layout Genshincards khi có thẻ bài chứa 2 giá trị kỹ năng và chạy `dotnet test tests/GitcgSkia.Tests/GitcgSkia.Tests.csproj` thành công.
- [x] 2.2 Biên dịch toàn bộ solution (`dotnet build`) và khởi chạy lại ứng dụng trên Aspire (`aspire start`) để xác nhận hệ thống hoạt động bình thường.
