using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BeerZdec.Services
{
    public class AppInfoService : IAppInfoService
    {
        private readonly string _filePath;

        public AppInfoService()
        {
            // Путь к файлу в папке приложения
            _filePath = Path.Combine(AppContext.BaseDirectory, "Resources", "about.txt");
        }

        public string GetAboutText()
        {
            if (File.Exists(_filePath))
            {
                return File.ReadAllText(_filePath, Encoding.UTF8);
            }

            return "Информация о приложении недоступна";
        }
    }
}
