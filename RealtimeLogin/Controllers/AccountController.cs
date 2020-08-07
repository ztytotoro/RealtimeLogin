using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using RealtimeLogin.Hubs;
using System;
using System.Threading.Tasks;

namespace RealtimeLogin.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private IMemoryCache cache;
        private IHubContext<LoginHub> hubContext;
        public AccountController(IMemoryCache cache, IHubContext<LoginHub> hubContext)
        {
            this.cache = cache;
            this.hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> Login(string username)
        {
            var loginId = Guid.NewGuid().ToString();
            await hubContext.Clients.All.SendAsync("login", username, loginId);
            cache.Remove(username);
            cache.Set(username, loginId);
            return Ok(loginId);
        }

        [HttpGet]
        public void Signout(string username, string loginId)
        {
            if(cache.Get<string>(username) == loginId)
            {
                cache.Remove(username);
            }
        }

        [HttpGet]
        public IActionResult CheckLogin(string username)
        {
            return Ok(cache.Get<string>(username));
        }
    }
}
