using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCServerManager.Library.Actions;
using MCServerManager.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MCServerManager.Pages.Server
{
	[Authorize]
	public class IndexModel : PageModel
	{
		private readonly ServerService _serverService;
		public GameServer Exemplar { get; private set; }

		public IndexModel(ServerService serverService)
		{
			_serverService = serverService;
		}

		public IActionResult OnGet(Guid id)
		{
			try
			{
				Exemplar = _serverService.GetServer(id);
			}
			catch
			{
				return NotFound();
			}

			return Page();
		}

		public IActionResult OnGetDelete(Guid id)
		{
			try
			{
				_serverService.DeleteServer(id);
			}
			catch
			{ }

			return RedirectToPage("List");
		}
	}
}
