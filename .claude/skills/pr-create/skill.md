---
description: Tao PR từ branch hiện tại (dùng gh). Base branch mặc định là master, có thể thay đổi qua đối số.
allowed-tools: Bash, Read, Glob, Grep, AskUserQuestion
argument-hint: [base branch (mặc định: master)]
---
# Tạo PR

Tạo PR bằng lệnh `gh`, lấy branch hiện tại làm compare branch.

## Quy trình

### 0. Chọn ngôn ngữ

Sử dụng tool `AskUserQuestion` để hỏi câu hỏi sau và chờ phản hồi:

> **Vui lòng chọn ngôn ngữ / Please select the language:**
> 1. Tiếng Việt (Vietnamese)
> 2. English
>
> * Mặc định là Tiếng Việt / Default is Vietnamese

Nếu người dùng trả lời `2` hoặc `English` hoặc `Tiếng Anh` thì viết message, nội dung PR, xác nhận bằng **Tiếng Anh**.
Các trường hợp còn lại (`1`, `Tiếng Việt`, không trả lời) thì viết review bằng **tiếng Việt**.

Tất cả các bước tiếp theo sẽ thực hiện bằng ngôn ngữ được chọn.

---

### 1. Kiểm tra trạng thái branch hiện tại và remote

```
# Lấy tên branch hiện tại
git rev-parse --abbrev-ref HEAD

# Fetch thông tin mới nhất từ remote
git fetch origin
```

Nếu branch hiện tại trùng với base branch thì hiển thị lỗi và kết thúc:

- LANG=Vietnamese: `Lỗi: Branch hiện tại trùng với base branch.`
- LANG=English: `Error: Current branch is the same as the base branch.`

### 2. Kiểm tra diff với remote

```
git status
git diff HEAD..origin/<tên branch hiện tại> --stat
```

Nếu có thay đổi chưa commit ở local, hoặc có diff với remote branch, hiển thị cảnh báo sau và **kết thúc ngay**:

- LANG=Vietnamese: `Có khác biệt với remote. Vui lóng push trước rồi chạy lại.`
- LANG=English: `There are difference with the remote. Please push first and try again.`

### 3. Xác định base branch

- Nếu `$ARGUMENTS` được chỉ định: dùng giá trị đó làm base branch.
- Nếu `$ARGUMENTS` trống: dùng `master` làm base branch.

### 4. Xác nhận branch

Hiển thị thông tin sau và dùng tool AskUserQuestion để xác nhận với người dùng:

```
base:    <base branch>
compare: <branch hiện tại>
```

- LANG=Vietnamese: "Tiếp tục với các branch này?"
- LANG=English: "Continue with these branches?"

Nếu người dùng từ chối thì kết thúc.

### 5. Tạo nội dung PR

#### 5.1. Xác nhận URL task github:

- LANG=Vietnamese: "Nhập URL task github (bỏ trống nếu không cần)"
- LANG=English: "Enter the github task URL (leave blank if not applicable)"

#### 5.2. Phân tích diff

Phân tích diff giữa base branch và compare branch:

```
# Commit log
git log <base branch>..HEAD --oneline

# Diff
git diff <base branch>..HEAD --stat
git diff <base branch>..HEAD
```

#### 5.3. Tạo nội dung PR

Đọc file `.github/PULL_REQUEST_TEMPLATE.md` bằng tool Read, sử dụng cấu trúc đó để tạo nội dung PR.

- Điền nội dung vào các section/placeholder trong template dựa trên diff, **bằng ngôn ngữ đã chọn**
- Giữ nguyên tiêu đề section (ví dụ `## Feature Summary`)
- Điền URL Github lấy từ bước 5-1 vào dòng `Link to ticket:` (nếu trống thì để trống)
- Không đưa HTML comment (`<!-- ... -->`) vào nội dung

Tiêu đề PR là 1 dòng tóm tắt nội dung thay đổi, viết bằng ngôn ngữ **Tiếng Anh**.

### 6. Xác nận lần cuối

Hiện thị *đầy đủ* tiêu đề và nội dung PR cho người dùng xem, xác nhận không có vấn đề.
Nếu cần sửa thì sửa rồi xác nhận lại.

- LANG=Vietnamese: "Tạo PR với nội dung này?"
- LANG=English: "Create a PR with this content?"

### 7. Tạo PR

```
gh pr create --base <base branch> --title "<tiêu đề>" --body "$cat <<'EOF'
<nội dung>
EOF
)"
```

Sau khi tạo xong, hiển thị URl của PR

### Lưu ý

- Tiêu đề viết bằng tiếng Anh, nội dung PR viết bằng ngôn ngữ đã chọn ở bước 0, nếu là Tiếng Việt (Vietnamse) thì phải có dấu tiếng Việt đầy đủ.
- Nếu `gh pr create` thất bại, hiển thị lỗi và kết thúc.
- Nếu người dùng yêu cầu tạo draft PR, thêm flag `--draft`.

