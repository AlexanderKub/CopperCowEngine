using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace CopperCowEngine.EditorApp.UIControls
{
    /// <summary>
    /// Interaction logic for UIVector2UserControlEditor.xaml
    /// </summary>
    public partial class UiVector2UserControlEditor : UserControl, ITypeEditor
    {
        public UiVector2UserControlEditor() 
        {
            InitializeComponent();
            DataContext = this;
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(UIVector2), typeof(UiVector2UserControlEditor),
            new FrameworkPropertyMetadata(null, 
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public UIVector2 Value 
        {
            get => (UIVector2)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public double XVal 
        {
            get => ((UIVector2)GetValue(ValueProperty)).X;
            set => SetValue(ValueProperty, new UIVector2(value, YVal));
        }
        
        public double YVal 
        {
            get => ((UIVector2)GetValue(ValueProperty)).Y;
            set => SetValue(ValueProperty, new UIVector2(XVal, value));
        }

        public FrameworkElement ResolveEditor(Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem propertyItem) 
        {
            var binding = new Binding("Value")
            {
                Source = propertyItem, 
                Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay
            };
            BindingOperations.SetBinding(this, ValueProperty, binding);
            return this;
        }
    }

    public class UIVector2  {
        public double X { get; set; }
        public double Y { get; set; }

        public UIVector2(double x, double y) 
        {
            X = x;
            Y = y;
        }
    }
}
