using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
                if (Utils.isUpper(precedingWord) && inputStringProperties.getCapDiff())
                    scalar = (currentValence > 0.0) ? scalar + Utils.ALL_CAPS_BOOSTER_SCORE : scalar - Utils.ALL_CAPS_BOOSTER_SCORE;
            }
            return scalar;
        }

        private float checkForNever(float currentValence, int startI, int i, int closeTokenIndex)
        {
            ArrayList wordsAndEmoticons = inputStringProperties.getWordsAndEmoticons();
            ArrayList isNegAL = new ArrayList();


            if (startI == 0)
            {
                isNegAL.Add(wordsAndEmoticons[i - 1]);
                if (isNegative(isNegAL))
                {
                    currentValence *= Utils.N_SCALAR;
                }
            }

            if (startI == 1)
            {
                String wordAtDistanceTwoLeft = wordsAndEmoticons[i - 2].ToString();
                String wordAtDistanceOneLeft = wordsAndEmoticons[i - 1].ToString();

                isNegAL.Add(wordsAndEmoticons[closeTokenIndex]);

                if ((wordAtDistanceTwoLeft.Equals("never")) && (wordAtDistanceOneLeft.Equals("so") || (wordAtDistanceOneLeft.Equals("this"))))
                {
                    currentValence *= 1.5f;
                }
                else if (isNegative(isNegAL))
                {
                    currentValence *= Utils.N_SCALAR;
                }
            }

            if (startI == 2)
            {
                String wordAtDistanceThreeLeft = wordsAndEmoticons[(i - 3)].ToString();
                String wordAtDistanceTwoLeft = wordsAndEmoticons[(i - 2)].ToString();
                String wordAtDistanceOneLeft = wordsAndEmoticons[(i - 1)].ToString();
                isNegAL.Add(wordsAndEmoticons[closeTokenIndex]);

                if ((wordAtDistanceThreeLeft.Equals("never")) &&
                        (wordAtDistanceTwoLeft.Equals("so") || wordAtDistanceTwoLeft.Equals("this")) ||
                        (wordAtDistanceOneLeft.Equals("so") || wordAtDistanceOneLeft.Equals("this")))
                {
                    currentValence *= 1.25f;
                }
                else if (isNegative(isNegAL))
                {
                    currentValence *= Utils.N_SCALAR;
                }
            }

            return currentValence;
        }

        private float checkForIdioms(float currentValence, int i)
        {

            ArrayList wordsAndEmoticons = inputStringProperties.getWordsAndEmoticons();
            String leftBiGramFromCurrent = String.Format("%s %s", wordsAndEmoticons[i - 1], wordsAndEmoticons[i]);
            String leftTriGramFromCurrent = String.Format("%s %s %s", wordsAndEmoticons[i - 2], wordsAndEmoticons[i - 1], wordsAndEmoticons[i]);
            String leftBiGramFromOnePrevious = String.Format("%s %s", wordsAndEmoticons[i - 2], wordsAndEmoticons[i - 1]);
            String leftTriGramFromOnePrevious = String.Format("%s %s %s", wordsAndEmoticons[i - 3], wordsAndEmoticons[i - 2], wordsAndEmoticons[i - 1]);
            String leftBiGramFromTwoPrevious = String.Format("%s %s", wordsAndEmoticons[i - 3], wordsAndEmoticons[i - 2]);

            Dictionary< string, float > boosterDictionary = Utils.GetBoosterDictionary();
            Dictionary < string, float > sentimentLadenIdioms = Utils.GetSentimentLadenIdiomsDictionary();

            ArrayList leftGramSequences = new ArrayList();

            leftGramSequences.Add(leftBiGramFromCurrent);
            leftGramSequences.Add(leftTriGramFromCurrent);
            leftGramSequences.Add(leftBiGramFromOnePrevious);
            leftGramSequences.Add(leftTriGramFromOnePrevious);
            leftGramSequences.Add(leftBiGramFromTwoPrevious);

            foreach (String leftGramSequence in leftGramSequences)
            {
                if (sentimentLadenIdioms.ContainsKey(leftGramSequence))
                {
                    currentValence = sentimentLadenIdioms[leftGramSequence];
                    break;
                }
            }

            if (wordsAndEmoticons.Count - 1 > i)
            {
                String rightBiGramFromCurrent = String.Format("%s %s", wordsAndEmoticons[i], wordsAndEmoticons[i + 1]);
                if (sentimentLadenIdioms.containsKey(rightBiGramFromCurrent))
                {
                    currentValence = sentimentLadenIdioms.get(rightBiGramFromCurrent);
                }
            }
            if (wordsAndEmoticons.size() - 1 > i + 1)
            {
                final String rightTriGramFromCurrent = String.format("%s %s %s", wordsAndEmoticons.get(i), wordsAndEmoticons.get(i + 1), wordsAndEmoticons.get(i + 2));
                if (sentimentLadenIdioms.containsKey(rightTriGramFromCurrent))
                {
                    currentValence = sentimentLadenIdioms.get(rightTriGramFromCurrent);
                }
            }

            if (boosterDictionary.containsKey(leftBiGramFromTwoPrevious) || boosterDictionary.containsKey(leftBiGramFromOnePrevious))
            {
                currentValence += -0.293f; // TODO review Language and English.DAMPENER_WORD_DECREMENT;
            }


            return currentValence;
        }
        private Dictionary<string, float> getSentiment()
        {
            ArrayList sentiments = new ArrayList();
            ArrayList wordsAndEmoticons = inputStringProperties.getWordsAndEmoticons();

            foreach (String item in wordsAndEmoticons)
            {
                float currentValence = 0.0f;
                int currentItemIndexOf = wordsAndEmoticons.IndexOf(item);
                String currentItemLower = item.ToLower();

                if (currentItemIndexOf < wordsAndEmoticons.Count - 1 &&
                        item.ToLower().Equals("kind") &&
                        wordsAndEmoticons[currentItemIndexOf + 1].ToString().ToLower().Equals("of") ||
                        Utils.BOOSTER_DICTIONARY.ContainsKey(item.ToLower()))
                {
                    sentiments.Add(currentValence);
                    continue;
                }
                
                if (Utils.WORD_VALENCE_DICTIONARY.ContainsKey(currentItemLower))
                {
                    currentValence = Utils.WORD_VALENCE_DICTIONARY[currentItemLower];

                    if (Utils.isUpper(item) && inputStringProperties.getCapDiff())
                    {
                        currentValence = (currentValence > 0.0) ? currentValence + Utils.ALL_CAPS_BOOSTER_SCORE : currentValence - Utils.ALL_CAPS_BOOSTER_SCORE;
                    }


                    int distance = 0;
                    float gramBasedValence = 0.0f;
                    while (distance < 3)
                    {

                        int closeTokenIndex = currentItemIndexOf - (distance + 1);
                        if (closeTokenIndex < 0)
                        {
                            closeTokenIndex = inputStringProperties.getWordsAndEmoticons().Count;
                        }

                        if ((currentItemIndexOf > distance) && !Utils.WORD_VALENCE_DICTIONARY.ContainsKey(wordsAndEmoticons[closeTokenIndex].ToString().ToLower()))
                        {
                            gramBasedValence = valenceModifier(wordsAndEmoticons[closeTokenIndex].ToString(), currentValence);
                            if (distance == 1 && gramBasedValence != 0.0f)
                                gramBasedValence *= 0.95f;
                            if (distance == 2 && gramBasedValence != 0.0f)
                                gramBasedValence *= 0.9f;

                            currentValence += gramBasedValence;

                            currentValence = checkForNever(currentValence, distance, currentItemIndexOf, closeTokenIndex);

                            if (distance == 2)
                                currentValence = checkForIdioms(currentValence, currentItemIndexOf);

                        }
                        distance++;
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
