namespace OpenApi.Library.Types
{
    public class ClientIdentifier : Primitive<string>
    {
        protected static readonly ClientIdentifier EmptyInstance = new ClientIdentifier(string.Empty);
        public ClientIdentifier(string value) : base(value) { }
        public static ClientIdentifier New(string value) => new ClientIdentifier(value);
        public static ClientIdentifier Empty => EmptyInstance;
    }
}
