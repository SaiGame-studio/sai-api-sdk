# AutoLoad Fix for ItemProfileManager

## Vấn đề đã được sửa

AutoLoad của ItemProfileManager không hoạt động do các nguyên nhân sau:

1. **Event không được trigger**: Chỉ có `LoginWithToken` và `RegisterWithToken` trigger event `OnAuthenticationSuccess`, nhưng `Login` và `Register` thông thường thì không.

2. **Auto-login không trigger event**: Khi user đã đăng nhập trước đó và có token hợp lệ, event không được trigger.

3. **Race condition**: ItemProfileManager có thể được khởi tạo sau khi event đã được trigger.

4. **Event access restriction**: Event `OnAuthenticationSuccess` chỉ có thể được invoke từ bên trong class `APIManager`.

## Các thay đổi đã thực hiện

### 1. APIManager.cs
- Thêm trigger event `OnAuthenticationSuccess` vào method `Login` và `Register`
- Đảm bảo tất cả các method authentication đều trigger event
- Thêm method `TriggerAuthenticationSuccess()` để cho phép trigger event từ bên ngoài class

### 2. LoginManager.cs
- Thêm trigger event `OnAuthenticationSuccess` trong `OnAutoLoginComplete` khi auto-login thành công
- Sử dụng `TriggerAuthenticationSuccess()` thay vì invoke event trực tiếp

### 3. ItemProfileManager.cs
- Thêm kiểm tra trong `Start()` để load data nếu đã có token hợp lệ
- Thêm coroutine `PeriodicAutoLoadCheck()` để kiểm tra định kỳ
- Thêm method `CheckAndTriggerAutoLoad()` để trigger thủ công
- Thêm method `TestAutoLoadStatus()` để debug
- Cải thiện logging và error handling

### 4. ItemProfileManagerTest.cs
- Script test để kiểm tra AutoLoad hoạt động

## Cách sử dụng

### Bật AutoLoad
1. Chọn ItemProfileManager trong scene
2. Tick vào checkbox "Auto Load" trong Inspector
3. Bật "Show Debug Log" để xem log

### Test AutoLoad
1. Thêm script `ItemProfileManagerTest` vào scene
2. Chạy game và xem Console để kiểm tra log
3. Hoặc sử dụng Context Menu "Run AutoLoad Test"

### Trigger thủ công
```csharp
// Từ code
ItemProfileManager.Instance.CheckAndTriggerAutoLoad();

// Từ Inspector
// Right-click ItemProfileManager -> Check and Trigger AutoLoad
```

### Trigger authentication success event
```csharp
// Từ code (nếu cần)
APIManager.Instance.TriggerAuthenticationSuccess();
```

## Debug

### Kiểm tra trạng thái
```csharp
// Từ code
ItemProfileManager.Instance.TestAutoLoadStatus();

// Từ Inspector
// Right-click ItemProfileManager -> Test AutoLoad Status
```

### Log messages
- `[ItemProfileManager] AutoLoad: Found valid token, loading item profiles`
- `[ItemProfileManager] AutoLoad: Authentication success, loading item profiles`
- `[ItemProfileManager] Periodic check: Found valid token but no profiles loaded, loading now`
- `Triggering OnAuthenticationSuccess event`

## Lưu ý

1. AutoLoad chỉ hoạt động khi có token hợp lệ
2. Nếu không có token, AutoLoad sẽ chờ event `OnAuthenticationSuccess`
3. Periodic check sẽ kiểm tra mỗi 5 giây nếu chưa load được data
4. Có thể tắt periodic check bằng cách tắt AutoLoad
5. Event `OnAuthenticationSuccess` chỉ có thể được invoke từ bên trong `APIManager` hoặc thông qua `TriggerAuthenticationSuccess()`

## Troubleshooting

### AutoLoad không hoạt động
1. Kiểm tra `autoLoad` đã được bật chưa
2. Kiểm tra APIManager có token hợp lệ không
3. Xem log để debug
4. Sử dụng `TestAutoLoadStatus()` để kiểm tra

### Event không được trigger
1. Đảm bảo sử dụng `LoginWithToken` hoặc `RegisterWithToken`
2. Kiểm tra auto-login có thành công không
3. Xem log của APIManager
4. Sử dụng `TriggerAuthenticationSuccess()` thay vì invoke event trực tiếp

### Race condition
1. Sử dụng `CheckAndTriggerAutoLoad()` để trigger thủ công
2. Đợi một chút trước khi kiểm tra
3. Sử dụng periodic check

### Compilation error
1. Không invoke event `OnAuthenticationSuccess` trực tiếp từ bên ngoài `APIManager`
2. Sử dụng `TriggerAuthenticationSuccess()` method
3. Đảm bảo tất cả các invoke event đều ở trong class `APIManager`