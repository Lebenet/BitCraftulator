using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace BitCraftulator;

public partial class ShowSteps : Window
{
    private int _quantity;
    private Dictionary<int, Dictionary<Recipe, int>> AllIngredients;
    public ShowSteps(Stack<List<(Recipe,int)>> steps, int quantity)
    {
        InitializeComponent();

        _quantity = quantity;
        StepsGrid.RowDefinitions.Add(new RowDefinition {Height = new GridLength()});
        AllIngredients = new Dictionary<int, Dictionary<Recipe, int>>
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
        while (steps.Count > 0)
        {
            var step = steps.Pop();
            List<Recipe> buildIngrs = new();
            List<Recipe> newStep = new();
            Dictionary<(Recipe, int), int> divs = new Dictionary<(Recipe, int), int>();
            for (var j = 0; j < step.Count; j++)
            {
                Recipe recipe = step[j].Item1;
                if (recipe.Ingredients!.Count == 0)
                {
                    buildIngrs.Add(recipe);
                    if (!divs.ContainsKey((recipe, step[j].Item2)))
                        divs[(recipe, step[j].Item2)] = 1;
                    else
                        divs[(recipe, step[j].Item2)]++;
                }
                else
                    newStep.Add(recipe);
            }

            (Recipe, int)[] keys = new (Recipe, int)[divs.Keys.Count];
            divs.Keys.CopyTo(keys, 0);
            
            foreach ((Recipe r, int nn) in keys)
            {
                int nn2 = divs[(r, nn)];
                divs.Remove((r, nn));
                divs[(r, nn2)] = nn;
            }

            foreach ((Recipe recipe, int n2) in CleanStep(buildIngrs))
            {
                int t;
                if (!AllIngredients[recipe.Tier].ContainsKey(recipe))
                    AllIngredients[recipe.Tier][recipe] = (t = (n2 * _quantity) / divs[(recipe,n2)]) > 0 ? t : 1;
                else
                    AllIngredients[recipe.Tier][recipe] += (t = (n2 * _quantity) / divs[(recipe,n2)]) > 0 ? t : 1;
            }
            
            if (newStep.Count != 0)
                DisplayStep(newStep, i++);
        }

        DisplayIngredients();
        
        Close.Click += (sender, e) => Close();
        //Console.WriteLine(StepsGrid.Children.Count);
    }

    public void DisplayIngredients()
    {
        
        Border border = new Border { Background = Brushes.Azure, BorderBrush = Brushes.Gray, BorderThickness = new Thickness(0, 3, 0, 0)};
        border.SetValue(Grid.RowProperty, 0);
        StepsGrid.Children.Add(border);
        
        Grid elemsGrid = new Grid {Name = "Step1"};
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
            tier.SetValue(Grid.RowProperty, elemsGrid.Children.Count);
            elemsGrid.Children.Add(tier);
            foreach (var recipe in AllIngredients[elem].Keys)
            {
                TextBlock line = new TextBlock { TextWrapping = TextWrapping.Wrap };

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
    

    public void DisplayStep(List<Recipe> stepRaw, int stepNumber)
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
            int t;
            if (elem.RecipeName.Contains("Output"))
                continue;

            TextBlock line = new TextBlock { TextWrapping = TextWrapping.Wrap };

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

            if (elem.Ingredients!.Count == 0)
                line.Text = $"Get {step[elem] * _quantity} {elem.RecipeName}";
            else
                line.Text = $"{( elem.Station!.ToLower().Contains("field") || elem.Station!.ToLower().Contains("pot") ? "Grow" : "Craft" )} {(elem.Ingredients.Count == 1 && elem.Ingredients[0].Name.Contains("Output") ? (t =((t = (step[elem] * _quantity - 1) / elem.Output[0].Quantity) > 0 ? t+1 : 1)) : (t =((t = (step[elem] * _quantity - 1) / elem.Output[0].Quantity) > 0 ? t+1 : 1)) + (elem.Output![0].Quantity * _quantity * step[elem] != 1 && elem.Output![0].Quantity * step[elem] * _quantity != step[elem] * _quantity ? $" (gives {t = elem.Output![0].Quantity * t}{(t == _quantity * step[elem] ? "" : $", need {_quantity * step[elem]}")})" : ""))} {elem.RecipeName} at {elem.Station} with tool {elem.SkillTool} (required level {elem.SkillLevel} minimum in {elem.SkillName} skill)";

            // Add the StackPanel to the grid
            elemsGrid.Children.Add(panel);
        }

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

    public Dictionary<Recipe, int> CleanStep(List<Recipe> stepRaw)
    {
        var res = new Dictionary<Recipe, int>();

        foreach (var elem in stepRaw)
        {
            if (Contains(res, elem))
                res[elem]++;
            else
                res[elem] = 1;
        }

        return res;
    }

    public bool Contains(Dictionary<Recipe, int> dico, Recipe check)
    {
        foreach (var elem in dico.Keys)
            if (elem.RecipeName == check.RecipeName)
                return true;
        return false;
    }
}