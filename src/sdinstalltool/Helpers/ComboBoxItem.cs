
namespace SDInstallTool.Helpers
{
    public class ComboBoxItem
    {
        public string Name;
        public string value = string.Empty;

        public ComboBoxItem(string Name, string value)
        {
            this.Name = Name;
            this.value = value;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
