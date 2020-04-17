using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages
{
    [Authorize]
    // Or then directly validate the claim via policy.
    // NOTE: In this example it will redirect to non-existing access denied page:
    //       "/Account/AccessDenied?ReturnUrl=%2Fproducts"
    // [Authorize("CustomClaim")]
    public class ProductsModel : PageModel
    {
        public bool UserHasCustomClaim { get; set; }

        public void OnGet()
        {
            UserHasCustomClaim = User.HasClaim("demotype", "demovalue1");
        }
    }
}