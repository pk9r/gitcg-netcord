# Design

## Context

Xem `proposal.md` để biết thêm bối cảnh và động lực. Trong `src/GitcgSkia/ImageCreators/Deck/Genshincards/GenshincardsDeckImageCreator.cs`, phương thức `DrawActionCardContentAsync` kết xuất các chỉ số kỹ năng của thẻ hành động. Khi thẻ có 2 giá trị kỹ năng (`ShouldRenderSecondarySkillValue` trả về `true`), cả hai số cần được vẽ song song trên cùng một đường chân chữ (baseline) ở vị trí Y = `skillValuePosition.Y + 24`, nằm ngay phía trên hai biểu tượng nguyên tố kỹ năng ở Y = `skillValuePosition.Y + 28`.

## Goals / Non-Goals

**Goals:**
- Sửa điểm gốc vẽ của `actionCard.SkillValue` trong nhánh 2 chỉ số kỹ năng: dịch chuyển trục X sang `-11` và trục Y xuống `+24` px (tương đồng với `SkillValue2` có trục X dịch `+9` và trục Y dịch `+24`).
- Thêm test case trong dự án `GitcgSkia.Tests` để xác thực layout Genshincards khi có thẻ bài sở hữu 2 chỉ số kỹ năng kết xuất ổn định.

**Non-Goals:**
- Không thay đổi logic của `ShouldRenderSecondarySkillValue` hay `ShouldRenderSecondarySkillElement`.
- Không thay đổi các creator khác (`SimplestDeckImageCreator`, `GameBackgroundDeckImageCreator`).

## Decisions

### Quyết định 1: Định dạng phép tính tọa độ điểm vẽ

- **Lựa chọn**: Sử dụng `skillValuePosition + new SKSizeI(-11, 24)` (hoặc `new SKPointI(skillValuePosition.X - 11, skillValuePosition.Y + 24)`).
- **Lý do**:
  - `SkillValue2` hiện tại đang dùng `skillValuePosition + new SKSizeI(9, 24)`.
  - Việc dùng phép cộng `+` với vector offset `(-11, 24)` giúp bảo đảm trục Y được cộng đúng `+24` pixel theo baseline của font chữ, tránh lỗi vô tình trừ tọa độ Y như khi dùng toán tử `- new SKSizeI(11, 24)`.
- **Phương án thay thế cân nhắc**:
  - `skillValuePosition - new SKSizeI(11, -24)`: Hoạt động đúng toán học nhưng dễ gây hiểu nhầm khi đọc mã nguồn.

### Quyết định 2: Kiểm thử hồi quy trong GitcgSkia.Tests

- **Lựa chọn**: Bổ sung test class hoặc test method mô phỏng deck chứa thẻ có 2 skill values (ví dụ thẻ có `SkillValue = 1`, `SkillValue2 = 2` và hai element tương ứng) và gọi `GenshincardsDeckImageCreator.CreateImageAsync`.
- **Lý do**: Đảm bảo pipeline render Genshincards hoạt động trơn tru với bộ dữ liệu này và không gặp lỗi đồ họa.

## Risks / Trade-offs

- [Risk]: Sai lệch tọa độ text rendering giữa các platform nếu typeface không đồng nhất.
  - Mitigation: `GenshincardsDeckImageCreator` tải font Roboto nhúng tĩnh thông qua `SkAssetsUtils.LoadRobotoBoldFontAsync()`, bảo đảm kích thước glyph và metrics baseline đồng nhất trên Windows/Linux/macOS.
