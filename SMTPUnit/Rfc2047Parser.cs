using System;
using System.Text;

namespace HmxLabs.SmtpUnit
{
    /// <summary>
    /// This is pretty much a straight copy and paste from: https://github.com/petrohi/Stratosphere.Imap/blob/master/Stratosphere/Imap/RFC2047Decoder.cs
    /// </summary>
    internal class Rfc2047Parser
    {
        /// <summary>
        /// Parse the input as per RFC 2047
        /// </summary>
        /// <param name="input_"></param>
        /// <returns></returns>
        public static string Parse(string input_)
        {
            var sb = new StringBuilder();
            var currentWord = new StringBuilder();
            var currentSurroundingText = new StringBuilder();
            var readingWord = false;
            var hasSeenAtLeastOneWord = false;

            var wordQuestionMarkCount = 0;
            var i = 0;
            while (i < input_.Length)
            {
                var currentChar = input_[i];
                char peekAhead;
                switch (currentChar)
                {
                    case '=':
                        peekAhead = (i == input_.Length - 1) ? ' ' : input_[i + 1];

                        if (!readingWord && peekAhead == '?')
                        {
                            if (!hasSeenAtLeastOneWord
                                // ReSharper disable once ConditionIsAlwaysTrueOrFalse -- just not gettig it right here!
                                || (hasSeenAtLeastOneWord && currentSurroundingText.ToString().Trim().Length > 0))
                            {
                                sb.Append(currentSurroundingText);
                            }

                            currentSurroundingText = new StringBuilder();
                            hasSeenAtLeastOneWord = true;
                            readingWord = true;
                            wordQuestionMarkCount = 0;
                        }
                        break;

                    case '?':
                        if (readingWord)
                        {
                            wordQuestionMarkCount++;

                            peekAhead = (i == input_.Length - 1) ? ' ' : input_[i + 1];

                            if (wordQuestionMarkCount > 3 && peekAhead == '=')
                            {
                                readingWord = false;

                                currentWord.Append(currentChar);
                                currentWord.Append(peekAhead);

                                sb.Append(ParseEncodedWord(currentWord.ToString()));
                                currentWord = new StringBuilder();

                                i += 2;
                                continue;
                            }
                        }
                        break;
                }

                if (readingWord)
                {
                    currentWord.Append(('_' == currentChar) ? ' ' : currentChar);
                    i++;
                }
                else
                {
                    currentSurroundingText.Append(currentChar);
                    i++;
                }
            }

            sb.Append(currentSurroundingText);

            return sb.ToString();
        }

        private static string ParseEncodedWord(string input_)
        {
            var sb = new StringBuilder();

            if (!input_.StartsWith("=?"))
                return input_;

            if (!input_.EndsWith("?="))
                return input_;

            // Get the name of the encoding but skip the leading =?
            var encodingName = input_.Substring(2, input_.IndexOf("?", 2, StringComparison.InvariantCultureIgnoreCase) - 2);
            var enc = Encoding.ASCII;
            if (!string.IsNullOrEmpty(encodingName))
            {
                enc = Encoding.GetEncoding(encodingName);
            }

            // Get the type of the encoding
            var type = input_[encodingName.Length + 3];

            // Start after the name of the encoding and the other required parts
            var startPosition = encodingName.Length + 5;

            switch (char.ToLowerInvariant(type))
            {
                case 'q':
                    sb.Append(ParseQuotedPrintable(enc, input_, startPosition, true));
                    break;
                case 'b':
                    var baseString = input_.Substring(startPosition, input_.Length - startPosition - 2);
                    var baseDecoded = Convert.FromBase64String(baseString);
                    var intermediate = enc.GetString(baseDecoded);
                    sb.Append(intermediate);
                    break;
            }
            return sb.ToString();
        }

        public static string ParseQuotedPrintable(Encoding enc_, string input_)
        {
            return ParseQuotedPrintable(enc_, input_, 0, false);
        }

        public static string ParseQuotedPrintable(Encoding enc_, string input_, int startPos_, bool skipQuestionEquals_)
        {
            var workingBytes = Encoding.ASCII.GetBytes(input_);

            var i = startPos_;
            var outputPos = i;

            while (i < workingBytes.Length)
            {
                var currentByte = workingBytes[i];
                var peekAhead = new char[2];
                switch (currentByte)
                {
                    case (byte)'=':
                        var canPeekAhead = (i < workingBytes.Length - 2);

                        if (!canPeekAhead)
                        {
                            workingBytes[outputPos] = workingBytes[i];
                            ++outputPos;
                            ++i;
                            break;
                        }

                        var skipNewLineCount = 0;
                        for (var j = 0; j < 2; ++j)
                        {
                            var c = (char)workingBytes[i + j + 1];
                            if ('\r' == c || '\n' == c)
                            {
                                ++skipNewLineCount;
                            }
                        }

                        if (skipNewLineCount > 0)
                        {
                            // If we have a lone equals followed by newline chars, then this is an artificial
                            // line break that should be skipped past.
                            i += 1 + skipNewLineCount;
                        }
                        else
                        {
                            try
                            {
                                peekAhead[0] = (char)workingBytes[i + 1];
                                peekAhead[1] = (char)workingBytes[i + 2];

                                var decodedByte = Convert.ToByte(new string(peekAhead, 0, 2), 16);
                                workingBytes[outputPos] = decodedByte;

                                ++outputPos;
                                i += 3;
                            }
                            catch (Exception)
                            {
                                // could not parse the peek-ahead chars as a hex number... so gobble the un-encoded '='
                                i += 1;
                            }
                        }
                        break;

                    case (byte)'?':
                        if (skipQuestionEquals_ && workingBytes[i + 1] == (byte)'=')
                        {
                            i += 2;
                        }
                        else
                        {
                            workingBytes[outputPos] = workingBytes[i];
                            ++outputPos;
                            ++i;
                        }
                        break;

                    default:
                        workingBytes[outputPos] = workingBytes[i];
                        ++outputPos;
                        ++i;
                        break;
                }
            }

            var output = string.Empty;

            var numBytes = outputPos - startPos_;
            if (numBytes > 0)
            {
                output = enc_.GetString(workingBytes, startPos_, numBytes);
            }

            return output;
        }
    }
}
