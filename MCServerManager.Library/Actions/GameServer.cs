using MCServerManager.Library.Data.Model;
using MCServerManager.Library.Data.Tools;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static MCServerManager.Library.Data.Model.ServerStatus;

namespace MCServerManager.Library.Actions
{
	/// <summary>
	/// Работа с сервером.
	/// </summary>
	public class GameServer
	{
		/// <summary>
		/// Информация о серверном приложении.
		/// </summary>
		[JsonIgnore]
		public ServerData ServerData { get; private set; }

		/// <summary>
		/// Идентификатор приложения.
		/// </summary>
		public Guid Id { get { return ServerData.Id; } }

		/// <summary>
		/// Автозапуск.
		/// </summary>
		public bool AutoStart { get { return ServerData.AutoStart; } }

		/// <summary>
		/// Название приложения.
		/// </summary>
		public string Name { get { return ServerData.Name; } }

		/// <summary>
		/// Расположение приложения.
		/// </summary>
		public string WorkDirectory { get { return ServerData.WorkDirectory; } }

		/// <summary>
		/// Программа для запуска.
		/// </summary>
		public string Programm { get { return ServerData.Programm; } }

		/// <summary>
		/// Аргументы запуска.
		/// </summary>
		public string Arguments { get { return ServerData.Arguments; } }

		/// <summary>
		/// Адрес сервера/ip.
		/// </summary>
		public string Address { get { return ServerData.Address; } }

		/// <summary>
		/// Используемый порт.
		/// </summary>
		public int? Port { get { return ServerData.Port; } }

		/// <summary>
		/// Состояние сервера.
		/// </summary>
		[JsonIgnore]
		public Status State { get; private set; }

		/// <summary>
		/// Процесс, управляющий серверным приложением.
		/// </summary>
		private Process _process;

		/// <summary>
		/// Список игроков сервера (версия списка, список игроков).
		/// </summary>
		public (Guid ListVersion, List<string> PlayersList) Players = (Guid.NewGuid(), new());

		/// <summary>
		/// Делегат события завершения запуска серверного приложения.
		/// </summary>
		/// <param name="id">Идентификатор сервера.</param>
		public delegate void ServerStartedEventHandler(Guid id);

		/// <summary>
		/// Cобытие завершения запуска серверного приложения.
		/// </summary>
		public event ServerStartedEventHandler ServerStarted;

		/// <summary>
		/// Делегат события завершения работы серверного приложения.
		/// </summary>
		/// <param name="id">Идентификатор сервера.</param>
		public delegate void StoppedServerEventHandler(Guid id);

		/// <summary>
		/// Cобытие завершения работы серверного приложения.
		/// </summary>
		public event StoppedServerEventHandler ClosedServer;

		/// <summary>
		/// Делегат события завершения работы серверного приложения при перезагрузке.
		/// </summary>
		delegate void ServerOffEventHandler();

		/// <summary>
		/// Cобытие завершения работы серверного приложения при перезагрузке.
		/// </summary>
		event ServerOffEventHandler ServerOff;

		/// <summary>
		/// Тип сообщения от сервера.
		/// </summary>
		private enum ServerMessageType
		{
			None,
			ServerStarted,
			PlayerJoined,
			PlayerLeft
		};

		/// <summary>
		/// Словарь регулярных выражений для определения типа сообщения сервера.
		/// </summary>
		private readonly Dictionary<ServerMessageType, Regex> RegularExpressions = new()
		{
			[ServerMessageType.ServerStarted] = new Regex(@"\[\d*:\d*:\d*\]\s\[Server\sthread/INFO\]:\sDone\s\(.*\)!\sFor\shelp,\stype\s\x22help\x22", RegexOptions.Compiled),
			[ServerMessageType.PlayerJoined] = new Regex(@"\[\d*:\d*:\d*\]\s\[Server\sthread/INFO\]:\s([^<>\s]*)\sjoined\sthe\sgame", RegexOptions.Compiled),
			[ServerMessageType.PlayerLeft] = new Regex(@"\[\d*:\d*:\d*\]\s\[Server\sthread/INFO\]:\s([^<>\s]*)\sleft\sthe\sgame", RegexOptions.Compiled)
		};

		/// <summary>
		/// Конструктор с параметром
		/// </summary>
		/// <param name="data">Информания о серверном приложении.</param>
		public GameServer(ServerData data)
		{
			CheckServerData(data);

			ServerData = data;
			State = Status.Off;
		}

		/// <summary>
		/// Обновляет настройки серверного приложения.
		/// </summary>
		/// <param name="data">Информания о серверном приложении.</param>
		public void UpdateData(ServerData data)
		{
			if (Id != data.Id)
			{
				throw new Exception("Идентификаторы не совпадают");
			}

			CheckServerData(data);
			ServerData = data;
		}

		/// <summary>
		/// Запускает серверное приложение.
		/// </summary>
		public async void Start()
		{
			if (State != Status.Off && State != Status.Error && State != Status.Reboot)
			{
				return;
			}

			if (State != Status.Reboot)
			{
				State = Status.Launch;
			}

			await Task.Run(() => StartServer());
		}

