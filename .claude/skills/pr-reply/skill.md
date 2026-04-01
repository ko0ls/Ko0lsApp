---
description: Trả lời các review comment trên PR GitHub — đọc, đánh giá từng comment, tạo fix (mỗi comment 1 commit), và đăng reply lên PR.
allowed-tools: Bash, Glob, Grep, Agent, Read, AskUserQuestion
argument-hint: <Số PR>
---

# PR Reply — Trả lời Review Comments

Đọc toàn bộ review comment trên một PR GitHub, đánh giá từng comment, tạo fix (mỗi comment 1 commit riêng), tổng hợp reply, và đăng lên PR.

> **Bỏ qua đối số:** Nếu `$ARGUMENTS` trống, yêu cầu người dùng nhập số PR.

> ## Điều kiện tiên quyết

- `gh` CLI đã xác thực
- Đang ở repository chứa PR cần xử lý (hoặc PR belongs về repo khác — xác định qua `gh pr view`)
- Kết quả phải ghi rõ tên model đã thực hiện đánh giá

---

## Quy trình

### 0. Chọn ngôn ngữ

Sử dụng tool `AskUserQuestion` để hỏi câu hỏi sau và chờ phản hồi:

> **Vui lòng chọn ngôn ngữ / Please select language:**
> 1. Tiếng Việt (Vietnamese)
> 2. English
>
> * Mặc định là Tiếng Việt / Default is Vietnamese

Nếu người dùng trả lời `2` hoặc `English` thì viết bằng **tiếng Anh**.
Các trường hợp còn lại (`1`, `Tiếng Việt`, không trả lời) thì viết bằng **tiếng Việt**.

Tất cả các bước tiếp theo sẽ thực hiện bằng ngôn ngữ được chọn.

---

### 1. Thu thập toàn bộ review comments

Chạy **song song** các lệnh sau:

```bash
# Metadata PR (tiêu đề, URL, head branch, author, trạng thái)
gh pr view $ARGUMENTS --json title,body,url,headRefName,baseRefName,author,state,headRepository

# Tất cả review body (dạng tổng hợp)
gh pr view $ARGUMENTS --json reviews --jq '.reviews[] |
  "=== REVIEW ===\nreviewer: \(.author.login)\nstate: \(.state)\nsubmittedAt: \(.submittedAt)\nbody: \(.body)\n"'

# Tất cả inline comments (comment theo dòng/file cụ thể)
gh api repos/{owner}/{repo}/pulls/$ARGUMENTS/comments \
  --paginate \
  --jq '.[] | "=== COMMENT ===\nid: \(.id)\nauthor: \(.user.login)\nfile: \(.path):\(.line // .original_line)\nbody: \(.body)\n"'
```

> **Lưu ý:** `{owner}/{repo}` lấy từ remote git hiện tại bằng `gh api user --jq '.login'` và `git remote get-url origin`.

> **Nếu `$ARGUMENTS` trống:** Sử dụng `AskUserQuestion` để yêu cầu:
> **Nhập số PR cần xử lý:**

---

### 2. Xác định nguồn review

Phân loại comment theo nguồn:

**Claude / Claude Code:**
- Tên reviewer: `claude`, `claude-code`, `anthropic`, `claude-opus`, `claude-sonnet`
- Hoặc body comment có prefix: `**Model thực hiện review:`, `**Reviewed by:`, `Model:`
- Hoặc có text `[Must]` / `[Want]` trong body (đánh giá kỹ thuật)

**GitHub Copilot:**
- Tên reviewer: `copilot`, `github-copilot`
- Hoặc prefix comment: `Copilot:`, `🤖`

**Reviewer người thật:**
- Các reviewer còn lại (không phải bot)

**CI / Automation:**
- Tên reviewer: `dependabot`, `renovate`, `github-actions`, `codecov`, `coverity-scan`, `lgtm`

---

### 3. Phân loại comment theo tag

Với **mỗi comment**, xác định tag:

1. **Kiểm tra tag rõ ràng (explicit):** tìm `[Must]`, `[Want]`, `[Ask]`, `[FYI]` trong body
2. **Nếu không có tag:** suy luận từ nội dung (infer):
   - Báo lỗi bug / security / arch violation → infer `[Must]`
   - Đề xuất cải thiện code style / perf / readability → infer `[Want]`
   - Hỏi "tại sao lại làm thế này?" / "có cần thiết không?" → infer `[Ask]`
   - Gợi ý tham khảo / link doc / alternative approach → infer `[FYI]`
3. Ghi rõ ràng: tag là **explicit** hay **infer**
4. Ghi nhận nội dung gốc của comment để dùng làm context khi đánh giá

