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

public class DependencyGraph(List<Recipe> recipes)
{
     private readonly Dictionary<string, Node> _nodes = new();

     public void Add(string name)
     {
          if (_nodes.ContainsKey(name)) return;
          AddInternal(name);
     }

     private Node AddInternal(string name)
     {
          if (_nodes.TryGetValue(name, out var ret)) return ret;
          
          Console.WriteLine($"Creating node for item \"{name}\"");
          
          Node node = new Node(name);
          _nodes[node.Name] = node;

          Recipe? recipe = GetRecipe(name);
          if (recipe != null)
          {
               if (recipe.Ingredients != null)
               {
                    bool isCircular = false;
                    foreach (Ingredient ingredient in recipe.Ingredients)
                    {
                         if (_nodes.TryGetValue(ingredient.Name!, out var ingredientNode))
                         {
                              if (ingredientNode.HasDependency(node))
                              {
                                   isCircular = true;
                                   break;
                              }
                         }
                    }

                    if (isCircular) return node;
                    
                    Console.WriteLine($"Found recipe \"{recipe.RecipeName}\" with ingredients [{IngredientsToString(recipe.Ingredients!)}]");
                    
                    foreach (Ingredient ingredient in recipe.Ingredients)
                    {
                         var ingredientNode = AddInternal(ingredient.Name!);
                         node.Dependencies.Add(ingredientNode);
                         ingredientNode.Dependents.Add(node);
                    }
               }
          }

          return node;
     }

     public string IngredientsToString(List<Ingredient> ingredients)
     {
          return string.Join(", ", ingredients.ConvertAll(e => $"{e.Quantity} {e.Name}"));
     }

     public string? Take()
     {
          foreach (Node node in _nodes.Values.ToList())
          {
               if (node.Dependents.Count == 0)
               {
                    RemoveInternal(node);
                    return node.Name;
               }
          }

          return null;
     }

     private void RemoveInternal(Node node)
     {
          foreach (var dependent in node.Dependents)
          {
               dependent.Dependencies.Remove(node);
          }
          
          foreach (var dependency in node.Dependencies)
          {
               dependency.Dependents.Remove(node);
          }

          _nodes.Remove(node.Name);
     }

     public Recipe? GetRecipe(string name)
     {
          foreach (var recipe in recipes)
          {
               if (recipe.Output != null && recipe.Output.Count != 0)
               {
                    if (recipe.Output.Any(e => e.Name == name))
                    {
                         return recipe;
                    }
               }
          }

          return null;
     }
     
     private class Node(string name)
     {
          public readonly string Name = name;
          public readonly HashSet<Node> Dependencies = new();
          public readonly HashSet<Node> Dependents = new();

          public bool HasDependency(Node node)
          {
               if (Dependencies.Contains(node)) return true;
               
               bool ret = false;
               
               foreach(Node dependency in Dependencies)
               {
                    ret = ret || dependency.HasDependency(node);
               }

               return ret;
          }

          public bool HasDependent(Node node)
          {
               if (Dependents.Contains(node)) return true;

               bool ret = false;

               foreach (Node dependent in Dependents)
               {
                    ret = ret || dependent.HasDependent(node);
               }

               return ret;
          }

          public override int GetHashCode()
          {
               return Name.GetHashCode();
          }
     }
}