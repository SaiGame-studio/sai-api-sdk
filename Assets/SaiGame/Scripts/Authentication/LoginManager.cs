using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoginManager : SaiBehaviour
{
    [Header("UI References")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public Button loginButton;
    public Button goToRegisterButton;
    public TextMeshProUGUI statusText;
    public GameObject loadingPanel;
    
    [Header("Scene Management")]
    public string mainMenuSceneName = SceneNames.MAIN_MENU;
    public string registerSceneName = SceneNames.REGISTER;
    
    protected override void Start()
    {
        base.Start();
        SetupUI();
        LoadRememberedEmail();
        CheckAutoLogin();
    }
    
    private void SetupUI()
    {
        if (loginButton != null)
            loginButton.onClick.AddListener(OnLoginClick);
            
        if (goToRegisterButton != null)
            goToRegisterButton.onClick.AddListener(OnGoToRegisterClick);
            
        if (statusText != null)
            statusText.text = "";
            
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }
    
    private void LoadRememberedEmail()
    {
        string rememberedEmail = APIManager.Instance.GetRememberedEmail();
        if (!string.IsNullOrEmpty(rememberedEmail) && emailInput != null)
        {
            emailInput.text = rememberedEmail;
        }
    }
    
    private void CheckAutoLogin()
    {
        string savedToken = APIManager.Instance.GetAuthToken();
        if (!string.IsNullOrEmpty(savedToken))
        {
            ShowStatus("Checking saved session...");
            ShowLoading(true);
            
            APIManager.Instance.VerifyToken(OnAutoLoginComplete);
        }
    }
    
    private void OnAutoLoginComplete(TokenInfoResponse tokenInfo)
    {
        ShowLoading(false);
        
        // Điều kiện login thành công: API trả về thông tin token và token hợp lệ
        if (tokenInfo != null && tokenInfo.IsValid())
        {
            // Token hợp lệ và chưa hết hạn
            ShowStatus("Login successful!");
            
            // Log thông tin token để debug
            long remainingSeconds = tokenInfo.GetRemainingSeconds();
            
            // Trigger authentication success event for auto-login
            if (APIManager.Instance != null)
            {
                APIManager.Instance.TriggerAuthenticationSuccess();
            }
            
            LoadGameScene();
        }
        else
        {
            // Token không hợp lệ hoặc đã hết hạn
            APIManager.Instance.ClearAuthToken();
            if (tokenInfo != null && !tokenInfo.IsValid())
            {
                ShowStatus("Session expired. Please login again.");
            }
            else
            {
                ShowStatus("Please login");
            }
        }
    }
    
    public void OnLoginClick()
    {
        string email = emailInput?.text ?? "";
        string password = passwordInput?.text ?? "";
        
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowStatus("Please enter email and password!");
            return;
        }
        
        if (!IsValidEmail(email))
        {
            ShowStatus("Invalid email format!");
            return;
        }
        
        ShowLoading(true);
        ShowStatus("Logging in...");
        
        APIManager.Instance.LoginWithToken(email, password, OnLoginComplete);
    }
    
    private void OnLoginComplete(TokenResponse response)
    {
        ShowLoading(false);
        
        if (response != null && !string.IsNullOrEmpty(response.token))
        {
            ShowStatus("Login successful!");
            
            // Lưu token vào APIManager
            APIManager.Instance.SetAuthToken(response.token);
            
            // Lưu email để ghi nhớ cho lần đăng nhập tiếp theo
            string email = emailInput?.text ?? "";
            if (!string.IsNullOrEmpty(email))
            {
                APIManager.Instance.SaveRememberedEmail(email);
            }
            
            // Gọi API tạo account cho user
            ShowStatus("Registering user profile...");
            ShowLoading(true);
            APIManager.Instance.RegisterProfileForCurrentUser((profileResponse) => {
                ShowLoading(false);
                if (profileResponse != null && profileResponse.status == "success")
                {
                    // Load game scene nếu tạo profile thành công
                    Invoke(nameof(LoadGameScene), 1f);
                }
                else
                {
                    ShowStatus("Failed to register user profile!");
                }
            });
        }
        else
        {
            ShowStatus("Login failed! Please check your credentials.");
        }
    }
    
    public void OnGoToRegisterClick()
    {
        if (!string.IsNullOrEmpty(registerSceneName))
        {
            SceneManager.LoadScene(registerSceneName);
        }
        else
        {
            ShowStatus("Register scene not configured!");
        }
    }
    
    private void LoadGameScene()
    {
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            ShowStatus("Game scene not configured!");
        }
    }
    
    private void ShowStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
    
    private void ShowLoading(bool show)
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(show);
    }
    
    private bool IsValidEmail(string email)
    {
        return email.Contains("@") && email.Contains(".");
    }
    
    // For testing purposes
    [ContextMenu("Test Login")]
    private void TestLogin()
    {
        if (emailInput != null) emailInput.text = "test@example.com";
        if (passwordInput != null) passwordInput.text = "password123";
        OnLoginClick();
    }
} 