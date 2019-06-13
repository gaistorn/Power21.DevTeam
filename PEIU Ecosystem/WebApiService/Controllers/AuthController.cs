using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PES.Service.WebApiService;
using PES.Service.WebApiService.Localization;
using PES.Toolkit.Auth;
using Power21.PEIUEcosystem.Models;

namespace WebApiService.Controllers
{
    [Route("api/auth")]
    [Authorize]
    [ApiController]
    //[RequireHttps]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AccountModel> _userManager;
        AccountRecordContext _accountContext;
        private readonly IStringLocalizer<LocalizedIdentityErrorDescriber> _localizer;
        private readonly SignInManager<AccountModel> _signInManager;
        private readonly IEmailSender _emailSender;
        

        public AuthController(UserManager<AccountModel> userManager,
            SignInManager<AccountModel> signInManager,
            IEmailSender emailSender,
            IStringLocalizer<LocalizedIdentityErrorDescriber> localizer,
            AccountRecordContext accountContext)
        {
            _userManager = userManager;
            _accountContext = accountContext;
            _signInManager = signInManager;
            _localizer = localizer;
            _emailSender = emailSender;
        }

        [HttpPost, Route("logout")]
        public async Task<IActionResult> LogOff()
        {
            Console.WriteLine("logout~");
            await _signInManager.SignOutAsync();
            //_logger.LogInformation(4, "User logged out.");
            return Ok();
        }

        [HttpPost, Route("logintoredirect")]
        public async Task<IActionResult> LoginToRedirect(string ReturnUrl = null)
        {
            Console.WriteLine("ReturnUrl : " + ReturnUrl);
            return Ok(StatusCodes.Status200OK);
        }

        [HttpGet, Route("me")]
        public async Task<IActionResult> Me(string redirecturl = null)
        {
            
            Console.WriteLine("Me~Me~Me");
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user != null)
            {
                JObject root = new JObject();
                root.Add("data", JObject.FromObject(user));
                return Ok(new { Result = IdentityResult.Success, User = user });
            }
            else
            {
                IdentityError error = (_userManager.ErrorDescriber as LocalizedIdentityErrorDescriber).UserNotFound();
                IdentityResult _result = IdentityResult.Failed(error);
                return Ok(new { Result = _result });
            }
            //Console.WriteLine("Call the me");
            //Console.WriteLine($"redirect Url : " + redirecturl);
            //JObject data = new JObject();
            //data.Add("id", 3);
            //data.Add("username", "최고은");
            //data.Add("email", "ccc@ccc.com");
            //data.Add("created_at", DateTime.Now);
            //data.Add("updated_at", DateTime.Now);

