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

            var test = Utils.getWordValenceDictionary("./vader_sentiment_lexicon.txt");

        }
    }
}
