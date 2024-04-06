using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BitCraftulator;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{

    
    private Dictionary<string, Recipe>[] RecipesByTier { get; set; }
    private Dictionary<string, Recipe> Recipes { get; set; }
    
    private Dictionary<string, Recipe>[] ItemsByTier { get; set; }
    private Dictionary<string, Recipe> Items { get; set; }

    private string?[] Tiers { get; set; }
    
    public MainWindow()
    {
        InitializeComponent();

        Tiers = new[] { "I", "II", "III", "IV", "V", "VI" };
        
        // init the recipes dictionaries
        RecipesByTier = new Dictionary<string, Recipe>[6];
        Recipes = new();
        ItemsByTier = new Dictionary<string, Recipe>[6];
        Items = new();
        
        for (var i = 0; i < 6; (RecipesByTier[i], ItemsByTier[i++]) = (new Dictionary<string, Recipe>(), new Dictionary<string, Recipe>())){}
        
        // fills them up with the recipes
        var recipes = JsonConvert.DeserializeObject<List<Recipe>>(File.ReadAllText("Recipes.json"));
        foreach (var recipe in recipes!)
        {
            if (recipe.Ingredients!.Count > 0)
            {
                RecipesByTier[recipe.Tier-1][recipe.RecipeName!] = recipe;
                Recipes[recipe.RecipeName!] = recipe;
            }
            else
            {
                ItemsByTier[recipe.Tier-1][recipe.RecipeName!] = recipe;
                Items[recipe.RecipeName!] = recipe;
            }
        }
    
        PrintRecipes("I");
        DisplayRecipes();
        
        // Attach click event handlers to the tier selection buttons
        I.Click += (sender, e) =>
        {
            string tier = ((Button)sender).Name;
            Tiers[to_int(tier) - 1] = Tiers[to_int(tier) - 1] is null ? tier: null;
            I.Background = I.Background == Brushes.Chartreuse ? Brushes.LightGray : Brushes.Chartreuse;
            DisplayRecipes();
        };
        II.Click += (sender, e) =>
        {
            string tier = ((Button)sender).Name;
            Tiers[to_int(tier) - 1] = Tiers[to_int(tier) - 1] is null ? tier: null;
            II.Background = II.Background == Brushes.Chartreuse ? Brushes.LightGray : Brushes.Chartreuse;
            DisplayRecipes();
        };
        III.Click += (sender, e) =>
        {
            string tier = ((Button)sender).Name;
            Tiers[to_int(tier) - 1] = Tiers[to_int(tier) - 1] is null ? tier: null;
            III.Background = III.Background == Brushes.Chartreuse ? Brushes.LightGray : Brushes.Chartreuse;
            DisplayRecipes();
        };
        IV.Click += (sender, e) =>
        {
            string tier = ((Button)sender).Name;
            Tiers[to_int(tier) - 1] = Tiers[to_int(tier) - 1] is null ? tier: null;
            IV.Background = IV.Background == Brushes.Chartreuse ? Brushes.LightGray : Brushes.Chartreuse;
            DisplayRecipes();
        };
        V.Click += (sender, e) =>
        {
            string tier = ((Button)sender).Name;
            Tiers[to_int(tier) - 1] = Tiers[to_int(tier) - 1] is null ? tier: null;
            V.Background = V.Background == Brushes.Chartreuse ? Brushes.LightGray : Brushes.Chartreuse;
            DisplayRecipes();
        };
        VI.Click += (sender, e) =>
        {
            string tier = ((Button)sender).Name;
            Tiers[to_int(tier) - 1] = Tiers[to_int(tier) - 1] is null ? tier: null;
            VI.Background = VI.Background == Brushes.Chartreuse ? Brushes.LightGray : Brushes.Chartreuse;
            DisplayRecipes();
        };
        SearchBar.PreviewMouseDown += (sender, e) =>
        {
            if (!SearchBar.IsFocused) SearchBar.Text = "";
        };
        Close.Click += (sender, e) => Close();
        Steps.Click += (sender, e) => DisplaySteps(((string)RecipeName.Content).Replace("_", " "));
    }
    
    public int to_int(string input) => input switch {"I"=>1,"II"=>2,"III"=>3,"IV"=>4,"V"=>5,"VI"=>6,_=>throw new NotImplementedException()};
    public string to_tier(int input) => input switch {1=>"I",2=>"II",3=>"III",4=>"IV",5=>"V",6=>"VI",_=>throw new NotImplementedException()};

    public void DisplaySteps(string recipeName)
    {
        if (recipeName is "Recipe Name")
            return;
        
        Stack<List<Recipe>> steps = new();
        Recipe recipe = Recipes[recipeName];
        Queue<Recipe?> q = new Queue<Recipe?>();
        
        steps.Push(new List<Recipe> {recipe});
        q.Enqueue(recipe);
        q.Enqueue(null);

        List<Recipe> build = new();
        while (q.Count > 0)
        {
            Recipe? curr = q.Dequeue();
            
            if (curr is null)
            {
                if (build.Count > 0)
                    steps.Push(build);
                build = new();
                if (q.Count > 0)
                    q.Enqueue(null);
            }
            
            else
            {
                foreach (var ingredient in curr.Ingredients!)
                {
                    for (var i = 0; i < ingredient.Quantity; i++)
                    {
                        Recipe ingr;
                        try
                        {
                            ingr = Recipes[ingredient.Name!];
                        }
                        catch (KeyNotFoundException)
                        {
                            ingr = Items[ingredient.Name!];
                        }
                        build.Add(ingr);
                        q.Enqueue(ingr);
                    }
                }
            }
        }

        var stepsWindow = new ShowSteps(steps);
        stepsWindow.Show();
        //PrintSteps(steps);
    }

    public void PrintSteps(Stack<List<Recipe>> steps)
    {
        while (steps.Count > 0)
        {
            var step = steps.Pop();
            foreach (var recipe in step)
            {
                Console.WriteLine(recipe);
            }
        }
    }
    
    public void DisplayRecipeIngredients(string recipeName)
    {

        Recipe recipe = Recipes[recipeName];
        RecipeName.Content = recipeName;
        Tier.Content = "Tier " + to_tier(recipe.Tier);
        SkillName.Content = "Skill: " + recipe.SkillName;
        SkillLevel.Content = "Level: " + recipe.SkillLevel;
        StationName.Content = "Station: " + recipe.Station;
        SkillTool.Content = "Tool: " + recipe.SkillTool;
        Effort.Content = "Effort: " + recipe.Effort;

        Ingredients.Children.Clear();
        foreach (var ingredient in recipe.Ingredients!)
        {
            Ingredients.RowDefinitions.Add(new RowDefinition());
            Label label = new Label { Content = ingredient.ToString(), HorizontalAlignment = HorizontalAlignment.Center};
            label.SetValue(Grid.RowProperty, Ingredients.Children.Count);
            Ingredients.Children.Add(label);
        }
        
        Output.Children.Clear();
        foreach (var output in recipe.Output!)
        {
            Output.RowDefinitions.Add(new RowDefinition());
            Label label = new Label { Content = output.ToString(), HorizontalAlignment = HorizontalAlignment.Center };
            label.SetValue(Grid.RowProperty, Output.Children.Count);
            Output.Children.Add(label);
        }
    }
    
    public void DisplayRecipes()
    {
        RecipesGrid.Children.Clear();

        foreach (var tier in Tiers)
        {
            if (tier is null)
                continue;
            
            var i = RecipesGrid.Children.Count;
            foreach (var (recipeName, recipe) in tier is "All" ? Recipes : RecipesByTier[to_int(tier) - 1])
            {
                // Main element to add
                var elemToAdd = new Border
                {
                    Margin = new Thickness(5),
                    BorderThickness = new Thickness(1),
                    BorderBrush = Brushes.Transparent,
                    CornerRadius = new CornerRadius(10),
                    Background = Brushes.LightCyan
                };
            
                // The Button inside
                var button = new Button
                {
                    Name = recipeName.Replace(" ", "_"),
                    Margin = new Thickness(5), 
                    Background = Brushes.Transparent, 
                    BorderBrush = Brushes.Transparent
                };
            
                // The StackPanel inside the Button
                var stackPanel = new StackPanel();
            
                // The Image and Label inside the StackPanel
                var image = new Image {Source = new BitmapImage {UriSource = new Uri("pack://application:,,,/Images/download.png")}};
                var textBlock = new TextBlock {Text = recipeName, TextWrapping = TextWrapping.Wrap};
            
                // Construct the final element to add
                stackPanel.Children.Add(image);
                stackPanel.Children.Add(textBlock);
                button.Content = stackPanel;
                elemToAdd.Child = button;
            
                elemToAdd.SetValue(Grid.ColumnProperty, i%5);
                elemToAdd.SetValue(Grid.RowProperty, i/5);
            
                RecipesGrid.Children.Add(elemToAdd);
                button.Click += (sender, e) => DisplayRecipeIngredients(((Button)sender).Name.Replace("_"," "));
            
                i++;
            }
        }
        
    }
    
    public void PrintRecipes(string tier = "All")
    {
        foreach (Recipe recipe in (tier is "All" ? Recipes.Values : RecipesByTier[to_int(tier)-1].Values))
            Console.WriteLine(recipe);
    }
    private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        
        Window window = Window.GetWindow((DependencyObject)sender);
        if (window != null)
        {
            window.Left += e.HorizontalChange;
            window.Top += e.VerticalChange;
        }
    }

}