---

### 4. Hiển thị danh sách comment đã phân loại

Xuất bảng theo thứ tự ưu tiên: `[Must]` → `[Want]` → `[Ask]` → `[FYI]`, mỗi reviewer một nhóm.

**Format — Tiếng Việt:**

```markdown
## Review Comments — #{$ARGUMENTS}

Tổng cộng: **{N} comments** | Phân loại: 🔴 {M} Must | 🟡 {W} Want | 🔵 {A} Ask | 🔷 {F} FYI

---

### 🔴 Claude Code — {n} comments

| # | Tag | File | Comment |
|---|-----|------|---------|
| 1 | 🔴 [Must] (explicit) | `src/Foo.cs:42` | "Null reference khi x==null" |
| 2 | 🟡 [Want] (infer) | `src/Bar.cs:10` | "Nên dùng LINQ thay vòng lặp" |

### 🟡 @nam — {n} comments

| # | Tag | File | Comment |
|---|-----|------|---------|
| 3 | 🟡 [Want] (infer) | `svc/Auth.cs:88` | "Có thể dùng const thay vì readonly" |

### 🔵 @long — {n} comments

| # | Tag | File | Comment |
|---|-----|------|---------|
| 4 | 🔵 [Ask] (explicit) | `svc/Auth.cs:95` | "Tại sao không dùng DI?" |
```

**Format — English:**

```markdown
## Review Comments — #{$ARGUMENTS}

Total: **{N} comments** | Breakdown: 🔴 {M} Must | 🟡 {W} Want | 🔵 {A} Ask | 🔷 {F} FYI

---

### 🔴 Claude Code — {n} comments

| # | Tag | File | Comment |
|---|-----|------|---------|
| 1 | 🔴 [Must] (explicit) | `src/Foo.cs:42` | "Null reference when x==null" |
```

---

### 5. Duyệt từng comment — đánh giá + hỏi user

Duyệt **từng comment** theo thứ tự ưu tiên: `[Must]` → `[Want]` → `[Ask]` → `[FYI]`.

#### 5.1. Đọc file liên quan (nếu cần)

Dùng `Read` để xem nội dung code tại vị trí comment được đề cập, giúp đánh giá chính xác.

#### 5.2. Đưa ra đánh giá

Viết đánh giá bằng ngôn ngữ đã chọn:

**Ví dụ — đánh giá đúng:**
```
### Comment #1 — @claude-code
`src/Foo.cs:42`
> "Null reference khi x==null"

**Đánh giá:** ✅ Đúng — Đoạn code không kiểm tra null trước khi gọi `.ToString()`.
**Nên fix:** Có — Bug tiềm ẩn, có thể crash production.
**Điểm chưa hợp lý:** Không có.
```

**Ví dụ — đánh giá có đúng có sai:**
```
### Comment #2 — @nam
`src/Bar.cs:10`
> "Nên dùng LINQ thay vòng lặp"

**Đánh giá:** ⚠️ Có đúng có sai — LINQ đọc hơn nhưng với dữ liệu lớn (100k+ phần tử) vòng for có thể perf tốt hơn.
**Nên fix:** Tùy trường hợp — Cần xem xét context.
**Điểm chưa hợp lý:** Không nêu rõ context để đánh giá.
```

**Ví dụ — đánh giá sai:**
```
### Comment #3 — @copilot
`src/Utils.cs:20`
> "Nên throw exception thay vì return null"

**Đánh giá:** ❌ Chưa chính xác — Trong trường hợp này return null là design hợp lý vì đây là utility method được gọi trong loop, throw exception sẽ làm chậm.
**Nên fix:** Không.
**Điểm chưa hợp lý:** Không xem xét context sử dụng thực tế của method.
```

#### 5.3. Hỏi user quyết định

Dùng `AskUserQuestion` (multiSelect: false) cho **từng comment**:

> **Comment #{n} — @{reviewer}: "{tóm tắt comment}"**
> 1. ✅ Đồng ý fix (tạo commit riêng)
> 2. ✏️ Đồng ý fix nhưng muốn sửa nội dung trước
> 3. 💬 Đồng ý reply nhưng không fix code
> 4. 🚫 Không đồng ý — sẽ trả lời giải thích

**Xử lý theo lựa chọn (mỗi comment chỉ hỏi đúng 1 lần, không hỏi thêm):**

