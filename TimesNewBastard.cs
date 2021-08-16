using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using ReLogic.Text;
using Terraria.GameContent;
using Terraria.ModLoader;
using TomatoLib;
using TomatoLib.Core.MonoModding;
using TomatoLib.Core.Utilities.Extensions;

namespace TimesNewBastard
{
    public class TimesNewBastard : TomatoMod
    {
        public Asset<DynamicSpriteFont> TimesNewRomanFont;
        public Asset<DynamicSpriteFont> TimesNewRomanFontLarge;
        public Asset<DynamicSpriteFont> SansSerifFont;
        public Asset<DynamicSpriteFont> SansSerifFontLarge;
        public DynamicSpriteFont MouseFont;
        public DynamicSpriteFont DeathText;
        public BastardDynamicSpriteFont BastardMouseFont;
        public BastardDynamicSpriteFont BastardDeathFont;

        public delegate void BastardIHateYouA(DynamicSpriteFont self, string text, SpriteBatch spriteBatch,
            Vector2 startPosition, Color color, float rotation, Vector2 origin, ref Vector2 scale,
            SpriteEffects spriteEffects, float depth);

        public override void Load()
        {
            base.Load();

            Type type = typeof(BastardDynamicSpriteFont);

            this.CreateDetour(type.GetCachedMethod("SetPages"),
                GetType().GetCachedMethod(nameof(SetPagesOverride)));

            this.CreateDetour(type.GetCachedMethod("IsCharacterSupported"),
                GetType().GetCachedMethod(nameof(IsCharacterSupportedOverride)));

            this.CreateDetour(type.GetCachedMethod("AreCharactersSupported"),
                GetType().GetCachedMethod(nameof(AreCharactersSupportedOverride)));

            this.CreateDetour(type.GetCachedMethod("InternalDraw"),
                GetType().GetCachedMethod(nameof(InternalDrawOverride)));

            this.CreateDetour(type.GetCachedMethod("MeasureString"),
                GetType().GetCachedMethod(nameof(MeasureStringOverride)));

            this.CreateDetour(type.GetCachedMethod("GetCharacterMetrics"),
                GetType().GetCachedMethod(nameof(GetCharacterMetricsOverride)));
        }

        public override void PostSetupContent()
        {
            base.PostSetupContent();

            TimesNewRomanFont = ModContent.Request<DynamicSpriteFont>("TimesNewBastard/Assets/TimesNewRoman", AssetRequestMode.ImmediateLoad);
            TimesNewRomanFontLarge = ModContent.Request<DynamicSpriteFont>("TimesNewBastard/Assets/TimesNewRomanLarge", AssetRequestMode.ImmediateLoad);
            SansSerifFont = ModContent.Request<DynamicSpriteFont>("TimesNewBastard/Assets/SansSerif", AssetRequestMode.ImmediateLoad);
            SansSerifFontLarge = ModContent.Request<DynamicSpriteFont>("TimesNewBastard/Assets/SansSerifLarge", AssetRequestMode.ImmediateLoad);

            MouseFont = FontAssets.MouseText.Value;
            DeathText = FontAssets.DeathText.Value;

            BastardMouseFont = new BastardDynamicSpriteFont(TimesNewRomanFont.Value, SansSerifFont.Value);
            BastardDeathFont = new BastardDynamicSpriteFont(TimesNewRomanFontLarge.Value, SansSerifFontLarge.Value);

            FontAssets.MouseText.SetFieldValue("ownValue", BastardMouseFont);
            FontAssets.DeathText.SetFieldValue("ownValue", BastardDeathFont);
        }

        public override void Unload()
        {
            base.Unload();

            FontAssets.MouseText.SetFieldValue("ownValue", MouseFont);
            FontAssets.DeathText.SetFieldValue("ownValue", DeathText);
        }

        public static bool IsCharacterSupportedOverride(Func<DynamicSpriteFont, char, bool> orig, DynamicSpriteFont self, char character)
        {
            if (self is not BastardDynamicSpriteFont bastard)
                return orig(self, character);

            return character is '\n' or '\r' || bastard.SpriteCharacters.ContainsKey(character);
        }

        public static bool AreCharactersSupportedOverride(Func<DynamicSpriteFont, IEnumerable<char>, bool> orig, DynamicSpriteFont self, IEnumerable<char> characters) => self is not BastardDynamicSpriteFont bastard ? orig(self, characters) : characters.All(bastard.IsCharacterSupported);

