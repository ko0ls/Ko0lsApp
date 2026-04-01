---
description: Hiển thị danh sách PR mà mình là Reviewer và trạng thái Approve
allowed-tools: Bash
---

# Danh sách PR cần review

Hiển thị danh sách các PR **open** mà mình được assign làm Reviewer, kèm trạng thái.
PR chưa approve sẽ hiện thị thêm link.

## Điều kiện tiên quyết

- `gh` CLI đã được xác thực

## Quy trình

### 1. Lấy danh sách PR

Chạy **song song** 2 lệnh sau để thu thập các open PR mà mình dùng tham gia review:

```bash
# PR được yêu cầu review (chưa review)
gh pr list --search "review-requested:@me" --state open --json number,title,url --limit 100

# PR đã submit review
gh pr list --search "review-by:@me" -- state open --json number,title,url --limit 100
```

### 2. Tổng hợp kết quả'

Gộp 2 mảng JSON, **loại bỏ trùng lặp theo số PR**.

### 3. Lấy trạng thái review của mình

Đầu tiên lấy username GitHub của mình bằng `gh api user --jq '.login'`.

Với mỗi PR đã tổng hợp, chạy lệnh sau để lấy **trạng thái** review mới nhất của mình**:

```bash
gh api "repos/{owner}/{repo}/pulls/{number}/reviews" \
  --paginate \
  --jq '[.[] | select(.user.login=="MY_LOGIN")] | last | .state // "PENDING"'
```

- `MY_LOGIN` thay bằng username đã lấy
- Nếu chưa có review nào (kết quả trống) thì coi là `"PENDING"`
- Trạng thái `COMMENTED` không phải Approve cũng không phải Changes Requested, nên coi là `⏳️` (cần review)

### 4. Xuất kết quả

Xuất theo format sau, sắp xếp theo số PR tăng dần.

Hiển thị chú thích trước, sau đó là bảng:

```
## Danh sách PR cần review

✅ = Đã Apporve  ⏳️ = Cần Review  🔄 = Yêu cầu thay đổi

| Trạng thái | PR | Tiêu đề |
|------------|----|---------|
| ✅ | #123 | Thêm tính năng A |
| ⏳️ | #456 | Sửa bug B |
| 🔄 | #789 | Refactor C |
```

Quy tắc icon trạng thái (dựa trên trạng thái review của mình):
- `APPROVED` -> `✅`
- 'CHANGES_REQUEST' -> `🔄`
- `COMMENTED` / `PENDING` / `DISMISSED` / trống / null -> `⏳️`

### 5. Hiển thị link PR chưa Approve

Tách hiển thị theo trạng thái "Cần review" và "Đã yêu cầu thay đổi (chờ đối phương xử lý)":

```
---

### PR cần review

- ⏳️ #456 Sửa bug B
  https://github.com/owner/repo/pull/456
- ⏳️ #101 Th năng mới D
  https://github.com/owner/repo/pull/101

### PR đã yêu cầu thay đổi (chờ đối phương xử lý)

- 🔄 #789 Refactor C
  https://github.com/owner/repo/pull/789
```

- **Cần review**: `COMMENTED` / `PENDING` / `DISMISSED` / trống / null (PR chưa Approve)
- **Đã yêu cầu thay đổi**: 'CHANGES_REQUEST' (mình đã yêu cầu thay đổi, chờ đối phương xử lý)
- Nếu section nào không có PR thì bỏ qua section đó
- Nếu tất cả PR đều APPROVED thì hiển thị "Không có PR nào chưa Approve".

## Lưu ý

- Toàn bộ output bằng tiếng Việt có dấu
- Nếu không có PR nào thì hiển thị "Không có PR open nào mà bạn là Reviewer"