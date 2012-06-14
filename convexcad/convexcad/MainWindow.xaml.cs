using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;
using _3DTools;

namespace convexcad
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SafeScreenSpaceLines3D blacklines = new SafeScreenSpaceLines3D();
        SafeScreenSpaceLines3D redlines = new SafeScreenSpaceLines3D();

        public MainWindow()
        {
            InitializeComponent();            
        }

        private void simpleButtonClick(object sender, RoutedEventArgs e)
        {
            this.mainViewport.Children.Clear();
            blacklines.Points.Clear();
            redlines.Points.Clear();

            //create new buffers for lines
            blacklines.Color = Colors.Black;
            redlines.Color = Colors.Red;
            redlines.Thickness = 4;

            //create the main scene
            MyScene s = new MyScene();
            Geometry.CSGScene.DebugLines = redlines;
            Geometry.CSGScene.TargetStage++;
            Geometry.CSGScene.Stages.Clear();
            s.Create();

            //get and add geometry
            MeshGeometry3D triangleMesh = Geometry.CSGScene.LastNode.GetGeometry();
            Color c = Colors.Blue;
            c.A = 100;
            Material material = new DiffuseMaterial(
                new SolidColorBrush(c));

            GeometryModel3D triangleModel = new GeometryModel3D(
                triangleMesh, material);
            ModelVisual3D model = new ModelVisual3D();
            model.Content = triangleModel;
            this.mainViewport.Children.Add(model);

            //get and add wireframe
            Geometry.CSGScene.LastNode.GetWireFrame(blacklines);

            /*Point3D line0a = new Point3D(-2, 0, 0);
            Point3D line0b = new Point3D(5, 0, 0);
            Point3D line1a = new Point3D(0, -5, 0);
            Point3D line1b = new Point3D(1, -1, 0);

            blacklines.AddLine(line0a, line0b);
            blacklines.AddLine(line1a, line1b);

            Point3D hitpoint = new Point3D();
            double hitu = 0;
            double hitv = 0;
            if (Math.IntersectLineLine2d(ref hitpoint, ref hitu, ref hitv, line0a, line0b, line1a, line1b))
            {
                redlines.AddCross(hitpoint, 1);
            }*/

            mainViewport.Children.Add(redlines);
            mainViewport.Children.Add(blacklines);
        }
    }

    public class MyScene : Geometry.CSGScene
    {
        public void Create()
        {
            Root = 
            Union(
                Rectangle(4, 3),
                Translate(2, 2, 0,Rectangle(4, 3)),
                Translate(-1, 3, 0,Rectangle(5, 6)),
                Translate(2, 1, 0,Rectangle(7, 1)),
                Translate(3, 1, 0,Rectangle(1, 7))
            );
            Root.Run();
        }
    }


}
