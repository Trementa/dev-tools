using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace FileLockProcessModuleTests;
public class WhoIsLockingTests
{
    [Fact]
    public void DummyTest()
    {
        var locks = FileLockProcessModule.Managers.PInvoke.FileLockUtil.WhoIsLocking("C:\\Users\\JorgenPramberg\\OneDrive - ONECO AS");

        locks.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void DummyTest2()
    {
        var locks = FileLockProcessModule.Managers.PInvoke.FileLockUtil.WhoIsLocking("C:\\Users\\JorgenPramberg");

        locks.Should().NotBeNullOrEmpty();
    }
}