- **Chọn 1 (✅ Fix):** Ghi nhận `FIX_AGREE`. Model tự soạn nội dung fix và commit.
- **Chọn 2 (✏️ Fix với thay đổi):** Ghi nhận `FIX_MODIFIED`. Model tự soạn nội dung fix thay thế, đánh giá lại comment gốc và commit.
- **Chọn 3 (💬 Reply không fix):** Ghi nhận `REPLY_ONLY`. Model tự soạn nội dung reply phù hợp với đánh giá đã đưa ra.
- **Chọn 4 (🚫 Không đồng ý):** Ghi nhận `DISAGREE`. Model tự soạn reply giải thích rõ ràng, lý do không fix.
- **Bỏ qua (không trả lời):** Ghi nhận `SKIPPED`.

#### 5.4. Tạo commit cho comment đồng ý fix (FIX_AGREE / FIX_MODIFIED)

Sau khi ghi nhận quyết định của user cho comment hiện tại, **tạo commit ngay** trước khi chuyển sang comment tiếp theo:

**Kiểm tra branch:**
```bash
# Lấy tên head branch của PR
gh pr view $ARGUMENTS --json headRefName --jq '.headRefName'

# Kiểm tra branch hiện tại
git rev-parse --abbrev-ref HEAD
```

**Nếu chưa đúng PR head branch:**
```bash
git fetch origin
git checkout -b fix/pr{$ARGUMENTS}-comment-{n} origin/{head-ref-name}
```

**Nếu đã ở đúng PR head branch:**
```bash
git checkout -b fix/pr{$ARGUMENTS}-comment-{n}
```

**Áp dụng fix:**
1. Đọc file bằng `Read` tại vị trí comment (`file:line`)
2. Viết fix (hoặc fix thay đổi cho FIX_MODIFIED)
3. Dùng `Edit` để thay đổi code
4. Kiểm tra bằng `git diff` trước khi commit

**Commit:**
```bash
git add .
git commit -m "[fix] Comment #{n} from @{reviewer}: {mô tả ngắn}

Resolved: {tag} comment
Reviewer: @{reviewer}
File: {file}:{line}
PR: #{$ARGUMENTS}
"
```

Sau khi commit xong, **ngay lập tức chuyển sang comment tiếp theo** (bước 5.1). Không dừng lại giữa chừng.

---

### 6. Tổng hợp danh sách trả lời

Sau khi duyệt xong **tất cả** comment, xuất bảng tổng hợp:

**Format — Tiếng Việt:**

```markdown
## Tổng hợp — #{$ARGUMENTS}

| # | Reviewer | Tag | Quyết định | Commit |
|---|----------|-----|------------|--------|
| 1 | @claude-code | 🔴 [Must] | ✅ Fix | `abc1234` |
| 2 | @nam | 🟡 [Want] | 🚫 Không fix — giải thích | — |
| 3 | @copilot | 🟡 [Want] | ✅ Fix | `def5678` |
| 4 | @nam | 🔵 [Ask] | 💬 Trả lời giải thích | — |
| 5 | @long | 🔴 [Must] | ✏️ Fix (custom) | `ghi9012` |

### Commit cần push:
- `abc1234` — `fix/pr{$ARGUMENTS}-comment-1`
- `def5678` — `fix/pr{$ARGUMENTS}-comment-3`
- `ghi9012` — `fix/pr{$ARGUMENTS}-comment-5`

### Reply cần đăng:
- **Comment #2 (@nam):** "Cảm ơn @nam, hiện tại dữ liệu nhỏ nên vòng for vẫn phù hợp về mặt perf..."
- **Comment #4 (@nam):** "Cảm ơn @nam, mình chọn cách này vì nó phù hợp với pattern hiện tại của codebase..."
```

**Format — English:**

```markdown
## Summary — #{$ARGUMENTS}

| # | Reviewer | Tag | Decision | Commit |
|---|----------|-----|----------|--------|
| 1 | @claude-code | 🔴 [Must] | ✅ Fix | `abc1234` |
| 2 | @nam | 🟡 [Want] | 🚫 Disagree — explain | — |
| 3 | @copilot | 🟡 [Want] | ✅ Fix | `def5678` |

### Commits to push:
- `abc1234` — `fix/pr{$ARGUMENTS}-comment-1`
- `def5678` — `fix/pr{$ARGUMENTS}-comment-3`

### Replies to post:
- **Comment #2 (@nam):** "Thanks @nam, the for-loop is more performant for small datasets..."
```

---

### 7. Hỏi user có push không

Dùng `AskUserQuestion`:

> **Bạn muốn làm gì tiếp?**
> 1. ✅ Push lên và đăng reply lên PR
> 2. 🔄 Push lên nhưng chưa đăng reply
> 3. ✏️ Cần cập nhật gì đó trước khi push
> 4. ❌ Chưa push — xem lại hoặc kết thúc

