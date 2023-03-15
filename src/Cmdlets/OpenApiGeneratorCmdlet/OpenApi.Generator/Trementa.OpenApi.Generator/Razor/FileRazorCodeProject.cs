#nullable enable
using System.Reflection;
using RazorLight.Razor;

namespace Trementa.OpenApi.Generator;

public class RazorTemplatesProject : RazorLightProject
{
    protected readonly EmbeddedRazorProject? EmbeddedRazorProject;
    protected readonly FileSystemRazorProject? FileSystemRazorProject;
    protected readonly Options Options;

    public RazorTemplatesProject(Options options)
    {
        Options = options;
        if (Directory.Exists(options.TemplateFolder))
            FileSystemRazorProject = new FileSystemRazorProject(options.TemplateFolder, options.CsTemplate);

        if (File.Exists(options.TemplateAssembly))
        {
            var assembly = Assembly.LoadFrom(options.TemplateAssembly);
            EmbeddedRazorProject = new EmbeddedRazorProject(assembly) {
                Extension = Options.CsTemplate
            };
        }
    }

    public override async Task<RazorLightProjectItem> GetItemAsync(string templateKey)
    {
        var item = await GetFileTemplate(templateKey);
        if ((item == null || !item.Exists) && EmbeddedRazorProject != null)
            item = await EmbeddedRazorProject.GetItemAsync(templateKey);

        if (!item.Exists)
            throw new Exception($"Template with key {templateKey} not found");

        return item;
    }

    protected async Task<RazorLightProjectItem?> GetFileTemplate(string templateKey)
    {
        if (FileSystemRazorProject == null)
            return null;
        return await FileSystemRazorProject.GetItemAsync(templateKey);
    }

    public override Task<IEnumerable<RazorLightProjectItem>> GetImportsAsync(string templateKey) =>
        Task.FromResult(Enumerable.Empty<RazorLightProjectItem>());
}
