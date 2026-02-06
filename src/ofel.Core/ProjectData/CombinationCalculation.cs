namespace Ofel.Core
{

    /// <summary>
    /// Représente une répartition de variables avec une dominante et une liste de mineurs
    /// </summary>
    public class VariableRepartition
    {
        public string Major { get; set; }
        public List<string> Minors { get; set; }

        public VariableRepartition(string major, List<string> minors)
        {
            Major = major;
            Minors = minors;
        }
    }
}
