# LoginUISetup - UI Login Integration với APIManager

## Tổng quan

`LoginUISetup` là một class được thiết kế để tạo và quản lý UI login tích hợp trực tiếp với `APIManager`. Thay vì sử dụng `LoginManager` riêng biệt, class này tích hợp trực tiếp với cơ chế login của APIManager thông qua inspector.

## Tính năng chính

### 1. Tự động tạo UI với kích thước lớn
- Tự động tạo Canvas, EventSystem, và các UI elements cần thiết
- Tạo form login với kích thước lớn, dễ đọc (700x800 pixels)
- Font chữ lớn hơn cho tất cả các elements
- Input fields lớn hơn với padding phù hợp
- Buttons lớn hơn và dễ nhấn

### 2. Tích hợp với APIManager
- Sử dụng trực tiếp `APIManager.Instance.LoginWithToken()`
- Tự động lưu và load remembered email
- Tự động verify token khi khởi động
- Tích hợp với cơ chế token storage của APIManager

### 3. Custom Editor với màu sắc đẹp
- Test login trực tiếp từ inspector
- Sync email với remembered email
- Quick actions để test UI
- Scene management buttons
- **Create Login UI** button (màu xanh lá)
- **Delete Login UI** button (màu đỏ) với confirmation dialog

## Cách sử dụng

### 1. Setup cơ bản

```csharp
// Thêm LoginUISetup vào GameObject trong scene
GameObject loginSetupGO = new GameObject("LoginUISetup");
LoginUISetup loginSetup = loginSetupGO.AddComponent<LoginUISetup>();

// Cấu hình
loginSetup.autoSetup = true; // Tự động tạo UI khi Start
loginSetup.apiManager = APIManager.Instance; // Gán APIManager
```

### 2. Tạo và xóa UI

```csharp
// Tạo UI thủ công
loginSetup.CreateLoginUI();

// Xóa UI thủ công
loginSetup.DeleteLoginUI();

// Hoặc từ Context Menu
// Right-click trên LoginUISetup component -> Create Login UI
// Right-click trên LoginUISetup component -> Delete Login UI
```

### 3. Test từ Inspector

1. Chọn GameObject có LoginUISetup component
2. Trong Inspector, scroll xuống phần "UI Management"
3. Nhấn **"Create Login UI"** (màu xanh lá) để tạo UI
4. Nhấn **"Delete Login UI"** (màu đỏ) để xóa UI
5. Scroll xuống phần "Test Login"
6. Nhập email và password
7. Nhấn **"TEST LOGIN"** (màu xanh dương) để test

### 4. Quick Actions

- **Clear Status** (màu vàng): Xóa status text
- **Show Loading** (màu xanh lá nhạt): Hiển thị loading panel
- **Hide Loading** (màu xám): Ẩn loading panel
- **Go to Register** (màu tím): Chuyển đến scene register
- **Go to Main Menu** (màu xanh lá): Chuyển đến scene main menu

## Cấu hình

### Auto Setup
```csharp
public bool autoSetup = true; // Tự động tạo UI khi Start
```

### APIManager Integration
```csharp
public APIManager apiManager; // Reference đến APIManager
```

### Scene Management
```csharp
public string mainMenuSceneName = "2_MainMenu";
public string registerSceneName = "0_Register";
```

### UI References (Auto-assigned)
```csharp
public TMP_InputField emailInput;
public TMP_InputField passwordInput;
public Button loginButton;
public Button goToRegisterButton;
public TextMeshProUGUI statusText;
public GameObject loadingPanel;
```

## Kích thước UI mới

### Main Panel
- **Kích thước**: 700x800 pixels (tăng từ 500x600)
- **Background**: Màu xanh đậm với độ trong suốt

### Title
- **Font size**: 48 (tăng từ 36)
- **Kích thước**: 300x120 pixels
- **Vị trí**: Y = 280

### Input Fields
- **Kích thước**: 550x70 pixels (tăng từ 400x50)
- **Font size**: 20 (tăng từ 16)
- **Padding**: 15px (tăng từ 10px)
- **Email position**: Y = 150
- **Password position**: Y = 50

### Buttons
- **Login Button**: 300x70 pixels (tăng từ 200x50), font size 24
- **Register Button**: 300x60 pixels (tăng từ 200x40), font size 22
- **Login position**: Y = -50
- **Register position**: Y = -150

### Status Text
- **Font size**: 22 (tăng từ 18)
- **Kích thước**: 600x80 pixels (tăng từ 450x60)
- **Vị trí**: Y = -250

### Loading Text
- **Font size**: 32 (tăng từ 24)

## Workflow

### 1. Khởi động
1. `Start()` được gọi
2. Nếu `autoSetup = true`, gọi `CreateLoginUI()`
3. Gọi `SetupUI()` để setup event listeners
4. Gọi `LoadRememberedEmail()` để load email đã lưu
5. Gọi `CheckAutoLogin()` để verify token nếu có

### 2. Login Process
1. User nhập email/password và nhấn Login
2. `OnLoginClick()` được gọi
3. Validate input
4. Gọi `APIManager.Instance.LoginWithToken()`
5. `OnLoginComplete()` xử lý response
6. Lưu token và email
7. Gọi `RegisterProfileForCurrentUser()`
8. Chuyển đến Main Menu scene

