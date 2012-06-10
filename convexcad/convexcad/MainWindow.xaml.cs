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
        public MainWindow()
        {
            InitializeComponent();


        }

        private void simpleButtonClick(object sender, RoutedEventArgs e)
        {
            /*MeshGeometry3D triangleMesh = new MeshGeometry3D();
            Point3D point0 = new Point3D(0, 0, 0);
            Point3D point1 = new Point3D(5, 0, 0);
            Point3D point2 = new Point3D(0, 0, 5);
            triangleMesh.Positions.Add(point0);
            triangleMesh.Positions.Add(point1);
            triangleMesh.Positions.Add(point2);
            triangleMesh.TriangleIndices.Add(0);
            triangleMesh.TriangleIndices.Add(2);
            triangleMesh.TriangleIndices.Add(1);
            Vector3D normal = new Vector3D(0, 1, 0);
            triangleMesh.Normals.Add(normal);
            triangleMesh.Normals.Add(normal);
            triangleMesh.Normals.Add(normal);*/

            MyScene s = new MyScene();
            s.Create();
            MeshGeometry3D triangleMesh = s.Root.GetGeometry();

            Material material = new DiffuseMaterial(
                new SolidColorBrush(Colors.DarkKhaki));
            GeometryModel3D triangleModel = new GeometryModel3D(
                triangleMesh, material);
            ModelVisual3D model = new ModelVisual3D();
            model.Content = triangleModel;
            this.mainViewport.Children.Add(model);
            this.mainViewport.Children.Add(s.Root.GetWireFrame());
        }
    }

    public class MyScene : Geometry.CSGScene
    {
        public void Create()
        {
            Root = 
            Union(
                Rectangle(4, 3),
                Translate(2, 0, 2,
                    Rectangle(4, 3)
                )
            );
            Root.Run();
        }
    }


}
