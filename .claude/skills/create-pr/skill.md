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

- [Muc 1 - mo ta thay doi/cong viec]
- [Muc 2 - mo ta thay doi/cong viec]
- [Muc N - mo ta thay doi/cong viec]

## How to check

1. [buoc kiem tra]
2. [buoc kiem tra]
```

### Huong dan viet Feature Summary (danh sach gach dau dong):
- Doc danh sach commit messages de hieu muc dich
- Doc git diff de xem cac file nao duoc them/sua/xoa
- **Moi file hoac nhom file tuong tu** -> 1 gach dau dong rieng
- Format: `- [Ten file/module]: [mo ta ngan thay doi]`
- Khong gop chung nhieu thay doi khac nhau vao 1 dau dong
- Khong lien quan den qua trinh (process), chi noi ket qua (outcome)

**Vi du:**
```
## Summary

- `Canvas/Utils/UtilsPoint.cs`: them cac extension methods cho `Point` — `DistanceTo`, `MidPoint`, `ProjectOnLineBound`, `ProjectOnLineUnbound`, `DistanceToLineBound`
- `Canvas/Utils/UtilsVector.cs`: them cac extension methods cho `Vector` — `IsValid`, `CreateVector`, `GetAngle`, `AngleTo`
- `Canvas/Shapes/ShapeBase.cs`: tao abstract base class cho cac shape, ho tro `IsSelected`, `IsMoveOver`, `MakeHighLight`, `ResetHighLight`
- `Canvas/Shapes/EnumLineType.cs`: tao enum `Solid`, `Dash`, `DashDot`
- `tests/`: them 86 unit tests cho cac class moi
```

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
>
> - [muc 1]
> - [muc 2]
>
> ### How to check
>
> 1. [buoc kiem tra]
> 2. [buoc kiem tra]
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

- [muc 1]
- [muc 2]

## How to check

1. [buoc kiem tra]
2. [buoc kiem tra]
```

### Ghi chu:
- Tra link GitHub de nguoi dung tu mo va tao PR
- Tra body de nguoi dung copy-paste vao PR body tren web
