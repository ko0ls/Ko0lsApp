name: create-pr
description: >
  /create-pr [compare-branch]: Phan tich git diff giua 2 nhanh,
  de xuat Feature Summary + How to Check, tra ve PR body hoan chinh
  de copy vao GitHub.
allowed-tools: Bash, AskUserQuestion, Read, Grep, Glob
agent: general-purpose
---

# /create-pr — Generate Pull Request Content

## Buoc 1 — Xac dinh compare branch

- Neu co argument dau vao (`$ARGUMENTS`) -> dung lam compare branch
- Neu khong co argument -> chay `git branch --show-current` de lay ten nhanh hien tai

## Buoc 2 — Hoi base branch

Dung `AskUserQuestion` de hoi nguoi dung:

- **dev** (default)
- **master**
- **main**
- **Nhap ten nhanh khac...**

## Buoc 3 — Chay git commands song song

Chay cac lenh git de lay thong tin:

```
git remote get-url origin
git diff {base}..{compare} --stat
git log {base}..{compare} --oneline
git diff {base}..{compare}
```

- `git remote get-url origin` -> lay repo URL de tao GitHub compare URL
- `git diff --stat` -> tong quan cac file thay doi
- `git log --oneline` -> danh sach commit messages
- `git diff` -> chi tiet thay doi (de phan tich noi dung)

## Buoc 4 — Claude de xuat noi dung PR

Dua tren ket qua git diff va log, Claude tu dong de xuat:

```
## Summary
[Feature summary - tom tat tinh nang dua tren files changed + commits]

## How to check
[How to check - huong dan kiem tra dua tren nhung gi thay doi]
```

### Huong dan viet Feature Summary:
- Doc danh sach commit messages de hieu muc dich
- Doc git diff de xem cac file nao duoc sua
- Tom tat thanh 1-3 cau, tap trung vao " cai gi thay doi / cai gi duoc them"
- Khong lien quan den qua trinh (process), chi noi ket qua (outcome)

### Huong dan viet How to check:
- Nhin vao cac file thay doi, neu la:
  - **UI/thay doi hien thi** -> mo app, di den man hinh X, kiem tra Y
  - **API/Logic** -> goi API X, verify tra ve Y
  - **Config** -> kiem tra setting X, verify gia tri Y
  - **Test** -> chay command `dotnet test`, verify pass
- 1-5 buoc ngan gon, co the danh so

## Buoc 5 — User review

Hien thi noi dung de xuat cho nguoi dung:

> **De xuat noi dung PR:**
>
> ### Summary
> [noi dung]
>
> ### How to check
> [noi dung]
>
> **Dong y tra ket qua?**

Dung `AskUserQuestion` voi 2 lua chon:

1. ** Dong y / OK ** -> Buoc 6
2. ** Chinh sua / Edit ** -> User mo ta phan can chinh sua -> Claude sua -> quay lai Buoc 5 (loop cho den khi OK)

## Buoc 6 — Tra ket qua cuoi cung

Sau khi nguoi dung dong y, tra ve:

### GitHub Compare URL
```
https://github.com/{owner}/{repo}/compare/{base}...{compare}
```
(Chep remote URL -> parse owner/repo -> tao URL)

### PR Body (de copy)
```
## Summary
[Feature summary da duoc xac nhan]

## How to check
[How to check da duoc xac nhan]
```

### Ghi chu:
- Tra link GitHub de nguoi dung tu mo va tao PR
- Tra body de nguoi dung copy-paste vao PR body tren web
