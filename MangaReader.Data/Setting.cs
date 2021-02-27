namespace MangaReader.Data
{
    public class Setting
    {
        [LiteDB.BsonId]
        public string Key { get; set; }
        public object Options { get; set; }
    }
}