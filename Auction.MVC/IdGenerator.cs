namespace Auction.MVC {
    public class IdGenerator {
        private long _id;
        public IdGenerator() {
            _id = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
         public long GetId() {
            return _id;
        }
    }
}
