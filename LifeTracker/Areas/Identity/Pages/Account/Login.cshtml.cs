﻿using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LifeTracker.Areas.Identity.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signInManager;

    public LoginModel(SignInManager<IdentityUser> signInManager)
    {
        _signInManager = signInManager;
    }
    
    [BindProperty]
    public InputModel Input { get; set; }
    public string ReturnUrl { get; set; }
    
    public void OnGet()
    {
        ReturnUrl = Url.Content("~/");
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        ReturnUrl = Url.Content("~/");
        
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, false, false);

            if (result.Succeeded) return LocalRedirect(ReturnUrl);
        }

        return Page();
    }
    
    public Task<IActionResult> OnPostRedirectRegister()
    {
        ReturnUrl = Url.Content("~/identity/account/register");
        return Task.FromResult<IActionResult>(LocalRedirect(ReturnUrl));
    }
    
    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}