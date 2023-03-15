using System;
using EnvDTE;
using System.IO;

namespace OpenApi.Generator.Test;

public class ProjectContainer
{
    public readonly string ProjectPath;
    public bool OverwriteExisting;
    public bool CleanFolder;

    public ProjectContainer(string projectName, string destinationFolder, bool overwriteExisting = false, bool cleanFolder = false)
    {
        OverwriteExisting = overwriteExisting;
        CleanFolder = cleanFolder;
        ProjectPath = GetProjectPath(projectName, destinationFolder);
        CleanAllFolderContent(ProjectPath);
    }

    public void AddToSolution()
    {
        var dte = (DTE)Marshal2.GetActiveObject("VisualStudio.DTE.17.0");
        dte.Solution.AddFromFile(ProjectPath);
    }

    protected string GetProjectPath(string projectName, string destinationFolder)
    {
        var dte = (DTE)Marshal2.GetActiveObject("VisualStudio.DTE.17.0");
        foreach (Project p in dte.Solution.Projects)
        {
            if (StringComparer.CurrentCultureIgnoreCase.Compare(p.Name, projectName) == 0)
                return p.FullName;
        }

        return Path.Combine(destinationFolder, projectName);
    }

    protected void CleanAllFolderContent(string projectPath)
    {
        if (!CleanFolder) return;

        var projectFolder = new DirectoryInfo(Path.GetDirectoryName(projectPath));
        foreach (DirectoryInfo dir in projectFolder.EnumerateDirectories())
        {
            dir.Delete(true);
        }
    }
}
