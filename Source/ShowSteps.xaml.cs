using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace BitCraftulator;

public partial class ShowSteps : Window
{
    private List<TextBlock> ingredients;
    private List<TextBlock> crafts;
    private Dictionary<int, Dictionary<Recipe, int>> AllIngredients;
    private Dictionary<string, Tuple<Recipe, int>> RecipesByStation;
    public ShowSteps(Stack<Tuple<Recipe, int>> recipes)
    {
        InitializeComponent();

        ingredients = new ();
        crafts = new();
        StepsGrid.RowDefinitions.Add(new RowDefinition {Height = new GridLength()});
        AllIngredients = new Dictionary<int, Dictionary<Recipe, int>>()
        {
            {1, new Dictionary<Recipe, int>()},
            {2, new Dictionary<Recipe, int>()},
            {3, new Dictionary<Recipe, int>()},
            {4, new Dictionary<Recipe, int>()},
            {5, new Dictionary<Recipe, int>()},
            {6, new Dictionary<Recipe, int>()},
            {7, new Dictionary<Recipe, int>()},
            {8, new Dictionary<Recipe, int>()}
        };
        
        var i = 0;
        while (recipes.Count > 0)
        {
            var recipe = recipes.Pop();
            Recipe r = recipe.Item1;
            int q = recipe.Item2;
            int t = r.Tier;
            if (recipe.Item1.Ingredients!.Count == 0)
            {
                if (AllIngredients[t].ContainsKey(r))
                    AllIngredients[t][r] += q;
                else
                    AllIngredients[t][r] = q;
            }
            else if (!r.RecipeName!.Contains("Output"))
                DisplayStep(recipe, i++);
        }

        DisplayIngredients();
        
        Close.Click += (sender, e) => Close();
        WriteRecipe.Click += (sender, e) => WriteToFile();
        //Console.WriteLine(StepsGrid.Children.Count);
    }

