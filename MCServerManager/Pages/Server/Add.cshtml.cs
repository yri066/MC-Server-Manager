using MCServerManager.Models;
using MCServerManager.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MCServerManager.Pages.Server
{
	[Authorize]
	public class AddModel : PageModel
	{
		[BindProperty]
		public ServerDetail Input { get; set; }
		private readonly ServerService _service;

		public AddModel(ServerService service)
		{
			_service = service;
		}
		public void OnGet()
		{
			Input = new();
		}

		public IActionResult OnPost()
		{
			try
			{
				if (ModelState.IsValid)
				{
					var id = _service.CreateServer(Input.Name, Input.AutoStart, Input.WorkDirectory, Input.Programm,
						Input.Arguments, Input.Address, Input.Port);
					return RedirectToPage("Index", new { id });
				}
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(
					string.Empty,
					ex.Message
					);
			}

			return Page();
		}
	}
}
