using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace iCabinet.Controls
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:iCabinet.Controls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:iCabinet.Controls;assembly=iCabinet.Controls"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:Loading/>
    ///
    /// </summary>
    public class Loading : Control
    {
        static Loading()
        {
            //重载默认样式
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Loading), new FrameworkPropertyMetadata(typeof(Loading)));
            //DependencyProperty 注册  FillColor
            FillColorProperty = DependencyProperty.Register("FillColor",
                typeof(Color),
                typeof(Loading),
                new UIPropertyMetadata(Color.FromRgb(64,158,255),
                new PropertyChangedCallback(OnUriChanged))
                );
            //Colors.DarkBlue为控件初始化默认值

        }
        //属性变更回调函数
        private static void OnUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //Border b = (Border)d;
            //MessageBox.Show(e.NewValue.ToString());

        }
        #region 自定义Fields
        // DependencyProperty属性定义   FillColorProperty=FillColor+Property组成
        public static readonly DependencyProperty FillColorProperty;
        #endregion
        //VS设计器属性支持
        [Description("背景色"), Category("个性配置"), DefaultValue("#ff409eff")]
        public Color FillColor
        {
            //GetValue,SetValue为固定写法，此处一般不建议处理其他逻辑
            get { return (Color)GetValue(FillColorProperty); }
            set { SetValue(FillColorProperty, value); }
        }
    }
}
