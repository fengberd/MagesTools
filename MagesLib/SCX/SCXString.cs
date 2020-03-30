using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using Mages.SCX.Tokens;

namespace Mages.SCX
{
    public class SCXString
    {
        public List<Token> Tokens = new List<Token>();

        public SCXString(SCXReader reader)
        {
            while (!reader.EOF)
            {
                var type = (TokenType)reader.ReadByte();
                switch (type)
                {
                case TokenType.CharacterName:
                case TokenType.DialogueLine:
                case TokenType.Present:
                case TokenType.Present_ResetAlignment:
                case TokenType.RubyBase:
                case TokenType.RubyTextStart:
                case TokenType.RubyTextEnd:
                case TokenType.SetAlignment_Center:
                case TokenType.WTFToken2:
                    Tokens.Add(new Token(type));
                    break;
                case TokenType.WTFToken:
                    Tokens.Add(new Token(type));
                    break;
                case TokenType.SetFontSize:
                case TokenType.SetTopMargin:
                case TokenType.SetLeftMargin:
                case TokenType.WTFShort:
                    Tokens.Add(new ShortToken(type, reader.ReadInt16()));
                    break;
                case TokenType.SetColor:
                case TokenType.EvaluateExpression:
                    Tokens.Add(new ExpressionToken(type, reader));
                    break;
                case TokenType.Terminator:
                    return;
                case TokenType.LineBreak:
                    CreateText(reader);
                    break;
                default:
                    if ((type & TokenType.TextMask) == 0)
                    {
                        throw new Exception("Unexpected token");
                    }
                    CreateText(reader);
                    break;
                }
            }
        }

        private void CreateText(SCXReader reader)
        {
            reader.BaseStream.Position--;
            Tokens.Add(new TextToken(reader));
        }

        public void Encode(BinaryWriter target)
        {
            foreach (var t in Tokens)
            {
                t.Encode(target);
            }
            target.Write((byte)TokenType.Terminator);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var t in Tokens)
            {
                sb.Append(t.ToString()).Append(" ");
            }
            if (sb.Length == 0)
            {
                return "(empty)";
            }
            sb.Length--;
            return sb.ToString();
        }
    }
}
