using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;

namespace Trementa.OpenApi.Generator.Generators.Models;

public record struct Schema(OpenApiSchema OpenApiSchema);
