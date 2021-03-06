﻿//
//  Emoji.Wpf — Emoji support for WPF
//
//  Copyright © 2017—2019 Sam Hocevar <sam@hocevar.net>
//
//  This library is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace NCEmoji.Wpf
{
    public static class EmojiData
    {
        public static EmojiTypeface Typeface { get; private set; }

        public static IEnumerable<Emoji> AllEmoji
        {
            get
            {
                foreach (var group in AllGroups)
                    foreach (var emoji in group.EmojiList)
                        yield return emoji;
            }
        }

        public static IEnumerable<Group> AllGroups { get; private set; }

        public static IDictionary<string, Emoji> Lookup { get; private set; }

        public static Regex MatchOne { get; private set; }
        public static Regex MatchMultiple { get; private set; }

        static EmojiData()
        {
            Typeface = new EmojiTypeface();
            ParseEmojiList();
        }

        public class Emoji
        {
            public string Name { get; set; }
            public string Text { get; set; }

            public Group Group => SubGroup.Group;
            public SubGroup SubGroup;

            public IList<Emoji> VariationList { get; } = new List<Emoji>();
        }

        public class SubGroup
        {
            public string Name { get; set; }
            public Group Group;

            public IList<Emoji> EmojiList { get; } = new List<Emoji>();
        }

        public class Group
        {
            public string Name { get; set; }
            public string Icon => SubGroups[0].EmojiList[0].Text;

            public IList<SubGroup> SubGroups { get; } = new List<SubGroup>();

            public int EmojiCount
            {
                get
                {
                    int i = 0;
                    foreach (var subgroup in SubGroups)
                        i += subgroup.EmojiList.Count;
                    return i;
                }
            }

            public IEnumerable<Emoji> EmojiList
            {
                get
                {
                    foreach (var subgroup in SubGroups)
                        foreach (var emoji in subgroup.EmojiList)
                            yield return emoji;
                }
            }
        }

        private static void ParseEmojiList()
        {
            //Emoji.Wpfがemoji-test.txtを読み込んで生成する正規表現
            string regextext = "(🧑(🏻|🏼|🏽|🏾|🏿)‍🤝‍🧑(🏻|🏼|🏽|🏾|🏿)|👩(🏻|🏼|🏽|🏾|🏿)‍🏭|👩(🏻|🏼|🏽|🏾|🏿)‍🚒|💁(🏻|🏼|🏽|🏾|🏿)‍♀️|🧎(🏻|🏼|🏽|🏾|🏿)‍♀️|💁(🏻|🏼|🏽|🏾|🏿)‍♂️|🏊(🏻|🏼|🏽|🏾|🏿)‍♀️|🏊(🏻|🏼|🏽|🏾|🏿)‍♂️|🙆(🏻|🏼|🏽|🏾|🏿)‍♀️|👨(🏻|🏼|🏽|🏾|🏿)‍🦯|💆(🏻|🏼|🏽|🏾|🏿)‍♂️|👮(🏻|🏼|🏽|🏾|🏿)‍♂️|🚣(🏻|🏼|🏽|🏾|🏿)‍♀️|🙆(🏻|🏼|🏽|🏾|🏿)‍♂️|👩(🏻|🏼|🏽|🏾|🏿)‍🦯|🚣(🏻|🏼|🏽|🏾|🏿)‍♂️|🙅(🏻|🏼|🏽|🏾|🏿)‍♀️|🙋(🏻|🏼|🏽|🏾|🏿)‍♂️|🦸(🏻|🏼|🏽|🏾|🏿)‍♂️|👨(🏻|🏼|🏽|🏾|🏿)‍🚒|🦸(🏻|🏼|🏽|🏾|🏿)‍♀️|🧙(🏻|🏼|🏽|🏾|🏿)‍♀️|🧙(🏻|🏼|🏽|🏾|🏿)‍♂️|🤸(🏻|🏼|🏽|🏾|🏿)‍♀️|🧏(🏻|🏼|🏽|🏾|🏿)‍♀️|🤸(🏻|🏼|🏽|🏾|🏿)‍♂️|👨(🏻|🏼|🏽|🏾|🏿)‍🚀|🚵(🏻|🏼|🏽|🏾|🏿)‍♀️|🦹(🏻|🏼|🏽|🏾|🏿)‍♀️|🧏(🏻|🏼|🏽|🏾|🏿)‍♂️|🚵(🏻|🏼|🏽|🏾|🏿)‍♂️|🦹(🏻|🏼|🏽|🏾|🏿)‍♂️|👩(🏻|🏼|🏽|🏾|🏿)‍🚀|🚴(🏻|🏼|🏽|🏾|🏿)‍♀️|🧎(🏻|🏼|🏽|🏾|🏿)‍♂️|🚴(🏻|🏼|🏽|🏾|🏿)‍♂️|🙋(🏻|🏼|🏽|🏾|🏿)‍♀️|🏋(🏻|🏼|🏽|🏾|🏿)‍♀️|🚶(🏻|🏼|🏽|🏾|🏿)‍♂️|👮(🏻|🏼|🏽|🏾|🏿)‍♀️|🙅(🏻|🏼|🏽|🏾|🏿)‍♂️|👨(🏻|🏼|🏽|🏾|🏿)‍🦼|👱(🏻|🏼|🏽|🏾|🏿)‍♀️|🏃(🏻|🏼|🏽|🏾|🏿)‍♂️|🧗(🏻|🏼|🏽|🏾|🏿)‍♂️|👨(🏻|🏼|🏽|🏾|🏿)‍🦲|👨(🏻|🏼|🏽|🏾|🏿)‍🦳|👨(🏻|🏼|🏽|🏾|🏿)‍🦱|💇(🏻|🏼|🏽|🏾|🏿)‍♀️|🧖(🏻|🏼|🏽|🏾|🏿)‍♀️|👨(🏻|🏼|🏽|🏾|🏿)‍🦰|👷(🏻|🏼|🏽|🏾|🏿)‍♀️|💂(🏻|🏼|🏽|🏾|🏿)‍♂️|🧖(🏻|🏼|🏽|🏾|🏿)‍♂️|👱(🏻|🏼|🏽|🏾|🏿)‍♂️|💇(🏻|🏼|🏽|🏾|🏿)‍♂️|💂(🏻|🏼|🏽|🏾|🏿)‍♀️|👷(🏻|🏼|🏽|🏾|🏿)‍♂️|🏃(🏻|🏼|🏽|🏾|🏿)‍♀️|🧗(🏻|🏼|🏽|🏾|🏿)‍♀️|🤽(🏻|🏼|🏽|🏾|🏿)‍♂️|👩(🏻|🏼|🏽|🏾|🏿)‍🦰|👩(🏻|🏼|🏽|🏾|🏿)‍🦱|🏄(🏻|🏼|🏽|🏾|🏿)‍♀️|💆(🏻|🏼|🏽|🏾|🏿)‍♀️|🏄(🏻|🏼|🏽|🏾|🏿)‍♂️|🙎(🏻|🏼|🏽|🏾|🏿)‍♀️|👳(🏻|🏼|🏽|🏾|🏿)‍♀️|👩(🏻|🏼|🏽|🏾|🏿)‍🦼|🙎(🏻|🏼|🏽|🏾|🏿)‍♂️|🏌(🏻|🏼|🏽|🏾|🏿)‍♀️|👨(🏻|🏼|🏽|🏾|🏿)‍🦽|🙍(🏻|🏼|🏽|🏾|🏿)‍♀️|👳(🏻|🏼|🏽|🏾|🏿)‍♂️|🙍(🏻|🏼|🏽|🏾|🏿)‍♂️|👩(🏻|🏼|🏽|🏾|🏿)‍🦽|🏌(🏻|🏼|🏽|🏾|🏿)‍♂️|🕵(🏻|🏼|🏽|🏾|🏿)‍♂️|👩(🏻|🏼|🏽|🏾|🏿)‍🦲|👩(🏻|🏼|🏽|🏾|🏿)‍🦳|🕵(🏻|🏼|🏽|🏾|🏿)‍♀️|🙇(🏻|🏼|🏽|🏾|🏿)‍♂️|🏋(🏻|🏼|🏽|🏾|🏿)‍♂️|🤽(🏻|🏼|🏽|🏾|🏿)‍♀️|👨(🏻|🏼|🏽|🏾|🏿)‍🌾|🤹(🏻|🏼|🏽|🏾|🏿)‍♀️|👩(🏻|🏼|🏽|🏾|🏿)‍🔬|🧍(🏻|🏼|🏽|🏾|🏿)‍♀️|👩(🏻|🏼|🏽|🏾|🏿)‍⚖️|🧘(🏻|🏼|🏽|🏾|🏿)‍♂️|👨(🏻|🏼|🏽|🏾|🏿)‍⚖️|👨(🏻|🏼|🏽|🏾|🏿)‍💻|👩(🏻|🏼|🏽|🏾|🏿)‍🏫|👩(🏻|🏼|🏽|🏾|🏿)‍💻|🧛(🏻|🏼|🏽|🏾|🏿)‍♂️|👨(🏻|🏼|🏽|🏾|🏿)‍🏫|👩(🏻|🏼|🏽|🏾|🏿)‍🎓|👨(🏻|🏼|🏽|🏾|🏿)‍🎤|🧘(🏻|🏼|🏽|🏾|🏿)‍♀️|👨(🏻|🏼|🏽|🏾|🏿)‍🎓|👩(🏻|🏼|🏽|🏾|🏿)‍⚕️|🤦(🏻|🏼|🏽|🏾|🏿)‍♂️|👨(🏻|🏼|🏽|🏾|🏿)‍✈️|🚶(🏻|🏼|🏽|🏾|🏿)‍♀️|👩(🏻|🏼|🏽|🏾|🏿)‍🎤|🤦(🏻|🏼|🏽|🏾|🏿)‍♀️|👨(🏻|🏼|🏽|🏾|🏿)‍⚕️|👨(🏻|🏼|🏽|🏾|🏿)‍🎨|🧍(🏻|🏼|🏽|🏾|🏿)‍♂️|🤷(🏻|🏼|🏽|🏾|🏿)‍♂️|🤷(🏻|🏼|🏽|🏾|🏿)‍♀️|👩(🏻|🏼|🏽|🏾|🏿)‍🌾|👨(🏻|🏼|🏽|🏾|🏿)‍🔬|👩(🏻|🏼|🏽|🏾|🏿)‍🎨|🤹(🏻|🏼|🏽|🏾|🏿)‍♂️|🤾(🏻|🏼|🏽|🏾|🏿)‍♂️|👩(🏻|🏼|🏽|🏾|🏿)‍🔧|🧚(🏻|🏼|🏽|🏾|🏿)‍♂️|🧛(🏻|🏼|🏽|🏾|🏿)‍♀️|👨(🏻|🏼|🏽|🏾|🏿)‍💼|👨(🏻|🏼|🏽|🏾|🏿)‍🔧|🧝(🏻|🏼|🏽|🏾|🏿)‍♂️|👩(🏻|🏼|🏽|🏾|🏿)‍💼|🤾(🏻|🏼|🏽|🏾|🏿)‍♀️|👩(🏻|🏼|🏽|🏾|🏿)‍🍳|👨(🏻|🏼|🏽|🏾|🏿)‍🏭|🧜(🏻|🏼|🏽|🏾|🏿)‍♂️|🧚(🏻|🏼|🏽|🏾|🏿)‍♀️|🙇(🏻|🏼|🏽|🏾|🏿)‍♀️|👨(🏻|🏼|🏽|🏾|🏿)‍🍳|👩(🏻|🏼|🏽|🏾|🏿)‍✈️|🧜(🏻|🏼|🏽|🏾|🏿)‍♀️|🧝(🏻|🏼|🏽|🏾|🏿)‍♀️|💂(🏻|🏼|🏽|🏾|🏿)‍♀|🏃(🏻|🏼|🏽|🏾|🏿)‍♀|🧜(🏻|🏼|🏽|🏾|🏿)‍♂|🙎(🏻|🏼|🏽|🏾|🏿)‍♂|💂(🏻|🏼|🏽|🏾|🏿)‍♂|🙎(🏻|🏼|🏽|🏾|🏿)‍♀|🏄(🏻|🏼|🏽|🏾|🏿)‍♂|👮(🏻|🏼|🏽|🏾|🏿)‍♀|💆(🏻|🏼|🏽|🏾|🏿)‍♀|👷(🏻|🏼|🏽|🏾|🏿)‍♂|👳(🏻|🏼|🏽|🏾|🏿)‍♀|🏌(🏻|🏼|🏽|🏾|🏿)‍♀|💇(🏻|🏼|🏽|🏾|🏿)‍♂|🤷(🏻|🏼|🏽|🏾|🏿)‍♀|👨(🏻|🏼|🏽|🏾|🏿)‍⚕|🧜(🏻|🏼|🏽|🏾|🏿)‍♀|🙍(🏻|🏼|🏽|🏾|🏿)‍♀|🏃(🏻|🏼|🏽|🏾|🏿)‍♂|🧗(🏻|🏼|🏽|🏾|🏿)‍♂|💇(🏻|🏼|🏽|🏾|🏿)‍♀|🧝(🏻|🏼|🏽|🏾|🏿)‍♂|👱(🏻|🏼|🏽|🏾|🏿)‍♀|🧛(🏻|🏼|🏽|🏾|🏿)‍♀|🕵(🏻|🏼|🏽|🏾|🏿)‍♀|🏄(🏻|🏼|🏽|🏾|🏿)‍♀|👩(🏻|🏼|🏽|🏾|🏿)‍⚖|🧖(🏻|🏼|🏽|🏾|🏿)‍♀|🚶(🏻|🏼|🏽|🏾|🏿)‍♀|👨(🏻|🏼|🏽|🏾|🏿)‍⚖|🕵(🏻|🏼|🏽|🏾|🏿)‍♂|👷(🏻|🏼|🏽|🏾|🏿)‍♀|🏌(🏻|🏼|🏽|🏾|🏿)‍♂|🧖(🏻|🏼|🏽|🏾|🏿)‍♂|🙍(🏻|🏼|🏽|🏾|🏿)‍♂|👱(🏻|🏼|🏽|🏾|🏿)‍♂|👳(🏻|🏼|🏽|🏾|🏿)‍♂|👩(🏻|🏼|🏽|🏾|🏿)‍⚕|🧗(🏻|🏼|🏽|🏾|🏿)‍♀|🙅(🏻|🏼|🏽|🏾|🏿)‍♂|🤷(🏻|🏼|🏽|🏾|🏿)‍♂|🧝(🏻|🏼|🏽|🏾|🏿)‍♀|🤸(🏻|🏼|🏽|🏾|🏿)‍♀|🧏(🏻|🏼|🏽|🏾|🏿)‍♀|🤸(🏻|🏼|🏽|🏾|🏿)‍♂|👨(🏻|🏼|🏽|🏾|🏿)‍✈|🙋(🏻|🏼|🏽|🏾|🏿)‍♂|🙇(🏻|🏼|🏽|🏾|🏿)‍♂|🤾(🏻|🏼|🏽|🏾|🏿)‍♂|🏋(🏻|🏼|🏽|🏾|🏿)‍♂|🦸(🏻|🏼|🏽|🏾|🏿)‍♂|🧎(🏻|🏼|🏽|🏾|🏿)‍♂|🤹(🏻|🏼|🏽|🏾|🏿)‍♀|⛹(🏻|🏼|🏽|🏾|🏿)‍♀️|🏋(🏻|🏼|🏽|🏾|🏿)‍♀|🦸(🏻|🏼|🏽|🏾|🏿)‍♀|🙋(🏻|🏼|🏽|🏾|🏿)‍♀|🧚(🏻|🏼|🏽|🏾|🏿)‍♀|🧚(🏻|🏼|🏽|🏾|🏿)‍♂|🚴(🏻|🏼|🏽|🏾|🏿)‍♂|🤹(🏻|🏼|🏽|🏾|🏿)‍♂|🦹(🏻|🏼|🏽|🏾|🏿)‍♀|🧏(🏻|🏼|🏽|🏾|🏿)‍♂|🚴(🏻|🏼|🏽|🏾|🏿)‍♀|🦹(🏻|🏼|🏽|🏾|🏿)‍♂|🚵(🏻|🏼|🏽|🏾|🏿)‍♂|🤾(🏻|🏼|🏽|🏾|🏿)‍♀|🙇(🏻|🏼|🏽|🏾|🏿)‍♀|🧍(🏻|🏼|🏽|🏾|🏿)‍♀|🧙(🏻|🏼|🏽|🏾|🏿)‍♂|🙆(🏻|🏼|🏽|🏾|🏿)‍♀|🙅(🏻|🏼|🏽|🏾|🏿)‍♀|🤽(🏻|🏼|🏽|🏾|🏿)‍♂|🧍(🏻|🏼|🏽|🏾|🏿)‍♂|🚣(🏻|🏼|🏽|🏾|🏿)‍♂|🤽(🏻|🏼|🏽|🏾|🏿)‍♀|👮(🏻|🏼|🏽|🏾|🏿)‍♂|🤦(🏻|🏼|🏽|🏾|🏿)‍♀|🚣(🏻|🏼|🏽|🏾|🏿)‍♀|🙆(🏻|🏼|🏽|🏾|🏿)‍♂|💆(🏻|🏼|🏽|🏾|🏿)‍♂|🧙(🏻|🏼|🏽|🏾|🏿)‍♀|🤦(🏻|🏼|🏽|🏾|🏿)‍♂|💁(🏻|🏼|🏽|🏾|🏿)‍♀|🧘(🏻|🏼|🏽|🏾|🏿)‍♀|🚵(🏻|🏼|🏽|🏾|🏿)‍♀|🏊(🏻|🏼|🏽|🏾|🏿)‍♂|🚶(🏻|🏼|🏽|🏾|🏿)‍♂|👩(🏻|🏼|🏽|🏾|🏿)‍✈|🏊(🏻|🏼|🏽|🏾|🏿)‍♀|🧘(🏻|🏼|🏽|🏾|🏿)‍♂|🧎(🏻|🏼|🏽|🏾|🏿)‍♀|🧛(🏻|🏼|🏽|🏾|🏿)‍♂|💁(🏻|🏼|🏽|🏾|🏿)‍♂|⛹(🏻|🏼|🏽|🏾|🏿)‍♂️|⛹(🏻|🏼|🏽|🏾|🏿)‍♂|⛹(🏻|🏼|🏽|🏾|🏿)‍♀|🙇(🏻|🏼|🏽|🏾|🏿)|🧔(🏻|🏼|🏽|🏾|🏿)|👨(🏻|🏼|🏽|🏾|🏿)|👱(🏻|🏼|🏽|🏾|🏿)|🧑(🏻|🏼|🏽|🏾|🏿)|🧎(🏻|🏼|🏽|🏾|🏿)|🤦(🏻|🏼|🏽|🏾|🏿)|💂(🏻|🏼|🏽|🏾|🏿)|🤷(🏻|🏼|🏽|🏾|🏿)|🙅(🏻|🏼|🏽|🏾|🏿)|🙆(🏻|🏼|🏽|🏾|🏿)|🧍(🏻|🏼|🏽|🏾|🏿)|🕵(🏻|🏼|🏽|🏾|🏿)|🙎(🏻|🏼|🏽|🏾|🏿)|💁(🏻|🏼|🏽|🏾|🏿)|👮(🏻|🏼|🏽|🏾|🏿)|🚶(🏻|🏼|🏽|🏾|🏿)|🙍(🏻|🏼|🏽|🏾|🏿)|👵(🏻|🏼|🏽|🏾|🏿)|🏃(🏻|🏼|🏽|🏾|🏿)|👴(🏻|🏼|🏽|🏾|🏿)|🧓(🏻|🏼|🏽|🏾|🏿)|🙋(🏻|🏼|🏽|🏾|🏿)|👧(🏻|🏼|🏽|🏾|🏿)|👩(🏻|🏼|🏽|🏾|🏿)|🧏(🏻|🏼|🏽|🏾|🏿)|💃(🏻|🏼|🏽|🏾|🏿)|💇(🏻|🏼|🏽|🏾|🏿)|🤶(🏻|🏼|🏽|🏾|🏿)|👰(🏻|🏼|🏽|🏾|🏿)|🏊(🏻|🏼|🏽|🏾|🏿)|🤵(🏻|🏼|🏽|🏾|🏿)|👋(🏻|🏼|🏽|🏾|🏿)|🤚(🏻|🏼|🏽|🏾|🏿)|🧕(🏻|🏼|🏽|🏾|🏿)|🖐(🏻|🏼|🏽|🏾|🏿)|👲(🏻|🏼|🏽|🏾|🏿)|🚣(🏻|🏼|🏽|🏾|🏿)|🤰(🏻|🏼|🏽|🏾|🏿)|🖖(🏻|🏼|🏽|🏾|🏿)|🤏(🏻|🏼|🏽|🏾|🏿)|🤞(🏻|🏼|🏽|🏾|🏿)|🤟(🏻|🏼|🏽|🏾|🏿)|🤘(🏻|🏼|🏽|🏾|🏿)|🤙(🏻|🏼|🏽|🏾|🏿)|🏄(🏻|🏼|🏽|🏾|🏿)|👈(🏻|🏼|🏽|🏾|🏿)|👉(🏻|🏼|🏽|🏾|🏿)|👆(🏻|🏼|🏽|🏾|🏿)|👌(🏻|🏼|🏽|🏾|🏿)|🤱(🏻|🏼|🏽|🏾|🏿)|👼(🏻|🏼|🏽|🏾|🏿)|💆(🏻|🏼|🏽|🏾|🏿)|🧜(🏻|🏼|🏽|🏾|🏿)|👬(🏻|🏼|🏽|🏾|🏿)|👫(🏻|🏼|🏽|🏾|🏿)|👭(🏻|🏼|🏽|🏾|🏿)|🛌(🏻|🏼|🏽|🏾|🏿)|🛀(🏻|🏼|🏽|🏾|🏿)|🧘(🏻|🏼|🏽|🏾|🏿)|🧛(🏻|🏼|🏽|🏾|🏿)|🤹(🏻|🏼|🏽|🏾|🏿)|🤾(🏻|🏼|🏽|🏾|🏿)|🧚(🏻|🏼|🏽|🏾|🏿)|🤽(🏻|🏼|🏽|🏾|🏿)|🧙(🏻|🏼|🏽|🏾|🏿)|🤸(🏻|🏼|🏽|🏾|🏿)|🚵(🏻|🏼|🏽|🏾|🏿)|🦹(🏻|🏼|🏽|🏾|🏿)|🚴(🏻|🏼|🏽|🏾|🏿)|🏋(🏻|🏼|🏽|🏾|🏿)|🦸(🏻|🏼|🏽|🏾|🏿)|👦(🏻|🏼|🏽|🏾|🏿)|🎅(🏻|🏼|🏽|🏾|🏿)|🖕(🏻|🏼|🏽|🏾|🏿)|👇(🏻|🏼|🏽|🏾|🏿)|🧝(🏻|🏼|🏽|🏾|🏿)|🦻(🏻|🏼|🏽|🏾|🏿)|🙌(🏻|🏼|🏽|🏾|🏿)|👐(🏻|🏼|🏽|🏾|🏿)|🤲(🏻|🏼|🏽|🏾|🏿)|👸(🏻|🏼|🏽|🏾|🏿)|🙏(🏻|🏼|🏽|🏾|🏿)|💅(🏻|🏼|🏽|🏾|🏿)|🤴(🏻|🏼|🏽|🏾|🏿)|🤳(🏻|🏼|🏽|🏾|🏿)|🏇(🏻|🏼|🏽|🏾|🏿)|🧗(🏻|🏼|🏽|🏾|🏿)|🦵(🏻|🏼|🏽|🏾|🏿)|🦶(🏻|🏼|🏽|🏾|🏿)|👂(🏻|🏼|🏽|🏾|🏿)|🕺(🏻|🏼|🏽|🏾|🏿)|👶(🏻|🏼|🏽|🏾|🏿)|👃(🏻|🏼|🏽|🏾|🏿)|👷(🏻|🏼|🏽|🏾|🏿)|🕴(🏻|🏼|🏽|🏾|🏿)|💪(🏻|🏼|🏽|🏾|🏿)|👏(🏻|🏼|🏽|🏾|🏿)|🧖(🏻|🏼|🏽|🏾|🏿)|🏂(🏻|🏼|🏽|🏾|🏿)|🧒(🏻|🏼|🏽|🏾|🏿)|👍(🏻|🏼|🏽|🏾|🏿)|👎(🏻|🏼|🏽|🏾|🏿)|🤜(🏻|🏼|🏽|🏾|🏿)|👳(🏻|🏼|🏽|🏾|🏿)|👊(🏻|🏼|🏽|🏾|🏿)|🤛(🏻|🏼|🏽|🏾|🏿)|🏌(🏻|🏼|🏽|🏾|🏿)|☝(🏻|🏼|🏽|🏾|🏿)|✊(🏻|🏼|🏽|🏾|🏿)|✍(🏻|🏼|🏽|🏾|🏿)|⛹(🏻|🏼|🏽|🏾|🏿)|✋(🏻|🏼|🏽|🏾|🏿)|✌(🏻|🏼|🏽|🏾|🏿)|👨‍👨‍👧‍👦|👩‍❤️‍💋‍👩|👨‍👨‍👧‍👧|👨‍❤️‍💋‍👨|👩‍👩‍👦‍👦|👩‍❤️‍💋‍👨|👨‍👩‍👦‍👦|👨‍👩‍👧‍👦|👨‍👨‍👦‍👦|👩‍👩‍👧‍👦|👩‍👩‍👧‍👧|👨‍👩‍👧‍👧|👩‍❤‍💋‍👨|👨‍❤‍💋‍👨|👩‍❤‍💋‍👩|👩‍👦‍👦|👩‍👧‍👧|👩‍👩‍👧|👩‍👧‍👦|🧑‍🤝‍🧑|👩‍👩‍👦|👩‍❤️‍👨|👨‍❤️‍👨|👩‍❤️‍👩|👨‍👩‍👧|👨‍👧‍👧|👨‍👧‍👦|👨‍👨‍👦|👨‍👨‍👧|👨‍👦‍👦|👨‍👩‍👦|👩‍❤‍👨|👨‍❤‍👨|👩‍❤‍👩|👁️‍🗨️|🏳️‍🌈|🏌️‍♀️|🏋️‍♂️|👁‍🗨️|🏋️‍♀️|🕵️‍♂️|👁️‍🗨|🏌️‍♂️|🕵️‍♀️|🧛‍♂️|💆‍♂️|👮‍♂️|👩‍💻|👨‍💼|💆‍♀️|👨‍💻|👮‍♀️|🕵️‍♀|🧛‍♀️|👨‍🎤|👩‍🔬|🚶‍♂️|👳‍♂️|👨‍🔬|🕵‍♂️|👳‍♀️|👩‍💼|🧜‍♂️|🧜‍♀️|🧝‍♂️|🕵‍♀️|💇‍♂️|🚶‍♀️|💂‍♂️|👩‍✈️|🧝‍♀️|👷‍♀️|🧚‍♀️|💂‍♀️|🧚‍♂️|🕵️‍♂|👨‍🚀|🧞‍♂️|🧙‍♀️|👨‍✈️|🧙‍♂️|🧞‍♀️|👩‍🚀|🦹‍♀️|👩‍🎨|👨‍🚒|🦹‍♂️|🧟‍♂️|👨‍🎨|🦸‍♀️|🧟‍♀️|🦸‍♂️|👩‍🚒|👩‍🎤|👷‍♂️|💇‍♀️|👩‍🦽|👨‍🏭|⛹️‍♀️|⛹️‍♂️|🏊‍♀️|👁‍🗨|🏊‍♂️|🚣‍♀️|🚣‍♂️|🏄‍♀️|🏄‍♂️|🏌️‍♀|🏌‍♀️|👩‍🏭|🏋‍♂️|🏌️‍♂|🧗‍♂️|🧖‍♀️|🧖‍♂️|👯‍♀️|👯‍♂️|🏳‍🌈|🏴‍☠️|👱‍♂️|🏃‍♀️|👨‍🦰|👨‍🦱|👨‍🦳|🧗‍♀️|🏋️‍♂|🏋‍♀️|🏋️‍♀|🐱‍💻|🐱‍🐉|🐱‍👤|🐱‍🚀|🐱‍👓|🐱‍🏍|🐕‍🦺|👩‍👧|👩‍👦|👨‍👧|👨‍👦|🧘‍♀️|🧘‍♂️|🤹‍♀️|🤹‍♂️|🤾‍♀️|🤾‍♂️|🤽‍♀️|🤽‍♂️|🤼‍♀️|🤼‍♂️|🤸‍♀️|🤸‍♂️|🚵‍♀️|🚵‍♂️|🚴‍♀️|🚴‍♂️|👨‍🦲|👱‍♀️|🏌‍♂️|👩‍🦱|🙆‍♂️|👩‍🦯|👨‍🎓|🙆‍♀️|🤦‍♂️|👩‍⚕️|💁‍♂️|💁‍♀️|🧍‍♂️|🙋‍♂️|👨‍⚕️|🧎‍♀️|🙋‍♀️|🤷‍♀️|🤷‍♂️|🧏‍♂️|👩‍🦰|🧏‍♀️|🧎‍♂️|🧍‍♀️|🙇‍♂️|🤦‍♀️|🙇‍♀️|👨‍🦼|👩‍🎓|👨‍🦯|🙅‍♂️|👨‍🔧|🏃‍♂️|🙍‍♂️|🙍‍♀️|🙅‍♀️|👩‍🔧|👩‍🌾|🙎‍♂️|👨‍🌾|👩‍🦲|👩‍🍳|👨‍🦽|👩‍🦳|🙎‍♀️|👨‍⚖️|👩‍🦼|👩‍🏫|👨‍🏫|👩‍⚖️|👨‍🍳|🧞‍♂|🤸‍♀|🤼‍♂|🧍‍♀|🇿🇲|🤸‍♂|🤽‍♀|🧝‍♂|🤽‍♂|🇦🇲|🧍‍♂|🤾‍♂|🤾‍♀|🤹‍♂|🇦🇪|🧝‍♀|🤹‍♀|🧘‍♂|🧘‍♀|🤼‍♀|🚵‍♀|🏋‍♂|🚶‍♀|🏄‍♂|🏌‍♀|🇦🇬|🧗‍♀|🚶‍♂|🧗‍♂|💇‍♂|🧖‍♀|🧖‍♂|👯‍♀|👯‍♂|🇦🇨|🏴‍☠|🏃‍♀|💇‍♀|🏃‍♂|🇦🇫|🏄‍♀|💆‍♀|🚣‍♂|🚣‍♀|🚵‍♂|🇦🇱|🚴‍♀|🇦🇩|🚴‍♂|🏋‍♀|🧟‍♂|🧎‍♂|🧞‍♀|⛹️‍♀|⛹‍♀️|🇦🇮|⛹️‍♂|⛹‍♂️|🏊‍♀|💆‍♂|🧎‍♀|🏊‍♂|🧟‍♀|🏌‍♂|🇿🇼|🧜‍♀|🇲🇸|🇲🇷|🇲🇶|🇲🇵|🇲🇴|🇲🇳|🇲🇲|🇲🇱|🇲🇰|🇲🇭|🇲🇬|🇲🇫|🇲🇪|🇲🇩|🇲🇨|🇲🇹|🇲🇺|🇲🇻|🇲🇼|🇴🇲|🇳🇿|🇳🇺|🇳🇷|🇳🇵|🇳🇴|🇳🇱|🇲🇦|🇳🇮|🇳🇫|🇳🇪|🇳🇨|🇳🇦|🇲🇿|🇲🇾|🇲🇽|🇳🇬|🇵🇦|🇱🇾|🇱🇺|🇯🇲|🇯🇪|🇮🇹|🇮🇸|🇮🇷|🇮🇶|🇮🇴|🇮🇳|🇮🇲|🇮🇱|🇮🇪|🇮🇩|🇮🇨|🇭🇺|🇭🇹|🇯🇴|🇯🇵|🇰🇪|🇰🇬|🇱🇹|🇱🇸|🇱🇷|🇱🇰|🇱🇮|🇱🇨|🇱🇧|🇱🇻|🇱🇦|🇰🇾|🇰🇼|🇰🇵|🇰🇳|🇰🇲|🇰🇮|🇰🇭|🇰🇿|🇵🇪|🇵🇫|🇵🇬|🇺🇦|🇹🇿|🇹🇼|🇹🇻|🇹🇹|🇹🇷|🇹🇴|🇹🇳|🇹🇲|🇹🇱|🇹🇰|🇹🇯|🇹🇭|🇹🇬|🇹🇫|🇺🇬|🇺🇲|🇺🇳|🇺🇸|🇦🇴|🇿🇦|🇾🇹|🇾🇪|🇽🇰|🇼🇸|🇼🇫|🇹🇩|🇻🇺|🇻🇮|🇻🇬|🇻🇪|🇻🇨|🇻🇦|🇺🇿|🇺🇾|🇻🇳|🇹🇨|🇹🇦|🇸🇿|🇷🇼|🇷🇺|🇷🇸|🇷🇴|🇷🇪|🇶🇦|🇵🇾|🇸🇦|🇵🇼|🇵🇸|🇵🇷|🇵🇳|🇵🇲|🇵🇱|🇵🇰|🇵🇭|🇵🇹|🇭🇷|🇸🇧|🇸🇩|🇸🇾|🇸🇽|🇸🇻|🇸🇹|🇸🇸|🇸🇷|🇸🇴|🇸🇨|🇸🇳|🇸🇱|🇸🇰|🇸🇯|🇸🇮|🇸🇭|🇸🇬|🇸🇪|🇸🇲|🇭🇳|🇰🇷|🇭🇰|🇧🇻|🧏‍♂|🇭🇲|🇧🇹|🙇‍♂|🙇‍♀|🇧🇸|🤦‍♂|🤦‍♀|🇧🇷|🤷‍♂|🤷‍♀|👨‍⚕|👩‍⚕|👨‍⚖|👩‍⚖|👨‍✈|🙋‍♀|🙋‍♂|🇧🇼|💁‍♀|👱‍♀|🇨🇭|🇨🇬|🇨🇫|🇨🇩|🙍‍♂|🙍‍♀|🇨🇨|👩‍✈|🙎‍♂|🇨🇦|🙅‍♂|🙅‍♀|🇧🇿|🙆‍♂|🙆‍♀|🇧🇾|💁‍♂|🙎‍♀|🇧🇶|👮‍♂|👮‍♀|🇦🇽|🇦🇼|🦸‍♂|🦸‍♀|🇦🇺|🦹‍♂|🦹‍♀|🇦🇹|🇦🇿|🧙‍♂|🇦🇸|🧚‍♂|🧚‍♀|🇦🇷|🧛‍♂|🧛‍♀|🇦🇶|🧜‍♂|🧙‍♀|🇨🇮|🇧🇦|🇧🇩|🇧🇴|🕵‍♂|🕵‍♀|🇧🇳|💂‍♂|💂‍♀|🇧🇲|👷‍♂|🇧🇧|👷‍♀|🇧🇯|🇧🇮|👳‍♂|👳‍♀|🇧🇭|🇧🇬|🇧🇫|🇧🇪|🇧🇱|👱‍♂|🧏‍♀|🇨🇱|🇪🇷|🇪🇸|🇪🇹|🇪🇺|🇫🇮|🇫🇯|🇫🇰|🇫🇲|🇫🇴|🇫🇷|🇬🇦|🇬🇧|🇬🇩|🇬🇪|🇬🇫|🇨🇰|🇬🇭|🇬🇮|🇬🇱|🇬🇲|🇬🇳|🇬🇵|🇬🇶|🇬🇷|🇬🇸|🇬🇹|🇬🇺|🇬🇼|🇬🇾|🇪🇭|🇪🇬|🇬🇬|🇩🇰|🇨🇽|🇩🇪|🇨🇼|🇩🇬|🇨🇻|🇨🇺|🇩🇯|🇨🇲|🇨🇳|🇨🇾|🇨🇷|🇨🇵|🇨🇴|🇨🇿|🇩🇿|🇪🇪|🇪🇨|🇩🇴|🇩🇲|🇪🇦|🗄️|🌡️|🗃️|🏖️|🏕️|🗒️|🕰️|🖇️|🗺️|🗓️|🖱️|🏔️|🛎️|🍽️|🌫️|🛡️|🗝️|8️⃣|7️⃣|6️⃣|5️⃣|4️⃣|3️⃣|2️⃣|1️⃣|0️⃣|🕊️|[*]️⃣|🗑️|#️⃣|🕷️|🕸️|🏵️|🕉️|🌶️|🛋️|🛏️|🗜️|🖲️|🗡️|🛠️|🛍️|🏜️|🖌️|🗂️|🖋️|🌤️|9️⃣|🌥️|🌦️|🗳️|🎟️|🌧️|🌨️|🖥️|🌩️|🏙️|🏷️|🛢️|🗞️|🏎️|🕯️|📽️|🎞️|🖨️|🌪️|🏝️|🛳️|🛤️|🏞️|🏟️|🏛️|🏗️|🏘️|🖍️|🏚️|🖊️|🛰️|🖼️|🛣️|🎗️|🕶️|🎚️|🎛️|🛩️|🕹️|🏍️|🌬️|🛥️|🎙️|🐿️|🎖️|🗣️|🗨️|🕳️|⛹‍♂|⛹‍♀|🅾️|🏋️|🖐️|🅿️|🏌️|🈂️|🈷️|🏳️|👁️|🅱️|🗯️|🕵️|🅰️|🕴️|📧|📬|💻|🔸|🖥|🔷|🔶|▫️|🖨|▪️|⌨️|◻️|🖱|◼️|📪|🖲|💽|💾|💿|📀|🟫|🧮|🟪|🔌|🔋|📟|🎥|🎶|🗳|🎙|🔺|🎚|🔹|🎛|🎤|🎧|📻|🟣|📠|🎷|🎹|🎺|🎻|📮|🪕|🥁|📭|📱|📲|☎️|📞|🎸|🟦|🎞|🟩|📃|📜|📄|📰|🟧|🗞|📑|🔖|🟥|🏷|💰|📤|💴|🟤|💶|💷|💸|💳|🧾|💹|💱|💲|✉️|📩|💵|📨|📒|📚|📽|🎬|📺|📷|📸|📹|🎵|🔍|🔎|🟨|📫|📓|🕯|🔦|🏮|📦|🪔|📔|📕|📖|📗|📥|📘|📙|💡|📼|📣|🔕|🪀|🪁|🎱|🔮|🧿|🎮|🕹|🎰|🎲|🏳|🧩|🧸|🎯|🏴|🎌|♥️|🚩|♦️|🏁|♣️|🔲|♟️|🃏|🔳|🀄|🎴|♠️|🎭|🥌|🎿|🏆|🏅|🥇|🥈|🥉|🥎|🏀|🏐|🏈|🏉|🎾|🥏|🛷|🎳|🏑|🏒|🥍|🏓|🏸|🥊|🥋|🥅|⛸️|🎣|🤿|🎽|🏏|🎼|🎖|🎨|👟|🥾|🥿|👠|👡|🩰|👢|👑|👒|🎩|🎓|🧢|👞|⛑️|💄|💍|🔻|💎|🔇|🔈|🔉|🔊|📢|✏️|📯|🔔|📿|🖼|🎒|👝|🧵|🧶|👓|🔘|🕶|🥽|🥼|🦺|👔|💠|👕|👖|🛍|🧣|🧥|🧦|👗|👘|🥻|🩱|🩲|🩳|👙|👚|👛|👜|🧤|✒️|9⃣|🖋|🔃|🔄|🔙|🔚|🔛|🔜|🔝|🛐|⚛️|🆖|🕉|✡️|⤵️|☸️|✝️|☦️|☪️|☮️|🕎|🔯|🔀|🔁|🔂|▶️|⏭️|🆕|☯️|⏯️|⤴️|↩️|🛂|🛃|🛄|🛅|⚠️|🚸|🚫|🚳|🚭|🚯|🚱|🚷|↪️|📵|☢️|☣️|⬆️|↗️|➡️|↘️|⬇️|↙️|⬅️|↖️|↕️|↔️|🔞|◀️|⏮️|🔼|〰️|©️|®️|™️|🆓|#⃣|🆒|[*]⃣|🆑|🅱|0⃣|1⃣|⁉️|🆎|🅰|3⃣|4⃣|🔤|5⃣|🔣|6⃣|🔢|7⃣|🔡|8⃣|🔠|2⃣|‼️|❇️|✴️|🔽|⏸️|⏹️|Ⓜ️|⏺️|🆔|⏏️|🎦|🔅|ℹ️|📶|📳|📴|♀️|♂️|⚕️|♾️|♻️|⚜️|🔱|📛|🔰|☑️|✔️|✖️|〽️|✳️|🚾|🚼|🚻|🚺|📉|📊|🈯|📋|📌|📍|📎|🈶|🖇|📏|📐|✂️|📈|🈷|🈂|🗄|🈁|🗑|🔒|🔓|🔏|🔐|🔑|🆚|🗝|🔨|🗃|🉐|📇|🗓|🔟|🖊|🟢|🖌|🟡|🟠|🔴|🖍|📝|🈵|💼|🈺|📁|㊙️|📂|㊗️|🈳|🈴|🗂|🈸|📅|🉑|📆|🈲|🈚|🗒|🈹|🪓|🔵|⛏️|🆙|🆗|🛏|🅾|🛋|🪑|🚽|🚿|🛁|🪒|🧴|🧷|🧹|🚪|🧺|🧼|🧽|🧯|🛒|🚬|⚰️|⚱️|🗿|🏧|🚮|🚰|🚹|🧻|🩺|🩹|💊|🛠|🆘|🗡|⚔️|🔫|🏹|🅿|🛡|🔧|🔩|⚙️|🗜|⚖️|🦯|🔗|⛓️|🧰|🧲|⚗️|🧪|🧫|🧬|🔬|🔭|📡|💉|🩸|⚒️|🔆|🧉|🎟|💑|💏|👬|👫|👭|🛌|🛀|🧘|🤹|🤾|🤽|🤼|🤸|🚵|🚴|🏋|⛹️|🏊|🚣|🏄|🏌|👪|🗣|👤|👥|🦓|🦄|🐎|🐴|🐆|🐅|🐯|🦁|🐈|🐱|🏂|🦝|🐺|🐩|🦮|🐕|🐶|🦧|🦍|🐒|🐵|👣|🦊|🦌|⛷️|🤺|👰|🤵|🧕|👲|👳|👸|🤴|👷|💂|🕵|👮|🤷|🤦|🙇|🧏|🙋|💁|🙆|🙅|🙎|🙍|🤰|🤱|👼|🎅|🧗|🧖|👯|🕴|🕺|💃|🏃|🧎|🧍|🚶|🏇|💇|🧟|🧞|🧝|🧜|🧛|🧚|🧙|🦹|🦸|🤶|💆|🐮|🐂|🐃|🐞|🐝|🐜|🐛|🦋|🐌|🐚|🐙|🦈|🐡|🐠|🐟|🐬|🐋|🐳|🦖|🦕|🐉|🐲|🐍|🦎|🦗|🕷|🕸|🦂|🍁|🍀|☘️|🌿|🌾|🌵|🌴|🌳|🌲|🌱|🐢|🌷|🌻|🌺|🥀|🌹|🏵|💮|🎫|💐|🦠|🦟|🌼|🐊|🐸|🦜|🐿|🐇|🐰|🐹|🐀|🐁|🐭|🦛|🦏|🐘|🦔|🦒|🐫|🐪|🐐|🐑|🐏|🐽|🐗|🐖|🐷|🐄|🦙|👵|🦇|🐨|🦚|🦩|🦉|🦢|🦆|🦅|🕊|🐧|🐦|🐥|🐻|🐤|🐓|🐔|🦃|🐾|🦡|🦘|🦨|🦦|🦥|🐼|🐣|🍂|👴|👩|😧|😦|🥺|😳|😲|😯|😮|☹️|🙁|😟|😕|🧐|🤓|😎|🥳|🤠|🤯|😵|🥴|🥶|🥵|😨|😰|😥|😢|👻|👺|👹|🤡|💩|☠️|💀|👿|😈|🤬|🤧|😠|😤|🥱|😫|😩|😓|😞|😣|😖|😱|😭|😡|👽|🤮|🤕|😋|😙|😚|☺️|😗|😘|🤩|😍|🥰|😇|😊|😉|🙃|🙂|😂|🤣|😅|😆|😁|😄|😃|😛|😜|🤪|😝|🤒|😷|😴|🤤|😪|😔|😌|🤥|😬|🙄|🤢|😒|😶|😑|😐|🤨|🤐|🤔|🤫|🤭|🤗|🤑|😏|👾|🤖|😺|🙏|🤝|🤲|👐|🙌|👏|🤜|🤛|👊|👎|👍|☝️|👇|🖕|👆|👉|👈|🤙|🤘|🤟|🤞|✍️|💅|🤳|💪|🧔|👨|👱|🧑|👧|👦|🧒|👶|👄|👅|✌️|👁|🦴|🦷|🧠|👃|🦻|👂|🦶|🦵|🦿|🦾|👀|🤏|👌|🖖|❣️|💟|💕|💞|💓|💗|💖|💝|💘|💌|💔|💋|🙉|🙈|😾|😿|🙀|😽|😼|😻|😹|😸|🙊|🧓|❤️|💛|🖐|🤚|👋|💤|💭|🗯|🗨|💬|💣|🕳|🧡|💨|💫|💥|💢|💯|🤍|🖤|🤎|💜|💙|💚|💦|🍃|🌸|🍈|🚎|🚍|🚌|🚋|🚞|🚝|🚊|🚉|🚈|🚇|🚆|🚅|🚄|🚃|🚂|🎪|💈|🎢|🎡|🎠|♨️|🌉|🌇|🚐|🚑|🚒|🚓|🚥|🚨|🛢|🛤|🛣|🚏|🛹|🛴|🚲|🛺|🦼|🌆|🦽|🏍|🏎|🚜|🚛|🚚|🚙|🚘|🚗|🚖|🚕|🚔|🛵|🚦|🌅|🏙|🍇|🏛|🏟|🏞|🏝|🏜|🏖|🏕|🗻|🌋|⛰️|🏔|🧭|🗾|🗺|🌐|🌏|🌎|🌍|🏺|🔪|🥄|🍴|🧱|🏘|🏚|🏠|🌃|🌁|🕋|⛩️|🕍|🛕|🕌|🗽|🗼|💒|🏰|🌄|🏯|🏬|🏫|🏪|🏩|🏨|🏦|🏥|🏤|🏣|🏢|🏡|🏭|🛑|🚧|🛶|🌀|🌬|🌫|🌪|🌩|🌨|🌧|🌦|🌥|🌤|⛈️|☁️|🌌|🌠|🌟|🪐|🌞|🌝|☀️|🌡|🌜|🌛|🌚|🌈|🌂|☂️|⛱️|🎗|🎁|🎀|🧧|🎑|🎐|🎏|🎎|🎍|🎋|🎊|🌙|🎉|🧨|🎇|🎆|🎄|🎃|🌊|💧|🔥|☄️|☃️|❄️|🎈|🌘|🌗|🌖|🕛|🕰|⏲️|⏱️|🧳|🛎|🛸|🚀|🛰|🚡|🚠|🕧|🚟|💺|🪂|🛬|🛫|🛩|✈️|🚢|🛥|⛴️|🛳|🚤|🚁|🍽|🕐|🕑|🌕|🌔|🌓|🌒|🌑|🕦|🕚|🕥|🕙|🕤|🕘|🕜|🕣|🕢|🕖|🕡|🕕|🕠|🕔|🕟|🕓|🕞|🕒|🕝|🕗|🥢|🏗|😀|🍟|🍕|🌭|🥪|🌮|🌯|🥙|🧆|🥚|🍳|🥘|🍲|🥣|🥗|🍔|🍿|🧂|🥫|🍱|🍘|🍙|🍚|🍛|🍜|🍝|🍠|🍢|🍣|🍤|🍥|🧈|🥮|🥓|🍗|🍆|🥑|🥕|🌽|🥥|🌶|🥒|🥬|🥦|🧄|🍅|🧅|🥝|🍄|🥩|🍓|🍑|🥜|🌰|🍞|🥐|🍐|🥖|🍏|🥨|🥯|🥞|🧇|🧀|🍖|🍒|🍡|🥔|🥟|🥧|🥭|🧃|🍋|🍫|🍬|🍭|🥤|🍮|🥃|🍯|🍼|🥂|🍍|🥛|🍻|🍵|🍺|🍶|🍾|🍷|🍹|🍸|🧁|🍰|🍌|🎂|🦀|🥡|🧊|🦞|🦐|🦑|🍊|🥠|🍎|🦪|🍦|🍪|🍧|🍉|🍨|🍩|⬆|◀|⚛|⏯|☹|☁|⬅|⏭|☀|⏩|⭐|↗|⬇|↙|⚾|⤵|⛅|⏏|⚽|⏹|⏪|↘|✡|⏬|☘|➡|⏫|⏮|⏸|⏺|♠|⛈|↪|♋|⛱|⚡|♊|☦|☺|☣|✨|☪|☮|⛳|↔|⛸|☄|♉|☃|♈|↩|☔|✝|☂|☸|↖|♟|⤴|♣|↕|♦|♥|⛎|⛄|♓|♒|♑|♐|☯|♏|♎|♍|♌|▶|❄|✴|☢|⛓|⛩|⛲|⛺|⚗|Ⓜ|ℹ|♨|✍|⚰|✊|⚱|☝|♿|™|®|✌|⌨|©|✉|⛪|⚖|⚙|✂|▫|◾|◻|⛹|◼|⬜|⬛|⛷|〰|⛰|⚫|☕|⛏|⚒|㊙|㊗|✒|⚔|✏|⚪|♀|✋|❗|✖|⛔|✔|⌛|⏳|⌚|⏰|⏱|☑|⏲|✅|⭕|⛑|⚜|♻|♾|⚕|♂|☠|❌|❎|➕|➖|❕|❔|❓|⁉|⚓|☎|⛵|‼|❇|⛽|⛴|✈|✳|⚠|〽|➿|❤|➰|➗|❣|◽|▪)";

            MatchOne = new Regex(regextext);
            MatchMultiple = new Regex(regextext + "+");
        }


    }
}

