namespace ConnectingLib
{
    /// <summary>
    /// класс для получения строки подключения к базе данных.
    /// елси база данных отсутствует - добавляет её к проекту.
    /// </summary>
    public static class SQLiteBaseBuilder
    {
        public static string GetConnectionString(AppDomain domain)
        {
            var folderPath = domain.BaseDirectory + "/BlogDatabase.db";

            if(!File.Exists(folderPath))
            {
                File.Create(folderPath);
            }

            return "Data Source=" + folderPath;
        }
    }
}