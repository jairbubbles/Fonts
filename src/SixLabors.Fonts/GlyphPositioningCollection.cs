// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using SixLabors.Fonts.Unicode;

namespace SixLabors.Fonts
{
    /// <summary>
    /// Represents a collection of glyph metrics that are mapped to input codepoints.
    /// </summary>
    public sealed class GlyphPositioningCollection
    {
        /// <summary>
        /// Contains a map between the index of a map within the collection and its offset.
        /// </summary>
        private readonly Dictionary<int, int> offsets = new();

        /// <summary>
        /// Contains a map between non-sequential codepoint offsets and their glyphss.
        /// </summary>
        private readonly Dictionary<int, GlyphMetrics[]> map = new();

        /// <summary>
        /// Adds the collection of glyph ids to the metrics collection.
        /// Adding subsequent collections will overwrite any glyphs that have been previously
        /// identified as fallbacks.
        /// </summary>
        /// <param name="fontMetrics">The font face with metrics.</param>
        /// <param name="collection">The glyph substitution collection.</param>
        /// <param name="options">The renderer options.</param>
        public void AddOrUpdate(IFontMetrics fontMetrics, GlyphSubstitutionCollection collection, RendererOptions options)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                collection.GetCodePointAndGlyphIds(i, out CodePoint codePoint, out int offset, out IEnumerable<int>? glyphIds);

                if (this.map.TryGetValue(offset, out GlyphMetrics[]? metrics)
                    && metrics[0].GlyphType != GlyphType.Fallback)
                {
                    continue;
                }

                var m = new List<GlyphMetrics>();
                foreach (int id in glyphIds)
                {
                    m.AddRange(fontMetrics.GetGlyphMetrics(codePoint, id, options.ColorFontSupport));
                }

                // TODO: It's like we have to get the ids at a given index.
                this.offsets[i] = offset;
                this.map[offset] = m.ToArray();
            }
        }

        /// <summary>
        /// Applies an offset to the glyphs at the given index and id.
        /// </summary>
        /// <param name="index">The zero-based index of the elements to offset.</param>
        /// <param name="glyphId">The id of the glyph to offset.</param>
        /// <param name="x">The x-offset.</param>
        /// <param name="y">The y-offset.</param>
        public void Offset(ushort index, ushort glyphId, short x, short y)
        {
            foreach (GlyphMetrics m in this.map[this.offsets[index]])
            {
                if (m.Index != glyphId)
                {
                    continue;
                }

                m.ApplyOffset(x, y);
            }
        }

        /// <summary>
        /// Updates the advanced metrics of the glyphs at the given index and id.
        /// </summary>
        /// <param name="index">The zero-based index of the elements to offset.</param>
        /// <param name="glyphId">The id of the glyph to offset.</param>
        /// <param name="x">The x-advance.</param>
        /// <param name="y">The y-advance.</param>
        public void Advance(ushort index, ushort glyphId, short x, short y)
        {
            foreach (GlyphMetrics m in this.map[this.offsets[index]])
            {
                if (m.Index != glyphId)
                {
                    continue;
                }

                m.ApplyAdvance(x, y);
            }
        }

        /// <summary>
        /// Gets the glyph metrics at the given codepoint offset.
        /// </summary>
        /// <param name="offset">The zero-based index within the input codepoint collection.</param>
        /// <param name="metrics">
        /// When this method returns, contains the glyph metrics associated with the specified offset,
        /// if the value is found; otherwise, the default value for the type of the metrics parameter.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns>The <see cref="T:GlyphMetrics[]"/>.</returns>
        public bool TryGetGlypMetricsAtOffset(int offset, out GlyphMetrics[]? metrics)
            => this.map.TryGetValue(offset, out metrics);
    }
}