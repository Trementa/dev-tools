using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RazorLight.Razor;
#nullable enable

namespace OpenApi.Generator
{
    public class RazorCodeProject : RazorLightProject
    {
        protected readonly EmbeddedRazorProject EmbeddedRazorProject =
            new EmbeddedRazorProject(Assembly.GetExecutingAssembly(), "");

        protected readonly FileSystemRazorProject? FileSystemRazorProject;
        protected readonly Options Options;

        public RazorCodeProject(Options options)
        {
            Options = options;
            if (Directory.Exists(options.TemplateDirectory))
                FileSystemRazorProject = new FileSystemRazorProject(options.TemplateDirectory, options.CsTemplate);
            EmbeddedRazorProject.Extension = Options.CsTemplate;
        }

        public override async Task<RazorLightProjectItem> GetItemAsync(string templateKey)
        {
            var item = await GetFileTemplate(templateKey);
            if (item == null || !item.Exists)
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
}

