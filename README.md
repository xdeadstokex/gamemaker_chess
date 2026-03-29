# ♟️ Chess Evolution:

Dự án biến thể Cờ Vua với cơ chế tích lũy điểm số và hệ thống tiến hóa kỹ năng.

---
## Phân loại cấp bậc và điểm
Tốt(1): Quân nhẹ
Tượng(3), Mã(3), Xe(5): Quân nặng
Hậu(9), Vua(0): Quân triều đình

### ⚔️ Quy tắc hấp thụ (Absorption Rule)
Khi quân **A** ăn quân **B**, quân **A** sẽ cộng dồn toàn bộ số điểm hiện tại của quân **B** vào điểm của chính nó.
> **Ví dụ:** Xe(5) ăn Mã(3) $\rightarrow$ Xe(8).

---

## ⚡ Hệ Thống Tiến Hóa (Evolution Mechanics)

Mỗi quân cờ sẽ mở khóa khả năng mới khi đạt đủ điều kiện nhất định:

### 1. Tốt (Pawn)
* **Điều kiện:** Ăn được 1 **Quân nặng**.
* **Khả năng:** Sở hữu khả năng di chuyển của chính nó và quân nặng mà nó vừa ăn.
* **Lưu ý:** Vẫn được tính là quân nhẹ. Nếu ăn thêm quân nặng khác sau khi đã tiến hóa, Tốt sẽ không được cộng thêm khả năng di chuyển mới.

* **Phong cấp:** Khi đến ô cuối cùng, có thể tăng cấp thành mọi quân Tiến hóa(trừ Vua). Tốt Tiến hóa **không** có khả năng **Phong cấp**.

### 2. Tượng (Bishop)
* **Điều kiện:** Tích lũy được **5 điểm** trở lên. (Tức cần thêm 2 điểm)
* **Khả năng:** Có thể di chuyển sang các ô **khác màu** (chỉ áp dụng cho các ô nằm cạnh nó).

### 3. Mã (Knight)
* **Điều kiện:** Tích lũy được **5 điểm** trở lên. (Tức cần thêm 2 điểm)
* **Khả năng [Euclid Jump]:** Có thể di chuyển tới bất kỳ ô nào cách nó 2 đơn vị (tính theo phần nguyên của khoảng cách Euclid). Tạo thành một vùng di chuyển hình tròn bán kính 2 quanh vị trí hiện tại.

### 4. Xe (Rook)
* **Điều kiện:** *(Đang cập nhật)*
* **Khả năng:** *(Đang cập nhật)*

### 5. Hậu (Queen)
* **Điều kiện:** Tích lũy được **15 điểm** trở lên. (Tức cần thêm 6 điểm)
* **Khả năng [Cải tử hoàn đồng]:** Sau khi bị tiêu diệt, có thể chọn **hiến tế** 1 quân nặng đang có trên bàn cờ để hồi sinh tại vị trí của quân đó (Mất 1 lượt để thực hiện). Sau khi hồi sinh, điểm số bị reset về 9.

### 6. Vua (King)
* **Điều kiện:** Tích lũy được **5 điểm** trở lên. (Tức cần thêm 5 điểm)
* **Khả năng [Berserk]:**Vua nhận được tầm di chuyển của **Hậu trong 3 lượt**. Sau 3 lượt, điểm tích lũy của Vua reset về 0. Lúc này Vua chết vẫn xử thua.


---

## 🛠️ Trạng thái dự án
* [ ] Định nghĩa luật chơi
Chưa có ăn tốt qua đường và phong cấp
* [ ] Phát triển Skill quân cờ (TyC/Python)
Đã có assets các quân thường và tiến hóa
* [ ] Giao diện người dùng (Aseprite Assets)
* [ ] AI đối thủ
Đã có AI random, nước đi ăn quân sẽ dc ưu tiên
## I need your help
* Func hơi loạn xạ, ước gì có ai sort lại kk