---

#### Chọn 1 — Push + đăng reply:

**7.1. Push các branch fix:**

```bash
# Push tất cả branch fix
git push origin \
  fix/pr{$ARGUMENTS}-comment-1 \
  fix/pr{$ARGUMENTS}-comment-3 \
  fix/pr{$ARGUMENTS}-comment-5
```

**7.2. Tạo PR cho từng fix branch (nếu muốn squash vào PR chính):**

Với mỗi fix branch, tạo PR riêng rồi squash merge:
```bash
gh pr create \
  --base {base-branch} \
  --title "fix: resolve comment #{n} on #{$ARGUMENTS}" \
  --body "Fixes comment #{n} from @{reviewer} on PR #{$ARGUMENTS}"
```

**7.3. Đăng reply lên PR chính:**

Với **comment có reply** (REPLY_ONLY, DISAGREE, FIX_AGREE sau khi đã squash), đăng reply bằng `gh`:

```bash
gh pr comment $ARGUMENTS --body "$(cat <<'EOF'
**Model thực hiện đánh giá:** {tên model}

**Comment #{n}:**
> {nội dung comment gốc}

**Phản hồi:**
{câu trả lời / giải thích}
EOF
)"
```

Hoặc đăng reply trực tiếp trên comment cụ thể bằng comment ID:
```bash
gh api repos/{owner}/{repo}/pulls/comments/{id}/replies \
  --method POST \
  --field body="{nội dung reply}"
```

**7.4. Báo cáo kết quả:**

Hiển thị tóm tắt sau khi push và đăng reply xong:
- Danh sách commit đã push (với hash)
- Danh sách reply đã đăng
- Link PR (nếu tạo PR mới cho fix)

---

#### Chọn 2 — Push nhưng chưa đăng reply:

Push các branch → hỏi tiếp:
> **Đã push xong. Bạn có muốn đăng reply lên PR không?**
> 1. ✅ Có, đăng reply ngay
> 2. ❌ Không, kết thúc

Nếu chọn 1 → thực hiện bước 7.3.
Nếu chọn 2 → kết thúc.

---

#### Chọn 3 — Cần cập nhật gì đó:

Hỏi:
> **Bạn muốn cập nhật gì?**
> 1. Sửa nội dung fix của một comment cụ thể
> 2. Sửa nội dung reply của một comment cụ thể
> 3. Xóa một comment khỏi danh sách cần xử lý
> 4. Quay lại bước 5 (xem lại từng comment)

Thực hiện cập nhật → quay lại bước 7 (hỏi push).

---

#### Chọn 4 — Chưa push:

Hỏi:
> **Bạn muốn làm gì?**
> 1. 🔙 Quay lại bước 5 (xem lại từng comment)
> 2. 📋 Xem lại bảng tổng hợp
> 3. ❌ Kết thúc (chưa push, chưa đăng reply)

---

## Định nghĩa tag bình luận

- **[Must]**: Bắt buộc sửa. Vi phạm kiến trúc, bug, vấn đề bảo mật, v.v.
- **[Want]**: Khuyến nghị cải thiện. Nâng cao tính đọc hiểu, bảo trì, hiệu năng
- **[Ask]**: Câu hỏi hoặc xác nhận. Khi muốn xác nhận ý đồ triển khai
- **[FYI]**: Thông tin tham khảo. Thông tin hữu ích hoặc đề xuất phương án thay thế

---

## Thứ tự ưu tiên xử lý

1. 🔴 `[Must]` — ưu tiên xử lý trước, đánh giá kỹ
2. 🟡 `[Want]` — đánh giá, hỏi user có fix không
3. 🔵 `[Ask]` — trả lời giải thích, có thể không cần fix
4. 🔷 `[FYI]` — đọc để hiểu context, thường không cần action

---

## Lưu ý

- Toàn bộ nội dung đánh giá và reply phải viết bằng **ngôn ngữ đã chọn** (chọn ở bước 0)
- Mỗi comment đồng ý fix **phải tạo commit riêng** để dễ revert nếu cần
- Commit message phải ghi rõ comment số, reviewer, và file để tiện tra cứu
- Nếu comment nằm trên dòng đã bị thay đổi trong commit mới hơn của PR, ghi nhận và thông báo user
- Nếu gặp lỗi khi push hoặc đăng reply, ghi rõ lỗi và tiếp tục với các mục còn lại
- Đối với `[Ask]` — ưu tiên trả lời giải thích hơn là fix, vì đây là câu hỏi chứ không phải yêu cầu
- Đối với `[FYI]` — đọc tham khảo, có thể reply cảm ơn hoặc ghi nhận
