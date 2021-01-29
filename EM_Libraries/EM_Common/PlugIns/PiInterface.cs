using System.Collections.Generic;

namespace EM_Common
{
    public abstract class PiInterface
    {
        public abstract string GetTitle();
        public abstract string GetDescription();
        public abstract string GetFullFileName();
        public abstract bool IsWebApplicable();
        public abstract void Run(Dictionary<string, object> arguments = null);
    }
}
