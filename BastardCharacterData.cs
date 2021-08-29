using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using ReLogic.Text;
using TomatoLib.Common.Utilities.Extensions;

namespace TimesNewBastard
{
    public readonly struct BastardCharacterData
    {
        private readonly (Texture2D, Texture2D) Textures;
        private readonly (Rectangle, Rectangle) Glyphs;
        private readonly (Rectangle, Rectangle) Paddings;
        private readonly (Vector3, Vector3) Kernings;

        public int Index { get; }

        public Texture2D Texture => GetBastard(Textures);

        public Rectangle Glyph => GetBastard(Glyphs);

        public Rectangle Padding => GetBastard(Paddings);

        public Vector3 Kerning => GetBastard(Kernings);

        public bool IsBastardized() => Index % 7 == 0;

        public T GetBastard<T>((T, T) tuple) => IsBastardized() ? tuple.Item2 : tuple.Item1;

        public BastardCharacterData((Texture2D, Texture2D) textures, (Rectangle, Rectangle) glyphs,
            (Rectangle, Rectangle) paddings, (Vector3, Vector3) kernings, int index = 0)
        {
            Textures = textures;
            Glyphs = glyphs;
            Paddings = paddings;
            Kernings = kernings;
            Index = index;
        }

        public GlyphMetrics ToGlyphMetric()
        {
            float kerningX = Math.Max(Kernings.Item1.X, Kernings.Item2.X);
            float kerningY = Math.Max(Kernings.Item2.Y, Kernings.Item2.Y);
            float kerningZ = Math.Max(Kernings.Item2.Z, Kernings.Item2.Z);

            return GlyphMetrics.FromKerningData(kerningX, kerningY, kerningZ);
        }

        public static BastardCharacterData WithIndex(BastardCharacterData data, int index) =>
            new(data.Textures, data.Glyphs, data.Paddings, data.Kernings, index);

        public static BastardCharacterData FromObjects(object dataOne, object dataTwo)
        {
            Type type = typeof(DynamicSpriteFont).GetCachedNestedType("SpriteCharacterData");

            (Texture2D, Texture2D) textures = new();
            (Rectangle, Rectangle) glyphs = new();
            (Rectangle, Rectangle) paddings = new();
            (Vector3, Vector3) kernings = new();

            textures.Item1 = type.GetCachedField("Texture").GetValue<Texture2D>(dataOne);
            glyphs.Item1 = type.GetCachedField("Glyph").GetValue<Rectangle>(dataOne);
            paddings.Item1 = type.GetCachedField("Padding").GetValue<Rectangle>(dataOne);
            kernings.Item1 = type.GetCachedField("Kerning").GetValue<Vector3>(dataOne);

            textures.Item2 = type.GetCachedField("Texture").GetValue<Texture2D>(dataTwo);
            glyphs.Item2 = type.GetCachedField("Glyph").GetValue<Rectangle>(dataTwo);
            paddings.Item2 = type.GetCachedField("Padding").GetValue<Rectangle>(dataTwo);
            kernings.Item2 = type.GetCachedField("Kerning").GetValue<Vector3>(dataTwo);

            return new BastardCharacterData(textures, glyphs, paddings, kernings);
        }
    }
}