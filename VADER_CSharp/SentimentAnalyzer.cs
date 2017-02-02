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
                scalar = Utils.BOOSTER_DICTIONARY.get(precedingWordLower);
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

                        if ((i > startI) && !Utils.WORD_VALENCE_DICTIONARY.ContainsKey(wordsAndEmoticons[closeTokenIndex.].ToString .toLowerCase()))
                        {
                            gramBasedValence = valenceModifier(wordsAndEmoticons.get(closeTokenIndex), currentValence);

                        }
                    }
                }
            }

            return null;

        }
    }
}
