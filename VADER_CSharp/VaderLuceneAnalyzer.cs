using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Lucene.Net;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Tokenattributes;
using System.Collections;

namespace VADER_CSharp
{
    public class VaderLuceneAnalyzer
    {

        public static ArrayList defaultSplit(String inputString)
        {
            StringReader reader = new StringReader(inputString);
            Tokenizer whiteSpaceTokenizer = new WhitespaceTokenizer(reader);

            TokenStream tokenStream = new LengthFilter(whiteSpaceTokenizer, 2, int.MaxValue);
            var termAttribute = tokenStream.GetAttribute<ITermAttribute>();
            tokenStream.Reset();

            ArrayList tokenizedString = new ArrayList();
            while (tokenStream.IncrementToken())
            {
                tokenizedString.Add(termAttribute.Term);
            }

            tokenStream.End();
            tokenStream.Dispose();

            return tokenizedString;
        }

        public static ArrayList removePunctuation(String inputString)
        {
            StringReader reader = new StringReader(inputString);
            Tokenizer standardTokenizer = new LetterTokenizer(reader);

            TokenStream tokenStream = new LengthFilter(standardTokenizer, 2, int.MaxValue);
            var termAttribute = tokenStream.GetAttribute<ITermAttribute>();
            tokenStream.Reset();

            ArrayList tokenizedString = new ArrayList();
            while (tokenStream.IncrementToken())
            {
                tokenizedString.Add(termAttribute.Term);
            }

            tokenStream.End();
            tokenStream.Dispose();

            return tokenizedString;
        }
    }
}
