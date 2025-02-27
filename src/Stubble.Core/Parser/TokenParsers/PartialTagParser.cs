﻿// <copyright file="PartialTagParser.cs" company="Stubble Authors">
// Copyright (c) Stubble Authors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using Stubble.Core.Imported;
using Stubble.Core.Tokens;

namespace Stubble.Core.Parser.TokenParsers
{
    /// <summary>
    /// A parser for partial tags
    /// </summary>
    public class PartialTagParser : Interfaces.InlineParser
    {
        private const char TagId = '>';

        /// <summary>
        /// Tries to match partial tag from the current slice
        /// </summary>
        /// <param name="processor">The processor</param>
        /// <param name="slice">The slice</param>
        /// <returns>If a match was found</returns>
        public override bool Match(Processor processor, ref StringSlice slice)
        {
            var tagStart = slice.Start - processor.CurrentTags.StartTag.Length;
            var index = slice.Start;
            var initialIndex = index;

            while (slice[index].IsWhitespace())
            {
                index++;
            }

            var match = slice[index];
            if (match == TagId)
            {
                index++;
                while (slice[index].IsWhitespace())
                {
                    index++;
                }

                slice.Start = index;
                var startIndex = slice.Start;
                var partialTag = new PartialToken
                {
                    LineIndent = processor.HasSeenNonSpaceOnLine ? 0 : processor.LineIndent,
                    TagStartPosition = tagStart,
                    ContentStartPosition = startIndex,
                    IsClosed = false
                };
                processor.CurrentToken = partialTag;

                while (!slice.IsEmpty && !slice.Match(processor.CurrentTags.EndTag))
                {
                    slice.NextChar();
                }

                if (slice.IsEmpty)
                {
                    slice.Start = initialIndex;
                    return false;
                }

                var content = new StringSlice(slice.Text, startIndex, slice.Start - 1);
                content.TrimEnd();
                var contentEnd = content.End + 1;

                partialTag.ContentEndPosition = contentEnd;
                partialTag.TagEndPosition = slice.Start + processor.CurrentTags.EndTag.Length;
                partialTag.IsClosed = true;
                slice.Start += processor.CurrentTags.EndTag.Length;
                return true;
            }

            return false;
        }
    }
}
