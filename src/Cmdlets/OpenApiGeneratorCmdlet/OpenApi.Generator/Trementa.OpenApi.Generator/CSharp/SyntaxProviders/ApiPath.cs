using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using OpenApi.Generator.CSharp.SyntaxProviders;
using Trementa.OpenApi.Generator;

namespace Trementa.OpenApi.Generator.CSharp.SyntaxProviders;

public record ApiPath(string Path, OpenApiPathItem PathItem, Options Options)
{
    public string GetOperationPath()
        => Path.StartsWith('/') ? Path.Substring(1) : Path;

    public string[] GetPathAsSubPaths()
        => Path.Split('/');

    public IEnumerable<Operation> GetOperations()
    {
        foreach (var keyPair in PathItem.Operations)
            yield return new Operation(keyPair.Key, keyPair.Value, Options);
    }
}
