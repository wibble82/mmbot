using convexcad.Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using convexcad;

namespace tests
{
    
    
    /// <summary>
    ///This is a test class for CSGSceneTest and is intended
    ///to contain all CSGSceneTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CSGSceneTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        [Serializable]
        public class RectNodeTest : CSGScene
        {
            public override Node Create()
            {
                return Rectangle(1,1);
            }
        }

        void CompareGeometry(CSGSceneTestFIle tst, Vertex[] vertices, Edge[] edges)
        {
            Assert.AreEqual(tst.ResultVertices.Length, vertices.Length);
            Assert.AreEqual(tst.ResultEdges.Length, edges.Length);
            for (int vidx = 0; vidx < tst.ResultVertices.Length; vidx++ )
            {
                Assert.AreEqual(tst.ResultVertices[vidx].Pos, vertices[vidx].Pos);
            }
            for (int eidx = 0; eidx < tst.ResultEdges.Length; eidx++)
            {
                Assert.AreEqual(tst.ResultEdges[eidx].VertIndices[0], edges[eidx].VertIndices[0]);
                Assert.AreEqual(tst.ResultEdges[eidx].VertIndices[1], edges[eidx].VertIndices[1]);
            }
        }

        public void RunDefaultSceneTest(CSGScene scene, string filename)
        {
            string fullpath = @"C:\Users\chris\mmbot\convexcad\convexcad\Scenes\" + filename;

            if (File.Exists(fullpath))
            {
                CSGSceneTestFIle tst = CSGScene.LoadTestFile(fullpath);
                if(scene.Root == null)
                    scene.Run();
                Vertex[] vertices = null;
                Edge[] edges = null;
                scene.Root.GetWeldedGeometry(out vertices, out edges);
                CompareGeometry(tst, vertices, edges);
            }
            else
            {
                scene.SaveTestFile(fullpath);
            }

        }

        public void RunCodeSceneTest(string filename, string classname)
        {
            string fullpath = @"C:\Users\chris\mmbot\convexcad\convexcad\Scenes\" + filename;

            CSGScene res = (CSGScene)SceneRunner.ExecuteCode(fullpath, "convexcad", classname, "Run", false);
            RunDefaultSceneTest(res, filename + "." + classname + ".dat");
        }

        [TestMethod()]
        public void RectangleTest()
        {
            RunCodeSceneTest("rectangle.cs", "RectangleTestScene");
        }
        [TestMethod()]
        public void RectangleTest2()
        {
            RunCodeSceneTest("rectangle.cs", "RectangleTestScene2");
        }
        [TestMethod()]
        public void RectangleTest3()
        {
            RunCodeSceneTest("rectangle.cs", "RectangleTestScene3");
        }
        [TestMethod()]
        public void RectangleTranslated()
        {
            RunCodeSceneTest("rectangle.cs", "RectangleTranslated");
        }
        [TestMethod()]
        public void RectangleTranslated2()
        {
            RunCodeSceneTest("rectangle.cs", "RectangleTranslated2");
        }
        [TestMethod()]
        public void RectangleTranslated3()
        {
            RunCodeSceneTest("rectangle.cs", "RectangleTranslated3");
        }
        [TestMethod()]
        public void RectangleRotateX()
        {
            RunCodeSceneTest("rectangle.cs", "RectangleRotateX");
        }
        [TestMethod()]
        public void RectangleRotateY()
        {
            RunCodeSceneTest("rectangle.cs", "RectangleRotateY");
        }
        [TestMethod()]
        public void RectangleRotateZ()
        {
            RunCodeSceneTest("rectangle.cs", "RectangleRotateZ");
        }
        [TestMethod()]
        public void RectangleRotateMany()
        {
            RunCodeSceneTest("rectangle.cs", "RectangleRotateMany");
        }

        [TestMethod()]
        public void UnionTest_RectRectOverlapping()
        {
            RunCodeSceneTest("union.cs", "UnionTest_RectRectOverlapping");
        }
        [TestMethod()]
        public void UnionTest_RectRectTouching()
        {
            RunCodeSceneTest("union.cs", "UnionTest_RectRectTouching");
        }
        [TestMethod()]
        public void UnionTest_RectRectNotOverlapping()
        {
            RunCodeSceneTest("union.cs", "UnionTest_RectRectNotOverlapping");
        }
        [TestMethod()]
        public void UnionTest_RectRectContainedA()
        {
            RunCodeSceneTest("union.cs", "UnionTest_RectRectContainedA");
        }
       /* [TestMethod()]
        public void UnionTest_RectRectContainedB()
        {
            RunCodeSceneTest("union.cs", "UnionTest_RectRectContainedB");
        }*/
        [TestMethod()]
        public void UnionTest_MultiRectTranslatedOverlapping()
        {
            RunCodeSceneTest("union.cs", "UnionTest_MultiRectTranslatedOverlapping");
        }

    }
}
