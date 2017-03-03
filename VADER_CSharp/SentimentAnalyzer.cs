using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VADER_CSharp
{
    public class SentimentAnalyzer
    {
        private String inputString;
        private TextProperties inputStringProperties;
        private Dictionary<String, float> polarity;

        public SentimentAnalyzer(string inputString)
        {
            this.inputString = inputString;
            setupInputStringProperties();
        }

        private void setupInputStringProperties()
        {
            inputStringProperties = new TextProperties(inputString);
        }

        public Dictionary<string, float> getPolarity()
        {
            return polarity;
        }

        public void analyse()
        {
            polarity = getSentiment();
        }

        private float valenceModifier(String precedingWord, float currentValence)
        {
            float scalar = 0.0f;
            String precedingWordLower = precedingWord.ToLower();
            if (Utils.BOOSTER_DICTIONARY.ContainsKey(precedingWordLower))
            {
                scalar = Utils.BOOSTER_DICTIONARY[precedingWordLower];
                if (currentValence < 0.0)
                    scalar *= -1;
                if (Utils.isUpper(precedingWord) && inputStringProperties.getCapDIff())
                    scalar = (currentValence > 0.0) ? scalar + Utils.ALL_CAPS_BOOSTER_SCORE : scalar - Utils.ALL_CAPS_BOOSTER_SCORE;
            }
            return scalar;
        }

        private int pythonIndexToJavaIndex(int pythonIndex)
        {
            return inputStringProperties.getWordsAndEmoticons().Count - Math.Abs(pythonIndex);
        }

        private float checkForNever(float currentValence, int startI, int i, int closeTokenIndex)
        {
           ArrayList wordsAndEmoticons = inputStringProperties.getWordsAndEmoticons();
 /*
            if (startI == 0)
            {
                if (isNegative(new ArrayList(wordsAndEmoticons[i - 1]).ToArray())
                {
                    currentValence *= Utils.N_SCALAR;
                }
            }

           

            if (startI == 1)
            {
                String wordAtDistanceTwoLeft = wordsAndEmoticons.get(i - 2);
                String wordAtDistanceOneLeft = wordsAndEmoticons.get(i - 1);
                if ((wordAtDistanceTwoLeft.equals("never")) && (wordAtDistanceOneLeft.equals("so") || (wordAtDistanceOneLeft.equals("this"))))
                {
                    currentValence *= 1.5f;
                }
                else if (isNegative(new ArrayList<>(Collections.singletonList(wordsAndEmoticons.get(closeTokenIndex)))))
                {
                    currentValence *= Utils.N_SCALAR;
                }
            }

            if (startI == 2)
            {
                String wordAtDistanceThreeLeft = wordsAndEmoticons[(i - 3)].ToString();
                String wordAtDistanceTwoLeft = wordsAndEmoticons[(i - 2)].ToString();
                String wordAtDistanceOneLeft = wordsAndEmoticons[(i - 1)].ToString();
                if ((wordAtDistanceThreeLeft.equals("never")) &&
                        (wordAtDistanceTwoLeft.equals("so") || wordAtDistanceTwoLeft.equals("this")) ||
                        (wordAtDistanceOneLeft.equals("so") || wordAtDistanceOneLeft.equals("this")))
                {
                    currentValence *= 1.25f;
                }
                else if (isNegative(new ArrayList(Collections.singletonList(wordsAndEmoticons.get(closeTokenIndex)))))
                {
                    currentValence *= Utils.N_SCALAR;
                }
            }*/

            return currentValence;
        }

        private Dictionary<string, float> getSentiment()
        {
            ArrayList sentiments = new ArrayList();
            ArrayList wordsAndEmoticons = inputStringProperties.getWordsAndEmoticons();

            foreach (String item in wordsAndEmoticons)
            {
                float currentValence = 0.0f;
                int i = wordsAndEmoticons.IndexOf(item);

                if (i < wordsAndEmoticons.Count - 1 &&
                        item.ToLower().Equals("kind") &&
                        wordsAndEmoticons[i + 1].ToString().ToLower().Equals("of") ||
                        Utils.BOOSTER_DICTIONARY.ContainsKey(item.ToLower()))
                {
                    sentiments.Add(currentValence);
                    continue;
                }

                String currentItemLower = item.ToLower();
                if (Utils.WORD_VALENCE_DICTIONARY.ContainsKey(currentItemLower))
                {
                    currentValence = Utils.WORD_VALENCE_DICTIONARY[currentItemLower];

                    if (Utils.isUpper(item) && inputStringProperties.getCapDIff())
                    {
                        currentValence = (currentValence > 0.0) ? currentValence + Utils.ALL_CAPS_BOOSTER_SCORE : currentValence - Utils.ALL_CAPS_BOOSTER_SCORE;
                    }


                    int startI = 0;
                    float gramBasedValence = 0.0f;
                    while (startI < 3)
                    {

                        int closeTokenIndex = i - (startI + 1);
                        if (closeTokenIndex < 0)
                        {
                            closeTokenIndex = pythonIndexToJavaIndex(closeTokenIndex);
                        }

                        if ((i > startI) && !Utils.WORD_VALENCE_DICTIONARY.ContainsKey(wordsAndEmoticons[closeTokenIndex].ToString().ToLower()))
                        {
                            gramBasedValence = valenceModifier(wordsAndEmoticons[closeTokenIndex].ToString(), currentValence);
                            if (startI == 1 && gramBasedValence != 0.0f)
                                gramBasedValence *= 0.95f;
                            if (startI == 2 && gramBasedValence != 0.0f)
                                gramBasedValence *= 0.9f;

                            currentValence += gramBasedValence;

                            currentValence = checkForNever(currentValence, startI, i, closeTokenIndex);

                            if (startI == 2)
                                currentValence = 0; //checkForIdioms(currentValence, i);

                        }
                        startI++;
                    }
                }
            }
            return null;
        }

        private Boolean hasAtLeast(ArrayList tokenList)
        {
            if (tokenList.Contains("least"))
            {
                int index = tokenList.IndexOf("least");
                if (index > 0 && tokenList[(index - 1)].Equals("at"))
                    return true;
            }
            return false;
        }

        private Boolean hasContraction(ArrayList tokenList)
        {
            foreach (String s in tokenList)
            {
                if (s.EndsWith("n't"))
                    return true;
            }
            return false;
        }

        private Boolean hasNegativeWord(ArrayList tokenList, ArrayList newNegWords)
        {
            foreach (String newNegWord in newNegWords)
            {
                if (tokenList.Contains(newNegWord))
                    return true;
            }
            return false;

        }

        private Boolean isNegative(ArrayList tokenList, ArrayList newNegWords, Boolean checkContractions)
        {
            newNegWords.AddRange(Utils.NEGATIVE_WORDS);
            Boolean result = hasNegativeWord(tokenList, newNegWords) || hasAtLeast(tokenList);
            if (checkContractions)
                return result;
            return result || hasContraction(tokenList);
        }

        private Boolean isNegative(ArrayList tokenList, ArrayList newNegWords)
        {
            newNegWords.AddRange(Utils.NEGATIVE_WORDS);
            return hasNegativeWord(tokenList, newNegWords) || hasAtLeast(tokenList) || hasContraction(tokenList);
        }

        private Boolean isNegative(ArrayList tokenList)
        {
            return hasNegativeWord(tokenList, Utils.NEGATIVE_WORDS) || hasAtLeast(tokenList) || hasContraction(tokenList);
        }


    }
}
