namespace Mages.SCX.Tokens
{
    public enum TokenType
    {
        CharacterName = 0b00000001,
        DialogueLine = 0b00000010,
        Present = 0b00000011,
        Present_ResetAlignment = 0b00001000,
        RubyBase = 0b00001001,
        WTFToken = 0b00001101, // Can be achievement or wtf
        WTFToken2 = 0b00001110,
        RubyTextStart = 0b00001010,
        RubyTextEnd = 0b00001011,
        SetAlignment_Center = 0b00001111,

        SetFontSize = 0b00001100,
        SetTopMargin = 0b00010001,
        SetLeftMargin = 0b00010010,
        WTFShort = 0b00011001,

        SetColor = 0b00000100,
        EvaluateExpression = 0b00010101,

        LineBreak = 0b00000000,
        TextMask = 0b10000000,
        Terminator = 0b11111111,
    }
}
