---
name: analyze-task
description: >
  analyze-task / analyze-task: reads a task description file (.md/.txt) from an input directory,
  analyzes it deeply with Opus 4-6, generates a detailed execution plan,
  asks for user confirmation (execute / edit / stop), then spawns an appropriate
  execution agent (Haiku / Sonnet / Opus based on complexity).
  Only runs when the user explicitly types /phan-tich or /analyze-task.
disable-model-invocation: true
allowed-tools: Read, Glob, Bash, Grep, Agent, AskUserQuestion, TaskOutput, TaskList, TaskGet, WebFetch, WebSearch, Edit, Write
agent: general-purpose
---

# analyze-task — Task Analyzer / Phân tích Công việc

## Mục đích | Purpose

Đọc file mô tả công việc trong thư mục đầu vào, phân tích sâu bằng Opus 4-6,
sinh kế hoạch chi tiết, xin xác nhận từ người dùng, sau đó spawn agent phù hợp để thực thi.

---

## Bước 1 — Xác định file đầu vào | Identify Input File

Liệt kê các file `.md` và `.txt` trong thư mục `$ARGUMENTS`:

```
Glob: {path: "$ARGUMENTS", pattern: "**/*.md"}
Glob: {path: "$ARGUMENTS", pattern: "**/*.txt"}
```

- **1 file duy nhất** → dùng trực tiếp file đó.
- **Nhiều file** → dùng `AskUserQuestion` để hỏi người dùng chọn file.

---

## Bước 2 — Đọc nội dung | Read File Content

`Read` file đã chọn, lưu nội dung để phân tích.

---

## Bước 3 — Phân tích sâu với Opus 4-6 | Deep Analysis (Opus 4-6)

Dựa trên nội dung file, phân tích toàn diện bao gồm:

### 3.1 Tóm tắt công việc | Summary
- Mục tiêu chính của công việc
- Ngữ cảnh và lý do cần thực hiện

### 3.2 Phạm vi | Scope
- **Trong phạm vi (In Scope)**: các thành phần cần làm
- **Ngoài phạm vi (Out of Scope)**: các thành phần KHÔNG làm

### 3.3 Các bước thực hiện chi tiết | Step-by-Step Plan
1. [Bước 1] — Mô tả ngắn gọn
2. [Bước 2] — Mô tả ngắn gọn
3. ...

### 3.4 Ước lượng độ phức tạp | Complexity Assessment
- **Độ phức tạp**: Thấp / Trung bình / Cao
- **Số bước ước tính**: N bước
- **Kỹ năng cần thiết**: [danh sách]

### 3.5 Đề xuất mô hình AI cho execution agent | Model Recommendation
- **Thấp** (task nhỏ, rõ ràng, ít bước) → `haiku`
- **Trung bình** (nhiều bước, cần suy luận) → `sonnet`
- **Cao** (phức tạp, kiến trúc mới, nhiều unknown unknowns) → `opus`

---

## Bước 4 — Trình bày kế hoạch & xin xác nhận | Present Plan & Confirm

Trình bày kế hoạch rõ ràng, sau đó dùng `AskUserQuestion` với 3 lựa chọn:

1. **✅ Thực thi kế hoạch / Execute plan** → chuyển sang Bước 5
2. **✏️  Chỉnh sửa kế hoạch / Edit plan** → yêu cầu user mô tả thay đổi,
   cập nhật kế hoạch → quay lại Bước 4
3. **⏹️  Dừng lại / Stop** → kết thúc, không làm gì

---

## Bước 5 — Spawn Execution Agent | Dispatch Agent

1. Tạo agent mới với model đã đề xuất ở Bước 3.5
2. Truyền cho agent:
   - Nội dung file mô tả công việc
   - Kế hoạch chi tiết đã được xác nhận
   - Hướng dẫn: "Thực hiện đúng kế hoạch. Báo cáo tiến độ và kết quả cuối cùng."
3. Agent được spawn với `run_in_background: true`
4. Khi agent hoàn thành, báo cáo tổng kết cho người dùng

---

## Ghi chú thiết kế | Design Notes

- **Two-phase model strategy**: Opus 4-6 chỉ dùng cho giai đoạn phân tích/sinh kế hoạch.
  Execution agent dùng model phù hợp với độ phức tạp thực tế → tiết kiệm chi phí.
- **Confirmation gate**: Không có bước execution nào xảy ra mà không có
  sự xác nhận rõ ràng từ người dùng.
- **Edit loop**: User có thể chỉnh sửa kế hoạch và xem lại trước khi thực thi.
- **Hỗ trợ song ngữ**: Toàn bộ skill body song ngữ Việt + Anh.
