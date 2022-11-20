using System.Management.Automation;

namespace SemVer.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "VersionsInRange")]
    public class SemVerCommand : Cmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        public string[] Versions
        { get; init; }

        [Parameter(Position = 1, Mandatory = true)]
        public string Range
        { get; init; }

        protected override void ProcessRecord()
        {
            var versions = System.Array.ConvertAll(Versions, vs => new Version(vs));
            var range = new Range(Range);
            WriteObject(range.Satisfying(versions));
        }
    }
}
