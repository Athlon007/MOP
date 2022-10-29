using MSCLoader;

namespace MOP.Rules.Types
{
    internal class NoLod : Rule
    {
        public string ObjectName { get; private set; }

        public NoLod(Mod mod, string filename, string objectName) : base(mod, filename)
        {
            ObjectName = objectName;
        }

        public override string ToString()
        {
            return base.ToString() + $" ObjectName: {ObjectName}";
        }
    }
}
