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
using System.Reflection;
using Microsoft.Win32;
using System.IO;
using System.ComponentModel;
using System.Threading;

namespace convexcad
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        Assembly CurrentAssembly = null;
        string CurrentAssemblyName = "";
        List<string> RecentItems = new List<string>();
        List<string> SceneNames = new List<string>();
        string CurrentSceneName = "";
        Shapes.Scene CurrentScene = null;
        FileSystemWatcher SceneFileWatcher = null;
        bool EnableFileWatcher = false;

        SafeScreenSpaceLines3D blacklines = new SafeScreenSpaceLines3D();
        SafeScreenSpaceLines3D redlines = new SafeScreenSpaceLines3D();

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }


        private void keyUp(object sender, KeyEventArgs e)
        {
            bool ctrldown = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

            if (e.Key == Key.F7)
                runButtonClick(sender, e);
            else if (e.Key == Key.F9)
                stepButtonClick(sender, e);
            else if (e.Key == Key.F10)
                repeatStepButtonClick(sender, e);
            else if (ctrldown && e.Key == Key.F7)
                restartButtonClick(sender, e);
            else if (ctrldown && e.Key == Key.N)
                newButtonClick(sender, e);
            else if (ctrldown && e.Key == Key.S)
                saveButtonClick(sender, e);
            else if (ctrldown && e.Key == Key.L)
                openButtonClick(sender, e);
            else if (ctrldown && e.Key == Key.R)
                reloadButtonClick(sender, e);
        }

        public MainWindow()
        {
            InitializeComponent();

            //Shapes.Primitives.Rectangle(new Vector3D(1, 1, 0));

            EventManager.RegisterClassHandler(typeof(Window), Keyboard.KeyUpEvent, new KeyEventHandler(keyUp), true);

            if (Properties.Settings.Default.RecentFiles != null)
            {
                foreach (string s in Properties.Settings.Default.RecentFiles)
                    RecentItems.Add(s);
            }

            sceneBox.DataContext = this;
            recentFilesBox.DataContext = this;
            reloadCheckBox.DataContext = this;
            showEdgesCheckBox.DataContext = this;
            showFacesCheckBox.DataContext = this;
            showConvexesCheckBox.DataContext = this;
        }

        public List<string> RecentMenuItems
        {
            get
            {
                return RecentItems.ToList();
            }
        }

        public List<string> SceneMenuItems
        {
            get
            {
                return SceneNames.ToList();
            }
        }

        public bool EnableAutoReload 
        { 
            get { return EnableFileWatcher; } 
            set { EnableFileWatcher = value; NotifyPropertyChanged("EnableAutoReload"); } 
        }

        public string SceneName
        {
            get { return CurrentSceneName; }
            set { CurrentSceneName = value; NotifyPropertyChanged("SceneName"); if(CurrentSceneName != null) RunScene(CurrentSceneName); }
        }

        bool mShowEdges = true;
        bool mShowFaces = true;
        bool mShowConvexes = false;
        public bool ShowEdges { get { return mShowEdges; } set { mShowEdges = value; NotifyPropertyChanged("ShowEdges"); ReloadAssembly();  } }
        public bool ShowFaces { get { return mShowFaces; } set { mShowFaces = value; NotifyPropertyChanged("ShowFaces"); ReloadAssembly(); } }
        public bool ShowConvexes { get { return mShowConvexes; } set { mShowConvexes = value; NotifyPropertyChanged("ShowConvexes"); ReloadAssembly(); } }

        void AddRecentScene(string name)
        {
            RecentItems.Remove(name);
            RecentItems.Insert(0, name);

            Properties.Settings.Default.RecentFiles = new System.Collections.Specialized.StringCollection(); ;
            Properties.Settings.Default.RecentFiles.AddRange(RecentItems.ToArray());
            Properties.Settings.Default.Save();

            NotifyPropertyChanged("RecentMenuItems");
        }

        bool DerivesFrom(Type derivedtype, Type basetype)
        {
            if (derivedtype == basetype)
                return true;
            else if (derivedtype.BaseType != null)
                return DerivesFrom(derivedtype.BaseType, basetype);
            else
                return false;
        }

        void ReloadAssembly()
        {
            int cursorpos = editorBox.CaretOffset;

            try
            {
                resultBox.Text = "Loading " + CurrentAssemblyName + "\n";

                CurrentAssembly = SceneRunner.BuildAssembly(CurrentAssemblyName);

                Type csgscene = typeof(Shapes.Scene);
                SceneNames = CurrentAssembly.GetTypes().Where(a => DerivesFrom(a, csgscene)).Select(a=>a.FullName).ToList();
                NotifyPropertyChanged("SceneMenuItems");

                if(CurrentSceneName != null && CurrentSceneName != "")
                    RunScene(CurrentSceneName);
                else
                    resultBox.Text += "Done\n";
            }
            catch (System.ApplicationException ex)
            {
                resultBox.Text += ex.Message+"\n";
            }
            finally
            {
                using (StreamReader reader = new StreamReader(CurrentAssemblyName))
                {
                    string newtxt = reader.ReadToEnd();
                    if (newtxt != editorBox.Text)
                        editorBox.Text = newtxt;
                }

                editorBox.CaretOffset = cursorpos;
            }
        }

        void LoadAssembly(string assembly)
        {
            CurrentAssemblyName = assembly;
            SceneFileWatcher = new FileSystemWatcher(System.IO.Path.GetDirectoryName(assembly));
            SceneFileWatcher.Changed += new FileSystemEventHandler(SceneFileWatcher_Changed);
            SceneFileWatcher.EnableRaisingEvents = true;
            ReloadAssembly();
        }

        void RunScene(string scenename)
        {
            mainViewport.Children.Clear();
            blacklines.Points.Clear();
            redlines.Points.Clear();

            //create new buffers for lines
            blacklines.Color = Colors.Black;
            redlines.Color = Colors.Red;
            redlines.Thickness = 4;

            try
            {
                //print which scene is running
                resultBox.Text = "Running scene " + scenename + "\n";

                //create the main scene
                Shapes.Scene.DebugLines = redlines;
                CurrentScene = (Shapes.Scene)CurrentAssembly.CreateInstance(scenename);
                CurrentScene.Run();

                //setup model if faces visible
                if (ShowFaces)
                {
                    MeshGeometry3D triangleMesh = new MeshGeometry3D();
                    foreach (Shapes.Shape s in Shapes.Scene.LastNode.Shapes)
                        s.BuildMeshGeometry3D(triangleMesh);

                    Color c = Colors.Blue; c.A = 100;
                    Material material = new DiffuseMaterial(new SolidColorBrush(c));
                    GeometryModel3D triangleModel = new GeometryModel3D(triangleMesh, material);
                    ModelVisual3D model = new ModelVisual3D();
                    model.Content = triangleModel;

                    mainViewport.Children.Add(model);
                }

                //setup screen space lines if edges visible
                if (ShowEdges)
                {
                    foreach (Shapes.Shape s in Shapes.Scene.LastNode.Shapes)
                        s.BuildScreenSpaceLines(blacklines);
                    foreach (Shapes.Shape s in Shapes.Scene.LastNode.Shapes)
                        s.BuildCrossesAtVertices(blacklines);
                }

                //success so print done
                resultBox.Text += "Done\n";
            }
            catch (System.ApplicationException ex)
            {
                //print exception if failed
                resultBox.Text += ex.Message + "\n";
            }
            finally
            {
                //add the line buffers to the main viewport
                mainViewport.Children.Add(redlines);
                mainViewport.Children.Add(blacklines);
            }
        }

        void SceneFileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            Thread.Sleep(100);
            if(EnableFileWatcher)
                Dispatcher.Invoke(new Action( delegate(){ ReloadAssembly(); } ) ) ;
        }

        private void mainViewport_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point mousePos = e.GetPosition(mainViewport);
            PointHitTestParameters hitParams = new PointHitTestParameters(mousePos);
            VisualTreeHelper.HitTest(mainViewport, null, HTResultCallback, hitParams);
            
        }

        private HitTestResultBehavior HTResultCallback(HitTestResult result)
        {
            // Did we hit 3D?
            RayHitTestResult rayResult = result as RayHitTestResult;
            if (rayResult != null)
            {
                // Did we hit a MeshGeometry3D?
                RayMeshGeometry3DHitTestResult rayMeshResult =
                    rayResult as RayMeshGeometry3DHitTestResult;

                if (rayMeshResult != null)
                {
                    // Yes we did!
                }
            }

            return HitTestResultBehavior.Continue;
        }

        private void openButtonClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "c# file (*.cs)|*.cs";
            if ((bool)dlg.ShowDialog())
            {
                Shapes.Scene.TargetStage = -1;
                AddRecentScene(dlg.FileName);
                LoadAssembly(dlg.FileName);
                SceneName = SceneNames.FirstOrDefault();
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            recentFilesBox.IsDropDownOpen = false;

            Shapes.Scene.TargetStage = -1;

            LoadAssembly(((MenuItem)sender).Header.ToString());

            SceneName = SceneNames.FirstOrDefault();
        }

        private void reloadButtonClick(object sender, RoutedEventArgs e)
        {
            Shapes.Scene.TargetStage = -1;
            ReloadAssembly();
        }

        private void sceneBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void editorBox_DocumentChanged(object sender, EventArgs e)
        {

        }

        private void newButtonClick(object sender, RoutedEventArgs e)
        {
            CurrentAssemblyName = "";
            CurrentSceneName = "";
            CurrentAssembly = null;
            CurrentScene = null;
            SceneNames = new List<string>();
            editorBox.Text = "";
        }

        void Save()
        {
            if (CurrentAssemblyName == "")
            {
                SaveFileDialog sd = new SaveFileDialog();
                sd.Filter = "C# file (*.cs)|*.cs";
                if ((bool)sd.ShowDialog())
                {
                    CurrentAssemblyName = sd.FileName;
                }
            }

            if (CurrentAssemblyName != "")
            {
                bool fw = EnableFileWatcher;
                EnableFileWatcher = false;
                using (StreamWriter writer = new StreamWriter(CurrentAssemblyName))
                {
                    writer.Write(editorBox.Text);
                }
                EnableFileWatcher = fw;
            }
        }

        private void saveButtonClick(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void saveAsButtonClick(object sender, RoutedEventArgs e)
        {
            CurrentAssemblyName = "";
            Save();
        }

        private void runButtonClick(object sender, RoutedEventArgs e)
        {
            Shapes.Scene.TargetStage = -1;
            Save();
            ReloadAssembly();
        }

        private void stepButtonClick(object sender, RoutedEventArgs e)
        {
            Shapes.Scene.TargetStage++;
            Save();
            ReloadAssembly();
        }

        private void restartButtonClick(object sender, RoutedEventArgs e)
        {
            Shapes.Scene.TargetStage = 0;
            Save();
            ReloadAssembly();
        }

        private void repeatStepButtonClick(object sender, RoutedEventArgs e)
        {
            Save();
            ReloadAssembly();
        }

        private void runTestButtonClick(object sender, RoutedEventArgs e)
        {
            mainViewport.Children.Clear();
            blacklines.Points.Clear();
            redlines.Points.Clear();

            //create new buffers for lines
            blacklines.Color = Colors.Black;
            redlines.Color = Colors.Red;
            redlines.Thickness = 4;

            resultBox.Text = "Testing...\n";

            try
            {
                Shapes.Shape s = Shapes.Primitives.Rectangle(new Vector3D(1, 2, 0));
                //create the main scene
                MeshGeometry3D triangleMesh = new MeshGeometry3D();
                s.BuildMeshGeometry3D(triangleMesh);

                Color c = Colors.Blue;
                c.A = 100;
                Material material = new DiffuseMaterial(new SolidColorBrush(c));

                GeometryModel3D triangleModel = new GeometryModel3D(triangleMesh, material);
                ModelVisual3D model = new ModelVisual3D();
                model.Content = triangleModel;

                //if (ShowFaces)
                    mainViewport.Children.Add(model);

                //if (ShowConvexes)
                    s.BuildScreenSpaceLines(redlines);

                resultBox.Text += "Done\n";
            }
            catch (System.ApplicationException ex)
            {
                resultBox.Text += ex.Message + "\n";
            }
            finally
            {
                mainViewport.Children.Add(redlines);
                mainViewport.Children.Add(blacklines);
            }

        }
    }


}
