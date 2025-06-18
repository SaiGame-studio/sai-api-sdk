using System;

[Serializable]
public class UserData
{
    public string email;
    public string password;
    public string password_confirmation;
    public string token;
    public int id;
    public string name;
    
    public UserData()
    {
        
    }
    
    public UserData(string email, string password)
    {
        this.email = email;
        this.password = password;
    }
    
    public UserData(string email, string password, string passwordConfirmation)
    {
        this.email = email;
        this.password = password;
        this.password_confirmation = passwordConfirmation;
    }
} 