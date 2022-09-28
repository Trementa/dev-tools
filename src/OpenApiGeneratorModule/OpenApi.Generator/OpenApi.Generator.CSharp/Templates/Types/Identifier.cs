namespace GK.WebLib.Types
{
    public class Identifier<TModel> : Primitive<string> where TModel : IJsonModel
    {
        protected Identifier(string value) : base(value)
        { }

        public static explicit operator Identifier<TModel>(string id) => new Identifier<TModel>(id);
    }
}
