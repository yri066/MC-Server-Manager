using MCServerManager.Library.Data.Model;
using static MCServerManager.Library.Data.Model.ServerStatus;

namespace MCServerManager.Library.Actions
{
    /// <summary>
    /// Работа с сервером
    /// </summary>
    ///
    public class GameServer
    {
        private ServerData _data { get; set; }
        public Guid Id { get { return _data.Id; } }
        public string Name { get { return _data.Name; } }
        public string Programm { get { return _data.Programm; } }
        public string Arguments { get { return _data.Arguments; } }
        public string WorkingDirectory { get { return _data.WorkDirectory; } }
        public string Addres { get { return Addres; } }
        public int Port { get { return Port; } }
        public Status State { get; private set; }

        /// <summary>
        /// Конструктор с параметром
        /// </summary>
        /// <param name="data">Информания о экземпляре приложения</param>
        public GameServer(ServerData data)
        {
            if (string.IsNullOrEmpty(data.Programm))
            {
                throw new ArgumentNullException(nameof(data.Programm));
            }

            if (string.IsNullOrEmpty(data.Arguments))
            {
                throw new ArgumentNullException(nameof(data.Arguments));
            }

            if (string.IsNullOrEmpty(data.WorkDirectory))
            {
                throw new ArgumentNullException(nameof(data.WorkDirectory));
            }

            if (!Directory.Exists(data.WorkDirectory))
            {
                //throw new DirectoryNotFoundException(nameof(data.WorkDirectory));
            }

            this._data = data;

            State = Status.Off;

            if (data.AutoStart)
            {
                Start();
            }
        }

        /// <summary>
        /// Запускает приложение
        /// </summary>
        public void Start()
        {

        }

        /// <summary>
        /// Завершает работу приложения
        /// </summary>
        public void Stop()
        {

        }

        /// <summary>
        /// Перезапускает сервер
        /// </summary>
        public void Restart()
        {

        }

        /// <summary>
        /// Обновляет настройки приложения
        /// </summary>
        /// <param name="data">Информания о экземпляре приложения</param>
        public void UpdateData(ServerData data)
        {

        }
    }
}
