using MCServerManager.Models;
using MCServerManager.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MCServerManager.Pages.Server
{
	[Authorize]
	public class EditModel : PageModel
	{
		[BindProperty]
		public ServerDetail Input { get; set; }
		private readonly ServerService _service;

		public EditModel(ServerService serverService)
		{
			_service = serverService;
		}
		public IActionResult OnGet(Guid id)
		{
			try
			{
				var server = _service.GetServerData(id);
				Input = new ServerDetail
				{
					Name = server.Name,
					AutoStart = server.AutoStart,
					WorkDirectory = server.WorkDirectory,
					Programm = server.Programm,
					Arguments = server.Arguments,
					Address = server.Address,
					Port = server.Port
				};
			}
			catch
			{
				return NotFound();
			}

			return Page();
		}

		public IActionResult OnPost(Guid id)
		{
			try
			{
				if (ModelState.IsValid)
				{
					_service.UpdateServer(id, Input.GetServerData(id));
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
