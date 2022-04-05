using MSCLoader;

namespace MOP.Rules.Types
{
    abstract class Rule
    {
        public Mod Mod { get; private set; }
        public string Filename { get; private set; }

        public Rule(Mod mod, string filename)
        {
            Mod = mod;
            Filename = filename;
        }

        public override string ToString()
        {
#if PRO
            return $"{Filename}" + (!Mod.Enabled ? " (Disabled)" : "");
#else
            return $"{Filename}" + (Mod.isDisabled ? " (Disabled)" : "");
#endif
        }
    }
}
