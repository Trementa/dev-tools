using System;
using System.Collections.Generic;
using System.Text;

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
