using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VADER_CSharp;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            System.Collections.ArrayList al = Utils.NEGATIVE_WORDS;
            System.Collections.ArrayList nl = new System.Collections.ArrayList();
            
           SentimentAnalyzer sa =new SentimentAnalyzer("this is a test");
           sa.analyse();

        }
    }
}
