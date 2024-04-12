using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace BitCraftulator;

public partial class ShowSteps : Window
{
    private int _quantity;
    public ShowSteps(Stack<List<Recipe>> steps, int quantity)
    {
        InitializeComponent();

        _quantity = quantity;
        
        var i = 0;
        while (steps.Count > 0)
            DisplayStep(steps.Pop(), i++);
        
        Close.Click += (sender, e) => Close();
        Console.WriteLine(StepsGrid.Children.Count);
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

    public void DisplayStep(List<Recipe> stepRaw, int stepNumber)
    {
        Dictionary<Recipe, int> step = CleanStep(stepRaw);
        
        StepsGrid.RowDefinitions.Add(new RowDefinition {Height = new GridLength()});
        Border border = new Border { Background = Brushes.Azure, BorderBrush = Brushes.Gray, BorderThickness = new Thickness(0, 3, 0, 0)};
        border.SetValue(Grid.RowProperty, StepsGrid.Children.Count);
        StepsGrid.Children.Add(border);
        
        Grid elemsGrid = new Grid {Name = "Step1"};
        for (var i = 0; i <= step.Count + 1; i++) elemsGrid.RowDefinitions.Add(new RowDefinition());
        border.Child = elemsGrid;

        Label stepLabel = new Label { Content = $"Step {stepNumber + 1}" };
        elemsGrid.Children.Add(stepLabel);

        foreach (var elem in step.Keys)
        {
            TextBlock line = new TextBlock { TextWrapping = TextWrapping.Wrap };
            line.SetValue(Grid.RowProperty, elemsGrid.Children.Count);
            if (elem.Ingredients!.Count == 0)
                line.Text = $"Get {step[elem] * _quantity} {elem.RecipeName}";
            else
                line.Text = $"{( elem.Station!.ToLower().Contains("field") ? "Grow" : "Craft" )} {(elem.Ingredients.Count == 1 && elem.Ingredients[0].Name.Contains("Output") ? (step[elem] * _quantity) + " (gives undefined amount)" : (step[elem] * _quantity) + (elem.Output![0].Quantity * _quantity * step[elem] != 1 && elem.Output![0].Quantity * step[elem] * _quantity != step[elem] * _quantity ? $" (gives {elem.Output![0].Quantity * _quantity * step[elem]})" : ""))} {elem.RecipeName} at {elem.Station} with tool {elem.SkillTool} (required level {elem.SkillLevel} minimum in {elem.SkillName} skill)";
            elemsGrid.Children.Add(line);
        }

        var fin = new TextBlock { TextWrapping = TextWrapping.Wrap };
        fin.SetValue(Grid.RowProperty, elemsGrid.Children.Count);
        elemsGrid.Children.Add(fin);

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