using FluentAssertions;
using OpenApi.Generator.CSharp.SyntaxProviders;
using Xunit;

namespace OpenApi.Generator.Test;

public class ExtensionTests
{
    [Fact]
    public void GetValidVariableNameTest()
    {
        var validName = "$select".GetValidVariableName();
        validName.ScrubbedName.Should().Be("_select");
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
