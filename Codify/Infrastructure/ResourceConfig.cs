using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codify.Infrastructure
{
    public static class ResourceConfig
    {
        // Format: [ProjectName].[Folder].[SubFolder].[FileName]
        private const string BasePath = "Codify.UI.ToolWindows.Resources";

        public static string ChatHtml => $"{BasePath}.ChatView.html";
        public static string ChatCss => $"{BasePath}.ChatView.style.css";
        public static string ChatJs => $"{BasePath}.ChatView.script.js";
    }
}