        public static void InternalDrawOverride(BastardIHateYouA orig, DynamicSpriteFont self, string text, SpriteBatch spriteBatch, Vector2 startPosition,
            Color color, float rotation, Vector2 origin, ref Vector2 scale, SpriteEffects spriteEffects, float depth)
        {
            if (self is not BastardDynamicSpriteFont bastard)
            {
                orig(self, text, spriteBatch, startPosition, color, rotation, origin, ref scale, spriteEffects, depth);
                return;
            }

            bastard.Index = 0;

            Matrix matrix = Matrix.CreateTranslation((0.0f - origin.X) * scale.X, (0.0f - origin.Y) * scale.Y, 0.0f) *
                            Matrix.CreateRotationZ(rotation);
            Vector2 size = Vector2.Zero;
            Vector2 direction = Vector2.One;
            bool newLine = true;
            float what = 0.0f;

            if ((uint)spriteEffects > 0U)
            {
                Vector2 vector2 = bastard.MeasureString(text);

                if (spriteEffects.HasFlag(SpriteEffects.FlipHorizontally))
                {
                    what = vector2.X * scale.X;
                    direction.X = -1f;
                }

                if (spriteEffects.HasFlag(SpriteEffects.FlipVertically))
                {
                    size.Y = (vector2.Y - bastard.LineSpacing) * scale.Y;
                    direction.Y = -1f;
                }
            }

            size.X = what;

            foreach (char character in text)
            {
                switch (character)
                {
                    case '\n':
                        size.X = what;
                        size.Y += bastard.LineSpacing * scale.Y * direction.Y;
                        newLine = true;
                        continue;

                    case '\r':
                        continue;
                }

                bastard.Index++;

                BastardCharacterData characterData = !bastard.SpriteCharacters.ContainsKey(character)
                    ? bastard.DefaultCharacterData
                    : bastard.SpriteCharacters[character];

                characterData = BastardCharacterData.WithIndex(characterData, bastard.Index);

                Vector3 kerning = characterData.Kerning;
                Rectangle padding = characterData.Padding;

                if (spriteEffects.HasFlag(SpriteEffects.FlipHorizontally))
                    padding.X -= padding.Width;

                if (spriteEffects.HasFlag(SpriteEffects.FlipVertically))
                    padding.Y = bastard.LineSpacing - characterData.Glyph.Height - padding.Y;

                if (newLine)
                    kerning.X = Math.Max(kerning.X, 0.0f);
                else
                    size.X += bastard.CharacterSpacing * scale.X * direction.X;

                size.X += kerning.X * scale.X * direction.X;
                Vector2 result = size;

                result.X += padding.X * scale.X;
                result.Y += padding.Y * scale.Y;

                Vector2.Transform(ref result, ref matrix, out result);
                result += startPosition;

                spriteBatch.Draw(characterData.Texture, result, characterData.Glyph, color, rotation,
                    Vector2.Zero, scale, spriteEffects, depth);

                size.X += (kerning.Y + kerning.Z) * scale.X * direction.X;
                newLine = false;
            }
        }

        public static Vector2 MeasureStringOverride(Func<DynamicSpriteFont, string, Vector2> orig, DynamicSpriteFont self, string text)
        {
            if (self is not BastardDynamicSpriteFont bastard)
                return orig(self, text);

            bastard.Index = 0;

            if (text.Length == 0)
                return Vector2.Zero;

            Vector2 size = Vector2.Zero;

            size.Y = bastard.LineSpacing;

            float xCap = 0f;
            int newLineCount = 0;
            float xPaddingIdk = 0f;
            bool newLine = false;

            foreach (char character in text)
                switch (character)
                {
                    case '\n':
                        newLine = true;

                        xCap = Math.Max(size.X + Math.Max(xPaddingIdk, 0.0f), xCap);
                        xPaddingIdk = 0.0f;
                        size = Vector2.Zero;
                        size.Y = bastard.LineSpacing;
                        newLineCount++;
                        continue;

                    case '\r':
                        continue;

                    default:
                        bastard.Index++;

                        BastardCharacterData characterData = !bastard.SpriteCharacters.ContainsKey(character)
                            ? bastard.DefaultCharacterData
                            : bastard.SpriteCharacters[character];

                        characterData = BastardCharacterData.WithIndex(characterData, bastard.Index);

                        Vector3 kerning = characterData.Kerning;

                        if (newLine)
                            kerning.X = Math.Max(kerning.X, 0.0f);
                        else
                            size.X += bastard.CharacterSpacing + xPaddingIdk;

                        size.X += kerning.X + kerning.Y;
                        xPaddingIdk = kerning.Z;
                        size.Y = Math.Max(size.Y, characterData.Padding.Height);

                        newLine = false;
                        break;
                }

            size.X += Math.Max(xPaddingIdk, 0.0f);
            size.Y += newLineCount * bastard.LineSpacing;
            size.X = Math.Max(size.X, xCap);

            return size;
        }

        public static void SetPagesOverride(Action<DynamicSpriteFont, object[]> orig, DynamicSpriteFont self, object[] pages)
        {
            if (self is BastardDynamicSpriteFont)
                return;

            orig(self, pages);
        }

        public static GlyphMetrics GetCharacterMetricsOverride(Func<DynamicSpriteFont, char, GlyphMetrics> orig, DynamicSpriteFont self, char character)
        {
            if (self is not BastardDynamicSpriteFont bastard)
                return orig(self, character);

            BastardCharacterData data = !bastard.SpriteCharacters.ContainsKey(character)
                ? bastard.DefaultCharacterData
                : bastard.SpriteCharacters[character];

            return data.ToGlyphMetric();
        }
    }
}