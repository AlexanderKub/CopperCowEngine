using Editor.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace Editor.UIControls
{
    /// <summary>
    /// Interaction logic for UIVector2UserControlEditor.xaml
    /// </summary>
    public partial class UIVector2UserControlEditor : UserControl, ITypeEditor
    {
        public UIVector2UserControlEditor() {
            InitializeComponent();
            DataContext = this;
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(UIVector2), typeof(UIVector2UserControlEditor),
            new FrameworkPropertyMetadata(null, 
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public UIVector2 Value {
            get {
                return (UIVector2)GetValue(ValueProperty);
            }
            set {
                SetValue(ValueProperty, value);
            }
        }

        public double XVal {
            get {
                return ((UIVector2)GetValue(ValueProperty)).X;
            }
            set {
                SetValue(ValueProperty, new UIVector2(value, YVal));
            }
        }
        
        public double YVal {
            get {
                return ((UIVector2)GetValue(ValueProperty)).Y;
            }
            set {
                SetValue(ValueProperty, new UIVector2(XVal, value));
            }
        }

        public FrameworkElement ResolveEditor(Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem propertyItem) {
            Binding binding = new Binding("Value");
            binding.Source = propertyItem;
            binding.Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay;
            BindingOperations.SetBinding(this, UIVector2UserControlEditor.ValueProperty, binding);
            return this;
        }
    }

    public class UIVector2  {
        public double X { get; set; }
        public double Y { get; set; }

        public UIVector2(double x, double y) {
            X = x;
            Y = y;
        }
    }
}
