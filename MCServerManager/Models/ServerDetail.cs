using MCServerManager.Library.Data.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using MCServerManager.Data.ValidationAttributes;

namespace MCServerManager.Models
{
	/// <summary>
	/// Данные о приложении
	/// </summary>
	public class ServerDetail
	{
		/// <summary>
		/// Название приложения
		/// </summary>
		[Required, StringLength(100), DisplayName("Название сервера")]
		public string Name { get; set; }

		/// <summary>
		/// Автозапуск
		/// </summary>
		[Required, DisplayName("Автозапуск")]
		public bool AutoStart { get; set; }

		/// <summary>
		/// Расположение приложения
		/// </summary>
		[Required, DirectoryExists, DisplayName("Директория расположения сервера")]
		public string WorkDirectory { get; set; }

		/// <summary>
		/// Программа для запуска
		/// </summary>
		[Required, StringLength(100), DisplayName("Программа для запуска")]
		public string Programm { get; set; }

		/// <summary>
		/// Аргументы запуска
		/// </summary>
		[StringLength(100), DisplayName("Аргументы для запуска")]
		public string? Arguments { get; set; }

		/// <summary>
		/// Адрес сервера(ip)
		/// </summary>
		[Required, StringLength(100), DisplayName("Адрес сервера")]
		public string Address { get; set; }

		/// <summary>
		/// Используемый порт
		/// </summary>
		[Range(1024, 65535), DisplayName("Используемый порт")]
		public int? Port { get; set; }

		public ServerData GetServerData(Guid id)
		{
			return new ServerData
			{
				Id = id,
				Name = Name,
				AutoStart = AutoStart,
				WorkDirectory = WorkDirectory,
				Programm = Programm,
				Arguments = Arguments,
				Address = Address,
				Port = Port
			};
		}
	}
}
