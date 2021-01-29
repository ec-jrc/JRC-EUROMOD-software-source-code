namespace EM_Transformer
{
    public partial class EM23Adapt
    {
        private void AdaptCZDefConfig_Par(EM2Item par)
        {
            if (par.name == "#1_DatabaseName" || par.name == "#2_DatabaseName" || par.name == "#3_DatabaseName")
            {
                if (par.properties.ContainsKey("Group")) par.properties["Group"] = par.name.Substring(1, 1);
                par.name = "#_DatabaseName";
            }
        }
    }
}
