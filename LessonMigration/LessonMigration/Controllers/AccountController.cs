using LessonMigration.Models;
using LessonMigration.ViewModels.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace LessonMigration.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

       // 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
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid) return View(registerVM);

            AppUser newUser = new AppUser()
            {
                FullName = registerVM.FullName,
                UserName = registerVM.UserName,
                Email = registerVM.Email
            };

            IdentityResult result = await _userManager.CreateAsync(newUser, registerVM.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(registerVM);
            }

            //await _signInManager.SignInAsync(newUser, false); // register olandan sonra avtomatik getmesin!!!!

            //return RedirectToAction("Index","Home");
            return RedirectToAction(nameof(Login));


        }

        public IActionResult EmailVerification()
        {
            return View();


        }
        public async Task SendEmail()
        {
            var apiKey = Environment.GetEnvironmentVariable("NAME_OF_THE_ENVIRONMENT_VARIABLE_FOR_YOUR_SENDGRID_KEY");
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("test@example.com", "Example User");
            var subject = "Sending with SendGrid is Fun";
            var to = new EmailAddress("test@example.com", "Example User");
            var plainTextContent = "and easy to do anywhere, even with C#";
            var htmlContent = "<strong>and easy to do anywhere, even with C#</strong>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);

        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");

        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid) return View(loginVM);
            AppUser user = await _userManager.FindByEmailAsync(loginVM.UserNameOrEmail);
            if (user is null)
            {
                user = await _userManager.FindByNameAsync(loginVM.UserNameOrEmail);
                
            }
            if (user is null)
            {
                ModelState.AddModelError("", "Email or password is wrong");
                return View();
            }

            if (!user.IsActivated)
            {
                ModelState.AddModelError("", "Contact with admin");
                return View(loginVM);
            }

            SignInResult signInResult = await _signInManager.PasswordSignInAsync(user, loginVM.Password, false,false); //false 2 dene yazilmalidi!!Passworldarin eyni olmasini yoxlayir!

            if(!signInResult.Succeeded)
            {
                ModelState.AddModelError("", "Email or password is wrong");
                return View();

            }
                
            return RedirectToAction("Index","Home");

        }
        


    }
}
