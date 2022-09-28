using MCServerManager.Library.Actions;
using MCServerManager.Library.Data.Model;
using MCServerManager.Library.Data.Tools;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Collections.Generic;
using System.IO;

namespace MCServerManager.Service
{
	/// <summary>
	/// Сервис управляния работой серверов.
	/// </summary>
	public class ServerService
	{
		/// <summary>
		/// Название файла настроек.
		/// </summary>
		private const string _keyGetFileSettings = "GameServers";

		/// <summary>
		/// Путь к файлу с информацией о настройках серверов.
		/// </summary>
		private readonly string _pathFileSettings;

		/// <summary>
		/// Список серверов.
		/// </summary>
		public List<GameServer> servers { get; private set; }

		/// <summary>
		/// Конфигурация.
		/// </summary>
		private readonly IConfiguration _configuration;

		public ServerService(IConfiguration configuration)
		{
			_configuration = configuration;
			_pathFileSettings = _configuration.GetValue<string>(_keyGetFileSettings);
			servers = new List<GameServer>();

			AutoRun();
		}

		/// <summary>
		/// Добавляет экземпляры серверов в список и запускает их.
		/// </summary>
		private void AutoRun()
		{
			var list = LoadServerData();

			foreach (var server in list)
			{
				try
				{
					AddServer(server);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}

			foreach (var server in servers)
			{
				if (server.serverData.AutoStart)
				{
					server.Start();
				}
			}
		}

		/// <summary>
		/// Создает новый сервер.
		/// </summary>
		/// <param name="name">Название.</param>
		/// <param name="autoStart">Автозапуск.</param>
		/// <param name="workDirectory">Расположение сервера.</param>
		/// <param name="programm">Программа для запуска.</param>
		/// <param name="arguments">Аргументы запуска.</param>
		/// <param name="addres">Адрес сервера.</param>
		/// <param name="port">Используемый порт.</param>
		/// <returns>Идентификатор сервера.</returns>
		public Guid CreateServer(string name, bool autoStart, string workDirectory, string programm,
			string arguments, string addres, int port)
		{
			var id = Guid.NewGuid();
			AddServer(new ServerData()
			{
				Id = id,
				Name = name,
				AutoStart = autoStart,
				WorkDirectory = workDirectory,
				Programm = programm,
				Arguments = arguments,
				Addres = addres,
				Port = port
			});

			return id;
		}

		/// <summary>
		/// Добавить новый сервер.
		/// </summary>
		/// <param name="serverData">Информация о сервере.</param>
		/// <exception cref="Exception">Директория или порт используются другим сервером.</exception>
		private void AddServer(ServerData serverData)
		{
			if (servers.Where(x => x.Port == serverData.Port).Any())
			{
				throw new Exception($"Порт {serverData.Port} занят другим сервером");
			}

			if (servers.Where(x => x.serverData.WorkDirectory == serverData.WorkDirectory).Any())
			{
				throw new Exception($"Указанная директория используется другим сервером");
			}

			servers.Add(new GameServer(serverData));

			SaveServerData();
		}

		//TODO: Реализовать удаление.
		/// <summary>
		/// Удалить указанный сервер.
		/// </summary>
		/// <param name="id">Идентификатор сервера.</param>
		/// <exception cref="NotImplementedException">Метод еще не реализован.</exception>
		public void DeleteServer(Guid id)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Обновить информацию указанного сервера.
		/// </summary>
		/// <param name="id">Идентификатор сервера.</param>
		/// <param name="serverData">Информация о сервере.</param>
		/// <exception cref="ArgumentException">Идентификаторы id и serverData.Id не совпадают.</exception>
		public void UpdateServer(Guid id, ServerData serverData)
		{
			var exemplar = GetServer(id);

			if (id != serverData.Id)
			{
				throw new ArgumentException("Ошибка идентификатора настроек");
			}

			exemplar.UpdateData(serverData);

			SaveServerData();
		}

		/// <summary>
		/// Запустить указанный сервер.
		/// </summary>
		/// <param name="id">Идентификатор сервера.</param>
		public void StartServer(Guid id)
		{
			GetServer(id).Start();
		}

		/// <summary>
		/// Остановить указанный сервер.
		/// </summary>
		/// <param name="id">Идентификатор сервера.</param>
		public void StopServer(Guid id)
		{
			GetServer(id).Stop();
		}

		//TODO: Реализовать метод.
		/// <summary>
		/// Перезагружает указанный сервер.
		/// </summary>
		/// <param name="id">Идентификатор сервера.</param>
		/// <exception cref="NotImplementedException">Метод еще не реализован.</exception>
		public void Restart(Guid id)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Загружает информацию о серверах.
		/// </summary>
		/// <returns>Список данных о серверах.</returns>
		private List<ServerData> LoadServerData()
		{
			return JsonTool.LoadJsonDataFromFile<List<ServerData>>(_pathFileSettings);
		}

		/// <summary>
		/// Сохраняет информацию о серверах.
		/// </summary>
		private void SaveServerData()
		{
			var list = new List<ServerData>();

			servers.ForEach(server => list.Add(server.serverData));

			JsonTool.SaveJsonDataToFile(_pathFileSettings, JsonTool.Serialize(list));
		}

		/// <summary>
		/// Получить экземпляе класса сервера по идентификатору.
		/// </summary>
		/// <param name="id">Идентификатор сервера.</param>
		/// <returns>Экземпляе класса</returns>
		/// <exception cref="Exception">Указанный сервер не найден.</exception>
		private GameServer GetServer(Guid id)
		{
			var exemplar = servers.Where(x => x.Id == id).FirstOrDefault();

			if (exemplar == null)
			{
				throw new Exception("Указанный сервер не найден");
			}

			return exemplar;
		}
	}
}
