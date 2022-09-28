using FluentAssertions;
using Xunit;

namespace OpenApi.Generator.Test
{
    public class ResponseMapperTests
    {
        public class Text
        { }
        public class Json
        { }
        public class JsonGeneric
        { }
        public class Generic
        { }
        public class Any
        { }
        public class Fallback
        { }

        protected readonly ResponseMapper ResponseMap = new ResponseMapper();

        public ResponseMapperTests()
        {
            ResponseMap.Add<Generic>("200", "*/*");
            ResponseMap.Add<Text>("201", "application/document");
            ResponseMap.Add<JsonGeneric>("2xx", "application/json");
            ResponseMap.Add<Fallback>("2xx", "application/*");
            ResponseMap.Add<Any>("201", "application/*");
            ResponseMap.Add<Json>("201", "application/json");
        }

        [Fact]
        public void GetAcceptedMediaTypesTest()
        {
            ResponseMap.GetAcceptedMediaTypes().Should().HaveCount(4);
        }

        [Fact]
        public void GetTypeJsonTest()
        {
            ResponseMap.Get(201, "application/json").Should().Be(typeof(Json));
        }

        [Fact]
        public void GetTypeAnyTest()
        {
            ResponseMap.Get(201, "application/json32").Should().Be(typeof(Any));
        }

        [Fact]
        public void GetTypeTextTest()
        {
            ResponseMap.Get(201, "application/document").Should().Be(typeof(Text));
        }

        [Fact]
        public void GetTypeGenericTest()
        {
            ResponseMap.Get(200, "application/document").Should().Be(typeof(Generic));
        }

        [Fact]
        public void GetTypeJsonGenericTest()
        {
            ResponseMap.Get(250, "application/json").Should().Be(typeof(JsonGeneric));
        }

        [Fact]
        public void GetTypeFallbackTest()
        {
            ResponseMap.Get(259, "application/xml").Should().Be(typeof(Fallback));
        }

        [Fact]
        public void NoTypeFoundGetUnknownTest()
        {
            ResponseMap.Get(250, "app/xml").Should().Be(typeof(Void));
        }
    }
}
