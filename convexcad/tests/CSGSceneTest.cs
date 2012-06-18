using convexcad.Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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


        /// <summary>
        ///A test for Rectangle
        ///</summary>
        [TestMethod()]
        public void RectangleTest()
        {
            CSGScene target = new CSGScene(); // TODO: Initialize to an appropriate value
            double x = 0F; // TODO: Initialize to an appropriate value
            double y = 0F; // TODO: Initialize to an appropriate value
            Node expected = null; // TODO: Initialize to an appropriate value
            Node actual;
            actual = target.Rectangle(x, y);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Difference
        ///</summary>
        [TestMethod()]
        public void DifferenceTest()
        {
            CSGScene target = new CSGScene(); // TODO: Initialize to an appropriate value
            Node[] nodes = null; // TODO: Initialize to an appropriate value
            Node expected = null; // TODO: Initialize to an appropriate value
            Node actual;
            actual = target.Difference(nodes);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Intersect
        ///</summary>
        [TestMethod()]
        public void IntersectTest()
        {
            CSGScene target = new CSGScene(); // TODO: Initialize to an appropriate value
            Node[] nodes = null; // TODO: Initialize to an appropriate value
            Node expected = null; // TODO: Initialize to an appropriate value
            Node actual;
            actual = target.Intersect(nodes);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Translate
        ///</summary>
        [TestMethod()]
        public void TranslateTest()
        {
            CSGScene target = new CSGScene(); // TODO: Initialize to an appropriate value
            double x = 0F; // TODO: Initialize to an appropriate value
            double y = 0F; // TODO: Initialize to an appropriate value
            double z = 0F; // TODO: Initialize to an appropriate value
            Node[] nodes = null; // TODO: Initialize to an appropriate value
            Node expected = null; // TODO: Initialize to an appropriate value
            Node actual;
            actual = target.Translate(x, y, z, nodes);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Union
        ///</summary>
        [TestMethod()]
        public void UnionTest()
        {
            CSGScene target = new CSGScene(); // TODO: Initialize to an appropriate value
            Node[] nodes = null; // TODO: Initialize to an appropriate value
            Node expected = null; // TODO: Initialize to an appropriate value
            Node actual;
            actual = target.Union(nodes);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Box
        ///</summary>
        [TestMethod()]
        public void BoxTest()
        {
            CSGScene target = new CSGScene(); // TODO: Initialize to an appropriate value
            double x = 0F; // TODO: Initialize to an appropriate value
            double y = 0F; // TODO: Initialize to an appropriate value
            double z = 0F; // TODO: Initialize to an appropriate value
            Node expected = null; // TODO: Initialize to an appropriate value
            Node actual;
            actual = target.Box(x, y, z);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
