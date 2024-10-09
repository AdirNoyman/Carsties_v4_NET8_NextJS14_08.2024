using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace IdentityService.Pages.Account.Register
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class Index : PageModel
    {
        [BindProperty] public RegisterViewModel Input { get; set; } = new();
        [BindProperty] public bool RegisterSuccess { get; set; }  
        
        private readonly ILogger<Index> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public Index(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }  

         

        // returnUrl = the url of the page they came from to the register page, which is the login page
        public IActionResult OnGet(string returnUrl)
        {
            Input = new RegisterViewModel
            {

                ReturnUrl = returnUrl

            };

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            // If the user clicked on cancel, redirect them to the home page
            if (Input.Button != "register") return Redirect("~/");

            // Check if the register form is valid before proceeding
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = Input.Username,
                    Email = Input.Email,
                    // We are not using email confirmation on this application so we just gonna leave it by passing 'true' 
                    EmailConfirmed = true
                };

                // Create the user
                var result = await _userManager.CreateAsync(user, Input.Password);

                // Add user claims
                if (result.Succeeded)
                {
                    await _userManager.AddClaimsAsync(user,
                    [
                        new Claim(JwtClaimTypes.Name, Input.FullName)
                    ]);

                    RegisterSuccess = true;
                }
            }

            return Page();
        }
    }
}