namespace FinalCoffee1.common.helperServices;
public class NotificationService
{
    public string? Message { get; set; }
    public string? MessageClass { get; set; }
    public void Notify(string message, string messageClass)
    {
        Message = message;
        MessageClass = messageClass;
    }

    public void Clear()
    {
        Message = "";
        MessageClass = "";
    }
}