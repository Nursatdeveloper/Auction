using Auction.MVC.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Auction.MVC.Controllers {
    [Route("user")]
    public class UserController : Controller{
        private readonly AuctionDbContext _context;
        public UserController(AuctionDbContext context) {
            _context = context;
        }

        [HttpGet]
        [Route("register")]
        public IActionResult RegisterView() {
            return View();
        }

        [HttpPost]
        [Route("register")]
        public IActionResult RegisterPost(RegiserUserDto regiserUserDto) {
            var user = new User() {
                Fio = regiserUserDto.Fio,
                Iin = regiserUserDto.Iin,
                Password = regiserUserDto.Password,
                Balance = regiserUserDto.Balance,
                Roles = regiserUserDto.Roles.Split(',')
            };

            _context.Users.Add(user);
            _context.SaveChanges();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Route("logout")]
        public async Task<IActionResult> Logout() {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Route("login")]
        public IActionResult LoginView() {
            return View();
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> LoginPost(LoginUserDto loginUserDto) { 
            var user = _context.Users.FirstOrDefault(x => x.Iin == loginUserDto.Iin);
            if(user != null) {
                var roles = "";
                var tradeIds = "";
                for(int i = 0; i<user.Roles.Length; i++) {
                    roles += user.Roles[i];
                    if(i != user.Roles.Length-1) { roles += ","; }
                }
                if(user.TradeIds != null) {
                    for(int i = 0; i<user.TradeIds.Length; i++) {
                        tradeIds += user.TradeIds[i].ToString();
                        if(i != user.TradeIds.Length - 1) { tradeIds += ","; }
                    }
                }
                if(user.Password == loginUserDto.Password) {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Fio),
                        new Claim("Iin", user.Iin),
                        new Claim("Balance", user.Balance.ToString()),
                        new Claim("Roles", roles),
                        new Claim("TradeIds", tradeIds),
                    };

                    if(user.Roles.Contains("Moderator")) {
                        claims.Add(new Claim("Moderator", "Moderator"));
                    }

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties {
                        
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    Console.WriteLine($"User {user.Fio} {loginUserDto.Iin} logged in at {DateTime.Now}.");
                    return RedirectToAction("Index", "Home");

                } else {
                    return View("InvalidCredentials");
                }
            } else {
                return View("NotFound");
            }
        }
    }
}
