namespace MangaReader.Data
{
    public class Setting
    {
        [LiteDB.BsonId]
        public string Key { get; set; }
        public object Options { get; set; }

        public Setting(string key, object options)
        {
            Key = key;
            Options = options;
        }

        private Setting()
        {
            Key = "";
            Options = Key;
        }
    }
}