using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;

namespace VADER_CSharp
{
    public class Utils
    {
        private static String[] PUNCTUATION_LIST_array = {
            ".", "!", "?", ",", ";", ":", "-", "'", "\"", "!!", "!!!", "??", "???", "?!?", "!?!", "?!?!", "!?!?"
        };

        private static String[] NEGATIVE_WORDS_array = {"aint", "arent", "cannot", "cant", "couldnt", "darent",
            "didnt", "doesnt", "ain't", "aren't", "can't", "couldn't", "daren't", "didn't", "doesn't",
            "dont", "hadnt", "hasnt", "havent", "isnt", "mightnt", "mustnt", "neither",
            "don't", "hadn't", "hasn't", "haven't", "isn't", "mightn't", "mustn't",
            "neednt", "needn't", "never", "none", "nope", "nor", "not", "nothing", "nowhere",
            "oughtnt", "shant", "shouldnt", "uhuh", "wasnt", "werent",
            "oughtn't", "shan't", "shouldn't", "uh-uh", "wasn't", "weren't",
            "without", "wont", "wouldnt", "won't", "wouldn't", "rarely", "seldom", "despite"
    };
        

        public static ArrayList PUNCTUATION_LIST = new ArrayList(PUNCTUATION_LIST_array);
        public static ArrayList NEGATIVE_WORDS = new ArrayList(NEGATIVE_WORDS_array);

        public static Dictionary<string, float> BOOSTER_DICTIONARY = new Dictionary<string, float>();
        public static Dictionary<string, float> SENTIMENTLADENIDIOMS_DICTIONARY = new Dictionary<string, float>();
        public static Dictionary<String, float> WORD_VALENCE_DICTIONARY = getWordValenceDictionary("vader_sentiment_lexicon.txt");
        public static float BOOSTER_WORD_INCREMENT = 0.293f;
        public static float DAMPENER_WORD_DECREMENT = -0.293f;

        public static float ALL_CAPS_BOOSTER_SCORE = 0.733f;
        public static float N_SCALAR = -0.74f;

        void loadBOOSTER_DICTIONARY()
        {
            BOOSTER_DICTIONARY.Add("decidedly", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("uber", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("barely", DAMPENER_WORD_DECREMENT);
            BOOSTER_DICTIONARY.Add("particularly", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("enormously", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("less", DAMPENER_WORD_DECREMENT);
            BOOSTER_DICTIONARY.Add("absolutely", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("kinda", DAMPENER_WORD_DECREMENT);
            BOOSTER_DICTIONARY.Add("flipping", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("awfully", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("purely", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("majorly", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("substantially", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("partly", DAMPENER_WORD_DECREMENT);
            BOOSTER_DICTIONARY.Add("remarkably", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("really", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("sort of", DAMPENER_WORD_DECREMENT);
            BOOSTER_DICTIONARY.Add("little", DAMPENER_WORD_DECREMENT);
            BOOSTER_DICTIONARY.Add("fricking", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("sorta", DAMPENER_WORD_DECREMENT);
            BOOSTER_DICTIONARY.Add("amazingly", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("kind of", DAMPENER_WORD_DECREMENT);
            BOOSTER_DICTIONARY.Add("just enough", DAMPENER_WORD_DECREMENT);
            BOOSTER_DICTIONARY.Add("fucking", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("occasionally", DAMPENER_WORD_DECREMENT);
            BOOSTER_DICTIONARY.Add("somewhat", DAMPENER_WORD_DECREMENT);
            BOOSTER_DICTIONARY.Add("kindof", DAMPENER_WORD_DECREMENT);
            BOOSTER_DICTIONARY.Add("friggin", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("incredibly", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("totally", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("marginally", DAMPENER_WORD_DECREMENT);
            BOOSTER_DICTIONARY.Add("more", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("considerably", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("fabulously", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("sort of", DAMPENER_WORD_DECREMENT);
            BOOSTER_DICTIONARY.Add("hardly", DAMPENER_WORD_DECREMENT);
            BOOSTER_DICTIONARY.Add("very", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("sortof", DAMPENER_WORD_DECREMENT);
            BOOSTER_DICTIONARY.Add("kind-of", DAMPENER_WORD_DECREMENT);
            BOOSTER_DICTIONARY.Add("scarcely", DAMPENER_WORD_DECREMENT);
            BOOSTER_DICTIONARY.Add("thoroughly", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("quite", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("most", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("completely", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("frigging", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("intensely", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("utterly", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("highly", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("extremely", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("unbelievably", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("almost", DAMPENER_WORD_DECREMENT);
            BOOSTER_DICTIONARY.Add("especially", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("fully", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("frickin", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("tremendously", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("exceptionally", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("flippin", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("hella", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("so", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("greatly", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("hugely", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("deeply", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("unusually", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("entirely", BOOSTER_WORD_INCREMENT);
            BOOSTER_DICTIONARY.Add("slightly", DAMPENER_WORD_DECREMENT);
            BOOSTER_DICTIONARY.Add("effing", BOOSTER_WORD_INCREMENT);
        }

        void loadSENTIMENTLADENIDIOMS_DICTIONARY()
        {
            
            SENTIMENTLADENIDIOMS_DICTIONARY.Add("cut the mustard", 2f);
            SENTIMENTLADENIDIOMS_DICTIONARY.Add("bad ass", 1.5f);
            SENTIMENTLADENIDIOMS_DICTIONARY.Add("kiss of death", -1.5f);
            SENTIMENTLADENIDIOMS_DICTIONARY.Add("yeah right", -2f);
            SENTIMENTLADENIDIOMS_DICTIONARY.Add("the bomb", 3f);
            SENTIMENTLADENIDIOMS_DICTIONARY.Add("hand to mouth", -2f);
            SENTIMENTLADENIDIOMS_DICTIONARY.Add("the shit", 3f);
        }

        public static Dictionary<string, float> getWordValenceDictionary(string filename)
        {
            Dictionary<string, float> lexDictionary = new Dictionary<string, float>();
            StreamReader file = new StreamReader(filename);
            string line;
            String currentText;
            float currentTextValence;
            try
            {
                while ((line = file.ReadLine()) != null)
                {   
                    String[] lexFileData = line.Split('\t');
                    currentText = lexFileData[0];
                    currentTextValence = float.Parse(lexFileData[1]);
                    lexDictionary.Add(currentText, currentTextValence);

                }

                file.Close();
                return lexDictionary;
            }
            catch { return null; }
        }

        public static Dictionary<string, float> GetBoosterDictionary()
        {
            return BOOSTER_DICTIONARY;
        }

        public static Dictionary<string, float> GetSentimentLadenIdiomsDictionary()
        {
            return SENTIMENTLADENIDIOMS_DICTIONARY;
        }

        public static Boolean isUpper (String tokenString)
        {
            if (tokenString.StartsWith("http://"))
            {
                return false;
            }

            Boolean result = !tokenString.Any(c => char.IsLower(c));
            return result;

        }


    }
}
