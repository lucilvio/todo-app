using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;


namespace Vue.TodoApp
{
    [AllowAnonymous]
    public class NotifyHub : Hub
    {       
    }
}