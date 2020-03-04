using Discord;
using System.Collections.Generic;

namespace DiscordNetBot
{
    /// <summary>
    /// Emotes
    /// </summary>
    public static class Emojis
    {
        #region Public Properties

        public static List<KeyValuePair<string, Emoji>> EmojisList { get; private set; }

        #endregion

        #region Public Methods

        public static void Initialize()
        {
            //EmojisList.AddRange(new KeyValuePair<string, Emoji>[] {
                // Digits 0 - 9
                //new KeyValuePair<string, Emoji>("DigitZero", new Emoji("\U00000030")),
                //new KeyValuePair<string, Emoji>("DigitOne", new Emoji("\U00000031")),
                //new KeyValuePair<string, Emoji>("DigitTwo", new Emoji("\U00000032")),
                //new KeyValuePair<string, Emoji>("DigitThree", new Emoji("\U00000033")),
                //new KeyValuePair<string, Emoji>("DigitFour", new Emoji("\U00000034")),
                //new KeyValuePair<string, Emoji>("DigitFive", new Emoji("\U00000035")),
                //new KeyValuePair<string, Emoji>("DigitSix", new Emoji("\U00000036")),
                //new KeyValuePair<string, Emoji>("DigitSeven", new Emoji("\U00000037")),
                //new KeyValuePair<string, Emoji>("DigitEight", new Emoji("\U00000038")),
                //new KeyValuePair<string, Emoji>("DigitNine", new Emoji("\U00000039")),
                //Thumbs
                //new KeyValuePair<string, Emoji>("ThumbsUp", new Emoji("\U0001F44D")),
                //new KeyValuePair<string, Emoji>("ThumbsDown", new Emoji("\U0001F44E")),
                //new KeyValuePair<string, Emoji>("Rofl", new Emoji("\U0001F923")),
                //new KeyValuePair<string, Emoji>("OkHand", new Emoji("\U0001F44C")),
                //new KeyValuePair<string, Emoji>("Smile", new Emoji("\U0001F642")),
                //new KeyValuePair<string, Emoji>("Money", new Emoji("\U0001F911")),
                //new KeyValuePair<string, Emoji>("HotFace", new Emoji("\U0001F975")),
            //});

        }

        #endregion
    }
}
