
using FinalCoffee1.common.helperClass;

namespace FinalCoffee1.common.helperServices;
public class SessService
{

    public UserType CurrentUserType { get; private set; }
    private bool? needAuthorized;
    public event Action OnChange;
    public void NotifyStateChanged() => OnChange?.Invoke();
    private bool currentUser;
    private int adminId = 1;
    public int getId()
    {
        return this.adminId;
    }
    public void setId(int id)
    {
        this.adminId = id;
    }
    public void SetCurrentUserType(UserType userType)
    {
        this.CurrentUserType = userType;
        OnChange?.Invoke();
    }

    public void SetCurrentUser(bool set)
    {
        this.currentUser = set;
        OnChange?.Invoke();
    }
    public bool IsUserLoggedIn()
    {
        return this.currentUser;
    }
    public bool setAdminRegistered()
    {
        OnChange?.Invoke();
        return true;
    }
    public bool isAdminRegistered()
    {
        var path = new FileManager().DirectoryPath("database", "admin.json");
        if (File.Exists(path))
        {
            return true;
        }
        return false;
    }
    public bool setNeedAuthorized(bool set)
    {
        this.needAuthorized = set;
        OnChange?.Invoke();
        return (bool)this.needAuthorized;
    }
    public bool isNeedAuthorized()
    {
        return (bool)this.needAuthorized;
    }
    public bool defaultNeedAuthorized()
    {
        if (this.needAuthorized.HasValue)
        {
            return (bool)this.needAuthorized;
        }
        else
        {
            this.needAuthorized = true;
            return (bool)this.needAuthorized;
        }
    }
    private bool isOnlyCoffee = false;
    public void setOnlyCoffee(bool set)
    {
        this.isOnlyCoffee = set;
        OnChange?.Invoke();
    }
    public bool getOnlyCoffee()
    {
        return this.isOnlyCoffee;
    }

    private bool isOnlyCustomer = false;
    public void setCustomerOnly(bool set)
    {
        this.isOnlyCustomer = set;
        OnChange?.Invoke();
    }
    public bool getCustomerOnly()
    {
        return this.isOnlyCustomer;
    }
}