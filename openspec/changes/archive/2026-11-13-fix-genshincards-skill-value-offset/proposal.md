# Proposal

## Why

Khi kết xuất ảnh bộ bài theo định dạng Genshincards bằng SkiaSharp (`GenshincardsDeckImageCreator`), các thẻ hành động có 2 chỉ số kỹ năng (`SkillValue` và `SkillValue2`) gặp lỗi định vị: chỉ số thứ nhất bị lệch lên trên 48 pixel. Nguyên nhân do phép tính `skillValuePosition - new SKSizeI(11, 24)` vô tình trừ cả tọa độ Y thay vì cộng 24 pixel theo đường baseline của SkiaSharp. Cần khắc phục lỗi này để hai chỉ số kỹ năng hiển thị thẳng hàng và khớp với các biểu tượng nguyên tố kỹ năng phía dưới.

## What Changes

- Sửa phép tính tọa độ điểm vẽ của chỉ số kỹ năng thứ nhất (`actionCard.SkillValue`) trong nhánh thẻ có 2 chỉ số tại `GenshincardsDeckImageCreator.cs` thành `skillValuePosition + new SKSizeI(-11, 24)`.
- Bổ sung unit test xác thực kết xuất layout Genshincards khi có thẻ bài sở hữu 2 chỉ số kỹ năng để ngăn ngừa lỗi hồi quy (regression).

## Capabilities

### New Capabilities

*(Không có)*

### Modified Capabilities

- `deck-image-rendering`: Bổ sung yêu cầu về kết xuất thẻ hành động có 2 chỉ số kỹ năng trong layout Genshincards, đảm bảo cả hai chỉ số được vẽ chính xác theo baseline và thẳng hàng với biểu tượng nguyên tố kỹ năng.

## Impact

- Mã nguồn bị ảnh hưởng: `src/GitcgSkia/ImageCreators/Deck/Genshincards/GenshincardsDeckImageCreator.cs`.
- Tests: Thêm test case kiểm tra Genshincards rendering trong `tests/GitcgSkia.Tests/`.
- Không ảnh hưởng đến các layout khác (`Simplest`, `GameBackground`) và không thay đổi public API.
