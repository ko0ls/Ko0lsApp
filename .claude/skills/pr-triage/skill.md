---
description: Triage nhiều PR GitHub, thu thập review từ mọi nguồn và đề xuất phương hướng xử lý. Chỉ định số PR làm đối số.
allowed-tools: Bash, Glob, Grep, Agent, Read, AskUserQuestion
argument-hint: <Số PR>
---

# PR Triage

Triage toàn diện nhiều PR GitHub — thu thập review từ mọi nguồn (Claude, Copilot, người thật, CI/bot) và đề xuất phương hướng xử lý.

> **Bỏ qua đối số:** Nếu `$ARGUMENTS` trống, yêu cầu người dùng nhập danh sách PR.

> ## Điều kiện tiên quyết

- `gh` CLI đã xác thực
- Kết quả triage phải ghi rõ tên model đã thực hiện

---

## Quy trình

### 0. Chọn ngôn ngữ

Sử dụng tool `AskUserQuestion` để hỏi câu hỏi sau và chờ phản hồi:

> **Vui lòng chọn ngôn ngữ triage / Please select triage language:**
> 1. Tiếng Việt (Vietnamese)
> 2. English
>
> * Mặc định là Tiếng Việt / Default is Vietnamese

Nếu người dùng trả lời `2` hoặc `English` thì viết triage bằng **tiếng Anh**.
Các trường hợp còn lại (`1`, `Tiếng Việt`, không trả lời) thì viết triage bằng **tiếng Việt**.

Tất cả các bước tiếp theo sẽ thực hiện bằng ngôn ngữ được chọn.

---

### 1. Parse danh sách số PR từ đối số

`$ARGUMENTS` có thể có các định dạng: `#123`, `123`, `123, 456, 789`, `123 456 789`, `#123,456`, v.v.

1. Loại bỏ tất cả `#`, dấu phẩy, khoảng trắng thừa bằng regex: `tr -s ' ,#' '\n'`
2. Tách thành mảng số PR (chỉ giữ các chữ số)
3. Loại bỏ giá trị trùng lặp

**Nếu `$ARGUMENTS` trống:**
Sử dụng `AskUserQuestion` để yêu cầu:

> **Nhập danh sách số PR (cách nhau bằng dấu phẩy hoặc khoảng trắng):**

**Giới hạn:** tối đa **10 PR** một lần gọi. Nếu hơn 10, cảnh báo và chỉ xử lý 10 PR đầu.

---

### 2. Thu thập metadata cho tất cả PR (song song)

Với mỗi PR, chạy **song song** các lệnh sau:

```bash
# Metadata đầy đủ (tiêu đề, mô tả, danh sách file thay đổi)
gh pr view {pr} --json title,body,url,reviews,comments,files

# Toàn bộ diff
gh pr diff {pr}

# Tất cả reviews (Claude, Copilot, người)
gh api repos/{owner}/{repo}/pulls/{pr}/reviews --paginate

# Tất cả review comments (comment theo file do Copilot v.v. đăng)
gh api repos/{owner}/{repo}/pulls/{pr}/comments --jq '.[] | "---\nauthor: \(.user.login)\nfile: \(.path):\(.line // .original_line)\nbody: \(.body)\n"'

# Trạng thái CI/checks
gh pr view {pr} --json statusCheckRollup --jq '[.statusCheckRollup.nodes[] | {name: .name, conclusion: .conclusion, status: .status}]'
```

> **Lưu ý:** `{owner}/{repo}` lấy từ remote git hiện tại bằng `gh api user` và `git remote`.

Nếu repo không phải là repo hiện tại (người dùng chỉ định PR từ repo khác), trước tiên xác định repo bằng:
```bash
gh pr view {pr} --json headRepository --jq '.headRepository.fullName // "OWNER/REPO"'
```

---

### 3. Xác định nguồn review

Phân loại các review đã có theo nguồn:

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

Phân loại các chỉ ra của mỗi review theo tag `[Must]` / `[Want]` / `[Ásk]` / `[FYI]`. Nếu tag không được ghi rõ thì suy luận từ nội dung.

---

### 4. Triage từng PR

Với mỗi PR, phân tích và phân loại:

#### 4.1. Trạng thái tổng thể

