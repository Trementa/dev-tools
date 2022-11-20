using System;
using System.Collections.Generic;
using System.Linq;

namespace Templates.Client.Response;
using Types;

public class ResponseTypeMapper
{
    const string Any = "*";

    protected readonly Dictionary<(string StatusCode, (string Type, string SubType) ContentType), Type>
        ResponseTypeMap = new Dictionary<(string, (string, string)), Type>();

    public IEnumerable<string> GetAcceptedMediaTypes()
    {
        CreateDefault2XxFallbackIfOnlyOne2XxEntryExists();
        return ResponseTypeMap.Select(r => $"{r.Key.ContentType.Type}/{r.Key.ContentType.SubType}").Distinct();
    }

    protected void CreateDefault2XxFallbackIfOnlyOne2XxEntryExists()
    {
        var validResponses = ResponseTypeMap.Where(r => r.Key.StatusCode[0] == '2');
        if (validResponses.Count() == 1)
        {
            var keyvalue = validResponses.First();
            ResponseTypeMap.Remove(keyvalue.Key);
            ResponseTypeMap.Add(("2xx", (Any, Any)), keyvalue.Value);
        }
    }

    public void Add<T>(string httpStatusCode, string contentType)
    {
        var type = contentType.Split('/');
        ResponseTypeMap.Add((httpStatusCode, (type[0], type[1])), typeof(T));
    }

    public Type Get(int statusCode, string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return typeof(Void);

        var splitType = contentType.Split('/');
        var (mainType, subType) = (splitType[0], splitType[1]);
        var status = statusCode.ToString();

        return ResponseTypeEvaluator.Continue(ResponseTypeMap)
            .GetOrElse(r => r.StatusCode == status && r.ContentType.Type == mainType && r.ContentType.SubType == subType)
            .GetOrElse(r => r.StatusCode == status && r.ContentType.Type == mainType && r.ContentType.SubType == Any)
            .GetOrElse(r => r.StatusCode == status && r.ContentType.Type == Any && r.ContentType.SubType == Any)
            .GetOrElse(r => r.StatusCode == $"{status[0]}xx" && r.ContentType.Type == mainType && r.ContentType.SubType == subType)
            .GetOrElse(r => r.StatusCode == $"{status[0]}xx" && r.ContentType.Type == mainType && r.ContentType.SubType == Any)
            .GetOrElse(r => r.StatusCode == $"{status[0]}xx" && r.ContentType.Type == Any && r.ContentType.SubType == Any)
            .GetOrElse(r => r.StatusCode == "default" && r.ContentType.Type == mainType && r.ContentType.SubType == subType)
            .GetOrElse(r => r.StatusCode == "default" && r.ContentType.Type == mainType && r.ContentType.SubType == Any)
            .GetOrElse(r => r.StatusCode == "default" && r.ContentType.Type == Any && r.ContentType.SubType == Any)
            .ResultOrDefault(() => typeof(Void));
    }

    /// <summary>
    /// Could have used a more general monadic expression here.
    /// </summary>
    abstract class ResponseTypeEvaluator
    {
        public abstract ResponseTypeEvaluator GetOrElse(Func<(string StatusCode, (string Type, string SubType) ContentType), bool> eval);

        public Type ResultOrDefault(Func<Type> getDefaultType) =>
            this switch {
                Choices.Result s => s.Value,
                _ => getDefaultType()
            };

        public static ResponseTypeEvaluator Continue(Dictionary<(string StatusCode, (string Type, string SubType) ContentType), Type> collection) => new Choices.Continue(collection);

        private static class Choices
        {
            public class Continue : ResponseTypeEvaluator
            {
                readonly Dictionary<(string StatusCode, (string Type, string SubType) ContentType), Type> Value;
                public Continue(Dictionary<(string StatusCode, (string Type, string SubType) ContentType), Type> value) =>
                    Value = value;

                public override ResponseTypeEvaluator GetOrElse(Func<(string StatusCode, (string Type, string SubType) ContentType), bool> eval)
                {
                    foreach (var v in Value)
                    {
                        if (eval(v.Key))
                            return new Result(v.Value);
                    }

                    return this;
                }
            }

            public class Result : ResponseTypeEvaluator
            {
                public readonly Type Value;
                public Result(Type type) => Value = type;

                public override ResponseTypeEvaluator GetOrElse(Func<(string StatusCode, (string Type, string SubType) ContentType), bool> eval) => this;
            }
        }
    }
}
