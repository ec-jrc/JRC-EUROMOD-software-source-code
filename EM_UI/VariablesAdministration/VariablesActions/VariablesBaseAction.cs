using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EM_UI.VariablesAdministration.VariablesActions
{
    internal class VariablesBaseAction
    {
        virtual internal bool Perform() { return false; }
        virtual internal bool UpdateVariables() { return false; }
        virtual internal bool UpdateAcronyms() { return false; }
        virtual internal bool UpdateFilterCheckboxes() { return false; }
    }
}
