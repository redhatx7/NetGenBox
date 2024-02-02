using System.IO;
using System.Threading.Tasks;

namespace NetGenBox.UI;

public class XilinxModuleModel
{
    public string ModuleName { get; set; }
    public string ModulePath { get; set; }

    

    public async Task<string> ReadModuleTextAsync()
    {
        if (File.Exists(ModulePath))
        {
            return await File.ReadAllTextAsync(ModulePath);
        }

        return string.Empty;
    }
    
}