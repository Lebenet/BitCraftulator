
using System.Windows;
using System.Windows.Controls.Primitives;

namespace BitCraftulator;

/// <summary>
/// Interaction logic for IngredientList.xaml
/// </summary>
public partial class RecipeInfo : Window
{
    private List<string> Outputs { get; }
    private List<(string, int)> Ingredients { get; }
    
    public RecipeInfo(Recipe recipe)
    {
        Console.WriteLine("111");
        InitializeComponent();

        Outputs = new();
        Ingredients = new();
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