            //JObject root = new JObject();
            //root.Add("status", "success");
            //root.Add("data", data);
            //return Ok(root);
        }

        [HttpGet, Route("forgotpassword"), AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(string userName)
        {
            var account = await _userManager.FindByEmailAsync(userName);
            if (account == null)
            {
                IdentityError error = (_userManager.ErrorDescriber as LocalizedIdentityErrorDescriber).UserNotFound();
                IdentityResult _result = IdentityResult.Failed(error);
                return Ok(new { Result = _result });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(account);
            return Ok(new { Result = IdentityResult.Success, Token = token });
        }

        [HttpPost, Route("resetpassword"), AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var account = await _userManager.FindByEmailAsync(model.Email);
                if (account == null)
                {
                    IdentityError error = (_userManager.ErrorDescriber as LocalizedIdentityErrorDescriber).UserNotFound();
                    IdentityResult _result = IdentityResult.Failed(error);
                    return Ok(new { Result = _result });
                }

                var result = await _userManager.ResetPasswordAsync(account, model.Token, model.NewPassword);
                return Ok(new { Result = result } );
            }
            else
            {
                return Ok(StatusCodes.Status400BadRequest);
            }
        }

        [HttpGet, Route("logintest"), AllowAnonymous]
        public async Task<IActionResult> LoginTest()
        {
            //return await ClaimsLogin(jo);
            JObject obj = new JObject();
            obj.Add("Email", "redwinelove@hotmail.com");
            obj.Add("Password", "Kkr5321293!");

            foreach (string key in Response.Headers.Keys)
            {
                Console.WriteLine($"{key} : {Response.Headers[key]}");
            }

            Response.Cookies.Append("babo", "it's you");

            return await OldLogin(obj);
        }

        [HttpPost, Route("login"), AllowAnonymous]
        public async Task<IActionResult> Login([FromBody]JObject jo)
        {
            Console.WriteLine("Call the login. correct");
            //return Ok();
            return await OldLogin(jo);
        }

        [HttpPost, Route("login2"), AllowAnonymous]
        public async Task<IActionResult> Login2()
        {
            Console.WriteLine("Call the login2. No Parameter. correct");
            return Ok();
        }


        //[HttpPost, Route("login"), AllowAnonymous]
        ////public async Task<IActionResult> Login([FromBody]JObject jo)
        //public async Task<IActionResult> Login()
        //{
        //    Console.WriteLine("call login~. No Parameter");
        //    //foreach (string key in Response.Headers.Keys)
        //    //{
        //    //    Console.WriteLine($"{key} : {Response.Headers[key]}");
        //    //}
        //    //Response.Cookies.Append("babo", "you~");
        //    //if (jo == null)
        //    //    return NoContent();
        //    //return await ClaimsLogin(jo);
        //    return Ok();
        //}

        //private async Task<IActionResult> ClaimsLogin([FromBody]JObject jo)
        //{
        //    bool isUservalid = false;
        //    LoginViewModel user = JsonConvert.DeserializeObject<LoginViewModel>(jo.ToString());

        //    if (ModelState.IsValid && isUservalid)
        //    {
        //        var claims = new List<Claim>();

        //        claims.Add(new Claim(ClaimTypes.Name, user.Email));


        //        var identity = new ClaimsIdentity(
        //            claims, JwtBearerDefaults.AuthenticationScheme);

        //        var principal = new ClaimsPrincipal(identity);

        //        var props = new AuthenticationProperties();
        //        props.IsPersistent = user.RememberMe;

        //        HttpContext.SignInAsync(
        //            IdentityConstants.ApplicationScheme,
        //            principal, props).Wait();
        //        string token = JasonWebTokenManager.GenerateToken(user.Email);
        //        return Ok(new { Token = token });
        //    }
        //    else
        //    {
        //        return BadRequest();
        //    }
        //}

        private async Task<IActionResult> OldLogin([FromBody]JObject jo)
        {
            //[FromBody]
            //JObject jo = null;
            Console.WriteLine($"Try Logging... {jo}");

            LoginViewModel user = JsonConvert.DeserializeObject<LoginViewModel>(jo.ToString());

            //AccountModel user = await _userManager.FindByNameAsync(input_user.Email);
            

            if (user != null)
            {
                Console.WriteLine($"Model State is Valid");
                Console.WriteLine(jo.ToString());
                if(string.IsNullOrEmpty( user.email) || string.IsNullOrEmpty(user.password))
                {
                    Console.WriteLine("Invalid User");
                    return NoContent();
                }
                var signResult = await _signInManager.PasswordSignInAsync(user.email, user.password, user.RememberMe, false);
                if (signResult.Succeeded)
                {
                    string token = JasonWebTokenManager.GenerateToken(user.email);

                    //if (string.IsNullOrEmpty(returnUrl) == false)
                    //{
                    //    Console.WriteLine("returnurl:" + returnUrl);
                    //    return Redirect(returnUrl);
                    //}
                    Console.WriteLine("Log-in Success: " + user.email);
                    return Ok(new { Result = signResult, Token = token });
                }
                else
                {
                    Console.WriteLine($"Login Failed");
                    //if (signResult.RequiresTwoFactor)
                    //{
                    //    return RedirectToAction("act", new { ReturnUrl = returnUrl, RememberMe = user.RememberMe });
                    //}
                    if (signResult.IsLockedOut)
                    {
                        IdentityError error = (_userManager.ErrorDescriber as LocalizedIdentityErrorDescriber).UserLockoutEnabled();
                        IdentityResult _result = IdentityResult.Failed(error);
                        return Ok(new { Result = _result });
                    }
                    else
                    {
                        return Ok(new { Result = signResult });
                    }
                }


            }
            else
            {
                Console.WriteLine("Invalid LoginViewModel");
                return Ok(StatusCodes.Status406NotAcceptable);
            }
        }

        [HttpPost, Route("register")]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            
            //model.Email = value["Email"].ToString();
            //model.Password = value["Password"].ToString();
            //model.ConfirmPassword = value["ConfirmPassword"].ToString();
            //model.Username
            if (ModelState.IsValid)
            {
                var user = new AccountModel
                {
                    
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    UserName = model.Email,
                    NormalizedUserName = model.Email.ToUpper(),
                    PhoneNumber = model.PhoneNumber,
                    RegistrationNumber = model.RegistrationNumber,
                    Address = model.Address

                };
                var result = await _userManager.CreateAsync(user, model.Password);
                //result.Errors
                if (result.Succeeded)
                {
                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                    // Send an email with this link
                    //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    //var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                    //await _emailSender.SendEmailAsync(model.Email, "Confirm your account",
                    //    "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\">link</a>");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    //_logger.LogInformation(3, "User created a new account with password.");
                }
                return Ok(new { Result = result });
            }

            // If we got this far, something failed, redisplay form
            return Ok(StatusCodes.Status400BadRequest);
        }
    }
}