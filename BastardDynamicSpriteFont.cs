using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ReLogic.Graphics;
using TomatoLib.Common.Utilities.Extensions;

namespace TimesNewBastard
{
    public class BastardDynamicSpriteFont : DynamicSpriteFont
    {
        public DynamicSpriteFont MainSpriteFont { get; }

        public DynamicSpriteFont BastardSpriteFont { get; }

        public Dictionary<char, BastardCharacterData> SpriteCharacters;
        public BastardCharacterData DefaultCharacterData;
        public int Index;

        public BastardDynamicSpriteFont(DynamicSpriteFont main, DynamicSpriteFont bastard) :
            base(Math.Max(main.CharacterSpacing, bastard.CharacterSpacing), 
                Math.Max(main.LineSpacing, bastard.LineSpacing),
                main.DefaultCharacter)
        {
            MainSpriteFont = main;
            BastardSpriteFont = bastard;

            SpriteCharacters = new Dictionary<char, BastardCharacterData>();

            static Dictionary<char, object> Dict(IDictionary dictionary)
            {
                List<DictionaryEntry> entries = new();

                foreach (DictionaryEntry entry in dictionary)
                    entries.Add(entry);

                return entries.ToDictionary(x => (char) x.Key, x => x.Value);
            }

            Dictionary<char, object> mainCharacters = Dict(main.GetFieldValue<DynamicSpriteFont, IDictionary>("_spriteCharacters"));
            Dictionary<char, object> bastardCharacters = Dict(bastard.GetFieldValue<DynamicSpriteFont, IDictionary>("_spriteCharacters"));

            foreach (char character in mainCharacters.Keys)
            {
                object dataOne = mainCharacters[character];
                object dataTwo = bastardCharacters[character];

                SpriteCharacters[character] = BastardCharacterData.FromObjects(dataOne, dataTwo);

                if (character == DefaultCharacter)
                    DefaultCharacterData = SpriteCharacters[character];
            }
        }
    }
}