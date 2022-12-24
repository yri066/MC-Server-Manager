using MCServerManager.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace MCServerManager.Pages.Server
{
	[Authorize]
	[Route("/Server/{id:guid}/[action]")]
	[ApiController]
	public class ActionController : ControllerBase
	{
		private readonly ServerService _serverService;

		public ActionController(ServerService serverService)
		{
			_serverService = serverService;
		}

		/// <summary>
		/// Получить информацию о сервере.
		/// </summary>
		/// <param name="id">Идентификатор сервера.</param>
		/// <returns>Информация о сервере.</returns>
		public object GetStatus(Guid id)
		{
			try
			{
				var server = _serverService.GetServer(id);

				return new
				{
					Status = server.State.ToString(),
					PlayersListVersion = server.Players.ListVersion
				};
			}
			catch (Exception ex)
			{
				HttpContext.Response.StatusCode = 404;
				return new { errorText = ex.Message };
			}
		}

		/// <summary>
		/// Запустить сервер.
		/// </summary>
		/// <param name="id">Идентификатор сервера.</param>
		/// <returns>Информация о сервере.</returns>
		public object Start(Guid id)
		{
			try
			{
				_serverService.StartServer(id);
			}
			catch (Exception ex)
			{
				HttpContext.Response.StatusCode = 404;
				return ex.Message;
			}

			return GetStatus(id);
		}

		/// <summary>
		/// Перезапустить сервер.
		/// </summary>
		/// <param name="id">Идентификатор сервера.</param>
		/// <returns>Информация о сервере.</returns>
		public object Restart(Guid id)
		{
			try
			{
				_serverService.Restart(id);
			}
			catch (Exception ex)
			{
				HttpContext.Response.StatusCode = 404;
				return ex.Message;
			}

			return GetStatus(id);
		}

		/// <summary>
		/// Остановить сервер.
		/// </summary>
		/// <param name="id">Идентификатор сервера.</param>
		/// <returns>Информация о сервере.</returns>
		public object Stop(Guid id)
		{
			try
			{
				_serverService.StopServer(id);
			}
			catch (Exception ex)
			{
				HttpContext.Response.StatusCode = 404;
				return ex.Message;
			}

			return GetStatus(id);
		}

		/// <summary>
		/// Выключить сервер.
		/// </summary>
		/// <param name="id">Идентификатор сервера.</param>
		/// <returns>Информация о сервере.</returns>
		public object Close(Guid id)
		{
			try
			{
				_serverService.CloseServer(id);
			}
			catch (Exception ex)
			{
				HttpContext.Response.StatusCode = 404;
				return ex.Message;
			}

			return GetStatus(id);
		}

		/// <summary>
		/// Получить список пользователей.
		/// </summary>
		/// <param name="id">Идентификатор сервера.</param>
		/// <returns>Список пользователей.</returns>
		public object GetUserList(Guid id)
		{
			try
			{
				return _serverService.GetServer(id).Players.PlayersList;
			}
			catch (Exception ex)
			{
				HttpContext.Response.StatusCode = 404;
				return new { errorText = ex.Message };
			}
		}
	}
}
