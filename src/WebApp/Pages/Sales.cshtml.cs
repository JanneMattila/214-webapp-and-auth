using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages
{
    [Authorize]
    public class SalesModel : PageModel
    {
        public string Name { get; set; }

        public void OnGet()
        {
            Name = User.Identity.Name;
        }
    }
}
