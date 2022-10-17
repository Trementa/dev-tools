using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;

namespace OpenApi.Generator.CSharp.SyntaxProviders;

public record ApiPath(string Path, OpenApiPathItem PathItem)
{
    public string GetOperationPath()
        => Path.StartsWith('/') ? Path.Substring(1) : Path;

    public string[] GetPathAsSubPaths()
        => Path.Split('/');

    public IEnumerable<Operation> GetOperations(ApiPath apiPath)
    {
        foreach (var keyPair in apiPath.PathItem.Operations)
        {
            yield return new Operation(keyPair.Key, keyPair.Value);
        }
    }
}