### 3. Auto Login
1. Kiểm tra token đã lưu
2. Gọi `APIManager.Instance.VerifyToken()`
3. Nếu token hợp lệ, chuyển đến Main Menu
4. Nếu token hết hạn, clear token và yêu cầu login lại

## Lợi ích

### 1. Đơn giản hóa
- Không cần tạo LoginManager riêng biệt
- Tích hợp trực tiếp với APIManager
- Tự động setup UI

### 2. Test dễ dàng
- Test trực tiếp từ inspector
- Quick actions để test UI
- Sync với remembered email
- **UI Management buttons với màu sắc đẹp**

### 3. Linh hoạt
- Có thể tạo UI thủ công hoặc tự động
- Có thể gán APIManager thủ công hoặc tự động tìm
- Có thể customize scene names
- **Có thể xóa UI dễ dàng**

### 4. UI lớn và dễ đọc
- Kích thước form lớn hơn
- Font chữ lớn hơn
- Buttons lớn hơn
- Padding phù hợp

## So sánh với LoginManager cũ

| Tính năng | LoginManager cũ | LoginUISetup mới |
|-----------|----------------|------------------|
| Tạo UI | Không | Có (tự động/thủ công) |
| Xóa UI | Không | Có (với confirmation) |
| APIManager Integration | Gián tiếp | Trực tiếp |
| Test từ Inspector | Không | Có |
| Auto Setup | Không | Có |
| Custom Editor | Không | Có |
| UI Size | Nhỏ | Lớn và dễ đọc |
| Font Size | Nhỏ | Lớn |
| Color-coded Buttons | Không | Có |

## Troubleshooting

### 1. APIManager không được gán
- Kiểm tra xem có APIManager trong scene không
- Sử dụng "Find APIManager" button trong inspector
- LoginUISetup sẽ tự động tạo APIManager nếu không tìm thấy

### 2. UI không hiển thị
- Kiểm tra `autoSetup` có được bật không
- Gọi `CreateLoginUI()` thủ công
- Kiểm tra Console để xem có lỗi gì không

### 3. Login không hoạt động
- Kiểm tra APIManager có được gán đúng không
- Kiểm tra baseURL trong APIManager
- Kiểm tra network connection
- Xem Console để debug

### 4. Xóa UI
- Sử dụng nút **"Delete Login UI"** (màu đỏ) trong inspector
- Hoặc gọi `DeleteLoginUI()` từ code
- UI sẽ được xóa hoàn toàn khỏi scene

### 5. Token Expires At không hiển thị
**Vấn đề**: Khi login bằng LoginUISetup, `Token Expires At` trong APIManager Inspector không có dữ liệu, nhưng khi login bằng APIManager Inspector thì có.

**Nguyên nhân**: LoginUISetup đã được sửa để sử dụng đúng method `LoginWithToken` của APIManager, và không gọi `SetAuthToken` thêm lần nữa để tránh ghi đè thông tin expire.

**Giải pháp**:
1. LoginUISetup sử dụng `apiManager.LoginWithToken()` - method này tự động gọi `SetAuthTokenWithExpire()`
2. Không gọi `SetAuthToken()` thêm lần nữa trong `OnLoginComplete`
3. Thêm debug logging để theo dõi quá trình login
4. Force update token display info sau khi login

**Debug**:
- Sử dụng Context Menu "Test Login Direct" để test login trực tiếp
- Kiểm tra Console logs để xem thông tin token
- Đảm bảo rằng `response.expires_at` và `response.expires_in` có giá trị từ API

**Đảm bảo tính nhất quán**:
- LoginUISetup và APIManager Inspector đều sử dụng cùng method `LoginWithToken`
- Cả hai đều gọi `SetAuthTokenWithExpire` với đầy đủ thông tin expire
- Token display info được update sau khi login

## Ví dụ sử dụng

```csharp
// Tạo LoginUISetup với cấu hình tùy chỉnh
public class GameManager : MonoBehaviour
{
    [SerializeField] private LoginUISetup loginUISetup;
    
    void Start()
    {
        // Tạo LoginUISetup nếu chưa có
        if (loginUISetup == null)
        {
            GameObject loginGO = new GameObject("LoginUISetup");
            loginUISetup = loginGO.AddComponent<LoginUISetup>();
        }
        
        // Cấu hình
        loginUISetup.autoSetup = true;
        loginUISetup.apiManager = APIManager.Instance;
        loginUISetup.mainMenuSceneName = "MainMenu";
        loginUISetup.registerSceneName = "Register";
    }
    
    // Xóa UI khi cần
    public void CleanupLoginUI()
    {
        if (loginUISetup != null)
        {
            loginUISetup.DeleteLoginUI();
        }
    }
}
```

## Màu sắc trong Inspector

### UI Management
- **Create Login UI**: Xanh lá (0.2, 0.8, 0.2)
- **Delete Login UI**: Đỏ (0.8, 0.2, 0.2)

### Test Login
- **TEST LOGIN**: Xanh dương (0.2, 0.6, 1.0)

### Quick Actions
- **Clear Status**: Vàng (1.0, 0.8, 0.2)
- **Show Loading**: Xanh lá nhạt (0.4, 0.8, 0.4)
- **Hide Loading**: Xám (0.6, 0.6, 0.6)

### Scene Management
- **Go to Register**: Tím (0.8, 0.4, 0.8)
- **Go to Main Menu**: Xanh lá (0.2, 0.8, 0.2)

### APIManager Integration
- **Find APIManager**: Xanh dương nhạt (0.4, 0.6, 1.0)