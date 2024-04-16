using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;

namespace BitCraftulator;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private Dictionary<int, Brush> RecipesBackground => new Dictionary<int, Brush>
    {
        {1, (SolidColorBrush)(new BrushConverter().ConvertFrom("#636A74"))},
        {2, (SolidColorBrush)(new BrushConverter().ConvertFrom("#52706A"))},
        {3, (SolidColorBrush)(new BrushConverter().ConvertFrom("#546E7D"))},
        {4, (SolidColorBrush)(new BrushConverter().ConvertFrom("#666699"))},
        {5, (SolidColorBrush)(new BrushConverter().ConvertFrom("#7E672A"))},
        {6, (SolidColorBrush)(new BrushConverter().ConvertFrom("#99334C"))},
        {7, (SolidColorBrush)(new BrushConverter().ConvertFrom("#996433"))},
        {8, (SolidColorBrush)(new BrushConverter().ConvertFrom("#419933"))},
    };

    private Dictionary<int, Brush> TextBackground => new Dictionary<int, Brush>
    {
        {1, (SolidColorBrush)(new BrushConverter().ConvertFrom("#B0B5BA"))},
        {2, (SolidColorBrush)(new BrushConverter().ConvertFrom("#7B857D"))},
        {3, (SolidColorBrush)(new BrushConverter().ConvertFrom("#9AB7BE"))},
        {4, (SolidColorBrush)(new BrushConverter().ConvertFrom("#8384B3"))},
        {5, (SolidColorBrush)(new BrushConverter().ConvertFrom("#C5A263"))},
        {6, (SolidColorBrush)(new BrushConverter().ConvertFrom("#CC7C81"))},
        {7, (SolidColorBrush)(new BrushConverter().ConvertFrom("#CC8378"))},
        {8, (SolidColorBrush)(new BrushConverter().ConvertFrom("#80C666"))},
    };
    
    Dictionary<int, Brush> TextColor => new Dictionary<int, Brush>
    {
        { 1, MixColors("#000000", 0.8, "#454A45", 0.2) },
        { 2, MixColors("#000000", 0.8, "#848A82", 0.2) },
        { 3, MixColors("#000000", 0.8, "#654842", 0.2) },
        { 4, MixColors("#000000", 0.8, "#7C7D8C", 0.2) },
        { 5, MixColors("#000000", 0.8, "#3A5D9C", 0.2) },
        { 6, MixColors("#000000", 0.8, "#3373BE", 0.2) },
        { 7, MixColors("#000000", 0.8, "#338C8C", 0.2) },
        { 8, MixColors("#000000", 0.8, "#7F3999", 0.2) }
    };

    Brush MixColors(string colorHex, double colorRatio, string reverseHex, double reverseRatio)
    {
        Color color = (Color)ColorConverter.ConvertFromString(colorHex);
        Color reverseColor = (Color)ColorConverter.ConvertFromString(reverseHex);
    
        byte r = (byte)(color.R * colorRatio + reverseColor.R * reverseRatio);
        byte g = (byte)(color.G * colorRatio + reverseColor.G * reverseRatio);
        byte b = (byte)(color.B * colorRatio + reverseColor.B * reverseRatio);
    
        return new SolidColorBrush(Color.FromRgb(r, g, b));
    }

    private Dictionary<string, int> rawMaterials;
    private Dictionary<string, Recipe>[] RecipesByTier { get; set; }
    private Dictionary<string, Recipe> Recipes { get; set; }
    
    private Dictionary<string, Recipe>[] ItemsByTier { get; set; }
    private Dictionary<string, Recipe> Items { get; set; }

    private string?[] Tiers { get; set; }
    
    public MainWindow()
    {
        InitializeComponent();

        Tiers = new[] { "I", "II", "III", "IV", "V", "VI", "VII", "DEV" };
        int numTiers = Tiers.Length;
        
        // init the recipes dictionaries
        RecipesByTier = new Dictionary<string, Recipe>[numTiers];
        Recipes = new();
        ItemsByTier = new Dictionary<string, Recipe>[numTiers];
        Items = new();
        
        for (var i = 0; i < numTiers; (RecipesByTier[i], ItemsByTier[i++]) = (new Dictionary<string, Recipe>(), new Dictionary<string, Recipe>())){}
        
        // fills them up with the recipes
        var recipes = JsonConvert.DeserializeObject<List<Recipe>>(File.ReadAllText("Recipes.json"));
        foreach (var recipe in recipes!)
        {
            if (recipe.IsValid())
            {
                // fix issue with High Quality ( not really cos there's random but eh)
                if (recipe.Ingredients!.Count == 1)
                {
                    var output = recipe.Output![0];
                    var ingredient = recipe.Ingredients![0];
                    if (ingredient.Name!.Contains("High Quality") && !output.Name!.Contains("Supplies") && !output.Name.Contains("Source"))
                    {
                        ingredient.Name = ingredient.Name!.Replace("High Quality", "").Trim() + " Output";
                        recipe.Output![0].Quantity = 1;
                    }
                }
                    
                if (recipe.Ingredients!.Count > 0 || recipe.Tier-1 >= 7)
                {
                    if (!Recipes.ContainsKey(recipe.RecipeName!))
                    {
                        RecipesByTier[recipe.Tier-1 < 7 ? recipe.Tier-1 : 7][recipe.RecipeName!] = recipe;
                        Recipes[recipe.RecipeName!] = recipe;
                    }
                    else
                    {
                        string n = recipe.RecipeName! + "\u200e";
                        while (Recipes.ContainsKey(n))
                            n += "\u200e";
                        RecipesByTier[recipe.Tier-1 < 7 ? recipe.Tier-1 : 7][n] = recipe;
                        Recipes[n] = recipe;
                    }
                }
                else
                {
                    if (!Items.ContainsKey(recipe.RecipeName!))
                    {
                        ItemsByTier[recipe.Tier-1][recipe.RecipeName!] = recipe;
                        Items[recipe.RecipeName!] = recipe;
                    }
                    else
                    {
                        string n = recipe.RecipeName! + "\u200e";
                        while (Recipes.ContainsKey(n))
                            n += "\u200e";
                        ItemsByTier[recipe.Tier-1][n] = recipe;
                        Items[n] = recipe;
                    }
                }
            }
        }

        
        for (var i = 0; i < Recipes.Count + 5; i += 5)
            RecipesGrid.RowDefinitions.Add(new RowDefinition {Height = new GridLength()});
    
        DisplayRecipes();
        
        // Attach click event handlers to the tier selection buttons
        I.Click += (sender, e) => ButtonUpdate((Button)sender);
        II.Click += (sender, e) => ButtonUpdate((Button)sender);
        III.Click += (sender, e) => ButtonUpdate((Button)sender);
        IV.Click += (sender, e) => ButtonUpdate((Button)sender);
        V.Click += (sender, e) => ButtonUpdate((Button)sender);
        VI.Click += (sender, e) => ButtonUpdate((Button)sender);
        VII.Click += (sender, e) => ButtonUpdate((Button)sender);
        DEV.Click += (sender, e) => ButtonUpdate((Button)sender);
        SearchBar.PreviewMouseDown += (sender, e) =>
        {
            if (!SearchBar.IsFocused) SearchBar.Text = "";
        };
        Close.Click += (sender, e) => Close();
        Steps.Click += (sender, e) => DisplaySteps(((string)RecipeName.Content).Replace("_", " "));
        Quantity.TextChanged += (sender, e) => UpdateRecipeIngredients(Quantity.Text);
        Quantity.PreviewMouseDown += (sender, e) =>
        {
            if (!Quantity.IsFocused) Quantity.Text = "";
        };
        SearchBar.TextChanged += (sender, e) => DisplayRecipes(SearchBar.Text.ToLower());
        DisplayMode.Click += (sender, e) =>
        {
            // Define the color and update the button's content
            /*var color = (string)DisplayMode.Content == "Light Mode" ? Colors.White : Colors.Black;
            TextColor = (string)DisplayMode.Content == "Light Mode" ? Brushes.White : Brushes.Black;
            //Background.Background = Background.Background == Brushes.WhiteSmoke ? Brushes.DimGray : Brushes.WhiteSmoke;
            DisplayMode.Content = (string)DisplayMode.Content == "Light Mode" ? "Dark Mode" : "Light Mode";
            
            // Create new styles for TextBlock, Label, and TextBox
            Style textBlockStyle = new Style(typeof(TextBlock));
            textBlockStyle.Setters.Add(new Setter(TextBlock.ForegroundProperty, new SolidColorBrush(color)));

            Style labelStyle = new Style(typeof(Label));
            labelStyle.Setters.Add(new Setter(Label.ForegroundProperty, new SolidColorBrush(color)));

            // Apply the new styles directly to the controls
            foreach (var textBlock in FindVisualChildren<TextBlock>(this))
                textBlock.Style = textBlockStyle;

            foreach (var label in FindVisualChildren<Label>(this))
                label.Style = labelStyle;

            // Apply the new style to every recipe in the recipes list
            foreach (TextBlock output in Output.Children)
                output.Foreground = TextColor;

            foreach (TextBlock ingredient in Ingredients.Children)
                ingredient.Foreground = TextColor;*/
        };
    }

    IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
    {
        if (depObj != null)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                if (child != null && child is T && !IsDescendantOfButton(child)) // Exclude controls that are descendants of buttons
                    yield return (T)child;

                foreach (T childOfChild in FindVisualChildren<T>(child))
                    yield return childOfChild;
            }
        }
    }
    
    bool IsDescendantOfButton(DependencyObject obj)
    {
        DependencyObject parent = VisualTreeHelper.GetParent(obj);
        while (parent != null)
        {
            if (parent is Button)
                return true;
            parent = VisualTreeHelper.GetParent(parent);
        }
        return false;
    }
    
    public void ButtonUpdate(Button sender)
    {
        var parent = (Border)FindName(sender.Name + "Border")!;
        string tier = sender.Name;
        Brush background = RecipesBackground[to_int(tier)];
        Tiers[to_int(tier) - 1] = Tiers[to_int(tier) - 1] is null ? tier: null;
        parent.Background = parent.Background.ToString() == background.ToString() ? Brushes.LightGray : background;
        DisplayRecipes(SearchBar.Text);
    }
    
    public int to_int(string input) => input switch {"I"=>1,"II"=>2,"III"=>3,"IV"=>4,"V"=>5,"VI"=>6,"VII"=>7,"DEV"=>8,_=>throw new NotImplementedException()};
    public string to_tier(int input) => input switch {1=>"I",2=>"II",3=>"III",4=>"IV",5=>"V",6=>"VI",7=>"VII",_=>"DEV"};

     public void DisplaySteps(string recipeName)
    {
        if (recipeName is "Recipe Name")
            return;

        int target = string.IsNullOrEmpty(Quantity.Text) ? 1 : int.Parse(Quantity.Text);

        DependencyGraph graph = new DependencyGraph(Recipes.Values.ToList());
        graph.Add(recipeName);

        Dictionary<string, int> quantities = new();
        Stack<Tuple<Recipe, int>> steps = new();

        quantities[recipeName] = target;

        string? next = graph.Take();
        while (next != null)
        {
            Recipe? recipe = graph.GetRecipe(next);
            if (recipe != null)
            {
                if (recipe.Output != null)
                {
                    int quantity = quantities[next];
                    int count = (int)Math.Ceiling((double)quantity / recipe.Output.First(e => e.Name == next).Quantity);

                    steps.Push(new Tuple<Recipe, int>(recipe, count));

                    if (recipe.Ingredients != null)
                    {
                        foreach (Ingredient ingredient in recipe.Ingredients)
                        {
                            quantities.TryGetValue(ingredient.Name!, out var newQuantity);
                            newQuantity += count * ingredient.Quantity;
                            quantities[ingredient.Name!] = newQuantity;
                        }
                    }
                }
            }

            next = graph.Take();
        }

        foreach ((string name, int quantity) in rawMaterials)
        {
            if (Items.ContainsKey(name))
            {
                steps.Push(new Tuple<Recipe, int>(Items[name], quantity)); 
            }
        }
        
        var stepsWindow = new ShowSteps(steps);
        stepsWindow.Show();
    }

    public void UpdateRecipeIngredients(string q = "")
    {
        var recipeName = RecipeName.Content.ToString()!;
        if (recipeName is "Recipe Name")
            return;

        int target = string.IsNullOrEmpty(Quantity.Text) ? 1 : int.Parse(Quantity.Text);

        DependencyGraph graph = new DependencyGraph(Recipes.Values.ToList());
        graph.Add(recipeName);

        Dictionary<string, int> quantities = new();
        Stack<Tuple<Recipe, int>> steps = new();
        rawMaterials = new();

        var outputEffort = 0;
        var totalEffort = 0;
        var outputCount = 0;

        quantities[recipeName] = target;

        var next = graph.Take();
        while (next != null)
        {
            Console.WriteLine($"Next item in recipe calculation: {next}");
            Console.WriteLine($"Graph is now: {graph.NodesToString()}");
            Recipe? recipe = graph.GetRecipe(next);
            if (recipe == null)
            {
                rawMaterials[next] = quantities[next];
            }
            else
            {
                if (recipe.Output != null)
                {
                    int quantity = quantities[next];
                    int count = (int)Math.Ceiling((double)quantity / recipe.Output.First(e => e.Name == next).Quantity);
                    totalEffort += count * recipe.Effort;
                    if (recipe.Output.Any(e => e.Name == recipeName))
                    {
                        outputEffort += count * recipe.Effort;
                        outputCount += count * recipe.Output.First(e => e.Name == recipeName).Quantity;
                    }

                    steps.Push(new Tuple<Recipe, int>(recipe, count));

                    if (recipe.Ingredients != null)
                    {
                        foreach (Ingredient ingredient in recipe.Ingredients)
                        {
                            quantities.TryGetValue(ingredient.Name!, out var newQuantity);
                            newQuantity += count * ingredient.Quantity;
                            quantities[ingredient.Name!] = newQuantity;
                        }
                    }
                }
            }

            next = graph.Take();
        }

        Effort.Content = $"Effort: {outputEffort} ({totalEffort} total)";
        
        Ingredients.Children.Clear();
        foreach (var ingredient in rawMaterials.Keys)
        {
            Ingredients.RowDefinitions.Add(new RowDefinition());
            TextBlock inputText = new TextBlock { Text = $"{ingredient}: {rawMaterials[ingredient]}", HorizontalAlignment = HorizontalAlignment.Center, TextWrapping = TextWrapping.Wrap, Foreground = Brushes.White };
            inputText.SetValue(Grid.RowProperty, Ingredients.Children.Count);
            Ingredients.Children.Add(inputText);
        }
        
        Output.Children.Clear();
        Output.RowDefinitions.Add(new RowDefinition());
        TextBlock outputText = new TextBlock { Text = $"{recipeName}: {outputCount}", HorizontalAlignment = HorizontalAlignment.Center, TextWrapping = TextWrapping.Wrap, Foreground = Brushes.White  };
        outputText.SetValue(Grid.RowProperty, Output.Children.Count);
        Output.Children.Add(outputText);
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
        UpdateRecipeIngredients();
    }
    
    public void DisplayRecipes(string search = "")
    {
        RecipesGrid.Children.Clear();

        if (search == "Search for an item name")
            search = "";
        
        foreach (var tier in Tiers)
        {
            if (tier is null)
                continue;
            
            var i = RecipesGrid.Children.Count;
            foreach (var (recipeName, recipe) in tier is "All" ? Recipes : RecipesByTier[to_int(tier) - 1])
            {
                if (!recipeName.ToLower().Contains(search))
                    continue;
                // Main element to add
                var elemToAdd = new Border
                {
                    Margin = new Thickness(5),
                    BorderThickness = new Thickness(1),
                    BorderBrush = Brushes.White,
                    CornerRadius = new CornerRadius(5),
                    Background = RecipesBackground[recipe.Tier is >= 1 and <= 8 ? recipe.Tier : recipe.Tier > 8 ? 8 : 1]
                };
            
                // The Button inside
                var button = new Button
                {
                    //Name = "_" + recipeName.Replace(" ", "_").Replace("'","__").Replace(":","999"),
                    Background = Brushes.Transparent, 
                    BorderBrush = Brushes.Transparent,
                    MinHeight = 30
                };
            
                // The StackPanel inside the Button
                var stackPanel = new StackPanel();
            
                // The Image and Label inside the StackPanel
                var image = new Image {Source = new BitmapImage {UriSource = new Uri("pack://application:,,,/Images/download.png")}}; // to change
                var border = new Border { Background = TextBackground[recipe.Tier is >= 1 and <= 8 ? recipe.Tier : recipe.Tier > 8 ? 8 : 1], Margin = new Thickness(-2), Width = 110, VerticalAlignment = VerticalAlignment.Bottom };
                var textBlock = new TextBlock { TextAlignment = TextAlignment.Center, Text = recipeName, TextWrapping = TextWrapping.Wrap, Foreground =  TextColor[to_int(to_tier(recipe.Tier))]};
                border.Child = textBlock;
                
                
                // Construct the final element to add
                stackPanel.Children.Add(image);
                stackPanel.Children.Add(border);
                button.Content = stackPanel;
                elemToAdd.Child = button;
            
                elemToAdd.SetValue(Grid.ColumnProperty, i%5);
                elemToAdd.SetValue(Grid.RowProperty, i/5);
            
                RecipesGrid.Children.Add(elemToAdd);
                button.Click += (sender, e) => DisplayRecipeIngredients(textBlock.Text);
                i++;
            }
        }
        
    }
    private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        
        Window window = GetWindow((DependencyObject)sender);
        if (window != null)
        {
            window.Left += e.HorizontalChange;
            window.Top += e.VerticalChange;
        }
    }

    private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        foreach (var ch in e.Text)
            if (!char.IsDigit(ch))
                e.Handled = true; // Mark the event as handled to prevent non-numeric input
    }
}