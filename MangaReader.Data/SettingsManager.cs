namespace MangaReader.Data
{
    public class SettingsManager
    {
        private readonly DataDb dataDb;

        public SettingsManager(DataDb dataDb)
        {
            this.dataDb = dataDb;
        }

        public TOptions? Get<TOptions>(string key)
            where TOptions : class
        {
            return dataDb.Settings.FindOne(s => s.Key == key)?.Options as TOptions;
        }
        public void Save<TOptions>(string key, TOptions options)
            where TOptions : class
        {
            Setting settings = new Setting(key, options);
            var found = dataDb.Settings.Update(settings);
            if (!found)
            {
                dataDb.Settings.Insert(settings);
            }
        }
    }
}