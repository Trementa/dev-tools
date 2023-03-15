using System;

namespace OpenApi.Generator.Test
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class SolutionFileAttribute : Attribute
    {
        public readonly string SolutionFile;

        public SolutionFileAttribute(string solutionPath) =>
            SolutionFile = solutionPath;
    }
}
