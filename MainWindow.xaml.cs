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
    
    public MainWindow()
    {
        InitializeComponent();

        RecipesByTier = new Dictionary<string, Recipe>[6];
        Recipes = new();
        for (var i = 0; i < 6; RecipesByTier[i++] = new Dictionary<string, Recipe>()){}
        
        var recipes = JsonConvert.DeserializeObject<List<Recipe>>(File.ReadAllText("Recipes.json"));
        foreach (var recipe in recipes!)
        {
            // RecipesByTier
            RecipesByTier[to_int(recipe.Tier) - 1][recipe.RecipeName] = recipe;
            
            // Recipes
            Recipes[recipe.RecipeName] = recipe;
        }
    
        
            
        PrintRecipes("I");
        DisplayRecipes();
        
        
        // Attach click event handlers to the buttons
        I.Click += (sender, e) => DisplayRecipes(((Button)sender).Name);
        II.Click += (sender, e) => DisplayRecipes(((Button)sender).Name);
        III.Click += (sender, e) => DisplayRecipes(((Button)sender).Name);
        IV.Click += (sender, e) => DisplayRecipes(((Button)sender).Name);
        V.Click += (sender, e) => DisplayRecipes(((Button)sender).Name);
        VI.Click += (sender, e) => DisplayRecipes(((Button)sender).Name);
        All.Click += (sender, e) => DisplayRecipes(((Button)sender).Name);
    }
    
    public int to_int(string input) => input switch {"I"=>1,"II"=>2,"III"=>3,"IV"=>4,"V"=>5,"VI"=>6,_=>throw new NotImplementedException()};

    public void DisplayRecipeIngredients(string recipeName)
    {
        
        Console.WriteLine("1");
        // Create and show the new window
        RecipeInfo newWindow = new RecipeInfo(Recipes[recipeName.Replace("_"," ")]);
        Console.WriteLine("11");
        newWindow.Show();
        Console.WriteLine("1111");
    }
    
    public void DisplayRecipes(string tier = "All")
    {
        Console.WriteLine($"display ingredients of tier: {tier}");
        RecipesGrid.Children.Clear();

        var i = 0;
        foreach (var (recipe, ingredients) in tier is "All" ? Recipes : RecipesByTier[to_int(tier) - 1])
        {
            // Main element to add
            var elemToAdd = new Border
            {
                Margin = new Thickness(5),
                BorderThickness = new Thickness(1),
                BorderBrush = Brushes.Black,
                CornerRadius = new CornerRadius(10)
            };
            
            // The Button inside
            var button = new Button
            {
                Name = recipe.Replace(" ", "_"),
                Margin = new Thickness(5), 
                Background = Brushes.Transparent, 
                BorderBrush = Brushes.Transparent
            };
            
            // The StackPanel inside the Button
            var stackPanel = new StackPanel();
            
            // The Image and Label inside the StackPanel
            var image = new Image {Source = new BitmapImage {UriSource = new Uri("pack://application:,,,/Images/download.png")}};
            var label = new Label {Content = recipe};
            
            // Construct the final element to add
            stackPanel.Children.Add(image);
            stackPanel.Children.Add(label);
            button.Content = stackPanel;
            elemToAdd.Child = button;
            
            elemToAdd.SetValue(Grid.ColumnProperty, i%6);
            elemToAdd.SetValue(Grid.RowProperty, i/6);
            
            RecipesGrid.Children.Add(elemToAdd);
            button.Click += (sender, e) => DisplayRecipeIngredients(((Button)sender).Name);
            
            i++;
        }
    }
    
    public void PrintRecipes(string tier)
    {
        foreach (Recipe recipe in RecipesByTier[to_int(tier)-1].Values)
        {
            Console.WriteLine($"['{recipe.RecipeName}'] :");
            Console.WriteLine();
            
            Console.WriteLine($"Ingredient{(recipe.Ingredients.Count > 0 ? "s" : "")}:");
            foreach (Ingredient ingredient in recipe.Ingredients)
                Console.WriteLine($"|\t{ingredient}");
            
            Console.WriteLine($"Output{(recipe.Output.Count > 0 ? "s" : "")}:");
            foreach (Output output in recipe.Output)
                Console.WriteLine($"|\t{output}");
            
            Console.WriteLine();
        }
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