Tính toán từ tất cả review và CI:

| Trạng thái | Định nghĩa |
|------------|------------|
| 🔴 BLOCKED | Có `[Must]` chưa resolve HOẶC CI fail |
| 🟡 NEEDS_ATTENTION | Có `[Want]` / `[Ask]` chưa resolve, không có Must unresolved |
| 🟢 READY_TO_MERGE | Không có issue nghiêm trọng, CI pass, review APPROVED |

#### 4.2. Phân loại comment theo tag

Tìm kiếm các tag trong review body và review comments:

- **[Must]** — bắt buộc sửa. Vi phạm kiến trúc, bug, vấn đề bảo mật, v.v.
- **[Want]** — khuyến nghị cải thiện. Nâng cao tính đọc hiểu, bảo trì, hiệu năng
- **[Ask]** — câu hỏi hoặc xác nhận ý đồ triển khai
- **[FYI]** — thông tin tham khảo hoặc đề xuất phương án thay thế

#### 4.3. Phát hiện xung đột giữa các nguồn review

- Nếu Claude có `[Must]` nhưng reviewer người đã APPROVE → flag "⚠️ Conflict review"
- Nếu Copilot có feedback nhưng Claude không có → ghi nhận bổ sung
- Nếu có nhiều reviewer người có trạng thái khác nhau → ghi nhận

#### 4.4. Đếm số lượng

Với mỗi PR, đếm:
- Số `[Must]` unresolved / resolved
- Số `[Want]` unresolved / resolved
- Số `[Ask]` unresolved
- Số `[FYI]`
- Trạng thái CI (pass/fail/pending)
- Trạng thái review mỗi nguồn (OK / unresolved / not-reviewed)

---

### 5. Đọc hướng dẫn triage của dự án (nếu có)

Đọc file `github/copilot-instructions.md` (bằng `Read`) để nắm các tiêu chí triage riêng của dự án, áp dụng bổ sung nếu có.

---

### 6. Tổng hợp và xuất báo cáo triage

**Output format — Tiếng Việt:**

```markdown
## PR Triage Report

**Số PR đã triage:** {N} | **Thời gian:** {timestamp}

---

### 🔴 BLOCKED — {count}

| PR | Người review | Must | Want | CI | Liên kết |
|----|--------------|------|------|----|----------|
| #123 | Claude ❌, @nam ⏳ | 2 | 1 | ❌ | [link] |
| #456 | @long ✅ | 1 | 0 | ❌ | [link] |

**Chi tiết #123:**
- 🔴 [Must] `src/Foo.cs:42` — Null reference khi x==null
- 🟡 [Want] `src/Bar.cs:10` — Có thể dùng LINQ thay vòng lặp
- ⚠️ Claude ❌ 2 Must unresolved | @nam ⏳ chưa review

**Chi tiết #456:**
- 🔴 [Must] `svc/Auth.cs:88` — SQL injection
- ⚠️ @long ✅ đã approve nhưng vẫn có 1 Must unresolved

---

### 🟡 NEEDS_ATTENTION — {count}

...

### 🟢 READY_TO_MERGE — {count}

...

---

### 📊 Tổng kết

| Trạng thái | Số PR |
|------------|-------|
| 🔴 BLOCKED | N |
| 🟡 NEEDS_ATTENTION | N |
| 🟢 READY_TO_MERGE | N |

| Nguồn review | Số PR có review |
|-------------|----------------|
| Claude | N |
| Copilot | N |
| Người | N |

| CI | Số PR |
|----|-------|
| ✅ Pass | N |
| ❌ Fail | N |
| ⏳ Pending | N |

---

### Hành động đề xuất

- 🟢 **{N} PR** có thể merge ngay
- 🟡 **{N} PR** cần resolve **{M} comment** trước khi merge
- 🔴 **{N} PR** bị blocked — cần tác giả xử lý trước
```

**Output format — English:**

