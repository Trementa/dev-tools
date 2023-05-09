using System.Linq;
using System.Threading;
using FluentAssertions;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Validations;
using OpenApi.Generator.CSharp.SyntaxProviders;
using Trementa.OpenApi.Generator;
using Xunit;
namespace OpenApi.Generator.Test;

public class SchemaTests
{
    [Fact]
    public async void GetTypeFromSchema()
    {
        var definitionSource = new DefinitionSource("https://api-spec.hydrogrid.eu/swagger.yaml");
        var source = await definitionSource.ReadAsync(CancellationToken.None);
        var document = new OpenApiStreamReader(new OpenApiReaderSettings {
            ReferenceResolution = ReferenceResolutionSetting.ResolveLocalReferences,
            RuleSet = ValidationRuleSet.GetDefaultRuleSet()
        }).Read(source, out var result);

        foreach(var schema in document.Components.Schemas)
        {
            var t = schema;
        }

    }

    [Fact]
    public void GetValidVariableNameWithReplacementMapTest()
    {
        var validName = "$select".GetValidVariableName(matchAndReplaceMap: new[] { ('$', "") });
        validName.ScrubbedName.Should().Be("select");
    }

    [Fact]
    public void GetValidVariableNameTestWithDefaultReplacementCharacter()
    {
        var validName = "$select!".GetValidVariableName(defaultReplacementCharacter: '*');
        validName.ScrubbedName.Should().Be("*select*");
    }

    [Fact]
    public void GetValidVariableNameTestWithDefaultReplacementCharacterAndReplacementMap()
    {
        var validName = "$select!?Something".GetValidVariableName(defaultReplacementCharacter: '*', matchAndReplaceMap: new[] { ('$', ""), ('?', "QuestionMark") });
        validName.ScrubbedName.Should().Be("select*QuestionMarkSomething");
    }
}
