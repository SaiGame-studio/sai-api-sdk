using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class RegisterManager : SaiBehaviour
{
    [Header("UI References")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField confirmPasswordInput;
    public Button registerButton;
    public Button goToLoginButton;
    public TextMeshProUGUI statusText;
    public GameObject loadingPanel;
    
    [Header("Scene Management")]
    public string loginSceneName = SceneNames.LOGIN;
    public string mainMenuSceneName = SceneNames.MAIN_MENU;
    
    protected override void Start()
    {
        base.Start();
        SetupUI();
    }
    
    private void SetupUI()
    {
        if (registerButton != null)
            registerButton.onClick.AddListener(OnRegisterClick);
            
        if (goToLoginButton != null)
            goToLoginButton.onClick.AddListener(OnGoToLoginClick);
            
        if (statusText != null)
            statusText.text = "";
            
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }
    
    public void OnRegisterClick()
    {
        string email = emailInput?.text ?? "";
        string password = passwordInput?.text ?? "";
        string confirmPassword = confirmPasswordInput?.text ?? "";
        
        // Validation
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            ShowStatus("Please fill in all fields!");
            return;
        }
        
        if (!IsValidEmail(email))
        {
            ShowStatus("Invalid email format!");
            return;
        }
        
        if (password.Length < 6)
        {
            ShowStatus("Password must be at least 6 characters!");
            return;
        }
        
        if (password != confirmPassword)
        {
            ShowStatus("Passwords do not match!");
            return;
        }
        
        ShowLoading(true);
        ShowStatus("Registering...");
        
        APIManager.Instance.RegisterWithToken(email, password, confirmPassword, OnRegisterComplete);
    }
    
    private void OnRegisterComplete(TokenResponse response)
    {
        ShowLoading(false);
        
        if (response != null && !string.IsNullOrEmpty(response.token))
        {
            ShowStatus("Registration successful!");
            
            // Lưu token vào APIManager
            APIManager.Instance.SetAuthToken(response.token);
            Debug.Log("Token saved: " + response.token);
            
            // Gọi API tạo account cho user
            ShowStatus("Registering user profile...");
            ShowLoading(true);
            APIManager.Instance.RegisterProfileForCurrentUser((profileResponse) => {
                ShowLoading(false);
                if (profileResponse != null && profileResponse.status == "success")
                {
                    // Chuyển đến game scene nếu tạo profile thành công
                    Invoke(nameof(LoadGameScene), 1.5f);
                }
                else
                {
                    ShowStatus("Failed to register user profile!");
                }
            });
        }
        else
        {
            ShowStatus("Registration failed! Please try again.");
        }
    }
    
    public void OnGoToLoginClick()
    {
        if (!string.IsNullOrEmpty(loginSceneName))
        {
            SceneManager.LoadScene(loginSceneName);
        }
        else
        {
            ShowStatus("Chưa thiết lập scene đăng nhập!");
        }
    }
    
    private void LoadLoginScene()
    {
        if (!string.IsNullOrEmpty(loginSceneName))
        {
            SceneManager.LoadScene(loginSceneName);
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
            LoadLoginScene();
        }
    }
    
    private void ShowStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
            Debug.Log("Register Status: " + message);
        }
    }
    
    private void ShowLoading(bool show)
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(show);
            
        if (registerButton != null)
            registerButton.interactable = !show;
    }
    
    private bool IsValidEmail(string email)
    {
        return email.Contains("@") && email.Contains(".");
    }
    
    // Validate password strength
    private bool IsStrongPassword(string password)
    {
        if (password.Length < 8) return false;
        
        bool hasUpper = false;
        bool hasLower = false;
        bool hasDigit = false;
        
        foreach (char c in password)
        {
            if (char.IsUpper(c)) hasUpper = true;
            if (char.IsLower(c)) hasLower = true;
            if (char.IsDigit(c)) hasDigit = true;
        }
        
        return hasUpper && hasLower && hasDigit;
    }
    
    // For testing purposes
    [ContextMenu("Test Register")]
    private void TestRegister()
    {
        if (emailInput != null) emailInput.text = "newuser@example.com";
        if (passwordInput != null) passwordInput.text = "password123";
        if (confirmPasswordInput != null) confirmPasswordInput.text = "password123";
        OnRegisterClick();
    }
} 