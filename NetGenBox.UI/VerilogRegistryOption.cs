using System.Collections.Generic;
using System.IO;
using System.Linq;
using TextMateSharp.Grammars;
using TextMateSharp.Internal.Grammars.Reader;
using TextMateSharp.Internal.Themes.Reader;
using TextMateSharp.Internal.Types;
using TextMateSharp.Registry;
using TextMateSharp.Themes;
using TextMateSharp.Grammars.Resources;

namespace NetGenBox.UI;

public class VerilogRegistryOption : IRegistryOptions
{

    //private Dictionary<string, GrammarDefinition> _availableGrammars = new Dictionary<string, GrammarDefinition>();
    private readonly RegistryOptions _options;

    public VerilogRegistryOption(ThemeName defaultTheme)
    {
      _options = new RegistryOptions(defaultTheme);
    }




    public ICollection<string> GetInjections(string scopeName) => _options.GetInjections(scopeName);
    
    public IRawTheme GetDefaultTheme()
    {
      return _options.GetDefaultTheme();
    }

    public IRawTheme GetTheme(string scopeName)
    {
      return _options.GetTheme(scopeName);
    }

    public IRawGrammar GetGrammar(string scopeName)
    {

      Stream stream;
      if (scopeName == "source.verilog")
      {
        stream = new FileStream("verilog.tmLanguage.json", FileMode.Open);
        if (stream == null)
          return (IRawGrammar) null;
        using (stream)
        {
          using (StreamReader reader = new StreamReader(stream))
            return GrammarReader.ReadGrammarSync(reader);
        }
      }

      return _options.GetGrammar(scopeName);
      
    }
    
    private string GetThemeFile(ThemeName name)
    {
      switch (name)
      {
        case ThemeName.Abbys:
          return "abyss-color-theme.json";
        case ThemeName.Dark:
          return "dark_vs.json";
        case ThemeName.DarkPlus:
          return "dark_plus.json";
        case ThemeName.DimmedMonokai:
          return "dimmed-monokai-color-theme.json";
        case ThemeName.KimbieDark:
          return "kimbie-dark-color-theme.json";
        case ThemeName.Light:
          return "light_vs.json";
        case ThemeName.LightPlus:
          return "light_plus.json";
        case ThemeName.Monokai:
          return "monokai-color-theme.json";
        case ThemeName.QuietLight:
          return "quietlight-color-theme.json";
        case ThemeName.Red:
          return "Red-color-theme.json";
        case ThemeName.SolarizedDark:
          return "solarized-dark-color-theme.json";
        case ThemeName.SolarizedLight:
          return "solarized-light-color-theme.json";
        case ThemeName.TomorrowNightBlue:
          return "tomorrow-night-blue-color-theme.json";
        case ThemeName.HighContrastLight:
          return "hc_light.json";
        case ThemeName.HighContrastDark:
          return "hc_black.json";
        default:
          return (string) null;
      }
    }
    
    public IRawTheme LoadTheme(ThemeName name) => this.GetTheme(this.GetThemeFile(name));
    

    
}