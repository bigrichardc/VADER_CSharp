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
        private string _inputString;
        private TextProperties inputStringProperties;
        private Dictionary<string, float> polarity;

        public SentimentAnalyzer(string inputString)
        {
            this._inputString = inputString;
            setupInputStringProperties();
        }

        public Dictionary<string, float> getSentimentAnalysis()
        {
            return getPolarity();
        }

        public Dictionary<string, float> getSentimentAnalysis(String inputString)
        {
            this._inputString = inputString;
            return getPolarity();
        }

        private void setupInputStringProperties()
        {
            inputStringProperties = new TextProperties(this._inputString);
        }

        public Dictionary<string, float> getPolarity()
        {
            List<float> sentiments = new List<float>();
            ArrayList wordsAndEmoticons = inputStringProperties.getWordsAndEmoticons();
            Dictionary< string, float > boosterDictionary = Utils.GetBoosterDictionary();
            Dictionary<string, float> valenceDictionary = Utils.GetWordValenceDictionary();

            foreach (string item in wordsAndEmoticons)
            {
                float currentValence = 0.0f;
                int i = wordsAndEmoticons.IndexOf(item);
                
                if (i < wordsAndEmoticons.Count - 1
                        && item.ToLower().Equals("kind")
                        && wordsAndEmoticons[i + 1].ToString().ToLower().Equals("of")
                        || boosterDictionary.ContainsKey(item.ToLower()))
                {
                    sentiments.Add(currentValence);
                    continue;
                }

                string currentItemLower = item.ToLower();
                if (valenceDictionary.ContainsKey(currentItemLower))
                {
                    currentValence = valenceDictionary[currentItemLower];
                    if (Utils.IsUpper(item) && inputStringProperties.isCapDifferential())
                    {
                        currentValence = (currentValence > 0.0) ? currentValence + Utils.ALL_CAPS_BOOSTER_SCORE : currentValence - Utils.ALL_CAPS_BOOSTER_SCORE;
                    }
                    int startI = 0;
                    float gramBasedValence;
                    while (startI < 3)
                    {
                        int closeTokenIndex = i - (startI + 1);
                        if (closeTokenIndex < 0)
                        {
                            //not sure if this is needed
                            //closeTokenIndex = pythonIndexToJavaIndex(closeTokenIndex);
                        }
                        if ((i > startI) && !valenceDictionary.ContainsKey(wordsAndEmoticons[closeTokenIndex].ToString().ToLower()))
                        {
                            gramBasedValence = valenceModifier(wordsAndEmoticons[closeTokenIndex].ToString(), currentValence);
                            if (startI == 1 && gramBasedValence != 0.0f)
                            {
                                gramBasedValence *= 0.95f;
                            }
                            if (startI == 2 && gramBasedValence != 0.0f)
                            {
                                gramBasedValence *= 0.9f;
                            }
                            currentValence += gramBasedValence;
                            currentValence = checkForNever(currentValence, startI, i, closeTokenIndex);
                            if (startI == 2)
                            {
                                currentValence = checkForIdioms(currentValence, i);
                            }
                        }
                        startI++;
                    }
                    if (i > 1 && !valenceDictionary.ContainsKey(wordsAndEmoticons[i - 1].ToString().ToLower()) && wordsAndEmoticons[i - 1].ToString().ToLower().Equals("least"))
                    {
                        if (!(wordsAndEmoticons[i - 2].ToString().ToLower().Equals("at") || wordsAndEmoticons[i - 2].ToString().ToLower().Equals("very")))
                        {
                            currentValence *= Utils.N_SCALAR;
                        }
                    }
                    else if (i > 0 && !valenceDictionary.ContainsKey(wordsAndEmoticons[i - 1].ToString().ToLower()) && wordsAndEmoticons[i - 1].Equals("least"))
                    {
                        currentValence *= Utils.N_SCALAR;
                    }
                }
                sentiments.Add(currentValence);
            }
            sentiments = checkConjunctionBut(wordsAndEmoticons, sentiments);

            return polarityScores(sentiments);
        }

        public void analyse()
        {
            polarity = getSentiment();
        }

        private float valenceModifier(string precedingWord, float currentValence)
        {
            float scalar = 0.0f;
            string precedingWordLower = precedingWord.ToLower();
            if (Utils.BOOSTER_DICTIONARY.ContainsKey(precedingWordLower))
            {
                scalar = Utils.BOOSTER_DICTIONARY[precedingWordLower];
                if (currentValence < 0.0)
                    scalar *= -1;
                if (Utils.IsUpper(precedingWord) && inputStringProperties.getCapDiff())
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
                string wordAtDistanceTwoLeft = wordsAndEmoticons[i - 2].ToString();
                string wordAtDistanceOneLeft = wordsAndEmoticons[i - 1].ToString();

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
                string wordAtDistanceThreeLeft = wordsAndEmoticons[(i - 3)].ToString();
                string wordAtDistanceTwoLeft = wordsAndEmoticons[(i - 2)].ToString();
                string wordAtDistanceOneLeft = wordsAndEmoticons[(i - 1)].ToString();
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
            string leftBiGramFromCurrent = String.Format("%s %s", wordsAndEmoticons[i - 1], wordsAndEmoticons[i]);
            string leftTriGramFromCurrent = String.Format("%s %s %s", wordsAndEmoticons[i - 2], wordsAndEmoticons[i - 1], wordsAndEmoticons[i]);
            string leftBiGramFromOnePrevious = String.Format("%s %s", wordsAndEmoticons[i - 2], wordsAndEmoticons[i - 1]);
            string leftTriGramFromOnePrevious = String.Format("%s %s %s", wordsAndEmoticons[i - 3], wordsAndEmoticons[i - 2], wordsAndEmoticons[i - 1]);
            string leftBiGramFromTwoPrevious = String.Format("%s %s", wordsAndEmoticons[i - 3], wordsAndEmoticons[i - 2]);

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
                string rightBiGramFromCurrent = String.Format("%s %s", wordsAndEmoticons[i], wordsAndEmoticons[i + 1]);
                if (sentimentLadenIdioms.ContainsKey(rightBiGramFromCurrent))
                {
                    currentValence = sentimentLadenIdioms[rightBiGramFromCurrent];
                }
            }
            if (wordsAndEmoticons.Count - 1 > i + 1)
            {
                string rightTriGramFromCurrent = String.Format("%s %s %s", wordsAndEmoticons[i], wordsAndEmoticons[i + 1], wordsAndEmoticons[i + 2]);
                if (sentimentLadenIdioms.ContainsKey(rightTriGramFromCurrent))
                {
                    currentValence = sentimentLadenIdioms[rightTriGramFromCurrent];
                }
            }

            if (boosterDictionary.ContainsKey(leftBiGramFromTwoPrevious) || boosterDictionary.ContainsKey(leftBiGramFromOnePrevious))
            {
                currentValence += -0.293f; // TODO review Language and English.DAMPENER_WORD_DECREMENT;
            }


            return currentValence;
        }

        private List<float> siftSentimentScores(List<float> currentSentimentState)
        {
            float positiveSentimentScore = 0.0f;
            float negativeSentimentScore = 0.0f;
            int neutralSentimentCount = 0;
            foreach (float valence in currentSentimentState)
            {
                if (valence > 0.0f)
                {
                    positiveSentimentScore = positiveSentimentScore + valence + 1.0f;
                }
                else if (valence < 0.0f)
                {
                    negativeSentimentScore = negativeSentimentScore + valence - 1.0f;
                }
                else
                {
                    neutralSentimentCount += 1;
                }
            }
            return new List<float> { 
                positiveSentimentScore,
                negativeSentimentScore,
                (float)neutralSentimentCount
            };
        }

        private Dictionary<string, float> polarityScores(List<float> currentSentimentState)
        {
            if (currentSentimentState.Count != 0)
            {
                float totalValence = 0.0f;
                foreach (float valence in currentSentimentState)
                {
                    totalValence += valence;
                }
                float punctuationAmplifier = boostByPunctuation();
                if (totalValence > 0.0f)
                {
                    totalValence += boostByPunctuation();
                }
                else if (totalValence < 0.0f)
                {
                    totalValence -= boostByPunctuation();
                }
                float compoundPolarity = normalizeScore(totalValence, Utils.NORMALIZE_SCORE_ALPHA_DEFAULT);
                List<float> siftedScores = siftSentimentScores(currentSentimentState);
                float positiveSentimentScore = siftedScores[0];
                float negativeSentimentScore = siftedScores[1];
                int neutralSentimentCount = (int) Math.Round(siftedScores[2]);
                if (positiveSentimentScore > Math.Abs(negativeSentimentScore))
                {
                    positiveSentimentScore += punctuationAmplifier;
                }
                else if (positiveSentimentScore < Math.Abs(negativeSentimentScore))
                {
                    negativeSentimentScore -= punctuationAmplifier;
                }
                float normalizationFactor = positiveSentimentScore
                                            + Math.Abs(negativeSentimentScore)
                                            + neutralSentimentCount;
                float normalizedPositivePolarity =
                    roundDecimal(Math.Abs(positiveSentimentScore / normalizationFactor), 3);
                float normalizedNegativePolarity =
                    roundDecimal(Math.Abs(negativeSentimentScore / normalizationFactor), 3);
                float normalizedNeutralPolarity =
                    roundDecimal(Math.Abs(neutralSentimentCount / normalizationFactor), 3);
                float normalizedCompoundPolarity = roundDecimal(compoundPolarity, 4);
                return new Dictionary<string, float>()
                {
                    {"compound", normalizedCompoundPolarity},
                    {"positive", normalizedPositivePolarity},
                    {"negative", normalizedNegativePolarity},
                    {"neutral", normalizedNeutralPolarity}
                };
            }
            else
            {
                return new Dictionary<string, float>()
                {

                    {"compound", 0.0f},
                    {"positive", 0.0f},
                    {"negative", 0.0f},
                    {"neutral", 0.0f}
                };
            }
        }

        private float boostByPunctuation()
        {
            return boostByExclamation() + boostByQuestionMark();
        }

        private float boostByExclamation()
        {
            int exclamationCount = TextProperties.countLetter(this._inputString, "!");
            return Math.Min(exclamationCount, 4) * Utils.EXCLAMATION_BOOST;
        }

        private float boostByQuestionMark()
        {
            float questionMarkAmplifier = 0.0f;
            int questionMarkCount = TextProperties.countLetter(this._inputString, "?");
            if (questionMarkCount > 1)
            {
                questionMarkAmplifier =
                    (questionMarkCount <= 3)
                        ? questionMarkCount * Utils.QUESTION_BOOST_COUNT_3
                        : Utils.QUESTION_BOOST;
            }
            return questionMarkAmplifier;
        }

        private List<float> checkConjunctionBut(ArrayList inputTokens, List<float> currentSentimentState)
        {
            if (inputTokens.Contains("but") || inputTokens.Contains("BUT"))
            {
                int index = inputTokens.IndexOf("but");
                if (index == -1)
                {
                    index = inputTokens.IndexOf("BUT");
                }

                foreach (float valence in currentSentimentState)
                {
                    int currentValenceIndex = currentSentimentState.IndexOf(valence);
                    if (currentValenceIndex < index)
                    {
                        currentSentimentState[currentValenceIndex] = valence * 0.5f;
                    }
                    else if (currentValenceIndex > index)
                    {
                        currentSentimentState[currentValenceIndex] = valence * 1.5f;
                    }
                }
            }
            return currentSentimentState;
        }

        private static float roundDecimal(float currentValue, int roundTo)
        {
            float n = (float)Math.Pow(10.0, (double)roundTo);
            float number = (float)Math.Round(currentValue * n);
            return number / n;
        }

        private float normalizeScore(float score, float alpha)
        {
            return (float)(score / Math.Sqrt((score * score) + alpha));
        }


        private Dictionary<string, float> getSentiment()
        {
            ArrayList sentiments = new ArrayList();
            ArrayList wordsAndEmoticons = inputStringProperties.getWordsAndEmoticons();

            foreach (string item in wordsAndEmoticons)
            {
                float currentValence = 0.0f;
                int currentItemIndexOf = wordsAndEmoticons.IndexOf(item);
                string currentItemLower = item.ToLower();

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

                    if (Utils.IsUpper(item) && inputStringProperties.getCapDiff())
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
            foreach (string s in tokenList)
            {
                if (s.EndsWith("n't"))
                    return true;
            }
            return false;
        }

        private Boolean hasNegativeWord(ArrayList tokenList, ArrayList newNegWords)
        {
            foreach (string newNegWord in newNegWords)
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
