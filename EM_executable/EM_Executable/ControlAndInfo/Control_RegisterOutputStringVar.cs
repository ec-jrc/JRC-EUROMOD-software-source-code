namespace EM_Executable
{
    public partial class Control
    {
        private void RegisterOutputStringVar()
        {
            if (string.IsNullOrEmpty(infoStore.country.data.listStringOutVar)) return;

            foreach (string outStringVar in infoStore.country.data.listStringOutVar.ToLower().Split(' '))
            {
                if (outStringVar.Trim() != string.Empty)
                    infoStore.operandAdmin.indexStringVars.Add(outStringVar, -1);
            }
        }
    }
}
