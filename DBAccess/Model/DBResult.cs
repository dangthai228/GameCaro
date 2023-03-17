namespace Caro.Game.DBAccess.Model
{
    public class DBResult<T>
    {
        public long ResponseStatus { get; set; }
        public string ResponseText { get; set; }
        public T? Data { get; set; }
    }
}
