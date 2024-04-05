using Newtonsoft.Json;
namespace BitCraftulator;

public class Ingredient{
     private string? _name;

     [JsonProperty("Name")]
     public string? Name {
          get => _name;
          set => _name = value ?? throw new ArgumentException("Ingredient Name is required");
     }

     [JsonProperty("Tier")]
     public int Tier { get; set; }

     [JsonProperty("Quantity")]
     public int Quantity { get; set; }

     public override string ToString() => $"[Tier {Tier}] {Name}: {Quantity}";
}

public class Output{
     private string? _name;

     [JsonProperty("Name")]
     public string? Name {
          get => _name;
          set => _name = value ?? throw new ArgumentException("Output Name is required");
     }
     [JsonProperty("Tier")]
     public int Tier { get; set; }

     [JsonProperty("Quantity")]
     public int Quantity { get; set; }
     
     public override string ToString() => $"[Tier {Tier}] {Name}: {Quantity}";
}

public class Recipe{
     private string? _recipeName;
     private string? _skillName;
     private string? _skillTool;
     private string? _station;
     private List<Ingredient>? _ingredients;
     private List<Output>? _output;


     [JsonProperty("RecipeName")]
     public string? RecipeName {
          get => _recipeName;
          set => _recipeName = value ?? throw new ArgumentException("Recipe Name is required");
     }

     [JsonProperty("Tier")]
     public int Tier { get; set; }

     [JsonProperty("SkillName")]
     public string? SkillName {
          get => _skillName;
          set => _skillName = value ?? throw new ArgumentException("Skill Name is required");
     }

     [JsonProperty("SkillLevel")]
     public int SkillLevel { get; set; }

     [JsonProperty("SkillTool")]
     public string? SkillTool {
          get => _skillTool;
          set => _skillTool = value ?? throw new ArgumentException("Skill Tool is required");
     }

     [JsonProperty("Station")]
     public string? Station {
          get => _station;
          set => _station = value ?? throw new ArgumentException("Station is required");
     }

     [JsonProperty("Effort")]
     public int Effort { get; set; }

     [JsonProperty("Ingredients")]
     public List<Ingredient>? Ingredients {
          get => _ingredients;
          set => _ingredients = value ?? throw new ArgumentException("Ingredients are required");
     }

     [JsonProperty("Output")]
     public List<Output>? Output {
          get => _output;
          set => _output = value ?? throw new ArgumentException("Output is required");
     }

     public override string ToString()
     {
          string res = $"['{RecipeName}'] :\r\n\tTier: {Tier}\r\n\tSkill: {SkillName}\r\n\t\tLevel: {SkillLevel}\r\n\t\tTool: {SkillTool}\r\n\t\tStation: {Station}\r\n\tIngredient{(Ingredients!.Count>1?"s":"")}: ({Ingredients!.Count})\r\n";

          foreach (Ingredient ingredient in Ingredients)
               res += $"\t\t{ingredient}\r\n";

          res += $"\tOutput{(Output!.Count>1?"s":"")}: ({Output!.Count})\r\n";
          foreach (Output output in Output)
               res += $"\t\t{output}\r\n";

          return res + "\r\n";
     }
}