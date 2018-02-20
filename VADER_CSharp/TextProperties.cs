using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VADER_CSharp
{
    public class TextProperties
    {
        private String inputText;
        private ArrayList wordsAndEmoticons;
        private ArrayList wordsOnly;
        private bool isCapDIff;

        public static int countLetter(String s, String l)
        {
            int numberOfLetters = 0;
            int positionOfLetter = s.IndexOf(l);
            while (positionOfLetter != -1)
            {
                numberOfLetters++;
                positionOfLetter = s.IndexOf(l, positionOfLetter + 1);
            }
            return numberOfLetters;
        }

        private void setWordsAndEmoticons()
        {
            setWordsOnly();

            ArrayList wordsAndEmoticonsList = VaderLuceneAnalyzer.defaultSplit(inputText);
            foreach (var currentWord in wordsOnly)
            {
                foreach (var currentPunc in Utils.PUNCTUATION_LIST)
                {
                    String pWord = currentWord.ToString() + currentPunc.ToString();
                    IList<string> words = new List<string> { };
                    foreach (var wordAndEmoticon in wordsAndEmoticonsList)
                    {
                        words.Add(wordAndEmoticon.ToString());
                    }

                    int pWordCount = words.Where(x => x.Equals(pWord)).Count();

                    while (pWordCount > 0)
                    {
                        int index = wordsAndEmoticonsList.IndexOf(pWord);
                        wordsAndEmoticonsList.Remove(pWord);
                        wordsAndEmoticonsList.Insert(index, currentWord);

                        words = new List<string> { };
                        foreach (var wordAndEmoticon in wordsAndEmoticonsList)
                        {
                            words.Add(wordAndEmoticon.ToString());
                        }

                        pWordCount = words.Where(x => x.Equals(pWord)).Count();

                    }

                    String wordP = currentPunc.ToString() + currentWord.ToString();
                    words = new List<string> { };
                    foreach (var wordAndEmoticon in wordsAndEmoticonsList)
                    {
                        words.Add(wordAndEmoticon.ToString());
                    }
                    int wordPCount = words.Where(x => x.Equals(wordP)).Count();

                    while (wordPCount > 0)
                    {
                        int index = wordsAndEmoticonsList.IndexOf(wordP);
                        wordsAndEmoticonsList.Remove(wordP);
                        wordsAndEmoticonsList.Insert(index, currentWord);

                        words = new List<string> { };
                        foreach (var wordAndEmoticon in wordsAndEmoticonsList)
                        {
                            words.Add(wordAndEmoticon.ToString());
                        }

                        wordPCount = words.Where(x => x.Equals(wordP)).Count();

                    }

                }
            }

            this.wordsAndEmoticons = wordsAndEmoticonsList;
        }


        public TextProperties(String inputText)
        {
            this.inputText = inputText;
            setWordsAndEmoticons();
            setAllCapDifferential();
        }
        
        private void setWordsOnly()
        {
            this.wordsOnly = VaderLuceneAnalyzer.removePunctuation(inputText);
        }

        private void setCapDiff(bool capDIff)
        {
            isCapDIff = capDIff;
        }

        public bool isCapDifferential()
        {
            return isCapDIff;
        }

        public ArrayList getWordsAndEmoticons()
        {
            return wordsAndEmoticons;
        }

        public ArrayList getWordsOnly()
        {
            return wordsOnly;
        }

        private bool setAllCapDifferential()
        {
            int countAllCaps = 0;
            foreach (String s in wordsAndEmoticons)
                if (Utils.IsUpper(s))
                    countAllCaps += 1;
            int capDifferential = wordsAndEmoticons.Count - countAllCaps;
            return (0 < capDifferential) && (capDifferential < wordsAndEmoticons.Count);
        }
        
        public Boolean getCapDiff()
        {
            return isCapDIff;
        }




    }
}
