namespace BitCraftulator;

public class Ingredient
{
    public string Tier { get; set; }
    public string Name { get; set; }
    public int Quantity { get; set; }
}

public class Output
{
    public string Tier { get; set; }
    public string Name { get; set; }
    public int Quantity { get; set; }
}

public class Recipe
{
    public string RecipeName { get; set; }
    public string Tier { get; set; }
    public string StationName { get; set; }
    public string SkillName { get; set; }
    public int SkillLevel { get; set; }
    public string ToolName { get; set; }
    public int Effort { get; set; }
    public List<Ingredient> Ingredients { get; set; }
    public List<Output> Output { get; set; }
}