		/// <summary>
		/// Запускает процесс, управляющий серверным приложением.
		/// </summary>
		private void StartServer()
		{
			_process = new Process();
			_process.StartInfo.WorkingDirectory = WorkDirectory;
			_process.StartInfo.FileName = Programm;
			_process.StartInfo.Arguments = Arguments;
			_process.StartInfo.UseShellExecute = false;
			_process.StartInfo.RedirectStandardInput = true;
			_process.StartInfo.RedirectStandardOutput = true;
			_process.EnableRaisingEvents = true;

			_process.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
			{
				GetServerMessage(e.Data);
			});

			_process.Exited += new EventHandler((sender, e) =>
			{
				ProcessClosed();
				ServerOff?.Invoke();
			});

			_process.Start();
			_process.BeginOutputReadLine();
		}

		/// <summary>
		/// Завершает работу серверого приложения.
		/// </summary>
		public void Stop()
		{
			if (State != Status.Run && State != Status.Reboot)
			{
				return;
			}

			if (State != Status.Reboot)
			{
				State = Status.Shutdown;
			}

			_process.StandardInput.WriteLine("stop");
		}

		/// <summary>
		/// Очищает ресурсы процесса после завершения работы.
		/// </summary>
		private void ProcessClosed()
		{
			_process.Dispose();
			Players.PlayersList.Clear();
			Players.ListVersion = Guid.NewGuid();
			if (State == Status.Launch)
			{
				State = Status.Error;
				return;
			}

			if (State != Status.Reboot)
			{
				State = Status.Off;
				// Вызывается событие отключения серверного приложения
				ClosedServer?.Invoke(Id);
			}
		}

		/// <summary>
		/// Перезапускает серверное приложение.
		/// </summary>
		public void Restart()
		{
			if (State != Status.Run)
			{
				return;
			}

			State = Status.Reboot;
			ServerOff += RunOffServer;
			Stop();
		}

		/// <summary>
		/// Запускает серверное приложение после завершения работы при перезапуске.
		/// </summary>
		private void RunOffServer()
		{
			ServerOff -= RunOffServer;
			Start();
		}

		/// <summary>
		/// Отключает серверное приложение не дожидаясь завершения работы.
		/// </summary>
		public void Close()
		{
			if (State == Status.Off || State == Status.Error)
			{
				return;
			}

			if (State == Status.Reboot)
			{
				ServerOff -= RunOffServer;
			}

			_process.Kill();
		}

		/// <summary>
		/// Выводит сообщение от серверного приложения.
		/// </summary>
		/// <param name="message">Текст сообщения.</param>
		private void GetServerMessage(string message)
		{
			if (string.IsNullOrEmpty(message))
			{
				return;
			}

			Console.WriteLine(message);
			CheckServerMessage(message);
		}

		/// <summary>
		/// Проверяет сообщение от серверного приложения и выполняет необходимые действия.
		/// </summary>
		/// <param name="message">Текст сообщения.</param>
		private void CheckServerMessage(string message)
		{
			switch (RegularExpressions.Where(Pair => Pair.Value.IsMatch(message)).Select(Pair => Pair.Key).DefaultIfEmpty(ServerMessageType.None).FirstOrDefault())
			{
				case ServerMessageType.ServerStarted:
					//Сервер запущен
					State = Status.Run;
					//Вызывается событие завершения запуска серверного приложения
					ServerStarted?.Invoke(Id);
					break;
				case ServerMessageType.PlayerJoined:
					//Добавление игрока в список
					Players.PlayersList.Add(RegularExpressions[ServerMessageType.PlayerJoined].Match(message).Groups[1].Value);
					Players.ListVersion = Guid.NewGuid();
					break;
				case ServerMessageType.PlayerLeft:
					//Удаление игрока из списка
					Players.PlayersList.Remove(RegularExpressions[ServerMessageType.PlayerLeft].Match(message).Groups[1].Value);
					Players.ListVersion = Guid.NewGuid();
					break;
			}
		}

		/// <summary>
		/// Отправляет команду в серверное приложение.
		/// </summary>
		/// <param name="message">Команда для серверного приложения.</param>
		public void SendServerCommand(string message)
		{
			if (State != Status.Run)
			{
				return;
			}

			if (string.IsNullOrEmpty(message))
			{
				return;
			}

			_process.StandardInput.WriteLine(message);
		}

		/// <summary>
		/// Проверяет данные серверного приложения.
		/// </summary>
		/// <param name="data">Информания о серверном приложении.</param>
		public void CheckServerData(ServerData data)
		{
			if (string.IsNullOrEmpty(data.Programm))
			{
				throw new ArgumentNullException(nameof(data.Programm), "Программа для запуска не задана");
			}

			if (string.IsNullOrEmpty(data.WorkDirectory))
			{
				throw new ArgumentNullException(nameof(data.WorkDirectory), "Директория не задана.");
			}

			if (!Directory.Exists(data.WorkDirectory))
			{
				throw new DirectoryNotFoundException($"Указанная директория не найдена: {data.WorkDirectory}");
			}

			if (data.Port != null)
			{
				if (data.Port <= 1023 || data.Port >= 65535)
				{
					throw new ArgumentOutOfRangeException(nameof(data.Port), "Значения порта задано вне допустимого диапазона 1024 - 65535");
				}
			}
		}
	}
}
