---
description: Thực hiện code review cho Github PR và đăng kết quả lên trang PR. Chỉ định số PR làm đối số.
allowed-tools: Bash, GLod, Grep, Read, AskUserQuestion
argument-hint:  <Số PR>
---

# Review PR & đăng lên Github

Thực hiện code review cho Github PR được chỉ định và đăng kết quả dưới dạng comment lên trang PR.

> ** Bỏ qua đối số:** Nếu `$ARGUMENTS` trống, sẽ review PR gắn với branch hiện tại.

> ## Điều kiện tiên quyết

- `gh` CLI ã ược xác thực
- Kết quả review phải ghi rõ tên model đã thực hiện review

## Quy trình

### 0. Chọn ngôn ngữ

Sử dụng tool `AskUserQuestion` để hỏi câu hỏi sau và chờ phản hồi:

> **Vui lòng chọn ngôn ngữ review / Please select the review language:**
> 1. Tiếng Việt (Vietnamese)
> 2. English
>
> * Mặc định là Tiếng Việt / Default is Vietnamese

Nếu người dùng trả lời `2` hoặc `English` thì viết review bằng **tiếng Anh**.
Các trường hợp còn lại (`1`, `Tiếng Việt`, không trả lời) thì viết review bằng **tiếng Việt**.

Tất cả các bước tiếp theo sẽ thực hiện bằng ngôn ngữ được chọn.

---

### 1. Đọc hướng dẫn review

Đọc file `github/copilot-instruction.md` để nắm các tiêu chí review riêng của dự án.

### 2. Lấy thông tin PR

Chạy **song song** các lệnh sau để nắm toàn cảnh PR:

```bash
# Thông tin meta PR (tiêu đề, mô tả, danh sách file thay đổi, số dòng thêm/xoá)
gh pr review $ARGUMENTS --json title,body,headRefName,baseRefName,url,author,state,additions,deletions,changedFiles,files

# Toàn bộ diff của PR
gh PR diff $ARGUMENTS

# Danh sách commit của PR
gh pr view $ARGUMENTS --json commits --jq '.commits[] | "\(.oid[:7]) \(.messageHeadline"'

# Kiểm tra review comment đã có (tránh trùng lặp)
gh pr view $ARGUMENTS --json reviews --jq '.review[] | "\(.author.login): \(.state) - \(.body[:100])"'
gh api repos/{owner}/{repos}/pulls/$ARGUMENTS/comments --jq '.[] | "\(.user.login): \(.path):\(.line // . original_line) - \(.body[:100])"'
```

### 3. Khảo sát chuyên sâu code

Đối với những chỗ không thể đánh giá chỉ từ diff, khảo sát thêm trong repository theo các góc độ:

- Nơi sử dụng class/interface đã thay đổi (dùng Grep để kiểm tra tham chiếu)
- Field/method mới thêm có thự sự được sử dụng không
- Core bị xóa có còn được tham chiếu từ nơi khác không
- Tính phù hợp của access modifier (`public` có thực sự cần thiết không)

### 4. Các góc độ review

Ngoài checklist trong `github/copilot-instructions.md`, kiểm tra thêm các mục sau:

#### Kiến trúc & Thiết kế
- Quan hệ phụ thuộc giữa các tầng (chỉ từ ngoài -> trong) có được tuân thủ không
- Tách biệt interface, đơn trách nhiệm, dependency injection có phù hợp không

#### Ràng buộc Autocad API
- Quản lý transaction, an toàn luồng, tương thích phiên bản

#### Chất lượng code
- Có còn dead code (code không dùng) không
- Tính nhất quán của comment đánh số và tài liệu
- Có thể xử lý lỗi không
- Tính đối xứng của đăng ký/hủy đăng ký event handler

#### Test & Chất lượng
- Có thêm test tương ứng cho tính năng mới không
- Có đảm bảo khả năng test (testability) không

#### Đa ngôn ngữ & UX
- Message hiển thị cho người dùng có được định nghĩa trong file `.resx` không

### 5. Cấu trúc và kết quả review

Tùy theo ngôn ngữ đã chọn, sử dụng template tương ứng.

#### Template Tiếng Việt

```markdown
## Code Review

**Model thực hiện review: {tên model}**

{Đánh giá tổng quan 1-2 câu}

---

### [Must] {Tiêu đề} - `{tên file}`

{Mô tả vấn đề}

```{ngôn ngữ}
{Trích dẫn code liên quan}
```

{Đề xuất sửa}

{Đường dẫn file và số dòng}

---

### [Want] {Tiêu đề} - `{tên file}`

...

---

### [FYI] {Tiêu đề}

...

---

### Tổng kết

| Phân loại | Số lượng |
|-----------|----------|
| Must      | N |
| Want      | N |
| Ask       | N |
| FYI       | N |

{Nhận xét tổng kết}
```

#### Englist Template

```markdown
## Code Review

**Reviewed by: {model name}**

{Overall assessment in 1-2 sentences}

---

### [Must] {Title} - `{filename}`

{Description of the issue}

```{language}
{Relevant code quote}
```

{Suggested fix}

{File path and line number}

---

### [Want] {Title} - `{filenam}`

...

---

### [FYI] {Title}

...

---

### Summary

| Category | Count |
|----------|-------|
| Must     | N     |
| Want     | N     |
| Ask      | N     |
| FYI      | N     |

{Closing remarks}
```

### Định nghĩa tag comment / Comment Tag Definitions
- **[Must]**: Bắt buộc sửa. Vi phạm kiến trúc, bug, vấn đề bảo mật, v.v. / Require fix: architecture violations, bugs, security issues, etc.
- **[Want]**: Cải thiện khuyến nghị. Nâng cao tính đọc hiểu, bảo trì, hiệu năng / Recommended improvement: readability, maintainability, performance
- **[Ask]**: Câu học hoặc xác nhận. Khi muốn xác nhận ý đồ triển khai / Question or clarification: intent verification
- **[FYI]**: Thông tin tham khảo. Thông tin hữu ích hoặc đề xuất phương án thay thế / Informational: useful context or alternative approaches

### 6. Đăng lên GitHub

Sử dụng lệnh `gh pr review` để đăng comment review. Dùng HEREDOC để truyền nội dung:

```bash
gh pr review $ARGUMENTS --body "$(cat <<'EOF'
{Nội dung review / review content}
EOF
)" --comment
```

### 7. Báo cáo sau khi đăng

Sau khi đăng xong, báo cáo cho người dùng bằng ngôn ngữ đã được chọn: tóm tắt review (số lượng theo phân loại và các chỉ ra chính).

## Lưu ý

- Toàn bộ nội dung review phải viết bằng **ngôn ngữ đã chọn** (chọn ở bước 0)
- Các chier ra phải bao gồm trích dẫn code và đường dẫn file: số dòng
- Nếu diff lớn, sử dụng Read/Grep song song để khảo sát hiệu quả
- Nếu đã có review trước đó, tránh trùng lặp
- Nếu không có vấn đề gì, ghi rõ "Không có vấn đề cần chỉ ra" (Tiếng Việt) hoặc "No issues found" (English)