```markdown
## PR Triage Report

**PRs triaged:** {N} | **Time:** {timestamp}

---

### 🔴 BLOCKED — {count}

| PR | Reviewers | Must | Want | CI | Link |
|----|----------|------|------|----|------|
| #123 | Claude ❌, @nam ⏳ | 2 | 1 | ❌ | [link] |
| #456 | @long ✅ | 1 | 0 | ❌ | [link] |

**Detail #123:**
- 🔴 [Must] `src/Foo.cs:42` — Null reference when x==null
- 🟡 [Want] `src/Bar.cs:10` — LINQ could replace the loop
- ⚠️ Claude ❌ 2 Must unresolved | @nam ⏳ not reviewed

---

### 🟡 NEEDS_ATTENTION — {count}

...

### 🟢 READY_TO_MERGE — {count}

...

---

### 📊 Summary

| Status | Count |
|--------|-------|
| 🔴 BLOCKED | N |
| 🟡 NEEDS_ATTENTION | N |
| 🟢 READY_TO_MERGE | N |

| Review source | PRs with review |
|---------------|-----------------|
| Claude | N |
| Copilot | N |
| Human | N |

| CI | Count |
|----|-------|
| ✅ Pass | N |
| ❌ Fail | N |
| ⏳ Pending | N |

---

### Recommended Actions

- 🟢 **{N} PR(s)** ready to merge
- 🟡 **{N} PR(s)** need **{M} comment(s)** resolved before merge
- 🔴 **{N} PR(s)** blocked — author needs to address first
```

---

### 7. Hỏi người dùng hành động tiếp theo

Sau khi xuất báo cáo, sử dụng `AskUserQuestion` để hỏi:

> **Bạn muốn làm gì tiếp?**
> 1. Đăng báo cáo triage lên tất cả PR (bằng comment)
> 2. Đăng báo cáo triage lên một số PR cụ thể
> 3. Chỉ hiển thị báo cáo (không đăng lên GitHub)
> 4. Triage lại (refresh dữ liệu mới nhất từ GitHub)

**Nếu chọn 1:**
Duyệt tất cả PR trong danh sách, đăng báo cáo triage của từng PR bằng:

```bash
gh pr comment {pr} --body "$(cat <<'EOF'
## PR Triage Report (auto-generated)

{tùy vào ngôn ngữ đã chọn — nội dung triage của PR này}
EOF
)"
```

**Nếu chọn 2:**
Hỏi tiếp: "Nhập số PR cần đăng (cách nhau bằng dấu phẩy):" rồi chỉ đăng lên các PR đó.

**Nếu chọn 3:**
Kết thúc. Hiển thị lại tất cả PR links để người dùng tham khảo.

**Nếu chọn 4:**
Quay lại bước 2, thu thập lại metadata từ GitHub (refresh).

---

## Định nghĩa tag bình luận / Comment Tag Definitions

- **[Must]**: Bắt buộc sửa. Vi phạm kiến trúc, bug, vấn đề bảo mật, v.v. / Require fix: architecture violations, bugs, security issues, etc.
- **[Want]**: Cải thiện khuyến nghị. Nâng cao tính đọc hiểu, bảo trì, hiệu năng / Recommended improvement: readability, maintainability, performance
- **[Ask]**: Câu hỏi hoặc xác nhận. Khi muốn xác nhận ý đồ triển khai / Question or clarification: intent verification
- **[FYI]**: Thông tin tham khảo. Thông tin hữu ích hoặc đề xuất phương án thay thế / Informational: useful context or alternative approaches

---

## Thứ tự ưu tiên xử lý

Khi xử lý nhiều PR, ưu tiên theo thứ tự:
1. 🔴 BLOCKED PR — giải quyết Must trước
2. 🟡 NEEDS_ATTENTION PR — giải quyết Want/Ask
3. 🟢 READY_TO_MERGE PR — kiểm tra cuối cùng rồi merge

---

## Lưu ý

- Toàn bộ nội dung triage phải viết bằng **ngôn ngữ đã chọn** (chọn ở bước 0)
- Nếu PR nào không có review nào từ người, ghi rõ "Chưa có review nào từ người"
- Nếu diff lớn, chỉ hiển thị tên file và số lượng thay đổi, không hiển thị full diff trong triage report
- Nếu gặp lỗi khi gọi `gh`, ghi rõ PR đó và tiếp tục với PR còn lại
- Nếu repo không có remote hoặc không xác định được repo, yêu cầu người dùng cung cấp repo thủ công