    public void DisplayIngredients()
    {
        
        Border border = new Border { Background = Brushes.Azure, BorderBrush = Brushes.Gray, BorderThickness = new Thickness(0, 3, 0, 0)};
        border.SetValue(Grid.RowProperty, 0);
        StepsGrid.Children.Add(border);
        
        Grid elemsGrid = new Grid();
        foreach (var key in AllIngredients.Keys)
            for (var i = 0; i <= AllIngredients[key].Count + AllIngredients.Count + 1; i++) elemsGrid.RowDefinitions.Add(new RowDefinition());
        border.Child = elemsGrid;

        TextBlock textBlock = new TextBlock { Text = "All ingredients:", TextWrapping = TextWrapping.Wrap };
        elemsGrid.Children.Add(textBlock);

        var j = 0;
        foreach (var elem in AllIngredients.Keys)
        {
            if (AllIngredients[elem].Count == 0)
                continue;
            
            TextBlock tier = new TextBlock { Text = $"Tier {++j}:", FontWeight = FontWeights.Bold, TextDecorations = TextDecorations.Underline, FontSize = FontSize + 2 };
            ingredients.Add(tier);
            tier.SetValue(Grid.RowProperty, elemsGrid.Children.Count);
            elemsGrid.Children.Add(tier);
            foreach (var recipe in AllIngredients[elem].Keys)
            {
                TextBlock line = new TextBlock { TextWrapping = TextWrapping.Wrap };
                ingredients.Add(line);

                // Add a StackPanel to hold the line and the delete button
                StackPanel panel = new StackPanel { Orientation = Orientation.Horizontal };

                // Add a button dynamically
                Button deleteButton = new Button { Content = "X", Visibility = Visibility.Hidden, HorizontalAlignment = HorizontalAlignment.Left, Background = Brushes.Red, Width = 20};
                deleteButton.Click += (sender, e) =>
                {
                    // Remove the element when the button is clicked
                    elemsGrid.Children.Remove(panel);
                    UpdateStepsStatus();
                };

                // Add the delete button to the StackPanel
                panel.Children.Add(deleteButton);

                panel.MouseEnter += (sender, e) =>
                {
                    // Show the delete button when the mouse enters the element
                    deleteButton.Visibility = Visibility.Visible;
                };

                panel.MouseLeave += (sender, e) =>
                {
                    // Hide the delete button when the mouse leaves the element
                    deleteButton.Visibility = Visibility.Hidden;
                };

                // Add the line to the StackPanel
                panel.Children.Add(line);
                panel.SetValue(Grid.RowProperty, elemsGrid.Children.Count);
                line.Text = $"[Tier {recipe.Tier}] {recipe.RecipeName} : {AllIngredients[elem][recipe]}";
                elemsGrid.Children.Add(panel);
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

    /*public void DisplayStep(List<Recipe> stepRaw, int stepNumber)
    {
        Dictionary<Recipe, int> step = CleanStep(stepRaw);
        
        StepsGrid.RowDefinitions.Add(new RowDefinition {Height = new GridLength()});
        Border border = new Border { Background = Brushes.Azure, BorderBrush = Brushes.Gray, BorderThickness = new Thickness(0, 3, 0, 0)};
        border.SetValue(Grid.RowProperty, StepsGrid.Children.Count + 1);
        StepsGrid.Children.Add(border);
        
        Grid elemsGrid = new Grid {Name = "Step1"};
        for (var i = 0; i <= step.Count + 1; i++) elemsGrid.RowDefinitions.Add(new RowDefinition());
        border.Child = elemsGrid;

        Label stepLabel = new Label { Content = $"Step {stepNumber + 1}" };
        elemsGrid.Children.Add(stepLabel);

        foreach (var elem in step.Keys)
        {
            if (elem.RecipeName.Contains("Output"))
                continue;
            
            TextBlock line = new TextBlock { TextWrapping = TextWrapping.Wrap };
            line.SetValue(Grid.RowProperty, elemsGrid.Children.Count);
            if (elem.Ingredients!.Count == 0)
                line.Text = $"Get {step[elem] * _quantity} {elem.RecipeName}";
            else
                line.Text = $"{( elem.Station!.ToLower().Contains("field") ? "Grow" : "Craft" )} {(elem.Ingredients.Count == 1 && elem.Ingredients[0].Name.Contains("Output") ? (step[elem] * _quantity) : (step[elem] * _quantity) + (elem.Output![0].Quantity * _quantity * step[elem] != 1 && elem.Output![0].Quantity * step[elem] * _quantity != step[elem] * _quantity ? $" (gives {elem.Output![0].Quantity * _quantity * step[elem]})" : ""))} {elem.RecipeName} at {elem.Station} with tool {elem.SkillTool} (required level {elem.SkillLevel} minimum in {elem.SkillName} skill)";
            elemsGrid.Children.Add(line);
        }

        var fin = new TextBlock { TextWrapping = TextWrapping.Wrap };
        fin.SetValue(Grid.RowProperty, elemsGrid.Children.Count);
        elemsGrid.Children.Add(fin);

    }*/
    

    public void DisplayStep(Tuple<Recipe, int> recipeRaw, int stepNumber)
    {
        StepsGrid.RowDefinitions.Add(new RowDefinition {Height = new GridLength()});
        Border border = new Border { Background = Brushes.Azure, BorderBrush = Brushes.Gray, BorderThickness = new Thickness(0, 3, 0, 0)};
        border.SetValue(Grid.RowProperty, StepsGrid.Children.Count + 1);
        StepsGrid.Children.Add(border);
        
        Grid elemsGrid = new Grid {Name = "Step"};
        elemsGrid.RowDefinitions.Add(new RowDefinition());
        elemsGrid.RowDefinitions.Add(new RowDefinition());
        border.Child = elemsGrid;

        Label stepLabel = new Label { Content = $"Step {stepNumber + 1}" };
        elemsGrid.Children.Add(stepLabel);
        
        
        TextBlock line = new TextBlock { TextWrapping = TextWrapping.Wrap };
        crafts.Add(line);
        line.SetValue(Grid.RowProperty, elemsGrid.Children.Count);
        Recipe recipe = recipeRaw.Item1;
        int count = recipeRaw.Item2;
        line.Text = $"{( recipe.Station!.ToLower().Contains("field") || recipe.Station!.ToLower().Contains("field") ? "Grow" : "Craft" )} {recipe.RecipeName} {count} times at {recipe.Station} with tool {recipe.SkillTool} (required level {recipe.SkillLevel} minimum in {recipe.SkillName} skill)";

        // Add a StackPanel to hold the line and the delete button
        StackPanel panel = new StackPanel { Orientation = Orientation.Horizontal };

        // Add a button dynamically
        Button deleteButton = new Button { Content = "X", Visibility = Visibility.Hidden, HorizontalAlignment = HorizontalAlignment.Left, Background = Brushes.Red, Width = 20};
        deleteButton.Click += (sender, e) =>
        {
            // Remove the element when the button is clicked
            elemsGrid.Children.Remove(panel);
            UpdateStepsStatus();
        };

        // Add the delete button to the StackPanel
        panel.Children.Add(deleteButton);

        panel.MouseEnter += (sender, e) =>
        {
            // Show the delete button when the mouse enters the element
            deleteButton.Visibility = Visibility.Visible;
        };

        panel.MouseLeave += (sender, e) =>
        {
            // Hide the delete button when the mouse leaves the element
            deleteButton.Visibility = Visibility.Hidden;
        };

        // Add the line to the StackPanel
        panel.Children.Add(line);
        panel.SetValue(Grid.RowProperty, elemsGrid.Children.Count);
        elemsGrid.Children.Add(panel);

    }

    public void UpdateStepsStatus()
    {
        for (var i = 0; i < StepsGrid.Children.Count; i++)
        {
            var step = (Border)StepsGrid.Children[i];
            var grid = (Grid)step.Child;
            if (grid.Children.Count == 1)
                StepsGrid.Children.Remove(step);
        }
    }

    public void WriteToFile()
    {
        string recipeAsStringMd = "### Ingredients:\r\n";
        string recipeAsStringTxt = "Ingredients:\r\n";

        foreach (var line in ingredients)
        {
            if (line.Text.EndsWith(":"))
            {
                recipeAsStringMd += $"\r\n####{line.Text}\r\n";
                recipeAsStringTxt += $"\r\n {line.Text}\r\n";
            }
            else
            {
                recipeAsStringMd += "-" + line.Text + "\r\n";
                recipeAsStringTxt += $"\t{line.Text}\r\n";
            }
        }

        int i = 1;
        foreach (var craft in crafts)
        {
            recipeAsStringMd += $"#####Step{i}\r\n- {craft.Text}\r\n";
            recipeAsStringTxt += $" Step{i}\r\n\t- {craft.Text}\r\n";
            i++;
        }

        Directory.CreateDirectory("../outputs");
        
        using (var sw = new StreamWriter("../outputs/output.md"))
            sw.Write(recipeAsStringMd);
        using (var sw = new StreamWriter("../outputs/output.txt"))
            sw.Write(recipeAsStringTxt);
    }
}