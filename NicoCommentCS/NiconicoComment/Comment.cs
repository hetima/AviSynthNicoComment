using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using NCEmoji.Wpf;
using NCNicoNico.Comment.Primitive;


namespace NCNicoNico.Comment
{

    
    public struct EmojiLoc
    {
        public int Index;
        public int Length;
    }

    public class Vote
    {
        public Comment VoteComment = null;
        public Comment ResultComment = null;
        public double StartTime;
        public double ResultTime;
        public double EndTime;
    }
    public class Perm
    {
        public Comment Comment = null;
        public double StartTime;
        public double EndTime=0;
    }
    public class Comment
    {
        public static NCColor DefaultColor = new NCColor(255, 255, 255);

        public int Count;
        //public int Number;
        public string Text;
        public int Vpos;
        //public string Mail;
        public string[] Mails; //lower, split

        private List<EmojiLoc> emojiLocs = null;
        public bool IsAA = false;
        public bool IsPerm = false;
        public bool IsVote = false;

        public NCSize Size = new NCSize(0, 0);
        public double LastY = 0.0;
        public double FontSize = 0.0;
        private NCColor? _color = null;

        public static string StripHtmlTags(string str)
        {
            return Regex.Replace(str.Replace("<br>", Environment.NewLine), "<[a-zA-Z/].*?>", String.Empty);
        }

        public static Dictionary<string, NCColor> CommandColorDictionary = new Dictionary<string, NCColor>()
        {
            { "black", new NCColor("#000000")},
            { "red", new NCColor("#ff0000") },
            { "pink", new NCColor("#FF8080")},
            { "orange", new NCColor("#FFC000")},
            { "yellow", new NCColor("#FFFF00")},
            { "green", new NCColor("#00FF00")},
            { "cyan", new NCColor("#00FFFF")},
            { "blue", new NCColor("#0000ff")},
            { "purple", new NCColor("#c000ff")},
            { "white", new NCColor("#ffffff")},
            { "white2", new NCColor("#cccc99")},
            { "niconicowhite", new NCColor("#cccc99")},
            { "red2", new NCColor("#cc0033")},
            { "truered", new NCColor("#cc0033")},
            { "pink2", new NCColor("#ff33cc")},
            { "orange2", new NCColor("#ff6600")},
            { "passionorange", new NCColor("#ff6600")},
            { "yellow2", new NCColor("#999900")},
            { "madyellow", new NCColor("#999900")},
            { "green2", new NCColor("#00cc66")},
            { "elementalgreen", new NCColor("#00cc66")},
            { "cyan2", new NCColor("#00cccc")},
            { "blue2", new NCColor("#3399ff")},
            { "marineblue", new NCColor("#3399ff")},
            { "purple2", new NCColor("#6633cc")},
            { "nobleviolet", new NCColor("#6633cc")},
            { "black2", new NCColor("#666666")},
        };

        public List<EmojiLoc> GetEmojiLocs()
        {
            if (emojiLocs != null) return emojiLocs;
            emojiLocs = new List<EmojiLoc>();
            foreach (Match m in EmojiData.MatchMultiple.Matches(Text))
            {
                EmojiLoc loc = new EmojiLoc()
                {
                    Length = m.Length,
                    Index = m.Index
                };
                emojiLocs.Add(loc);
            }
            return emojiLocs;
        }

        public NCColor Color { get
            {
                if (_color.HasValue)
                {
                    return _color.Value;
                }

                //var mails = Mail.Split(' ');
                var commandDic = CommandColorDictionary;
                foreach (var mail in Mails)
                {
                    if (commandDic.ContainsKey(mail))
                    {
                        _color = commandDic[mail];
                        return (NCColor)_color;
                    }
                    var col = NCColor.Parse(mail);
                    if (col.HasValue)
                    {
                        _color = col;
                        return _color.Value;
                    }
                }
                _color = DefaultColor;
                return _color.Value;
            }
        }
        public bool ContainsCommand(string key)
        {
            var keyLow = key.ToLower();
            foreach (var mail in Mails)
            {
                if (mail == keyLow) return true;
            }
            return false;
        }


    }
}
