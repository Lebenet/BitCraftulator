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

        // init the recipes dictionaries
        RecipesByTier = new Dictionary<string, Recipe>[6];
        Recipes = new();
        for (var i = 0; i < 6; RecipesByTier[i++] = new Dictionary<string, Recipe>()){}
        
        // fills them up with the recipes
        var recipes = JsonConvert.DeserializeObject<List<Recipe>>(File.ReadAllText("Recipes.json"));
        foreach (var recipe in recipes!)
        {
            RecipesByTier[recipe.Tier-1][recipe.RecipeName!] = recipe;
            Recipes[recipe.RecipeName!] = recipe;
        }
    
        PrintRecipes("I");
        DisplayRecipes();
        
        // Attach click event handlers to the tier selection buttons
        I.Click += (sender, e) => DisplayRecipes(((Button)sender).Name);
        II.Click += (sender, e) => DisplayRecipes(((Button)sender).Name);
        III.Click += (sender, e) => DisplayRecipes(((Button)sender).Name);
        IV.Click += (sender, e) => DisplayRecipes(((Button)sender).Name);
        V.Click += (sender, e) => DisplayRecipes(((Button)sender).Name);
        VI.Click += (sender, e) => DisplayRecipes(((Button)sender).Name);
    }
    
    public int to_int(string input) => input switch {"I"=>1,"II"=>2,"III"=>3,"IV"=>4,"V"=>5,"VI"=>6,_=>throw new NotImplementedException()};
    public string to_tier(int input) => input switch {1=>"I",2=>"II",3=>"III",4=>"IV",5=>"V",6=>"VI",_=>throw new NotImplementedException()};

    public void DisplayRecipeIngredients(string recipeName)
    {
        
        Console.WriteLine("1");
    }
    
    public void DisplayRecipes(string tier = "All")
    {
        Console.WriteLine($"display ingredients of tier: {tier}");
        //RecipesGrid.Children.Clear();

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
            button.Click += (sender, e) => DisplayRecipeIngredients(((Button)sender).Name);
            
            i++;
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